using System;
using System.Collections.Generic;
using TissueScatter.Core.Photons;
using Xunit;

namespace TissueScatter.Net.Test
{
    public class PhotonTest
    {
        [Fact]
        public void FilterPhotonsTest()
        {
            var x = new List<double> { 1, 0.1, 100, 1.5, 0.5, 0.3 };
            var y = new List<double> { 1.5, 0.3, 1.1, 0.7, 10, 0.17 };
            var z = new List<double> { 11, 0.7, 1.2, 1.7, 1.8, 0.2 };

            var xyBound = 1.0;
            var zBound = 1.0;

            var results = Photons.FilterPhotons(x, y, z, xyBound, zBound);

            Assert.Contains(1, results);
            Assert.Contains(5, results);
            Assert.True(results.Count == 2);
        }

        [Fact]
        public void FilterPhotonOutOfSkinTest()
        {
            var x = new List<double> { 1, 0.1, 100, 1.5, 0.5, 0.3 };
            var y = new List<double> { 1.5, 0.3, 1.1, 0.7, 10, 0.17 };
            var z = new List<double> { 11, -0.7, 1.2, 1.7, 1.8, 0.2 };

            var xyBound = 1.0;
            var zBound = 1.0;
            
            var results = Photons.FilterPhotons(x, y, z, xyBound, zBound);

            Assert.DoesNotContain(1, results);
            Assert.Contains(5, results);
            Assert.True(results.Count == 1);
        }

        [Fact]
        public void DetectorBasicTest()
        {
            //Test data
            var x = new List<double> { 1, 0.1, 100, 1.1, 0.5, 0.3 };
            var y = new List<double> { 1.1, 0.3, 1.1, 0.7, 10, 0.17 };
            var z = new List<double> { -11, -0.7, 1.2, -1.7, 1.8, 0.2 };

            var amplitudes = new List<double> { 1, 1.1, 2, 0.7, 0.2, 1.8 };
            var traveledDistances = new List<double> { 2, 1, 2.3, 0.9, 0.1, 2.35 };

            var detectorDistance = 1.0;
            var detectorWidth = 0.5;

            //Expected Results
            var expectedAmplitude = 1.7;
            var expectedTraveledDistances = new List<double> { 2, 0.9 };

            var detectorData = Photons.DetectorPhotons(x, y, z, traveledDistances, amplitudes, detectorDistance, detectorWidth);

            Assert.Equal(expectedAmplitude, detectorData.TotalAmplitude);
            Assert.Equal(expectedTraveledDistances, detectorData.TraveledDistances);
        }

        [Fact]
        public void DetectorDifferentListLengthTest()
        {
            var x = new List<double> { 1, 2, 3, 4 };
            var y = new List<double> { 1, 2, 3 };
            var z = new List<double> { 1, 2, 3 };

            var amplitudes = new List<double> { 1, 2, 3 };
            var traveledDistances = new List<double> { 1, 2, 3 };

            Assert.Throws<ArgumentException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances, amplitudes, 1,
                1));

            x = new List<double> { 1, 2, 3 };
            y = new List<double> { 1, 2, 3, 4 };
            z = new List<double> { 1, 2, 3 };

            amplitudes = new List<double> { 1, 2, 3 };
            traveledDistances = new List<double> { 1, 2, 3 };

            Assert.Throws<ArgumentException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances, amplitudes, 1,
                1));

            x = new List<double> { 1, 2, 3 };
            y = new List<double> { 1, 2, 3 };
            z = new List<double> { 1, 2, 3, 4 };

            amplitudes = new List<double> { 1, 2, 3 };
            traveledDistances = new List<double> { 1, 2, 3 };

            Assert.Throws<ArgumentException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances, amplitudes, 1,
                1));

            x = new List<double> { 1, 2, 3 };
            y = new List<double> { 1, 2, 3 };
            z = new List<double> { 1, 2, 3 };

            amplitudes = new List<double> { 1, 2, 3, 4 };
            traveledDistances = new List<double> { 1, 2, 3 };

            Assert.Throws<ArgumentException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances, amplitudes, 1,
                1));

            x = new List<double> { 1, 2, 3 };
            y = new List<double> { 1, 2, 3 };
            z = new List<double> { 1, 2, 3 };

            amplitudes = new List<double> { 1, 2, 3 };
            traveledDistances = new List<double> { 1, 2, 3, 4 };

            Assert.Throws<ArgumentException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances, amplitudes, 1,
                1));
        }

        [Fact]
        public void DetectorNegativeDistanceOrWidthTest()
        {
            //Test data
            var x = new List<double> { 1, 0.1, 100, 1.1, 0.5, 0.3 };
            var y = new List<double> { 1.1, 0.3, 1.1, 0.7, 10, 0.17 };
            var z = new List<double> { -11, -0.7, 1.2, -1.7, 1.8, 0.2 };

            var amplitudes = new List<double> { 1, 1.1, 2, 0.7, 0.2, 1.8 };
            var traveledDistances = new List<double> { 2, 1, 2.3, 0.9, 0.1, 2.35 };

            var detectorDistance = -1.0;
            var detectorWidth = 0.5;

            Assert.Throws<ArgumentOutOfRangeException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances,
                amplitudes, detectorDistance, detectorWidth));

            detectorDistance = 1.0;
            detectorWidth = -0.5;

            Assert.Throws<ArgumentOutOfRangeException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances,
                amplitudes, detectorDistance, detectorWidth));

            detectorDistance = -1.0;
            detectorWidth = -0.5;

            Assert.Throws<ArgumentOutOfRangeException>(() => Photons.DetectorPhotons(x, y, z, traveledDistances,
                amplitudes, detectorDistance, detectorWidth));
        }

        [Fact]
        public void UpdatePositionsBasicTest()
        {
            //TODO determine if there can and should be a test for this, as it contains randomness
        }

        [Fact]
        public void AreWeDoneBasicTest()
        {
            int detector1 = 9999, detector2 = 9999;

            Assert.True(Photons.AreWeDone(detector1, detector2));

            detector1 = detector2 = 10000;

            Assert.False(Photons.AreWeDone(detector1, detector2));
        }
    }
}
