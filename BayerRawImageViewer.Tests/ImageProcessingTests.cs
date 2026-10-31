using OpenCvSharp;
using Xunit;

namespace BayerRawImageViewer.Tests
{
    /// <summary>
    /// Compare each ImageProcessing step against the equivalent OpenCV call.
    /// </summary>
    public class ImageProcessingTests
    {
        public static IEnumerable<object[]> DemosaicCases()
        {
            foreach (string pattern in Enum.GetNames<BayerPattern>())
                foreach (var (width, height) in new[] { (64, 48), (31, 17), (3, 3) })
                    yield return new object[] { pattern, width, height };
        }

        [Fact]
        public void SubtractBlackLevel_MatchesOpenCv()
        {
            ushort[] raw = TestHelpers.RandomPixels(64 * 48, 16, seed: 3);
            using var src = ToMat(raw, 64, 48);
            using var expected = new Mat();
            Cv2.Subtract(src, new Scalar(3000), expected);

            Assert.Equal(ToUShorts(expected), ImageProcessing.SubtractBlackLevel(raw, 3000));
        }

        // BayerPattern is internal, so test data names it by string
        [Theory]
        [MemberData(nameof(DemosaicCases))]
        public void DemosaicBilinear_MatchesOpenCv(string patternName, int width, int height)
        {
            var pattern = Enum.Parse<BayerPattern>(patternName);
            ushort[] raw = TestHelpers.RandomPixels(width * height, 16, seed: width);
            using var src = ToMat(raw, width, height);
            using var expected = new Mat();
            Cv2.Demosaicing(src, expected, ToOpenCvCode(pattern), 3);

            Assert.Equal(ToUShorts(expected), ImageProcessing.DemosaicBilinear(raw, width, height, pattern));
        }

        [Fact]
        public void ComputeGrayWorldGains_MatchesOpenCvSum()
        {
            ushort[] bgr = TestHelpers.RandomPixels(64 * 48 * 3, 16, seed: 4);
            using var mat = ToMat(bgr, 64 * 3, 48).Reshape(3);
            Scalar sums = Cv2.Sum(mat);

            var (blueGain, redGain) = ImageProcessing.ComputeGrayWorldGains(bgr);

            Assert.Equal(sums[1] / sums[0], blueGain, 12);
            Assert.Equal(sums[1] / sums[2], redGain, 12);
        }

        [Fact]
        public void ToBgr8_WithoutGain_MatchesOpenCvConvertTo()
        {
            ushort[] bgr = TestHelpers.RandomPixels(64 * 48 * 3, 16, seed: 5);
            using var src = ToMat(bgr, 64 * 3, 48);
            using var expected = new Mat();
            src.ConvertTo(expected, MatType.CV_8UC1, 1 / 256.0);

            Assert.Equal(ToBytes(expected), ImageProcessing.ToBgr8(bgr, 1.0, 1.0));
        }

        [Fact]
        public void ToBgr8_WithGain_MatchesOpenCvWithinOne()
        {
            const double blueGain = 1.37, redGain = 0.81;
            ushort[] bgr = TestHelpers.RandomPixels(64 * 48 * 3, 16, seed: 6);
            using var src = ToMat(bgr, 64 * 3, 48).Reshape(3);

            // Same steps as the original OpenCV pipeline
            Cv2.Split(src, out var planes);
            using var bPlane = planes[0] * blueGain;
            using var rPlane = planes[2] * redGain;
            using var merged = new Mat();
            Cv2.Merge(new Mat[] { bPlane, planes[1], rPlane }, merged);
            using var expected = new Mat();
            merged.ConvertTo(expected, MatType.CV_8UC3, 1 / 256.0);
            foreach (var plane in planes)
                plane.Dispose();

            byte[] e = ToBytes(expected);
            byte[] a = ImageProcessing.ToBgr8(bgr, blueGain, redGain);
            Assert.Equal(e.Length, a.Length);
            Assert.All(Enumerable.Range(0, e.Length), i => Assert.InRange(a[i] - e[i], -1, 1));
        }

        [Fact]
        public void ReduceTo8Bit_MatchesOpenCvConvertTo()
        {
            ushort[] raw = TestHelpers.RandomPixels(64 * 48, 16, seed: 7);
            using var src = ToMat(raw, 64, 48);
            using var expected = new Mat();
            src.ConvertTo(expected, MatType.CV_8UC1, 1 / 256.0);

            Assert.Equal(ToBytes(expected), ImageProcessing.ReduceTo8Bit(raw));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(14)]
        [InlineData(16)]
        public void ReduceToDepth_MatchesOpenCvConvertTo(int depth)
        {
            ushort[] raw = TestHelpers.RandomPixels(64 * 48, 16, seed: 8);
            using var src = ToMat(raw, 64, 48);
            using var expected = new Mat();
            src.ConvertTo(expected, MatType.CV_16UC1, 1.0 / (1 << (16 - depth)));

            Assert.Equal(ToUShorts(expected), ImageProcessing.ReduceToDepth(raw, depth));
        }

        // OpenCV names patterns differently and outputs RGB, which together
        // give the same B, G, R result for the same two letters
        private static ColorConversionCodes ToOpenCvCode(BayerPattern pattern) => pattern switch
        {
            BayerPattern.BGGR => ColorConversionCodes.BayerBG2RGB,
            BayerPattern.RGGB => ColorConversionCodes.BayerRG2RGB,
            BayerPattern.GRBG => ColorConversionCodes.BayerGR2RGB,
            BayerPattern.GBRG => ColorConversionCodes.BayerGB2RGB,
            _ => throw new ArgumentOutOfRangeException(nameof(pattern)),
        };

        private static Mat ToMat(ushort[] data, int cols, int rows)
        {
            var mat = new Mat(rows, cols, MatType.CV_16UC1);
            mat.SetArray(data);
            return mat;
        }

        private static ushort[] ToUShorts(Mat mat)
        {
            using var flat = mat.Reshape(1);
            flat.GetArray(out ushort[] data);
            return data;
        }

        private static byte[] ToBytes(Mat mat)
        {
            using var flat = mat.Reshape(1);
            flat.GetArray(out byte[] data);
            return data;
        }
    }
}
