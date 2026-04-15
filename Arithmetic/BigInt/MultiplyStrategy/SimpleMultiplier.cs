using Arithmetic.BigInt.Interfaces;
using System;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        uint[] result = MultiplyMagnitudes(a.GetDigits(), b.GetDigits());
        bool isNegative = a.IsNegative != b.IsNegative;
        return new BetterBigInteger(result, isNegative);
    }

    internal static uint[] MultiplyMagnitudes(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        uint[] result = new uint[a.Length + b.Length];

        for (int i = 0; i < a.Length; i++)
        {
            ulong carry = 0;
            ulong valA = a[i];

            for (int j = 0; j < b.Length; j++)
            {
                ulong current = result[i + j] + (valA * b[j]) + carry;
                result[i + j] = (uint)current;
                carry = current >> 32;
            }

            if (carry > 0)
            {
                result[i + b.Length] = (uint)carry;
            }
        }
        return result;
    }
}