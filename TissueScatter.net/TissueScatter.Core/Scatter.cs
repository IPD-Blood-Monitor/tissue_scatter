using System.Collections.Generic;
using System.Linq;
using TissueScatter.Core.CustomRandom;

namespace TissueScatter.Core
{
    public static class Scatter
    {
        /// <summary>
        /// Calculates scattering through tissue for a specific wavelength
        /// </summary>
        /// <param name="wavelength">wavelength (nanometers)</param>
        /// <param name="distanceToDetector1">Distance to first detector (in cm)</param>
        /// <param name="distanceToDetector2">Distance to second detector (in cm) Det2 > Det 1</param>
        /// <param name="width">Half width of both the light source and the detectors (cm)</param>
        /// <param name="dSkin">Thickness of the skin (cm)</param>
        /// <param name="dMuscle">Thickness of the msucle (cm)</param>
        /// <param name="dBone">Thickness of the bone. Photons that go trought the bone are dropped</param>
        /// <param name="concentrationBlood">Concentration of hemoglobin ~150g/liter</param>
        /// <param name="ratioOxygen">Ration of oxygenated to deoxyganted hemoglobin (between 0 and 1)</param>
        public static ScatterData Scatterlight(uint wavelength, double distanceToDetector1, double distanceToDetector2,
            double width, double dSkin, double dMuscle, double dBone, double concentrationBlood, double ratioOxygen)
        {
            /*
             * We place the two detectors as a ring around the light source
             * and bound the model volume by 5*distanceToDetector2
             *
             * For absorption, each photon is given a starting weight, which is reduced by absorption
             */

            double xBound, yBound, zBound;
            xBound = yBound = 5.0 * distanceToDetector2;
            zBound = dSkin + dMuscle + dBone;

            var scatteringCoefficients = Coefficients.Coefficients.ObtainScatteringCoefficients(wavelength);
            var absorptionCoefficients = Coefficients.Coefficients.ObtainAbsorptionCoefficients(wavelength);

            var absorptionCoefficient = Coefficients.Coefficients.CalculateAbsorptionCoefficient(absorptionCoefficients.AbsorptionBlood,
                absorptionCoefficients.AbsorptionOxygenatedBlood, concentrationBlood, ratioOxygen);

            // Assume the ares of the light emitter is 0.1 cm^2 and that contact with the skin is not perfect
            var numPhotons = 5000000; //5e6

            var randomInstance = RandomDistribution.Instance;
            var xPos = randomInstance.UniformDistribution(-width, width, numPhotons);
            var yPos = randomInstance.UniformDistribution(-width, width, numPhotons);
            var zPos = randomInstance.UniformDistribution(0, 1 / scatteringCoefficients.MuSkin, numPhotons);

            var distance = new List<double>(numPhotons);
            for (int i = 0; i < numPhotons; i++)
            {
                distance.Add(0);
            }

            var amplitude = new List<double>(numPhotons);
            for (int i = 0; i < numPhotons; i++)
            {
                amplitude.Add(100);
            }

            var keepScattering = true;
            var idsInModel = new List<int>();
            for (int i = 0; i < numPhotons; i++)
            {
                idsInModel.Add(i);
            }

            double detectedPhotons1 = 0, detectedPhotons2 = 0;
            bool firstD1 = true, firstD2 = true;

            List<double> lengthsToD1 = new List<double>();
            List<double> lengthsToD2 = new List<double>();

            while (keepScattering)
            {
                // Remove all photons outside the model boundaries from the calculation
                //var size = xPos.Count;
                //var tempX = new List<double>(size);
                //var tempY = new List<double>(size);
                //var tempZ = new List<double>(size);
                //var tempDistance = new List<double>(size);
                //foreach (var index in idsInModel)
                //{
                //    tempX.Add(xPos[index]);
                //    tempY.Add(yPos[index]);
                //    tempZ.Add(zPos[index]);
                //    tempDistance.Add(distance[index]);
                //}
                //xPos = new List<double>(tempX);
                //yPos = new List<double>(tempY);
                //zPos = new List<double>(tempZ);
                //distance = new List<double>(tempDistance);

                var tempList = new List<double>(xPos);
                xPos = FilterOnIndexes(xPos, idsInModel);
                yPos = FilterOnIndexes(yPos, idsInModel);
                zPos = FilterOnIndexes(zPos, idsInModel);
                distance = FilterOnIndexes(distance, idsInModel);

                //xPos = xPos.Where(item => idsInModel.Contains(xPos.IndexOf(item))).ToList();
                //yPos = yPos.Where(item => idsInModel.Contains(yPos.IndexOf(item))).ToList();
                //zPos = zPos.Where(item => idsInModel.Contains(zPos.IndexOf(item))).ToList();
                //distance = distance.Where(item => distance.Contains(distance.IndexOf(item))).ToList();

                //Determine which photons are in which tissue and their indexes
                amplitude = FilterOnIndexes(amplitude, idsInModel);
                //amplitude = amplitude.Where(item => idsInModel.Contains(amplitude.IndexOf(item))).ToList();

                //var idxSkin = idsInModel.Where(index => zPos[index] <= dSkin).ToList(); //TODO veranderd dit zodat het niet meer gebruik maakt van idsInModel maar gewoon de index returnt van het item in de lijst waar de waarde voor geldt
                var idxSkin = new List<int>();
                for (int i = 0; i < zPos.Count; i++)
                {
                    if (zPos[i] <= dSkin)
                    {
                        idxSkin.Add(i);
                    }
                }
                var numInSkin = idxSkin.Count;

                //var idxmuscle = idsInModel.Where(index => zPos[index] <= dMuscle && zPos[index] > dSkin).ToList();
                var idxMuscle = new List<int>();
                for (int i = 0; i < zPos.Count; i++)
                {
                    if (zPos[i] <= dMuscle && zPos[i] > dSkin)
                    {
                        idxMuscle.Add(i);
                    }
                }
                var numInMuscle = idxMuscle.Count;

                //var idxBone = idsInModel.Where(index => zPos[index] <= dBone && zPos[index] > dSkin + dMuscle).ToList();
                var idxBone = new List<int>();
                for (int i = 0; i < zPos.Count; i++)
                {
                    if (zPos[i] <= dBone && zPos[i] > dSkin + dMuscle)
                    {
                        idxBone.Add(i);
                    }
                }
                var numInBone = idxBone.Count;

                // Determine the distance each photon travels using the right scattering coefficient
                // then use angles and that length to update photon positions

                // Assume that the vast majority of blood is in the muscle not the skin or bone, hence the abs coefficient is 0
                if (numInSkin > 0)
                {
                    var data = Photons.Photons.UpdatePositions(xPos, yPos, zPos, distance, idxSkin, amplitude, numInSkin,
                        scatteringCoefficients.MuSkin, 0);
                    xPos = new List<double>(data.X);
                    yPos = new List<double>(data.Y);
                    zPos = new List<double>(data.Z);
                    amplitude = new List<double>(data.Amplitudes);
                    distance = new List<double>(data.TraveledDistances);
                }

                if (numInMuscle > 0)
                {
                    var data = Photons.Photons.UpdatePositions(xPos, yPos, zPos, distance, idxMuscle, amplitude, numInMuscle,
                        scatteringCoefficients.MuMuscle, absorptionCoefficient);
                    xPos = new List<double>(data.X);
                    yPos = new List<double>(data.Y);
                    zPos = new List<double>(data.Z);
                    amplitude = new List<double>(data.Amplitudes);
                    distance = new List<double>(data.TraveledDistances);
                }

                // As with skin, we assume no blood in the bone(not true if you include marrow) ==> abs coef is 0
                if (numInBone > 0)
                {
                    var data = Photons.Photons.UpdatePositions(xPos, yPos, zPos, distance, idxBone, amplitude, numInBone,
                        scatteringCoefficients.MuBone, 0);
                    xPos = new List<double>(data.X);
                    yPos = new List<double>(data.Y);
                    zPos = new List<double>(data.Z);
                    amplitude = new List<double>(data.Amplitudes);
                    distance = new List<double>(data.TraveledDistances);
                }

                // Need to remove photons that are outside the calculation bounds and count up those that are detected
                var indexes = Photons.Photons.FilterPhotons(xPos, yPos, zPos, xBound, zBound);
                idsInModel = new List<int>(indexes);
                numPhotons = idsInModel.Count;

                // Calculate the intensity at each detector
                var photons1 = Photons.Photons.DetectorPhotons(xPos, yPos, zPos, distance, amplitude,
                    distanceToDetector1, width);
                var lengths1 = photons1.TraveledDistances;
                detectedPhotons1 += photons1.TotalAmplitude;

                var photons2 = Photons.Photons.DetectorPhotons(xPos, yPos, zPos, distance, amplitude,
                    distanceToDetector2, width);
                var lengths2 = photons2.TraveledDistances;
                detectedPhotons2 += photons2.TotalAmplitude;

                if (firstD1)
                {
                    lengthsToD1 = new List<double>(lengths1);
                    firstD1 = false;
                }
                else
                {
                    lengthsToD1.AddRange(lengths1);
                }

                if (firstD2)
                {
                    lengthsToD2 = new List<double>(lengths2);
                    firstD2 = false;
                }
                else
                {
                    lengthsToD2.AddRange(lengths2);
                }

                // Check to see if we have sufficient signal-to-noise at both detectors
                keepScattering = Photons.Photons.AreWeDone(detectedPhotons1, detectedPhotons2);
            }

            if (numPhotons < 100000)
            {
                // If we run out of photons before signal-to-noise is above threshold, inject some more
                xPos.AddRange(randomInstance.UniformDistribution(-width, width, 5000000));
                yPos.AddRange(randomInstance.UniformDistribution(-width, width, 5000000));
                zPos.AddRange(randomInstance.UniformDistribution(0, 1 / scatteringCoefficients.MuSkin, 5000000));

                for (int i = 0; i < 5000000; i++)
                {
                    amplitude.Add(100);
                }

                idsInModel = Photons.Photons.FilterPhotons(xPos, yPos, zPos, xBound, zBound);
                numPhotons = idsInModel.Count;
            }

            return new ScatterData
            {
                DetectedPhotons1 = detectedPhotons1,
                DetectedPhotons2 = detectedPhotons2,
                LengthToD1 = lengthsToD1.Average(),
                LengthToD2 = lengthsToD2.Average()
            };
        }

        private static List<T> FilterOnIndexes<T>(List<T> originalList, List<int> indexes)
        {
            var newList = new List<T>(indexes.Count);

            foreach (var index in indexes)
            {
                newList.Add(originalList[index]);
            }

            return newList;
        }
    }

    public struct ScatterData
    {
        public double DetectedPhotons1;
        public double DetectedPhotons2;

        public double LengthToD1;
        public double LengthToD2;
    }
}
