using System;
using System.Collections.Generic;

namespace TissueScatter.Net.Photons
{
    public static class Photons
    {
        public static List<int> FilterPhotons(List<double> x, List<double> y, List<double> z, double xyBound,
            double zBound)
        {
            var indexes = new List<int>();
            for (int i = 0; i < x.Count; i++)
            {
                if (Math.Abs(x[i]) <= xyBound && Math.Abs(y[i]) <= xyBound && z[i] <= zBound && z[i] >= 0)
                {
                    indexes.Add(i);
                }
            }
            
            return indexes;
        }

        public static void DetectorPhotons(List<double> x, List<double> y, List<double> z, double xyBound,
            double zBound, List<double> distance, List<double> amplitude, double distanceToDetector, double detectorWidth)
        {

        }
    }
}
