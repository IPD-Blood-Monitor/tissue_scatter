using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TissueScatter.Net.Test
{
    public class ScatterTest
    {
        [Fact(Skip = "Bad Test, needs changes")]
        public void ScatterBasicTest()
        {
            //Inputs
            uint wavelength = 660;
            var distanceToDetector1 = 0.1;
            var distanceToDetector2 = 0.3;
            var width = 0.05;
            var dSkin = 0.05;
            var dMuscle = 1;
            var dBone = 3;
            var concentrationBlood = 0.150;
            var ratioOxygen = 0.9;

            //Expected values
            var detectedPhotons1 = 25771000.0;
            var detectedPhotons2 = 143900.0;

            var averageLength1 = 0.08431636209891455;
            var averageLength2 = 0.23217345351881793;

            var data = Scatter.Scatterlight(wavelength, distanceToDetector1, distanceToDetector2, width, dSkin, dMuscle,
                dBone, concentrationBlood, ratioOxygen);

            Assert.Equal(detectedPhotons1, data.DetectedPhotons1);
            Assert.Equal(detectedPhotons2, data.DetectedPhotons2);
            Assert.Equal(averageLength1, data.LengthToD1);
            Assert.Equal(averageLength2, data.LengthToD2);
        }

        [Fact(Skip = "Test not finished")]
        public void ScatterCheckRationTest()
        {
            
        }
    }
}
