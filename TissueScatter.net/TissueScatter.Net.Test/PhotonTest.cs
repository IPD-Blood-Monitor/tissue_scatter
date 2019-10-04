using System;
using System.Collections.Generic;
using System.Text;
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

            var results = Photons.Photons.FilterPhotons(x, y, z, xyBound, zBound);

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

            var results = Photons.Photons.FilterPhotons(x, y, z, xyBound, zBound);

            Assert.DoesNotContain(1, results);
            Assert.Contains(5, results);
            Assert.True(results.Count == 1);
        }

        [Fact]
        public void DetectorPhotonsTest()
        {

        }
    }
}
