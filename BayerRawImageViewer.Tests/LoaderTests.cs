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
        public void Load_FileTooSmall_Throws()
        {
            using var file = new TempFile();
            File.WriteAllBytes(file.Path, new byte[10]);

            Assert.Throws<ArgumentException>(() => new BayerRaw(file.Path, Width, Height, Width * 2, 12, RawType.RawType_Unpacked));
        }
    }
}
