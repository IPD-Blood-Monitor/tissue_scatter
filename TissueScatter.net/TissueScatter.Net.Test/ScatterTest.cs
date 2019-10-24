using System;
using TissueScatter.Core;
using Xunit;

namespace TissueScatter.Net.Test
{
    public class ScatterTest
    {
        //This test is actually not very good at all. Very little value should be attached to the outcome of this test
        [Fact(Skip = "Bad Test, needs changes")]
        //[Fact]
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

            //Expected values (ballpark figures, actually)
            var detectedPhotons1 = 22777800.0;
            var detectedPhotons2 = 90400.0;

            var averageLength1 = 0.08154087172417027;
            var averageLength2 = 0.22765482292591624;

            var data = Scatter.Scatterlight(wavelength, distanceToDetector1, distanceToDetector2, width, dSkin, dMuscle,
                dBone, concentrationBlood, ratioOxygen);

            var photonTolerance = 50000;
            var distanceTolerance = 0.01;
            Assert.True(Math.Abs(detectedPhotons1 - data.DetectedPhotons1) < photonTolerance, "Tolerance was " + Math.Abs(detectedPhotons1 - data.DetectedPhotons1));
            Assert.True(Math.Abs(detectedPhotons2 - data.DetectedPhotons2) < photonTolerance, "Tolerance was " + Math.Abs(detectedPhotons2 - data.DetectedPhotons2));
            Assert.True(Math.Abs(averageLength1 - data.LengthToD1) < distanceTolerance, "Tolerance was " + Math.Abs(averageLength1 - data.LengthToD1));
            Assert.True(Math.Abs(averageLength2 - data.LengthToD2) < distanceTolerance, "Tolerance was " + Math.Abs(averageLength2 - data.LengthToD2));
        }

        [Fact(Skip = "Test not finished")]
        public void ScatterCheckRationTest()
        {
            
        }
    }
}
