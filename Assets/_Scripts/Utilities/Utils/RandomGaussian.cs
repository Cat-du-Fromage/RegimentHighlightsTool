using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using Random = Unity.Mathematics.Random;
using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public class RandomGaussian
    {
        private bool hasDeviate;
        private double storedDeviate;
        private Random random;

        public RandomGaussian(uint seed)
        {
            random = new Random();
            random = Random.CreateFromIndex(seed);
        }

        public double NextGaussian(double mean, double stddev)
        {
            if (hasDeviate)
            {
                hasDeviate = false;
                return mean + storedDeviate * stddev;
            }

            double v1, v2, rsq;
            do
            {
                v1 = 2.0 * random.NextDouble() - 1.0;
                v2 = 2.0 * random.NextDouble() - 1.0;
                rsq = v1 * v1 + v2 * v2;
            }
            while (rsq >= 1.0 || rsq == 0.0);

            double fac = sqrt(-2.0 * log(rsq) / rsq);
            storedDeviate = v1 * fac;
            hasDeviate = true;

            return mean + v2 * fac * stddev;
        }
    }
}
