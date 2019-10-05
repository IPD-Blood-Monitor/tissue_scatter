using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public static PhotonDetectorData DetectorPhotons(List<double> x, List<double> y, List<double> z, List<double> distance, List<double> amplitude, double distanceToDetector, double detectorWidth)
        {
            var allLists = new[] { x, y, z, distance, amplitude };
            if (allLists.Any(list => list.Count != x.Count))
            {
                throw new ArgumentException("All lists should be of equal size.");
            }

            if (distanceToDetector < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(distanceToDetector), distanceToDetector, "Value should be positive.");
            }
            if (detectorWidth < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(detectorWidth), detectorWidth, "Value should be positive.");
            }

            var traveledDistances = new List<double>();
            var totalAmplitude = 0.0;
            for (int i = 0; i < x.Count; i++)
            {
                var radiusOfPhoton = Math.Sqrt((x[i] * x[i]) + (y[i] * y[i]));
                if (radiusOfPhoton > (distanceToDetector - detectorWidth) &&
                    radiusOfPhoton <= (distanceToDetector + detectorWidth) && z[i] < 0)
                {
                    traveledDistances.Add(distance[i]);
                    totalAmplitude += amplitude[i];
                }
            }

            return new PhotonDetectorData
            {
                TraveledDistances = traveledDistances,
                TotalAmplitude = totalAmplitude
            };
        }

        public static PhotonPositionData UpdatePositions(List<double> x, List<double> y, List<double> z,
            List<double> traveledDistances, List<int> photonIds, List<double> amplitude, int numberOfPhotons, double mu,
            double absorptionCoefficient)
        {
            return new PhotonPositionData();
        }
    }

    public struct PhotonDetectorData
    {
        public double TotalAmplitude;
        public List<double> TraveledDistances;
    }

    public struct PhotonPositionData
    {
        public List<double> X;
        public List<double> Y;
        public List<double> Z;
        public List<double> Amplitudes;
        public List<double> TraveledDistances;
    }
}
