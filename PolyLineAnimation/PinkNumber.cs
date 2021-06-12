﻿// https://www.firstpr.com.au/dsp/pink-noise/

using System;
using System.Runtime.InteropServices;

namespace PolyLineAnimation
{
    class PinkNumber
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 rand();

        private int max_key;
        private int key;
        private uint[] white_values = new uint[5];
        private uint range;

        public PinkNumber(uint range = 128)
        {
            max_key = 0x1f; // Five bits set
            this.range = range;
            key = 0;
            for (int i = 0; i < 5; i++)
                white_values[i] = (uint)(rand() % (range / 5));
        }

        public int GetNextValue()
        {
            int last_key = key;
            uint sum;

            key++;
            if (key > max_key)
                key = 0;
            // Exclusive-Or previous value with current value. This gives
            // a list of bits that have changed.
            int diff = last_key ^ key;
            sum = 0;
            for (int i = 0; i < 5; i++)
            {
                // If bit changed get new random number for corresponding
                // white_value
                if ((diff & (1 << i)) != 0)
                    white_values[i] = (uint)(rand() % (range / 5));
                sum += white_values[i];
            }

            return (int)sum;
        }
    }
}