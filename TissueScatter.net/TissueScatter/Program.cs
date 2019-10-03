using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NumSharp;
using TissueScatter.Net.Coefficients;

namespace TissueScatter
{
    class Program
    {
        static void Main(string[] args)
        {
            var absorption = Coefficients.ObtainAbsorptionCoefficients(660);
            var scatteringCoefficients = Coefficients.ObtainScatteringCoefficients(660);
        }
    }
}
