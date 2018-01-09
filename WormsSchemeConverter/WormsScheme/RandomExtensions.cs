﻿using System;

namespace WormsScheme
{
    /// <summary>
    /// From https://bitbucket.org/Superbest/superbest-random
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Generates normally distributed numbers.
        /// </summary>
        /// <param name="r"></param>
        /// <param name = "mu">Mean of the distribution</param>
        /// <param name = "sigma">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }

        /// <summary>
        /// Generates values from a triangular distribution.
        /// </summary>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Triangular_distribution for a description of the triangular probability distribution and the algorithm for generating one.
        /// </remarks>
        /// <param name="r"></param>
        /// <param name = "a">Minimum</param>
        /// <param name = "b">Maximum</param>
        /// <param name = "c">Mode (most frequent value)</param>
        /// <returns></returns>
        public static double NextTriangular(this Random r, double a, double b, double c)
        {
            var u = r.NextDouble();

            return u < (c - a) / (b - a)
                       ? a + Math.Sqrt(u * (b - a) * (c - a))
                       : b - Math.Sqrt((1 - u) * (b - a) * (b - c));
        }

        /// <summary>
        /// Equally likely to return true or false. Uses <see cref="Random.Next()"/>.
        /// </summary>
        /// <returns></returns>
        public static bool NextBoolean(this Random r)
        {
            return r.Next(2) > 0;
        }

        public static int NextBoundedInt(this Random r,
                                            int weightedValue,
                                            int lowerBound,
                                            int upperBound,
                                            double sigma = 1)
        {
            var randomInt = int.MinValue;
            var numberOfTries = 0;


            while (randomInt > upperBound || randomInt < lowerBound )
            {
                randomInt = (int)Math.Round(r.NextGaussian(weightedValue, sigma));
                numberOfTries++;

                if (numberOfTries >= 10)
                    break;
            }
            
            return numberOfTries == 10 ? weightedValue : randomInt;
        }
    }
}