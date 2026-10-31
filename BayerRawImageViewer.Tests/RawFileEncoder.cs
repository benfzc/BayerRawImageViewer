using static BayerRawImageViewer.BayerRaw;

namespace BayerRawImageViewer.Tests
{
    /// <summary>
    /// Encodes N-bit pixel values into the on-disk raw layouts that BayerRaw reads.
    /// Written independently from the loaders (bit by bit) so it can verify them.
    /// </summary>
    internal static class RawFileEncoder
    {
        public static int RowBytes(int width, int depth, RawType type)
        {
            if (depth == 8)
                return width;
            if (type == RawType.RawType_Unpacked)
                return width * 2;
            return width * depth / 8;
        }

        public static byte[] Encode(ushort[] pixels, int width, int height, int depth, RawType type, int stride)
        {
            var file = new byte[stride * height];
            // Fill the padding with a recognizable pattern; it must never be decoded as pixels
            Array.Fill(file, (byte)0xAA);

            for (int row = 0; row < height; row++)
            {
                var src = pixels.AsSpan(row * width, width);
                var dst = file.AsSpan(row * stride, RowBytes(width, depth, type));
                dst.Clear();

                if (depth == 8)
                    EncodeRow8(src, dst);
                else if (type == RawType.RawType_Unpacked)
                    EncodeRowUnpacked16(src, dst);
                else if (type == RawType.RawType_Packed)
                    EncodeRowPacked(src, dst, depth);
                else
                    EncodeRowMipi(src, dst, depth);
            }

            return file;
        }

        private static void EncodeRow8(ReadOnlySpan<ushort> src, Span<byte> dst)
        {
            for (int i = 0; i < src.Length; i++)
                dst[i] = (byte)src[i];
        }

        // One little-endian 16-bit word per pixel, value not shifted
        private static void EncodeRowUnpacked16(ReadOnlySpan<ushort> src, Span<byte> dst)
        {
            for (int i = 0; i < src.Length; i++)
            {
                dst[2 * i] = (byte)(src[i] & 0xff);
                dst[2 * i + 1] = (byte)(src[i] >> 8);
            }
        }

        // Pixels form one continuous LSB-first bit stream
        private static void EncodeRowPacked(ReadOnlySpan<ushort> src, Span<byte> dst, int depth)
        {
            int bitPos = 0;
            foreach (ushort value in src)
                WriteBitsLsbFirst(dst, ref bitPos, value, depth);
        }

        // MIPI CSI-2: per group, the 8 MSBs of each pixel come first,
        // followed by the remaining LSBs of all pixels as an LSB-first bit stream
        private static void EncodeRowMipi(ReadOnlySpan<ushort> src, Span<byte> dst, int depth)
        {
            int lowBits = depth - 8;
            int groupPixels = depth == 12 ? 2 : 4;
            int groupBytes = groupPixels * depth / 8;

            for (int g = 0; g * groupPixels < src.Length; g++)
            {
                var pixels = src.Slice(g * groupPixels, groupPixels);
                var group = dst.Slice(g * groupBytes, groupBytes);

                for (int i = 0; i < groupPixels; i++)
                    group[i] = (byte)(pixels[i] >> lowBits);

                var lowPart = group.Slice(groupPixels);
                int bitPos = 0;
                for (int i = 0; i < groupPixels; i++)
                    WriteBitsLsbFirst(lowPart, ref bitPos, pixels[i] & ((1 << lowBits) - 1), lowBits);
            }
        }

        private static void WriteBitsLsbFirst(Span<byte> dst, ref int bitPos, int value, int bits)
        {
            for (int b = 0; b < bits; b++, bitPos++)
            {
                if (((value >> b) & 1) != 0)
                    dst[bitPos >> 3] |= (byte)(1 << (bitPos & 7));
            }
        }
    }
}
