using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Xunit;
using static BayerRawImageViewer.BayerRaw;

namespace BayerRawImageViewer.Tests
{
    /// <summary>A temporary file that is deleted on dispose.</summary>
    internal sealed class TempFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.GetTempFileName();

        public void Dispose()
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }
    }

    internal static class TestHelpers
    {
        public static ushort[] RandomPixels(int count, int depth, int seed)
        {
            var rng = new Random(seed);
            var pixels = new ushort[count];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = (ushort)rng.Next(1 << depth);
            return pixels;
        }

        /// <summary>Pixel values as BayerRaw holds them internally: MSB-aligned to 16 bits.</summary>
        public static ushort[] ToRaw16(ushort[] pixels, int depth)
        {
            return pixels.Select(p => (ushort)(p << (16 - depth))).ToArray();
        }

        /// <summary>Write pixels to a raw file and load it with BayerRaw.</summary>
        public static BayerRaw LoadBayerRaw(TempFile file, ushort[] pixels, int width, int height, int depth, RawType type, int padding = 16)
        {
            int stride = RawFileEncoder.RowBytes(width, depth, type) + padding;
            File.WriteAllBytes(file.Path, RawFileEncoder.Encode(pixels, width, height, depth, type, stride));
            return new BayerRaw(file.Path, width, height, stride, depth, type);
        }

        /// <summary>
        /// Apply a UI Bayer label ("BG", "RG", "GR", "GB") to BayerRaw.
        /// This is the only place that depends on the type BayerRaw uses for patterns.
        /// </summary>
        public static void SetBayerPattern(BayerRaw bayerRaw, string uiLabel)
        {
            bayerRaw.SetBayerPattern(OpenCvReference.ToOpenCvCode(uiLabel));
        }

        /// <summary>Returns the bitmap as tightly packed B, G, R bytes.</summary>
        public static byte[] ToBgrBytes(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                int rowBytes = bmp.Width * 3;
                var bytes = new byte[rowBytes * bmp.Height];
                for (int y = 0; y < bmp.Height; y++)
                    Marshal.Copy(bd.Scan0 + y * bd.Stride, bytes, y * rowBytes, rowBytes);
                return bytes;
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
        }

        /// <summary>Assert every channel of every pixel differs by at most tolerance.</summary>
        public static void AssertImagesClose(Bitmap expected, Bitmap actual, int tolerance)
        {
            Assert.Equal(expected.Width, actual.Width);
            Assert.Equal(expected.Height, actual.Height);

            byte[] e = ToBgrBytes(expected);
            byte[] a = ToBgrBytes(actual);
            int maxDiff = 0, worst = 0;
            for (int i = 0; i < e.Length; i++)
            {
                int diff = Math.Abs(e[i] - a[i]);
                if (diff > maxDiff)
                {
                    maxDiff = diff;
                    worst = i;
                }
            }

            int pixel = worst / 3;
            Assert.True(maxDiff <= tolerance,
                $"Max channel difference {maxDiff} > {tolerance} at (x={pixel % expected.Width}, y={pixel / expected.Width}), " +
                $"channel {"BGR"[worst % 3]}: expected {e[worst]}, actual {a[worst]}");
        }
    }
}
