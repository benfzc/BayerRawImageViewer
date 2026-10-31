using System.Drawing;
using Xunit;
using static BayerRawImageViewer.BayerRaw;

namespace BayerRawImageViewer.Tests
{
    /// <summary>
    /// Pin down what each UI Bayer option means. The UI label names the top-left
    /// 2x2 block of the sensor, read row by row:
    ///   "BG" = B G / G R,  "RG" = R G / G B,  "GR" = G R / B G,  "GB" = G B / R G
    /// </summary>
    public class BayerPatternTests
    {
        private const int Width = 16, Height = 16, Depth = 8;

        [Theory]
        [InlineData("BG", 1, 1)]
        [InlineData("RG", 0, 0)]
        [InlineData("GR", 1, 0)]
        [InlineData("GB", 0, 1)]
        public void LightingRedSites_ProducesRedImage(string label, int redX, int redY)
        {
            Color color = RenderWithLitSites(label, redX, redY);
            Assert.Equal(Color.FromArgb(255, 0, 0).ToArgb(), color.ToArgb());
        }

        [Theory]
        [InlineData("BG", 0, 0)]
        [InlineData("RG", 1, 1)]
        [InlineData("GR", 0, 1)]
        [InlineData("GB", 1, 0)]
        public void LightingBlueSites_ProducesBlueImage(string label, int blueX, int blueY)
        {
            Color color = RenderWithLitSites(label, blueX, blueY);
            Assert.Equal(Color.FromArgb(0, 0, 255).ToArgb(), color.ToArgb());
        }

        /// <summary>
        /// Set only the sites at (siteX, siteY) of every 2x2 block to full scale
        /// and return the color of a pixel in the image interior.
        /// </summary>
        private static Color RenderWithLitSites(string label, int siteX, int siteY)
        {
            var pixels = new ushort[Width * Height];
            for (int y = siteY; y < Height; y += 2)
                for (int x = siteX; x < Width; x += 2)
                    pixels[y * Width + x] = 255;

            using var file = new TempFile();
            using var bayerRaw = TestHelpers.LoadBayerRaw(file, pixels, Width, Height, Depth, RawType.RawType_Unpacked);
            TestHelpers.SetBayerPattern(bayerRaw, label);

            using var bmp = bayerRaw.ToBitmap();
            return bmp.GetPixel(Width / 2, Height / 2);
        }
    }
}
