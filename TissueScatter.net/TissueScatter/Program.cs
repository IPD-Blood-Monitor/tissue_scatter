using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NumSharp;

namespace TissueScatter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ObtainAbsorptionCoefficients(660);
        }

        private struct ScatteringCoefficients
        {
            public double muSkin;
            public double muBone;
            public double muMuscle;
        }

        static Tuple<long, long> ObtainAbsorptionCoefficients(uint waveLength)
        {
            var waveLengths = new List<int>();
            var absorptionBlood = new List<int>();
            var absorptionOBlood = new List<int>();

            var data = File.ReadAllLines("Resources/BloodAbsorptionData.txt").Skip(17);

            foreach (var line in data)
            {
                var cells = line.Split('\t');

                waveLengths.Add(int.Parse(cells[0]));
                absorptionBlood.Add(int.Parse(cells[1]));
                absorptionOBlood.Add(int.Parse(cells[2]));
            }

            if (waveLength < waveLengths.First() && waveLength > waveLengths.Last())
            {
                throw new ArgumentException(
                    "Wavelength can't be larger or smaller than the minimum or larger than the maximum Wavelength in the tabulate data",
                    nameof(waveLength));
            }

            if (waveLength == waveLengths.First())
            {
                return new Tuple<long, long>(absorptionBlood.First(), absorptionOBlood.First());
            }

            if (waveLength == waveLengths.Last())
            {
                return new Tuple<long, long>(absorptionBlood.Last(), absorptionOBlood.Last());
            }

            int i = 0;
            while (waveLength > waveLengths[i])
            {
                i++;
            }

            var factor = (float)(waveLength - waveLengths[i - 1]) / (waveLengths[i] - waveLengths[i - 1]);

            long absBlood = (long) (absorptionBlood[i - 1] + ((absorptionBlood[i] - absorptionBlood[i - 1]) * factor));
            long absOBlood = (long) (absorptionOBlood[i - 1] + (absorptionOBlood[i] - absorptionOBlood[i - 1]) * factor);

            return new Tuple<long, long>(absBlood, absOBlood);
        }

        static ScatteringCoefficients ObtainScatteringCoefficients(uint wavelength)
        {
            ScatteringCoefficients coefficients;

            double aSkin = 46.0;
            double aBone = 22.9;
            double aMuscle = 13.0;
            double bSkin = -1.421;
            double bBone = -0.716;
            double bMuscle = -1.470;
            double ldRef = 500;

            coefficients.muSkin = aSkin * Math.Pow(wavelength / ldRef, bSkin);
            coefficients.muBone = aBone * Math.Pow(wavelength / ldRef, bBone);
            coefficients.muMuscle = aMuscle * Math.Pow(wavelength / ldRef, bMuscle);

            return coefficients;
        }
    }
}
