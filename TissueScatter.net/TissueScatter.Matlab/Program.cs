using System;
using System.Collections.Generic;
using System.IO;
using MatFileHandler;
using TissueScatter.Core;

namespace TissueScatter.Matlab
{
    class Program
    {
        static void Main(string[] args)
        {
            /* TODO
             * ratio 0-1 15 steps
             * 75 - 200 g/L 20 steps or so
             * wavelength:
             * 450-550
             * 550-590
             * 750-850
             */
            double ratioStart = 0;
            double ratioEnd = 1;
            double ratioIncrease = 0.1;

            double concentrationStart = 75;
            double concentrationStop = 200;
            double concentrationIncrement = 12.5;

            // TODO Add variable that stores the used parameters, or possibly create a dictionary with the parameters on one side and the data on the other side

            var parameters = ScatterParameters.GetExampleParameters();
            List<uint> wavelengths = new List<uint>();

            if (args.Length == 0)
            {
                Console.WriteLine("Enter the wavelength");
                wavelengths.Add(Convert.ToUInt32(Console.ReadLine()));
                parameters.Wavelength = wavelengths[0];
                Console.WriteLine("Enter the ratio");
                var ratioInput = Convert.ToDouble(Console.ReadLine());
                ratioStart = ratioInput;
                ratioEnd = ratioInput;
                ratioIncrease = 100;
                Console.WriteLine("Enter the concentration");
                var concentrationInput = Convert.ToDouble(Console.ReadLine());
                concentrationStart = concentrationInput;
                concentrationStop = concentrationInput;
                concentrationIncrement = 100;
            }
            else if (args[0] == "-sweep-parameters")
            {
                for (uint i = 450; i < 550; i += 10)
                {
                    wavelengths.Add(i);
                }
                for (uint i = 550; i < 590; i += 10)
                {
                    wavelengths.Add(i);
                }

                for (uint i = 750; i < 890; i += 10)
                {
                    wavelengths.Add(i);
                }
            }
            else
            {
                wavelengths.Add(uint.Parse(args[0]));
            }

            double ratio = 0.0;

            const int numOfSamples = 3;

            DateTime start = DateTime.Now;
            Console.WriteLine("Started at {0:G}", start);

            for (int i = 0; i < wavelengths.Count; i++)
            {
                uint wavelength = wavelengths[i];
                for (ratio = ratioStart; ratio <= ratioEnd; ratio += ratioIncrease)
                {
                    for (double concentration = concentrationStart; concentration <= concentrationStop; concentration += concentrationIncrement)
                    {
                        var builder = new DataBuilder();
                        var detectedPhotons = builder.NewArray<double>(numOfSamples, 2);
                        parameters.ConcentrationBlood = concentration / 1000;
                        parameters.RatioOxygen = ratio;
                        parameters.Wavelength = wavelength;

                        for (int n = 0; n < numOfSamples; n++)
                        {
                            var data = Scatter.Scatterlight(parameters);
                            detectedPhotons[n, 0] = data.DetectedPhotons1;
                            detectedPhotons[n, 1] = data.DetectedPhotons2;
                        }

                        var variable = builder.NewVariable("detectedPhotons1", detectedPhotons);
                        var matFile = builder.NewFile(new[] { variable });

                        var path = $"output_data/{wavelength}/{ratio}/{concentration}.mat";
                        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            var writer = new MatFileWriter(fileStream);
                            writer.Write(matFile);
                        }
                    }
                }
                DateTime tmpEnd = DateTime.Now;
                TimeSpan tmpSpendTime = new TimeSpan(tmpEnd.Ticks - start.Ticks);

                using (StreamWriter writer = new StreamWriter($"output_data/{wavelength}/time.txt"))
                {
                    writer.WriteLine($"Time spend: {tmpSpendTime}");
                }
            }

            DateTime end = DateTime.Now;
            TimeSpan spendTime = new TimeSpan(end.Ticks - start.Ticks);

            Console.WriteLine("Stopped at {0:G}", end);
            Console.WriteLine($"Time spend: {spendTime}");

            Console.ReadKey();
        }
    }
}
