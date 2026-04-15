using Arithmetic.BigInt.Interfaces;
using System;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    internal delegate uint[] SpanMultiplyDelegate(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b);
    
    private readonly SpanMultiplyDelegate _baseStrategy;
    private readonly int _threshold;

    public KaratsubaMultiplier() : this(SimpleMultiplier.MultiplyMagnitudes, BetterBigInteger.KARATSUBA_THRESHOLD) { }

    public KaratsubaMultiplier(SpanMultiplyDelegate baseStrategy, int threshold = BetterBigInteger.KARATSUBA_THRESHOLD)
    {
        _baseStrategy = baseStrategy ?? throw new ArgumentNullException(nameof(baseStrategy));
        _threshold = threshold;
    }

    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        uint[] resultDigits = MultiplyRecursive(a.GetDigits(), b.GetDigits());
        bool resultSign = a.IsNegative != b.IsNegative; 
        
        return new BetterBigInteger(resultDigits, resultSign);
    }

    private uint[] MultiplyRecursive(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        int maxLength = Math.Max(a.Length, b.Length);

        if (maxLength < _threshold)
        {
            return _baseStrategy(a, b);
        }

        int m = maxLength / 2;

        ReadOnlySpan<uint> a0 = a.Slice(0, Math.Min(a.Length, m));
        ReadOnlySpan<uint> b0 = b.Slice(0, Math.Min(b.Length, m));

        ReadOnlySpan<uint> a1 = a.Length > m ? a.Slice(m) : ReadOnlySpan<uint>.Empty;
        ReadOnlySpan<uint> b1 = b.Length > m ? b.Slice(m) : ReadOnlySpan<uint>.Empty;

        uint[] z0 = MultiplyRecursive(a0, b0);
        uint[] z2 = MultiplyRecursive(a1, b1);

        uint[] sumA = BetterBigInteger.AddMagnitudes(a0, a1);
        uint[] sumB = BetterBigInteger.AddMagnitudes(b0, b1);
        uint[] z1 = MultiplyRecursive(sumA, sumB);

        uint[] zMid = BetterBigInteger.SubtractMagnitudes(z1, z2);
        zMid = BetterBigInteger.SubtractMagnitudes(zMid, z0);

        uint[] result = new uint[a.Length + b.Length];

        AddAtOffset(result, z0, 0);          // Z0 никуда не сдвигается
        AddAtOffset(result, zMid, m);        // ZMid сдвигается на m
        AddAtOffset(result, z2, 2 * m);      // Z2 сдвигается на 2m

        return BetterBigInteger.TrimZeros(result);
    }

    private static void AddAtOffset(uint[] result, uint[] addend, int offset)
    {
        ulong carry = 0;
        for (int i = 0; i < addend.Length; i++)
        {
            int resIdx = offset + i;
            if (resIdx >= result.Length) break; 

            ulong sum = result[resIdx] + (ulong)addend[i] + carry;
            result[resIdx] = (uint)sum;
            carry = sum >> 32;
        }

        int currentOffset = offset + addend.Length;
        while (carry > 0 && currentOffset < result.Length)
        {
            ulong sum = result[currentOffset] + carry;
            result[currentOffset] = (uint)sum;
            carry = sum >> 32;
            currentOffset++;
        }
    }
}