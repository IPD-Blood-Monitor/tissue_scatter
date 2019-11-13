using System;
using System.Collections.Generic;
using System.IO;

namespace TissueScatter.Core
{
    public static class Ratio
    {
        static void Main(string[] args)
        {
            var coefficients = new Coefficients.Coefficients(Directory.GetCurrentDirectory());
            var absorption = coefficients.ObtainAbsorptionCoefficients(660);
            var scatteringCoefficients = Coefficients.Coefficients.ObtainScatteringCoefficients(660);

            var data400 = Scatter.Scatterlight(450, 0.1, 0.3, 0.05, 0.05, 1, 3, 0.150, 0.9, Directory.GetCurrentDirectory());
            var data660 = Scatter.Scatterlight(660, 0.1, 0.3, 0.05, 0.05, 1, 3, 0.150, 0.9, Directory.GetCurrentDirectory());
            var data900 = Scatter.Scatterlight(900, 0.1, 0.3, 0.05, 0.05, 1, 3, 0.150, 0.9, Directory.GetCurrentDirectory());

            var datas = new List<ScatterData>
            {
                data400,
                data660,
                data900
            };
            var wavelengths = new List<int> { 450, 660, 900 };

            CalculateRatio(datas, wavelengths);
        }

        private static double CalculateRatio(List<ScatterData> datas, List<int> waveLengths)
        {
            var coefficients = new Coefficients.Coefficients(Directory.GetCurrentDirectory());

            var R11 = datas[0].DetectedPhotons1 / datas[0].DetectedPhotons2;
            var R22 = datas[1].DetectedPhotons1 / datas[1].DetectedPhotons2;
            var R33 = datas[2].DetectedPhotons1 / datas[2].DetectedPhotons2;

            var R12 = R11 / R22;
            var R13 = R11 / R33;

            var alpha12 = Math.Log(R12);
            var alpha13 = Math.Log(R13);

            var alpha1 = coefficients.ObtainAbsorptionCoefficients((uint) waveLengths[0]);
            var alpha2 = coefficients.ObtainAbsorptionCoefficients((uint) waveLengths[1]);
            var alpha3 = coefficients.ObtainAbsorptionCoefficients((uint) waveLengths[2]);

            var alphaD1 = alpha1.AbsorptionBlood;
            var alphaD2 = alpha2.AbsorptionBlood;
            var alphaD3 = alpha3.AbsorptionBlood;

            var alphaO1 = alpha1.AbsorptionOxygenatedBlood;
            var alphaO2 = alpha2.AbsorptionOxygenatedBlood;
            var alphaO3 = alpha3.AbsorptionOxygenatedBlood;

            var topLine = alpha12 * (alphaD3 + alphaD1) - alpha13 * (alphaD1 + alphaD2);
            var bottomLine = alpha13 * (alpha1.AbsorptionOxygenatedBlood - alpha2.AbsorptionOxygenatedBlood +
                                        alpha1.AbsorptionBlood - alpha2.AbsorptionBlood) -
                             alpha12 * (alpha1.AbsorptionOxygenatedBlood + alpha3.AbsorptionOxygenatedBlood -
                                        alpha1.AbsorptionBlood - alpha3.AbsorptionBlood);

            var ratio = Math.Sqrt(Math.Pow(topLine / bottomLine, 2));

            return ratio;
        }

    }
}
