using System;
using TissueScatter.Core.Coefficients;
using Xunit;

namespace TissueScatter.Net.Test
{
    public class CoefficientsTest
    {
        [Fact]
        public void AbsorptionCoefficientsTest()
        {
            const uint testWavelength1 = 660;
            const uint testWavelength2 = 940;

            var validReturnData1 = new AbsorptionCoefficients
            {
                AbsorptionBlood = 3200,
                AbsorptionOxygenatedBlood = 316
            };

            var validReturnData2 = new AbsorptionCoefficients
            {
                AbsorptionBlood = 802,
                AbsorptionOxygenatedBlood = 1214
            };

            var coefficients = Coefficients.ObtainAbsorptionCoefficients(testWavelength1);

            Assert.Equal(validReturnData1.AbsorptionBlood, coefficients.AbsorptionBlood);
            Assert.Equal(validReturnData1.AbsorptionOxygenatedBlood, coefficients.AbsorptionOxygenatedBlood);

            coefficients = Coefficients.ObtainAbsorptionCoefficients(testWavelength2);

            Assert.Equal(validReturnData2.AbsorptionBlood, coefficients.AbsorptionBlood);
            Assert.Equal(validReturnData2.AbsorptionOxygenatedBlood, coefficients.AbsorptionOxygenatedBlood);
        }

        [Fact]
        public void AbsorptionCoefficientsNoInterpolationTest()
        {
            const uint wavelength = 600;

            var validReturnData = new AbsorptionCoefficients
            {
                AbsorptionBlood = 14600,
                AbsorptionOxygenatedBlood = 3200
            };

            var coefficients = Coefficients.ObtainAbsorptionCoefficients(wavelength);

            Assert.Equal(validReturnData.AbsorptionBlood, coefficients.AbsorptionBlood);
            Assert.Equal(validReturnData.AbsorptionOxygenatedBlood, coefficients.AbsorptionOxygenatedBlood);
        }

        [Fact]
        public void AbsorptionCoefficientsExtremesTest()
        {
            const uint testWavelength1 = 450;
            const uint testWavelength2 = 995;

            var validReturnData1 = new AbsorptionCoefficients
            {
                AbsorptionBlood = 58000,
                AbsorptionOxygenatedBlood = 68000
            };

            var validReturnData2 = new AbsorptionCoefficients
            {
                AbsorptionBlood = 372,
                AbsorptionOxygenatedBlood = 1052
            };

            var coefficients = Coefficients.ObtainAbsorptionCoefficients(testWavelength1);

            Assert.Equal(validReturnData1.AbsorptionBlood, coefficients.AbsorptionBlood);
            Assert.Equal(validReturnData1.AbsorptionOxygenatedBlood, coefficients.AbsorptionOxygenatedBlood);

            coefficients = Coefficients.ObtainAbsorptionCoefficients(testWavelength2);

            Assert.Equal(validReturnData2.AbsorptionBlood, coefficients.AbsorptionBlood);
            Assert.Equal(validReturnData2.AbsorptionOxygenatedBlood, coefficients.AbsorptionOxygenatedBlood);
        }

        [Fact]
        public void AbsorptionCoefficientsOutOfRangeTest()
        {
            const uint tooLowWavelength = 449;
            const uint tooHighWavelength = 996;

            Assert.Throws<ArgumentOutOfRangeException>(() => Coefficients.ObtainAbsorptionCoefficients(tooLowWavelength));
            Assert.Throws<ArgumentOutOfRangeException>(() =>Coefficients.ObtainAbsorptionCoefficients(tooHighWavelength));
        }

        [Fact]
        public void ObtainScatteringCoefficientsTest()
        {
            const uint testWavelength1 = 660;
            const uint testWavelength2 = 940;

            var validReturnData1 = new ScatteringCoefficients()
            {
                MuSkin = 31.004324624854842,
                MuBone = 18.771740745896697,
                MuMuscle = 8.643699764668847
            };

            var validReturnData2 = new ScatteringCoefficients
            {
                MuSkin = 18.757697501740985,
                MuBone = 14.572647582340004,
                MuMuscle = 5.139623591212953
            };

            var coefficients = Coefficients.ObtainScatteringCoefficients(testWavelength1);

            Assert.Equal(validReturnData1.MuSkin, coefficients.MuSkin);
            Assert.Equal(validReturnData1.MuBone, coefficients.MuBone);
            Assert.Equal(validReturnData1.MuMuscle, coefficients.MuMuscle);

            coefficients = Coefficients.ObtainScatteringCoefficients(testWavelength2);

            Assert.Equal(validReturnData2.MuSkin, coefficients.MuSkin);
            Assert.Equal(validReturnData2.MuBone, coefficients.MuBone);
            Assert.Equal(validReturnData2.MuMuscle, coefficients.MuMuscle);
        }

        [Fact]
        public void CalculateAbsorptionCoefficientsTest()
        {
            var absorptionBlood = 3200;
            var AbsorptionOxygenatedBlood = 316;
            var concentrationBlood = 0.150;
            var ratio = 0.9;
            var coefficient = Coefficients.CalculateAbsorptionCoefficient(absorptionBlood, AbsorptionOxygenatedBlood, concentrationBlood, ratio);

            const double excpectedValue = 0.001405581395348837;
            Assert.Equal(excpectedValue, coefficient);
        }
    }
}
