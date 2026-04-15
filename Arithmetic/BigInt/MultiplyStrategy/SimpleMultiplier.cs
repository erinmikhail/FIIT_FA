using Arithmetic.BigInt.Interfaces;
using System;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();

        uint[] result = new uint[aDigigts.Length + bDigits.Length];

        for (int i = 0; i < aDigits.Length; i++)
        {
            ulong carry = 0; // в старш разр перен
            ulong valA = aDigits[i];

            for (int j = 0; j < bDigits.Length; j++)
            {
                ulong valB = bDigits[j];
                ulong current = result[i + j] + (valA * valB) + carry;
                result[i + j] = (uint)current;
                carry = current >> 32;
            }

            if (carry > 0)
            {
                result[i + bDigits.Length] = (uint)carry;
            }
        }

        bool isNegative = a.IsNegative() != b.IsNegative();
        return new BetterBigInteger(result, isNegative);
    }
}