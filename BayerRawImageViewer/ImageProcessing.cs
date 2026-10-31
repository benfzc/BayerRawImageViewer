using System.Collections.Concurrent;

namespace BayerRawImageViewer
{
    /// <summary>
    /// Bayer CFA layouts, named by the top-left 2x2 block read row by row.
    /// For example BGGR is:  B G
    ///                       G R
    /// </summary>
    internal enum BayerPattern
    {
        BGGR,
        RGGB,
        GRBG,
        GBRG
    }

    /// <summary>
    /// Image processing steps of the viewer, as pure functions.
    /// Images are flat row-major arrays: a raw image has one value per pixel,
    /// a color image has interleaved B, G, R values. 16-bit data is MSB-aligned.
    /// Rounding and saturation follow the original OpenCV-based implementation.
    /// </summary>
    internal static class ImageProcessing
    {
        /// <summary>Subtract the black level, clamping at zero.</summary>
        public static ushort[] SubtractBlackLevel(ushort[] raw, int ob)
        {
            var result = NewImage<ushort>(raw.Length);
            ForEachRange(raw.Length, (start, end) =>
            {
                for (int i = start; i < end; i++)
                    result[i] = (ushort)Math.Max(raw[i] - ob, 0);
            });
            return result;
        }

        /// <summary>
        /// Bilinear demosaic. Each missing color is the average of the nearest
        /// pixels of that color. The 1-pixel image border repeats its inner
        /// neighbor, the same as OpenCV.
        /// </summary>
        /// <returns>Interleaved B, G, R image</returns>
        public static ushort[] DemosaicBilinear(ushort[] raw, int width, int height, BayerPattern pattern)
        {
            if (width < 3 || height < 3)
                throw new ArgumentException("Image must be at least 3x3 pixels.");

            var bgr = NewImage<ushort>(width * height * 3);
            var (dx, dy) = OffsetFromBggr(pattern);

            Parallel.For(1, height - 1, y =>
            {
                ReadOnlySpan<ushort> up = raw.AsSpan((y - 1) * width, width);
                ReadOnlySpan<ushort> mid = raw.AsSpan(y * width, width);
                ReadOnlySpan<ushort> down = raw.AsSpan((y + 1) * width, width);
                Span<ushort> dst = bgr.AsSpan(y * width * 3, width * 3);

                // In BGGR, even rows hold B and G, odd rows hold G and R.
                // Channel index (B = 0, R = 2) of the color sampled on this row, and of the other one
                int rowChannel = ((y + dy) & 1) == 0 ? 0 : 2;
                int columnChannel = 2 - rowChannel;

                // Green and non-green sites alternate along a row
                bool isGreen = ((1 + dx + y + dy) & 1) == 1;
                for (int x = 1; x < width - 1; x++)
                {
                    int o = x * 3;
                    if (isGreen)
                    {
                        // Green site: this row's color is left/right, the other color is up/down
                        dst[o + rowChannel] = (ushort)Average2(mid[x - 1], mid[x + 1]);
                        dst[o + 1] = mid[x];
                        dst[o + columnChannel] = (ushort)Average2(up[x], down[x]);
                    }
                    else
                    {
                        // Blue or red site: green is on the 4 sides, the opposite color on the 4 diagonals
                        dst[o + rowChannel] = mid[x];
                        dst[o + 1] = (ushort)Average4(up[x], down[x], mid[x - 1], mid[x + 1]);
                        dst[o + columnChannel] = (ushort)Average4(up[x - 1], up[x + 1], down[x - 1], down[x + 1]);
                    }

                    isGreen = !isGreen;
                }

                // Left and right border columns
                dst.Slice(3, 3).CopyTo(dst.Slice(0, 3));
                dst.Slice((width - 2) * 3, 3).CopyTo(dst.Slice((width - 1) * 3, 3));
            });

            // Top and bottom border rows
            int rowLength = width * 3;
            bgr.AsSpan(rowLength, rowLength).CopyTo(bgr.AsSpan(0, rowLength));
            bgr.AsSpan((height - 2) * rowLength, rowLength).CopyTo(bgr.AsSpan((height - 1) * rowLength, rowLength));

            return bgr;
        }

