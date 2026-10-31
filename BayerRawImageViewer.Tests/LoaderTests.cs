using Xunit;
using static BayerRawImageViewer.BayerRaw;

namespace BayerRawImageViewer.Tests
{
    /// <summary>
    /// Verify every raw layout decodes to the expected 16-bit MSB-aligned values.
    /// The decoded raw is read back through saveUnpackRaw(16), which writes it unchanged.
    /// </summary>
    public class LoaderTests
    {
        private const int Width = 64;
        private const int Height = 6;

        [Theory]
        [InlineData("Unpacked", 8)]
        [InlineData("Unpacked", 10)]
        [InlineData("Unpacked", 12)]
        [InlineData("Unpacked", 14)]
        [InlineData("Packed", 8)]
        [InlineData("Packed", 10)]
        [InlineData("Packed", 12)]
        [InlineData("Packed", 14)]
        [InlineData("MIPI", 8)]
        [InlineData("MIPI", 10)]
        [InlineData("MIPI", 12)]
        [InlineData("MIPI", 14)]
        public void Load_DecodesAllLayouts(string typeName, int depth)
        {
            // RawType is internal, so test data names it by string
            var type = Enum.Parse<RawType>("RawType_" + typeName);
            ushort[] pixels = TestHelpers.RandomPixels(Width * Height, depth, seed: depth);
            using var input = new TempFile();
            using var output = new TempFile();

            var bayerRaw = TestHelpers.LoadBayerRaw(input, pixels, Width, Height, depth, type);
            bayerRaw.saveUnpackRaw(16, output.Path);

            byte[] bytes = File.ReadAllBytes(output.Path);
            var decoded = new ushort[Width * Height];
            Buffer.BlockCopy(bytes, 0, decoded, 0, bytes.Length);

            Assert.Equal(TestHelpers.ToRaw16(pixels, depth), decoded);
        }

        [Fact]
        public void Load_StrideSmallerThanRow_Throws()
        {
            // Unpacked 10-bit needs 2 bytes per pixel, but the stride is set for packed 10-bit
            // (the reported crash: switching raw type keeps the old stride)
            int packedStride = Width * 5 / 4;
            using var file = new TempFile();
            File.WriteAllBytes(file.Path, new byte[packedStride * Height]);

            var ex = Assert.Throws<ArgumentException>(() => new BayerRaw(file.Path, Width, Height, packedStride, 10, RawType.RawType_Unpacked));
            Assert.Contains($"{Width * 2} bytes", ex.Message);
        }

        [Theory]
        [InlineData("Packed", 10, 66)]
        [InlineData("Packed", 12, 65)]
        [InlineData("MIPI", 14, 66)]
        public void Load_WidthNotMultipleOfPixelGroup_Throws(string typeName, int depth, int width)
        {
            var type = Enum.Parse<RawType>("RawType_" + typeName);
            int stride = width * 2;
            using var file = new TempFile();
            File.WriteAllBytes(file.Path, new byte[stride * Height]);

            Assert.Throws<ArgumentException>(() => new BayerRaw(file.Path, width, Height, stride, depth, type));
        }

        [Fact]
        public void Load_ImageSmallerThan3x3_Throws()
        {
            using var file = new TempFile();
            File.WriteAllBytes(file.Path, new byte[2 * 2 * Height]);

            Assert.Throws<ArgumentException>(() => new BayerRaw(file.Path, 2, Height, 4, 12, RawType.RawType_Unpacked));
        }

        [Fact]
        public void Load_FileTooSmall_Throws()
        {
            using var file = new TempFile();
            File.WriteAllBytes(file.Path, new byte[10]);

            Assert.Throws<ArgumentException>(() => new BayerRaw(file.Path, Width, Height, Width * 2, 12, RawType.RawType_Unpacked));
        }
    }
}
