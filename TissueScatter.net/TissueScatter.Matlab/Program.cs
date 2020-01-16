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
            double ratioStart = 0.7;
            double ratioEnd = 1;
            double ratioIncrease = 0.1;

            double concentrationStart = 75;
            double concentrationStop = 200;
            double concentrationIncrement = 12.5;

            // TODO Add variable that stores the used parameters, or possibly create a dictionary with the parameters on one side and the data on the other side

            var parameters = ScatterParameters.GetExampleParameters();
            List<uint> wavelengths = new List<uint>();

            if (args.Length == 0 || args[0] == "-sweep-parameters-better")
            {
                //Set ratio and concentration variables
                //Then call regular sweep-parameters
                //ratioStart = 0.9;
                //concentrationStart = 150;
                if (args.Length == 0)
                {
                    args = new string[1];
                }
                args[0] = "-sweep-parameters";
            }
            if (args[0] == "-sweep-parameters")
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
            else if (args[0] == "-manual")
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
            else
            {
                wavelengths.Add(uint.Parse(args[0]));
            }

            double ratio = 0.0;

            const int numOfSamples = 3;

            DateTime start = DateTime.Now;
            Console.WriteLine("Started at {0:G}", start);

            var infoPath = $"output_data/{start:yyyy-MM-dd--HH-mm-ss}/info.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(infoPath)));
            using (StreamWriter writer = new StreamWriter(infoPath))
            {
                if (args.Length > 0)
                {
                    writer.WriteLine($"Run using parameters: {args[0]}");
                    writer.WriteLine($"Ratio start: {ratioStart}");
                    writer.WriteLine($"Ratio end: {ratioEnd}");
                    writer.WriteLine($"Ratio increment: {ratioIncrease}");

                    writer.WriteLine($"Concentration start: {concentrationStart}");
                    writer.WriteLine($"Concentration end: {concentrationStop}");
                    writer.WriteLine($"Concentration increment: {concentrationIncrement}");

                    writer.WriteLine($"Wavelengths:");
                    foreach (var wavelength in wavelengths)
                    {
                        writer.WriteLine(wavelength);
                    }
                }
                if(args[0] == "-manual")
                {
                    writer.WriteLine("Run using parameters: ");
                    writer.WriteLine($"Wavelength: {wavelengths[0]}");
                    writer.WriteLine($"Ratio: {ratioStart}");
                    writer.WriteLine($"Concentration: {concentrationStart}");
                }
            }

            for (int i = wavelengths.Count - 1; i >= 0; i--)
            {
                Console.WriteLine();
                uint wavelength = wavelengths[i];
                Console.WriteLine($"Wavelength: {wavelength}");
                Console.WriteLine($"Run {wavelengths.Count - i} of {wavelengths.Count}");
                //for (ratio = ratioStart; ratio <= ratioEnd; ratio += ratioIncrease)
                for (ratio = ratioEnd; ratio >= ratioStart; ratio -= ratioIncrease)
                {
                    for (double concentration = concentrationStop; concentration >= concentrationStart; concentration -= concentrationIncrement)
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
                            Console.Write(".");
                        }

                        var variable = builder.NewVariable("detectedPhotons1", detectedPhotons);
                        var matFile = builder.NewFile(new[] { variable });

                        var path = $"output_data/{start:yyyy-MM-dd--HH-mm-ss}/{wavelength}/{ratio}/{concentration}.mat";
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

                using (StreamWriter writer = new StreamWriter($"output_data/{start:yyyy-MM-dd--HH-mm-ss}/{wavelength}/time.txt"))
                {
                    writer.WriteLine($"Time spend: {tmpSpendTime}");
                }
            }

            DateTime end = DateTime.Now;
            TimeSpan spendTime = new TimeSpan(end.Ticks - start.Ticks);

            Console.WriteLine("Stopped at {0:G}", end);
            Console.WriteLine($"Time spend: {spendTime}");
            Console.WriteLine("Done");
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }
}