        /// <summary>
        /// Gray world white balance: scale blue and red so that their sums equal the green sum.
        /// </summary>
        public static (double BlueGain, double RedGain) ComputeGrayWorldGains(ushort[] bgr)
        {
            long sumB = 0, sumG = 0, sumR = 0;
            ForEachRange(bgr.Length / 3, (start, end) =>
            {
                long b = 0, g = 0, r = 0;
                for (int i = start * 3; i < end * 3; i += 3)
                {
                    b += bgr[i];
                    g += bgr[i + 1];
                    r += bgr[i + 2];
                }

                Interlocked.Add(ref sumB, b);
                Interlocked.Add(ref sumG, g);
                Interlocked.Add(ref sumR, r);
            });

            return (Gain(sumG, sumB), Gain(sumG, sumR));
        }

        /// <summary>
        /// Apply blue/red gains and convert a 16-bit B, G, R image to 8 bits.
        /// Pass gains of 1.0 to skip white balance.
        /// </summary>
        public static byte[] ToBgr8(ushort[] bgr, double blueGain, double redGain)
        {
            // The result depends only on the 16-bit input value, so compute it
            // once for every possible value and look it up per pixel
            byte[] blueTable = BuildTo8BitTable(blueGain);
            byte[] greenTable = BuildTo8BitTable(1.0);
            byte[] redTable = BuildTo8BitTable(redGain);

            var result = NewImage<byte>(bgr.Length);
            ForEachRange(bgr.Length / 3, (start, end) =>
            {
                for (int i = start * 3; i < end * 3; i += 3)
                {
                    result[i] = blueTable[bgr[i]];
                    result[i + 1] = greenTable[bgr[i + 1]];
                    result[i + 2] = redTable[bgr[i + 2]];
                }
            });
            return result;
        }

        /// <summary>Convert a 16-bit raw to 8 bits.</summary>
        public static byte[] ReduceTo8Bit(ushort[] raw16)
        {
            var result = new byte[raw16.Length];
            for (int i = 0; i < raw16.Length; i++)
                result[i] = To8Bit(raw16[i]);
            return result;
        }

        /// <summary>Convert a 16-bit raw to the given depth (LSB-aligned, rounded).</summary>
        public static ushort[] ReduceToDepth(ushort[] raw16, int depth)
        {
            double scale = 1.0 / (1 << (16 - depth));
            var result = new ushort[raw16.Length];
            for (int i = 0; i < raw16.Length; i++)
                result[i] = SaturateToUInt16(raw16[i] * scale);
            return result;
        }

        // BGGR coordinates of pixel (x, y) are (x + dx, y + dy)
        private static (int dx, int dy) OffsetFromBggr(BayerPattern pattern) => pattern switch
        {
            BayerPattern.BGGR => (0, 0),
            BayerPattern.GBRG => (1, 0),
            BayerPattern.GRBG => (0, 1),
            BayerPattern.RGGB => (1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(pattern)),
        };

        private static int Average2(int a, int b) => (a + b + 1) >> 1;

        private static int Average4(int a, int b, int c, int d) => (a + b + c + d + 2) >> 2;

        private static double Gain(long target, long sum) => sum == 0 ? 1.0 : (double)target / sum;

        // Table of To8Bit(ApplyGain(v, gain)) for every 16-bit value v
        private static byte[] BuildTo8BitTable(double gain)
        {
            var table = new byte[ushort.MaxValue + 1];
            for (int v = 0; v < table.Length; v++)
                table[v] = To8Bit(ApplyGain((ushort)v, gain));
            return table;
        }

        // Gain is applied at 16 bits, then scaled to 8 bits: two roundings, as in the OpenCV version
        private static ushort ApplyGain(ushort value, double gain) => SaturateToUInt16(value * gain);

        private static byte To8Bit(ushort value) => (byte)Math.Min(Math.Round(value / 256.0), 255);

        // Math.Round rounds half to even, matching OpenCV's cvRound
        private static ushort SaturateToUInt16(double value) => (ushort)Math.Clamp(Math.Round(value), 0, ushort.MaxValue);

        // Skips zero-initialization; callers must write every element
        private static T[] NewImage<T>(int length) => GC.AllocateUninitializedArray<T>(length);

        // Split [0, count) into chunks processed in parallel
        private static void ForEachRange(int count, Action<int, int> body)
        {
            Parallel.ForEach(Partitioner.Create(0, count), range => body(range.Item1, range.Item2));
        }
    }
}
