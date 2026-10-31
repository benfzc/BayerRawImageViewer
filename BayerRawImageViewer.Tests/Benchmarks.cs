using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using static BayerRawImageViewer.BayerRaw;

namespace BayerRawImageViewer.Tests
{
    /// <summary>
    /// Timing of the full pipeline at the default resolution. Results are printed,
    /// not asserted: shared CI runners are too noisy for absolute limits.
    /// Run with: dotnet test --filter Category=Benchmark --logger "console;verbosity=detailed"
    /// </summary>
    [Trait("Category", "Benchmark")]
    public class Benchmarks
    {
        private const int Width = 5120, Height = 2880, Depth = 10, Stride = 6400;
        private const int Iterations = 5;

        private readonly ITestOutputHelper output;

        public Benchmarks(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Pipeline_5120x2880_Packed10()
        {
            using var file = new TempFile();
            var data = new byte[Stride * Height];
            new Random(0).NextBytes(data);
            File.WriteAllBytes(file.Path, data);

            var loadTimes = new List<double>();
            var bitmapTimes = new List<double>();

            // The first round is a warm-up (JIT, native library loading) and is not recorded
            for (int i = 0; i <= Iterations; i++)
            {
                var sw = Stopwatch.StartNew();
                using var bayerRaw = new BayerRaw(file.Path, Width, Height, Stride, Depth, RawType.RawType_Packed);
                double load = sw.Elapsed.TotalMilliseconds;

                bayerRaw.EnableAwb = true;
                bayerRaw.OB = 64;
                TestHelpers.SetBayerPattern(bayerRaw, "GB");

                sw.Restart();
                using var bmp = bayerRaw.ToBitmap();
                double toBitmap = sw.Elapsed.TotalMilliseconds;

                if (i > 0)
                {
                    loadTimes.Add(load);
                    bitmapTimes.Add(toBitmap);
                }
            }

            output.WriteLine($"CPU cores: {Environment.ProcessorCount}, iterations: {Iterations}");
            output.WriteLine($"Load     : median {Median(loadTimes),7:F1} ms");
            output.WriteLine($"ToBitmap : median {Median(bitmapTimes),7:F1} ms");
        }

        private static double Median(List<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            return sorted[sorted.Count / 2];
        }
    }
}
