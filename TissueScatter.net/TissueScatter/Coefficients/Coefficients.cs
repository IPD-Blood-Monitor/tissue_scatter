using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TissueScatter.Net.Coefficients
{
    public static class Coefficients
    {
        private const double mgPerMol = 64500.0;

        public static double MgPerMol => mgPerMol;

        /// <summary>
        /// Returns the absorption coefficients for a given wavelength from a tabulated file.
        /// Linear interpolation is used to obtain values between wavelengths in the file.
        /// </summary>
        /// <param name="waveLength">The wavelength in nanometers</param>
        /// <returns></returns>
        public static AbsorptionCoefficients ObtainAbsorptionCoefficients(uint waveLength)
        {
            var waveLengths = new List<int>();
            var absorptionBlood = new List<int>();
            var absorptionOBlood = new List<int>();

            var data = File.ReadAllLines("Resources/BloodAbsorptionData.txt").Skip(17);

            foreach (var line in data)
            {
                var cells = line.Split('\t');

                waveLengths.Add(int.Parse(cells[0]));
                absorptionOBlood.Add(int.Parse(cells[1]));
                absorptionBlood.Add(int.Parse(cells[2]));
            }

            if (waveLength < waveLengths.First() && waveLength > waveLengths.Last())
            {
                throw new ArgumentException(
                    "Wavelength can't be larger or smaller than the minimum or larger than the maximum Wavelength in the tabulate data",
                    nameof(waveLength));
            }

            if (waveLength == waveLengths.First())
            {
                return new AbsorptionCoefficients
                {
                    AbsorptionBlood = absorptionBlood.First(),
                    AbsorptionOxygenatedBlood = absorptionOBlood.First()
                };
            }

            if (waveLength == waveLengths.Last())
            {
                return new AbsorptionCoefficients
                {
                    AbsorptionBlood = absorptionBlood.Last(),
                    AbsorptionOxygenatedBlood = absorptionOBlood.Last()
                };
            }

            int i = 0;
            while (waveLength > waveLengths[i])
            {
                i++;
            }

            var factor = (float)(waveLength - waveLengths[i - 1]) / (waveLengths[i] - waveLengths[i - 1]);

            long absBlood = (long)(absorptionBlood[i - 1] + ((absorptionBlood[i] - absorptionBlood[i - 1]) * factor));
            long absOBlood = (long)(absorptionOBlood[i - 1] + (absorptionOBlood[i] - absorptionOBlood[i - 1]) * factor);

            return new AbsorptionCoefficients
            {
                AbsorptionBlood = absBlood,
                AbsorptionOxygenatedBlood = absOBlood
            };
        }

        /// <summary>
        /// Returns the scattering coefficients for the given wavelength
        /// </summary>
        /// <param name="wavelength">The wavelength in nanometers</param>
        /// <returns></returns>
        public static ScatteringCoefficients ObtainScatteringCoefficients(uint wavelength)
        {
            ScatteringCoefficients coefficients = new ScatteringCoefficients();

            double aSkin = 46.0;
            double aBone = 22.9;
            double aMuscle = 13.0;
            double bSkin = -1.421;
            double bBone = -0.716;
            double bMuscle = -1.470;
            double ldRef = 500;

            coefficients.MuSkin = aSkin * Math.Pow(wavelength / ldRef, bSkin);
            coefficients.MuBone = aBone * Math.Pow(wavelength / ldRef, bBone);
            coefficients.MuMuscle = aMuscle * Math.Pow(wavelength / ldRef, bMuscle);

            return coefficients;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="absorptionBlood"></param>
        /// <param name="absorptionOBlood"></param>
        /// <param name="concentrationBlood"></param>
        /// <param name="ratio"></param>
        public static double CalculateAbsorptionCoefficient(double absorptionBlood, double absorptionOBlood, double concentrationBlood, double ratio)
        {
            var concentrationHbO2 = concentrationBlood * ratio; // mg/l of oxygenated hemoglobin
            var concentrationHb = concentrationBlood * (1 - ratio); // mg/l of deoxygenated hemoglobin

            // /cm absorption coefficient ((1/(cm.mol/l)*mg/l/mg = 1/cm)) oxygenated hemoglobin
            var absorptionHbO2 = concentrationHbO2 * absorptionOBlood / MgPerMol;
            // /cm absorption coefficient ((1/(cm.mol/l)*mg/l/mg = 1/cm)) deoxygenated hemoglobin
            var absorptionHb = concentrationHb * absorptionBlood / MgPerMol;

            // The 2 absorption values can be added together because of Beers law
            return absorptionHbO2 + absorptionHb;
        }
    }

    public struct ScatteringCoefficients
    {
        public double MuSkin;
        public double MuBone;
        public double MuMuscle;
    }

    public struct AbsorptionCoefficients
    {
        public double AbsorptionBlood;
        public double AbsorptionOxygenatedBlood;
    }
}
