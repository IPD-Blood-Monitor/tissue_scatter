using System;
using System.Collections.Generic;
using System.Linq;
using TissueScatter.Core.CustomRandom;

namespace TissueScatter.Core.Photons
{
    public static class Photons
    {
        /// <summary>
        /// Filter out all phtons that are out of bounds
        /// </summary>
        /// <param name="x">List of x positions</param>
        /// <param name="y">List of y positions</param>
        /// <param name="z">List of z positions</param>
        /// <param name="xyBound">XY bound that the photons should be in</param>
        /// <param name="zBound">Z bound that the photons should be in</param>
        /// <returns>A list of indexes of the photons that are in bounds</returns>
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

        /// <summary>
        /// Updates photon positions. Uses exponential distribution to generate distances (based on scattering length)
        /// </summary>
        /// <param name="x">List of x positions</param>
        /// <param name="y">List of y positions</param>
        /// <param name="z">List of z positions</param>
        /// <param name="traveledDistances">Traveled distance per photon</param>
        /// <param name="photonIndexes">Indexes of the photons that should be used in this calculation</param>
        /// <param name="amplitude">Amplitudes per photon</param>
        /// <param name="numberOfPhotons">Total number of photons</param>
        /// <param name="mu">MuValue for the layer we are in (muSkin, muMuscle etc...)</param>
        /// <param name="absorptionCoefficient">Absorption Coefficient for the current layer we are in</param>
        /// <returns></returns>
        public static PhotonPositionData UpdatePositions(List<double> x, List<double> y, List<double> z,
            List<double> traveledDistances, List<int> photonIndexes, List<double> amplitude, int numberOfPhotons, double mu,
            double absorptionCoefficient)
        {
            var distances = RandomDistribution.Instance.ExponentialDistribution(1 / mu, numberOfPhotons);

            for (int i = 0; i < photonIndexes.Count; i++)
            {
                traveledDistances[photonIndexes[i]] += distances[i];
            }

            var theta = RandomDistribution.Instance.UniformDistribution(-Math.PI / 2.0, Math.PI / 2.0, numberOfPhotons);
            var phi = RandomDistribution.Instance.UniformDistribution(0, 2.0 * Math.PI, numberOfPhotons);

            for (int i = 0; i < distances.Count; i++)
            {
                x[photonIndexes[i]] = distances[i] * Math.Cos(theta[i]) * Math.Cos(phi[i]);
                y[photonIndexes[i]] = distances[i] * Math.Cos(theta[i]) * Math.Cos(phi[i]);
                z[photonIndexes[i]] = distances[i] * Math.Sin(theta[i]);

                amplitude[photonIndexes[i]] = amplitude[photonIndexes[i]] * Math.Exp(-absorptionCoefficient * distances[i]);
            }

            return new PhotonPositionData
            {
                X = x,
                Y = y,
                Z = z,
                Amplitudes = amplitude,
                TraveledDistances = traveledDistances
            };
        }

        //TODO I think this should actually be called AreWeNotDone (see github issue)
        /// <summary>
        /// Determine if the signal to noise ratio is good enough
        /// </summary>
        /// <param name="det1">Photons at detector 1</param>
        /// <param name="det2">Photons at detector 2</param>
        /// <returns></returns>
        public static bool AreWeDone(double det1, double det2)
        {
            var threshold = 0.01;

            if (det1 == 0.0 || det2 == 0.0)
            {
                return true;
            }

            if (Math.Sqrt(det1) / det1 > threshold || Math.Sqrt(det2) / det2 > threshold)
            {
                return true;
            }

            return false;
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
