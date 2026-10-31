using Xunit;
using static BayerRawImageViewer.BayerRaw;

namespace BayerRawImageViewer.Tests
{
    /// <summary>
    /// End-to-end comparison of BayerRaw against the OpenCV reference pipeline.
    /// </summary>
    public class PipelineTests
    {
        // Rounding in the gain / scale steps may differ by one code value
        private const int BitmapTolerance = 1;

        public static IEnumerable<object[]> BitmapCases()
        {
            foreach (string label in new[] { "BG", "RG", "GR", "GB" })
                foreach (bool awb in new[] { false, true })
                    foreach (int ob in new[] { 0, 64 })
                        yield return new object[] { label, awb, ob };
        }

        [Theory]
        [MemberData(nameof(BitmapCases))]
        public void ToBitmap_MatchesOpenCv_RandomRaw(string label, bool awb, int ob)
        {
            const int width = 64, height = 48, depth = 12;
            ushort[] pixels = TestHelpers.RandomPixels(width * height, depth, seed: 1);
            AssertBitmapMatches(pixels, width, height, depth, label, awb, ob);
        }

        [Theory]
        [MemberData(nameof(BitmapCases))]
        public void ToBitmap_MatchesOpenCv_SmoothRaw(string label, bool awb, int ob)
        {
            // A smooth, colored scene is closer to real sensor data than random noise
            const int width = 30, height = 22, depth = 10;
            var pixels = new ushort[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int channel = (x & 1) + (y & 1);    // 0, 1 or 2 depending on CFA site
                    pixels[y * width + x] = (ushort)((x * 20 + y * 10 + channel * 150) % 1024);
                }

            AssertBitmapMatches(pixels, width, height, depth, label, awb, ob);
        }

        [Theory]
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(14)]
        [InlineData(16)]
        public void SaveUnpackRaw_MatchesOpenCv(int outputDepth)
        {
            const int width = 64, height = 48, depth = 12;
            ushort[] pixels = TestHelpers.RandomPixels(width * height, depth, seed: 2);
            using var input = new TempFile();
            using var output = new TempFile();

            var bayerRaw = TestHelpers.LoadBayerRaw(input, pixels, width, height, depth, RawType.RawType_Unpacked);
            bayerRaw.saveUnpackRaw(outputDepth, output.Path);

            byte[] expected = OpenCvReference.UnpackedRawFile(TestHelpers.ToRaw16(pixels, depth), width, height, outputDepth);
            Assert.Equal(expected, File.ReadAllBytes(output.Path));
        }

        private static void AssertBitmapMatches(ushort[] pixels, int width, int height, int depth, string label, bool awb, int ob)
        {
            using var file = new TempFile();
            var bayerRaw = TestHelpers.LoadBayerRaw(file, pixels, width, height, depth, RawType.RawType_Unpacked);
            bayerRaw.EnableAwb = awb;
            bayerRaw.OB = ob;
            TestHelpers.SetBayerPattern(bayerRaw, label);

            using var actual = bayerRaw.ToBitmap();
            using var expected = OpenCvReference.ToBitmap(TestHelpers.ToRaw16(pixels, depth), width, height, label, awb, ob << (16 - depth));

            TestHelpers.AssertImagesClose(expected, actual, BitmapTolerance);
        }
    }
}
