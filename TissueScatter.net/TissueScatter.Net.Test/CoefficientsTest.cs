using TissueScatter.Net.Coefficients;
using Xunit;

namespace TissueScatter.Net.Test
{
    public class CoefficientsTest
    {
        [Fact]
        public void ObtainAbsorptionCoefficientsTest()
        {
            const uint testWavelength1 = 660;
            const uint testWavelength2 = 940;

            var validReturnData1 = new AbsorptionCoefficients
            {
                AbsorptionBlood = 316,
                AbsorptionOxygenatedBlood = 3200
            };

            var validReturnData2 = new AbsorptionCoefficients
            {
                AbsorptionBlood = 1214,
                AbsorptionOxygenatedBlood = 802
            };

            var coefficients = Coefficients.Coefficients.ObtainAbsorptionCoefficients(testWavelength1);

            Assert.Equal(validReturnData1.AbsorptionBlood, coefficients.AbsorptionBlood);
            Assert.Equal(validReturnData1.AbsorptionOxygenatedBlood, coefficients.AbsorptionOxygenatedBlood);

            coefficients = Coefficients.Coefficients.ObtainAbsorptionCoefficients(testWavelength2);

            Assert.Equal(validReturnData2.AbsorptionBlood, coefficients.AbsorptionBlood);
            Assert.Equal(validReturnData2.AbsorptionOxygenatedBlood, coefficients.AbsorptionOxygenatedBlood);
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

            var coefficients = Coefficients.Coefficients.ObtainScatteringCoefficients(testWavelength1);

            Assert.Equal(validReturnData1.MuSkin, coefficients.MuSkin);
            Assert.Equal(validReturnData1.MuBone, coefficients.MuBone);
            Assert.Equal(validReturnData1.MuMuscle, coefficients.MuMuscle);

            coefficients = Coefficients.Coefficients.ObtainScatteringCoefficients(testWavelength2);

            Assert.Equal(validReturnData2.MuSkin, coefficients.MuSkin);
            Assert.Equal(validReturnData2.MuBone, coefficients.MuBone);
            Assert.Equal(validReturnData2.MuMuscle, coefficients.MuMuscle);
        }
    }
}
