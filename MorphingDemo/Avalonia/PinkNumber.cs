// https://www.firstpr.com.au/dsp/pink-noise/
// Voss algorithm which creates pink noise by adding a series of white noise sources at successively lower octaves.

using System;

namespace Avalonia
{
    public class PinkNumber
    {
        private static Random s_random = new Random();

        private static int rand() => s_random.Next();

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
                white_values[i] = (uint) (rand() % (range / 6));
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
            sum = (uint) (rand() % (range/6));
            for (int i = 0; i < 5; i++)
            {
                // If bit changed get new random number for corresponding
                // white_value
                if ((diff & (1 << i)) != 0)
                    white_values[i] = (uint) (rand() % (range / 6));
                sum += white_values[i];
            }

            return (int) sum;
        }
    }
}