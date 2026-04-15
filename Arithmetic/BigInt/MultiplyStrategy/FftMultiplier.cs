using Arithmetic.BigInt.Interfaces;
using System;
using System.Numerics;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();

        int n = 1;
        int requiredLength = (aDigits.Length + bDigits.Length) * 2;
        while (n < requiredLength) n <<= 1;

        Complex[] fa = new Complex[n];
        Complex[] fb = new Complex[n];

        for (int i = 0; i < aDigits.Length; i++)
        {
            fa[2 * i] = new Complex(aDigits[i] & 0xFFFF, 0);       // Младшие 16 бит
            fa[2 * i + 1] = new Complex(aDigits[i] >> 16, 0);      // Старшие 16 бит
        }

        for (int i = 0; i < bDigits.Length; i++)
        {
            fb[2 * i] = new Complex(bDigits[i] & 0xFFFF, 0);
            fb[2 * i + 1] = new Complex(bDigits[i] >> 16, 0);
        }

        FFT(fa, false);
        FFT(fb, false);

        for (int i = 0; i < n; i++)
        {
            fa[i] *= fb[i];
        }

        FFT(fa, true);

        uint[] result = new uint[aDigits.Length + bDigits.Length];
        long carry = 0;

        for (int i = 0; i < n; i++)
        {
            long value = (long)Math.Round(fa[i].Real / n) + carry;
            
            uint current16 = (uint)(value & 0xFFFF);
            carry = value >> 16;

            if (i % 2 == 0)
            {
                result[i / 2] = current16;
            }
            else
            {
                result[i / 2] |= (current16 << 16);
            }
        }

        bool resultSign = a.IsNegative != b.IsNegative;
        return new BetterBigInteger(BetterBigInteger.TrimZeros(result), resultSign);
    }

    private static void FFT(Complex[] a, bool invert)
    {
        int n = a.Length;

        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; j >= bit; bit >>= 1) j -= bit;
            j += bit;
            
            if (i < j)
            {
                var temp = a[i];
                a[i] = a[j];
                a[j] = temp;
            }
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double angle = 2 * Math.PI / len * (invert ? -1 : 1);
            Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));
            
            for (int i = 0; i < n; i += len)
            {
                Complex w = Complex.One;
                for (int j = 0; j < len / 2; j++)
                {
                    Complex u = a[i + j];
                    Complex v = a[i + j + len / 2] * w;
                    
                    a[i + j] = u + v;
                    a[i + j + len / 2] = u - v;
                    w *= wlen;
                }
            }
        }
    }
}