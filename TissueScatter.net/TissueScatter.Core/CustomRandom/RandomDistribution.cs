using System;
using System.Collections.Generic;
using NumSharp;

namespace TissueScatter.Core.CustomRandom
{
    public class RandomDistribution
    {
        public static RandomDistribution Instance => _instance ?? (_instance = new RandomDistribution());

        private static RandomDistribution _instance;
        private readonly Random _random;

        private RandomDistribution()
        {
            _random = new Random();
        }

        public List<double> ExponentialDistribution(double scale, int size)
        {
            //var exponentialList = new List<double>(size);

            //for (int i = 0; i < size; i++)
            //{
            //    var randomNumber = _random.NextDouble();
            //    var exponentialNumber = 1 / scale * Math.Exp(-(randomNumber / scale));
            //    exponentialList.Add(exponentialNumber);
            //}

            //return exponentialList;

            return new List<double>(np.random.exponential(-1/scale, size).ToArray<double>());
        }

        public List<double> UniformDistribution(double low, double high, int size)
        {
            //var uniformList = new List<double>(size);

            //for (int i = 0; i < size; i++)
            //{
            //    var random = _random.NextDouble();
            //    var mappedRandom = random * (high - low) + low;
            //    uniformList.Add(mappedRandom);
            //}

            //return uniformList;

            return new List<double>(np.random.uniform(low, high, size).ToArray<double>());
        }
    }
}
