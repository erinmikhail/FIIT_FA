using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;

    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;

    internal const int KARATSUBA_THRESHOLD = 64;
    internal const int FFT_THRESHOLD = 1024;

    public bool IsNegative => _signBit == 1;

    internal static IMultiplier SimpleStrategy { get; } = new SimpleMultiplier();
    internal static IMultiplier KaratsubaStrategy { get; } = new KaratsubaMultiplier();
    internal static IMultiplier FFTStrategy { get; } = new FftMultiplier();

    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        if (digits == null || digits.Length == 0)
        {
            _signBit = 0;
            _smallValue = 0;
            _data = null;
            return;
        }

        int realLength = digits.Length;
        while (realLength > 0 && digits[realLength - 1] == 0)
        {
            realLength--;
        }

        if (realLength == 0)
        {
            _signBit = 0;
            _smallValue = 0;
            _data = null;
        }
        else if (realLength == 1)
        {
            _signBit = isNegative ? 1 : 0;
            _smallValue = digits[0];
            _data = null;
        }
        else
        {
            _signBit = isNegative ? 1 : 0;
            _smallValue = 0;
            _data = new uint[realLength];
            Array.Copy(digits, _data, realLength);
        }
    }

    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false)
        : this(digits.ToArray(), isNegative)
    {
    }

    public BetterBigInteger(string value, int radix)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Некорректная строка");
        if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException(nameof(radix), "Основание должно быть от 2 до 36");

        bool isNegative = false;
        int startIndex = 0;

        if (value[0] == '-')
        {
            startIndex = 1;
            isNegative = true;
        }
        else if (value[0] == '+') startIndex = 1;

        if (startIndex == 1 && value.Length == 1) throw new ArgumentException("Некорректная строка");

        List<uint> result = new List<uint>();

        for (int i = startIndex; i < value.Length; ++i)
        {
            char c = value[i];
            int digitValue;

            if (c >= '0' && c <= '9') digitValue = c - '0';
            else if (c >= 'A' && c <= 'Z') digitValue = c - 'A' + 10;
            else if (c >= 'a' && c <= 'z') digitValue = c - 'a' + 10;
            else throw new FormatException($"Некорректный символ: '{c}'");

            if (digitValue >= radix) throw new FormatException($"Символ '{c}' некорректен для СС {radix}");

            ulong carry = (ulong)digitValue;
            for (int j = 0; j < result.Count; j++)
            {
                ulong temp = (ulong)result[j] * (ulong)radix + carry;
                result[j] = (uint)temp;
                carry = temp >> 32;
            }

            if (carry > 0) result.Add((uint)carry);
        }

        uint[] digits = result.ToArray();

        int realLength = digits.Length;
        while (realLength > 0 && digits[realLength - 1] == 0) realLength--;

        if (realLength == 0)
        {
            _signBit = 0;
            _smallValue = 0;
            _data = null;
        }
        else if (realLength == 1)
        {
            _signBit = isNegative ? 1 : 0;
            _smallValue = digits[0];
            _data = null;
        }
        else
        {
            _signBit = isNegative ? 1 : 0;
            _smallValue = 0;
            _data = new uint[realLength];
            Array.Copy(digits, _data, realLength);
        }
    }


    public ReadOnlySpan<uint> GetDigits()
    {
        if (_data != null)
        {
            return _data;
        }

        return MemoryMarshal.CreateReadOnlySpan(ref _smallValue, 1);
    }

    public int CompareTo(IBigInteger? other)
    {
        if (other == null) return 1;
        if (!this.IsNegative && other.IsNegative) return 1;
        if (this.IsNegative && !other.IsNegative) return -1;

        var thisDigits = this.GetDigits();
        var otherDigits = other.GetDigits();

        if (thisDigits.Length > otherDigits.Length)
        {
            return this.IsNegative ? -1 : 1;
        }
        if (thisDigits.Length < otherDigits.Length)
        {
            return this.IsNegative ? 1 : -1;
        }

        for (int i = thisDigits.Length - 1; i >= 0; i--)
        {
            if (thisDigits[i] > otherDigits[i])
            {
                return this.IsNegative ? -1 : 1;
            }
            if (thisDigits[i] < otherDigits[i])
            {
                return this.IsNegative ? 1 : -1;
            }
        }

        return 0;
    }
    public bool Equals(IBigInteger? other) => CompareTo(other) == 0;
    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_signBit);

        var digits = this.GetDigits();
        for (int i = 0; i < digits.Length; i++)
        {
            hash.Add(digits[i]);
        }

        return hash.ToHashCode();
    }


    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b)
    {
        if (a.IsNegative == b.IsNegative)
        {
            uint[] sum = AddMagnitudes(a.GetDigits(), b.GetDigits());
            return new BetterBigInteger(sum, a.IsNegative);
        }
        else
        {
            int cmp = CompareMagnitudes(a.GetDigits(), b.GetDigits());
            if (cmp == 0) return BetterBigInteger.Zero;

            var bigger = cmp > 0 ? a : b;
            var smaller = cmp > 0 ? b : a;

            uint[] diff = SubtractMagnitudes(bigger.GetDigits(), smaller.GetDigits());
            return new BetterBigInteger(diff, bigger.IsNegative);
        }
    }
    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b)
    {
        return a + (-b);
    }
    public static BetterBigInteger operator -(BetterBigInteger a)
    {
        if (a.IsZero)
        {
            return a;
        }
        if (a._data != null)
        {
            return new BetterBigInteger(a._data, !a.IsNegative);
        }

        return new BetterBigInteger([a._smallValue], !a.IsNegative);
    }
    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b)
    {
        return DivRem(a, b).Quotient;
    }
    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b)
    {
        return DivRem(a, b).Remainder;
    }



    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    {
        if (a.IsZero || b.IsZero) return BetterBigInteger.Zero;
        if (a.IsOne) return b;
        if (b.IsOne) return a;

        int lengthA = a.GetDigits().Length;
        int lengthB = b.GetDigits().Length;
        int maxLength = Math.Max(lengthA, lengthB);

        IMultiplier strategy;

        if (maxLength < KARATSUBA_THRESHOLD)
        {
            strategy = SimpleStrategy;
        }
        else if (maxLength < FFT_THRESHOLD)
        {
            strategy = KaratsubaStrategy; // Для средних чисел O(n^1.58)
        }
        else
        {
            strategy = FFTStrategy; // Для гигантских чисел O(n log n log log n)
        }

        return strategy.Multiply(a, b);
    }

    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        return (-a) - BetterBigInteger.One;
    }
    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b) => BitwiseOp(a, b, (x, y) => x & y);
    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b) => BitwiseOp(a, b, (x, y) => x | y);
    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b) => BitwiseOp(a, b, (x, y) => x ^ y);
    //вспомогательный метод чтобы не дублировать код для &, |, ^
    private static BetterBigInteger BitwiseOp(BetterBigInteger a, BetterBigInteger b, Func<uint, uint, uint> op)
    {
        bool resultNegative = op(a.IsNegative ? 1u : 0u, b.IsNegative ? 1u : 0u) == 1;

        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();

        int maxLength = Math.Max(aDigits.Length, bDigits.Length);
        uint[] newDigits = new uint[maxLength];

        ulong carryA = 1, carryB = 1;

        for (int i = 0; i < maxLength; ++i)
        {
            uint blockA = i >= aDigits.Length ? (a.IsNegative ? 0xFFFFFFFFu : 0u) : aDigits[i];
            if (a.IsNegative && i < aDigits.Length)
            {
                ulong temp = (ulong)(~blockA) + carryA;
                blockA = (uint)temp;
                carryA = temp >> 32;
            }

            uint blockB = i >= bDigits.Length ? (b.IsNegative ? 0xFFFFFFFFu : 0u) : bDigits[i];
            if (b.IsNegative && i < bDigits.Length)
            {
                ulong temp = (ulong)(~blockB) + carryB;
                blockB = (uint)temp;
                carryB = temp >> 32;
            }

            newDigits[i] = op(blockA, blockB);
        }

        if (resultNegative)
        {
            ulong carryRes = 1;
            for (int i = 0; i < maxLength; ++i)
            {
                ulong temp = (ulong)(~newDigits[i]) + carryRes;
                newDigits[i] = (uint)temp;
                carryRes = temp >> 32;
            }
        }

        return new BetterBigInteger(newDigits, resultNegative);
    }
    public static BetterBigInteger operator <<(BetterBigInteger a, int shift)
    {
        if (shift == 0) { return a; }
        if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift), "Shift must be non-negative.");

        int blockShift = shift / 32;
        int bitShift = shift % 32;
        var oldDigits = a.GetDigits();

        uint[] newDigits = new uint[oldDigits.Length + blockShift + 1];
        for (int i = 0; i < oldDigits.Length; ++i)
        {
            uint part1 = oldDigits[i] << bitShift;
            uint part2 = (bitShift > 0 && i > 0) ? (oldDigits[i - 1] >> (32 - bitShift)) : 0;
            newDigits[i + blockShift] = part1 | part2;
        }

        if (bitShift > 0)
        {
            newDigits[oldDigits.Length + blockShift] = oldDigits[^1] >> (32 - bitShift);
        }

        return new BetterBigInteger(newDigits, a.IsNegative);
    }
    public static BetterBigInteger operator >>(BetterBigInteger a, int shift)
    {
        if (shift == 0) { return a; }
        if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift), "Сдвиг не может быть отрицательным");

        int blockShift = shift / 32;
        int bitShift = shift % 32;
        var oldDigits = a.GetDigits();

        if (blockShift >= oldDigits.Length)
        {
            return a.IsNegative ? new BetterBigInteger([1], true) : BetterBigInteger.Zero;
        }

        uint[] newDigits = new uint[oldDigits.Length - blockShift];
        bool lostAnySetBit = false;

        for (int i = 0; i < blockShift; ++i)
            if (oldDigits[i] != 0) lostAnySetBit = true;

        uint mask = (1u << bitShift) - 1;
        if ((oldDigits[blockShift] & mask) != 0) lostAnySetBit = true;

        for (int i = 0; i < newDigits.Length; ++i)
        {
            var part1 = oldDigits[i + blockShift] >> bitShift;
            if (bitShift > 0)
            {
                uint part2 = ((i + blockShift + 1) > oldDigits.Length - 1) ? 0 : oldDigits[i + blockShift + 1] << (32 - bitShift);
                newDigits[i] = part1 | part2;
            }
            else
            {
                newDigits[i] = oldDigits[i + blockShift];
            }
        }

        if (a.IsNegative && lostAnySetBit)
        {
            newDigits = AddMagnitudes([1], newDigits);
        }

        return new BetterBigInteger(newDigits, a.IsNegative);
    }

    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;

    public override string ToString() => ToString(10);
    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36) throw new ArgumentOutOfRangeException(nameof(radix));
        if (this.IsZero) return "0";

        var sb = new StringBuilder();
        uint[] tempDigits = GetDigits().ToArray();
        int lastIndex = tempDigits.Length - 1;

        while (lastIndex >= 0)
        {
            ulong remainder = 0;
            for (int i = lastIndex; i >= 0; --i)
            {
                var current = (remainder << 32) | (ulong)tempDigits[i];
                tempDigits[i] = (uint)(current / (uint)radix);
                remainder = current % (uint)radix;
            }

            if (remainder < 10) sb.Append((char)('0' + remainder));
            else sb.Append((char)('A' + (remainder - 10)));

            while (lastIndex >= 0 && tempDigits[lastIndex] == 0) lastIndex--;
        }

        if (this.IsNegative) sb.Append('-');

        char[] chars = new char[sb.Length];
        for (int i = 0; i < sb.Length; i++) chars[i] = sb[sb.Length - 1 - i];
        return new string(chars);
    }

    public bool IsZero => _data == null && _smallValue == 0;
    public bool IsOne => _data == null && _smallValue == 1 && !IsNegative;
    public bool IsEven => (_data == null ? _smallValue : _data[0]) % 2 == 0;

    public static BetterBigInteger One { get; } = new BetterBigInteger([1u]);
    public static BetterBigInteger Zero { get; } = new BetterBigInteger([0u]);

    private static int CompareMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        if (a.Length > b.Length) { return 1; }
        if (a.Length < b.Length) { return -1; }

        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] > b[i]) { return 1; }
            if (a[i] < b[i]) { return -1; }
        }

        return 0;
    }

    internal static uint[] AddMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        int maxLength = Math.Max(a.Length, b.Length);
        uint[] result = new uint[maxLength + 1];

        ulong carry = 0;
        for (int i = 0; i < maxLength; i++)
        {
            ulong valA = i < a.Length ? a[i] : 0;
            ulong valB = i < b.Length ? b[i] : 0;

            ulong currentSum = valA + valB + carry;

            result[i] = (uint)currentSum;
            carry = currentSum >> 32;
        }
        result[maxLength] = (uint)carry;
        return result;
    }

    internal static uint[] SubtractMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        uint[] result = new uint[a.Length];
        long borrow = 0;

        for (int i = 0; i < a.Length; i++)
        {
            long valA = a[i];
            long valB = i < b.Length ? b[i] : 0;

            long diff = valA - valB - borrow;

            if (diff < 0)
            {
                diff += (1L << 32);
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }

            result[i] = (uint)diff;
        }

        return result;
    }

    public static (BetterBigInteger Quotient, BetterBigInteger Remainder) DivRem(BetterBigInteger a, BetterBigInteger b)
    {
        if (b.IsZero) throw new DivideByZeroException();

        var diff = CompareMagnitudes(a.GetDigits(), b.GetDigits());

        if (diff == -1)
        {
            return (BetterBigInteger.Zero, a);
        }

        if (diff == 0)
        {
            bool resultSign = a.IsNegative ^ b.IsNegative;
            return (new BetterBigInteger([1u], resultSign), BetterBigInteger.Zero);
        }

        bool quotientNegative = a.IsNegative ^ b.IsNegative;
        bool remainderNegative = a.IsNegative;

        var (quotDigits, remDigits) = DivRemMagnitudes(a.GetDigits(), b.GetDigits());

        return (new BetterBigInteger(quotDigits, quotientNegative),
                new BetterBigInteger(remDigits, remainderNegative));
    }

    private static (uint[] qDigits, uint[] rDigits) DivRemMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        uint[] qDigits = new uint[a.Length - b.Length + 1];
        uint[] rDigits = new uint[b.Length];

        int shift = System.Numerics.BitOperations.LeadingZeroCount(b[^1]);

        uint[] normA = new uint[a.Length + 1];

        if (shift == 0)
        {
            b.CopyTo(rDigits);
            a.CopyTo(normA);
        }
        else
        {
            for (int i = 0; i < b.Length; ++i)
            {
                if (i == 0) rDigits[0] = b[0] << shift;
                else rDigits[i] = (b[i] << shift) | (b[i - 1] >> (32 - shift));
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (i == 0) normA[0] = a[0] << shift;
                else normA[i] = (a[i] << shift) | (a[i - 1] >> (32 - shift));
            }
            normA[a.Length] = a[^1] >> (32 - shift);
        }

        for (int j = qDigits.Length - 1; j >= 0; --j)
        {
            var higherBlock = j + b.Length;
            var lowerBlock = higherBlock - 1;

            ulong qHat;
            if (normA[j + b.Length] == rDigits[^1]) qHat = 0xFFFFFFFFu;
            else
            {
                ulong dividend = ((ulong)normA[higherBlock] << 32 | normA[lowerBlock]);
                qHat = dividend / rDigits[^1];
            }

            qDigits[j] = (uint)qHat;
            ulong borrow = 0;

            for (int i = 0; i < b.Length; ++i)
            {
                ulong temp = (ulong)rDigits[i] * qHat + borrow;
                long res = (long)normA[j + i] - (uint)temp;
                normA[j + i] = (uint)res;
                borrow = (temp >> 32) + (res < 0 ? 1ul : 0ul);
            }

            bool overshot = normA[j + b.Length] < borrow;
            normA[j + b.Length] -= (uint)borrow;

            if (overshot)
            {
                --qDigits[j];
                ulong carry = 0;
                for (int i = 0; i < b.Length; ++i)
                {
                    ulong sum = (ulong)normA[j + i] + rDigits[i] + carry;
                    normA[j + i] = (uint)sum;
                    carry = sum >> 32;
                }
                normA[j + b.Length] += (uint)carry;
            }
        }

        if (shift == 0)
        {
            normA.AsSpan(0, b.Length).CopyTo(rDigits);
        }
        else
        {
            for (int i = 0; i < b.Length; ++i)
            {
                if (i == b.Length - 1) rDigits[i] = normA[i] >> shift;
                else rDigits[i] = (normA[i] >> shift) | (normA[i + 1] << (32 - shift));
            }
        }

        return (qDigits, rDigits);
    }

    internal static uint[] TrimZeros(uint[] arr)
    {
        int realLength = arr.Length;
        while (realLength > 0 && arr[realLength - 1] == 0)
        {
            realLength--;
        }

        if (realLength == arr.Length) { return arr; }

        uint[] trimmed = new uint[realLength];
        Array.Copy(arr, trimmed, realLength);
        return trimmed;
    }
}