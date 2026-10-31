using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BayerRawImageViewer
{
    /// <summary>
    /// A Bayer raw image loaded from file. Pixels are kept as 16-bit values,
    /// MSB-aligned (a 10-bit value v is stored as v &lt;&lt; 6), one per pixel, row-major.
    ///
    /// Bit diagrams below use letters for pixels in file order: A is the 1st pixel
    /// of a group, B the 2nd, and so on; A0 is the LSB of pixel A.
    /// </summary>
    internal class BayerRaw
    {
        /// <summary>
        /// Packed RAW14: a continuous LSB-first bit stream, 4 pixels in 7 bytes.
        ///   bit     7   6   5   4   3   2   1   0
        ///   byte 0  A7  A6  A5  A4  A3  A2  A1  A0
        ///   byte 1  B1  B0  A13 A12 A11 A10 A9  A8
        ///   byte 2  B9  B8  B7  B6  B5  B4  B3  B2
        ///   byte 3  C3  C2  C1  C0  B13 B12 B11 B10
        ///   byte 4  C11 C10 C9  C8  C7  C6  C5  C4
        ///   byte 5  D5  D4  D3  D2  D1  D0  C13 C12
        ///   byte 6  D13 D12 D11 D10 D9  D8  D7  D6
        /// </summary>
        private bool LoadPackedRaw14(string pathname)
        {
            byte[] data = File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * height * 7 / 4)
                return false;

            unpackedRaw = new ushort[width * height];
            for (int row = 0; row < height; row++)
            {
                Span<ushort> dst = unpackedRaw.AsSpan(row * width, width);
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    ReadOnlySpan<byte> b = data.AsSpan(offset, 7);

                    int pixelA = b[0] | (b[1] & 0x3f) << 8;
                    int pixelB = b[1] >> 6 | b[2] << 2 | (b[3] & 0x0f) << 10;
                    int pixelC = b[3] >> 4 | b[4] << 4 | (b[5] & 0x03) << 12;
                    int pixelD = b[5] >> 2 | b[6] << 6;

                    dst[col] = MsbAlign(pixelA);
                    dst[col + 1] = MsbAlign(pixelB);
                    dst[col + 2] = MsbAlign(pixelC);
                    dst[col + 3] = MsbAlign(pixelD);

                    offset += 7;
                }
            }

            return true;
        }

        /// <summary>
        /// Packed RAW12: a continuous LSB-first bit stream, 2 pixels in 3 bytes.
        ///   bit     7   6   5   4   3   2   1   0
        ///   byte 0  A7  A6  A5  A4  A3  A2  A1  A0
        ///   byte 1  B3  B2  B1  B0  A11 A10 A9  A8
        ///   byte 2  B11 B10 B9  B8  B7  B6  B5  B4
        /// </summary>
        private bool LoadPackedRaw12(string pathname)
        {
            byte[] data = File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * height * 3 / 2)
                return false;

            unpackedRaw = new ushort[width * height];
            for (int row = 0; row < height; row++)
            {
                Span<ushort> dst = unpackedRaw.AsSpan(row * width, width);
                int offset = row * stride;
                for (int col = 0; col < width; col += 2)
                {
                    ReadOnlySpan<byte> b = data.AsSpan(offset, 3);

                    int pixelA = b[0] | (b[1] & 0x0f) << 8;
                    int pixelB = b[1] >> 4 | b[2] << 4;

                    dst[col] = MsbAlign(pixelA);
                    dst[col + 1] = MsbAlign(pixelB);

                    offset += 3;
                }
            }

            return true;
        }

        /// <summary>
        /// Packed RAW10: a continuous LSB-first bit stream, 4 pixels in 5 bytes.
        ///   bit     7   6   5   4   3   2   1   0
        ///   byte 0  A7  A6  A5  A4  A3  A2  A1  A0
        ///   byte 1  B5  B4  B3  B2  B1  B0  A9  A8
        ///   byte 2  C3  C2  C1  C0  B9  B8  B7  B6
        ///   byte 3  D1  D0  C9  C8  C7  C6  C5  C4
        ///   byte 4  D9  D8  D7  D6  D5  D4  D3  D2
        /// </summary>
        private bool LoadPackedRaw10(string pathname)
        {
            byte[] data = File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * 5 / 4 * height)
                return false;

            unpackedRaw = new ushort[width * height];
            for (int row = 0; row < height; row++)
            {
                Span<ushort> dst = unpackedRaw.AsSpan(row * width, width);
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    ReadOnlySpan<byte> b = data.AsSpan(offset, 5);

                    int pixelA = b[0] | (b[1] & 0x03) << 8;
                    int pixelB = b[1] >> 2 | (b[2] & 0x0f) << 6;
                    int pixelC = b[2] >> 4 | (b[3] & 0x3f) << 4;
                    int pixelD = b[3] >> 6 | b[4] << 2;

                    dst[col] = MsbAlign(pixelA);
                    dst[col + 1] = MsbAlign(pixelB);
                    dst[col + 2] = MsbAlign(pixelC);
                    dst[col + 3] = MsbAlign(pixelD);

                    offset += 5;
                }
            }

            return true;
        }

        /// <summary>
        /// Unpacked raw.
        /// 8-bit: one byte per pixel.
        /// 9 to 16-bit: one little-endian 16-bit word per pixel, value in the low bits.
        ///   e.g. 10-bit:
        ///   bit     7   6   5   4   3   2   1   0
        ///   byte 0  A7  A6  A5  A4  A3  A2  A1  A0
        ///   byte 1  0   0   0   0   0   0   A9  A8
        /// </summary>
        private bool LoadUnpackedRaw(string pathname)
        {
            byte[] data = File.ReadAllBytes(pathname);
            if (data.Length < stride * height)
                return false;

            unpackedRaw = new ushort[width * height];
            for (int row = 0; row < height; row++)
            {
                Span<ushort> dst = unpackedRaw.AsSpan(row * width, width);
                int offset = row * stride;
                if (depth == 8)
                {
                    for (int col = 0; col < width; col++)
                        dst[col] = MsbAlign(data[offset + col]);
                }
                else if (depth > 8 && depth <= 16)
                {
                    for (int col = 0; col < width; col++)
                    {
                        int pixel = data[offset + 2 * col] | data[offset + 2 * col + 1] << 8;
                        dst[col] = MsbAlign(pixel);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// MIPI CSI-2 RAW10: 4 pixels in 5 bytes. The 8 MSBs of each pixel come
        /// first, the last byte collects the 2 LSBs of all four.
        ///   bit     7   6   5   4   3   2   1   0
        ///   byte 0  A9  A8  A7  A6  A5  A4  A3  A2
        ///   byte 1  B9  B8  B7  B6  B5  B4  B3  B2
        ///   byte 2  C9  C8  C7  C6  C5  C4  C3  C2
        ///   byte 3  D9  D8  D7  D6  D5  D4  D3  D2
        ///   byte 4  D1  D0  C1  C0  B1  B0  A1  A0
        /// </summary>
        private bool LoadMIPIRaw10(string pathname)
        {
            byte[] data = File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * 5 / 4 * height)
                return false;

            unpackedRaw = new ushort[width * height];
            for (int row = 0; row < height; row++)
            {
                Span<ushort> dst = unpackedRaw.AsSpan(row * width, width);
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    ReadOnlySpan<byte> b = data.AsSpan(offset, 5);

                    int pixelA = b[0] << 2 | (b[4] & 0x03);
                    int pixelB = b[1] << 2 | (b[4] & 0x0c) >> 2;
                    int pixelC = b[2] << 2 | (b[4] & 0x30) >> 4;
                    int pixelD = b[3] << 2 | (b[4] & 0xc0) >> 6;

                    dst[col] = MsbAlign(pixelA);
                    dst[col + 1] = MsbAlign(pixelB);
                    dst[col + 2] = MsbAlign(pixelC);
                    dst[col + 3] = MsbAlign(pixelD);

                    offset += 5;
                }
            }

            return true;
        }

        /// <summary>
        /// MIPI CSI-2 RAW12: 2 pixels in 3 bytes. The 8 MSBs of each pixel come
        /// first, the last byte collects the 4 LSBs of both.
        ///   bit     7   6   5   4   3   2   1   0
        ///   byte 0  A11 A10 A9  A8  A7  A6  A5  A4
        ///   byte 1  B11 B10 B9  B8  B7  B6  B5  B4
        ///   byte 2  B3  B2  B1  B0  A3  A2  A1  A0
        /// </summary>
        private bool LoadMIPIRaw12(string pathname)
        {
            byte[] data = File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * 3 / 2 * height)
                return false;

            unpackedRaw = new ushort[width * height];
            for (int row = 0; row < height; row++)
            {
                Span<ushort> dst = unpackedRaw.AsSpan(row * width, width);
                int offset = row * stride;
                for (int col = 0; col < width; col += 2)
                {
                    ReadOnlySpan<byte> b = data.AsSpan(offset, 3);

                    int pixelA = b[0] << 4 | (b[2] & 0x0f);
                    int pixelB = b[1] << 4 | (b[2] & 0xf0) >> 4;

                    dst[col] = MsbAlign(pixelA);
                    dst[col + 1] = MsbAlign(pixelB);

                    offset += 3;
                }
            }

            return true;
        }

        /// <summary>
        /// MIPI CSI-2 RAW14: 4 pixels in 7 bytes. The 8 MSBs of each pixel come
        /// first, the last 3 bytes collect the 6 LSBs of all four.
        ///   bit     7   6   5   4   3   2   1   0
        ///   byte 0  A13 A12 A11 A10 A9  A8  A7  A6
        ///   byte 1  B13 B12 B11 B10 B9  B8  B7  B6
        ///   byte 2  C13 C12 C11 C10 C9  C8  C7  C6
        ///   byte 3  D13 D12 D11 D10 D9  D8  D7  D6
        ///   byte 4  B1  B0  A5  A4  A3  A2  A1  A0
        ///   byte 5  C3  C2  C1  C0  B5  B4  B3  B2
        ///   byte 6  D5  D4  D3  D2  D1  D0  C5  C4
        /// </summary>
        private bool LoadMIPIRaw14(string pathname)
        {
            byte[] data = File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * 7 / 4 * height)
                return false;

            unpackedRaw = new ushort[width * height];
            for (int row = 0; row < height; row++)
            {
                Span<ushort> dst = unpackedRaw.AsSpan(row * width, width);
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    ReadOnlySpan<byte> b = data.AsSpan(offset, 7);

                    int pixelA = b[0] << 6 | (b[4] & 0x3f);
                    int pixelB = b[1] << 6 | (b[4] & 0xc0) >> 6 | (b[5] & 0x0f) << 2;
                    int pixelC = b[2] << 6 | (b[5] & 0xf0) >> 4 | (b[6] & 0x03) << 4;
                    int pixelD = b[3] << 6 | (b[6] & 0xfc) >> 2;

                    dst[col] = MsbAlign(pixelA);
                    dst[col + 1] = MsbAlign(pixelB);
                    dst[col + 2] = MsbAlign(pixelC);
                    dst[col + 3] = MsbAlign(pixelD);

                    offset += 7;
                }
            }

            return true;
        }

        // Scale a value of the input depth to 16 bits
        private ushort MsbAlign(int value)
        {
            return (ushort)(value << (16 - depth));
        }

        public BayerRaw(string pathname, int _width, int _height, int _stride, int _depth, RawType _type)
        {
            width = _width; height = _height; stride = _stride; depth = _depth; type = _type;
            bool readSuccess = false;

            if (!File.Exists(pathname))
            {
                throw new ArgumentException("File does not exist.");
            }

            switch (type)
            {
                case RawType.RawType_Packed:
                    switch (depth)
                    {
                        case 8:
                            // the 8-bit unpacked raw and packed raw are exactly the same
                            readSuccess = LoadUnpackedRaw(pathname); break;
                        case 10:
                            readSuccess = LoadPackedRaw10(pathname); break;
                        case 12:
                            readSuccess = LoadPackedRaw12(pathname); break;
                        case 14:
                            readSuccess = LoadPackedRaw14(pathname); break;
                    }
                    
                    break;
                case RawType.RawType_Unpacked:
                    readSuccess = LoadUnpackedRaw(pathname);
                    break;
                case RawType.RawType_MIPI:
                    switch (depth)
                    {
                        case 8:
                            readSuccess = LoadUnpackedRaw(pathname);
                            break;
                        case 10:
                            readSuccess = LoadMIPIRaw10(pathname);
                            break;
                        case 12:
                            readSuccess = LoadMIPIRaw12(pathname);
                            break;
                        case 14:
                            readSuccess = LoadMIPIRaw14(pathname);
                            break;
                    }
                    break;
            }

            if (!readSuccess)
            {
                throw new ArgumentException("The depth or resolution settings might be incorrect.");
            }
        }

        public void SetBayerPattern(BayerPattern pattern)
        {
            bayerPattern = pattern;
        }

        public Bitmap ToBitmap()
        {
            ushort[] raw = ImageProcessing.SubtractBlackLevel(unpackedRaw, ob);
            ushort[] bgr = ImageProcessing.DemosaicBilinear(raw, width, height, bayerPattern);

            var (blueGain, redGain) = enableAwb ? ImageProcessing.ComputeGrayWorldGains(bgr) : (1.0, 1.0);
            byte[] bgr8 = ImageProcessing.ToBgr8(bgr, blueGain, redGain);

            return CreateBitmap(bgr8, width, height);
        }

        public void saveUnpackRaw(int _depth, string _pathname)
        {
            switch (_depth)
            {
                case 8:
                case 10:
                case 12:
                case 14:
                case 16:
                    break;
                default:
                    return;
            }

            using var fs = File.Open(_pathname, FileMode.Create);
            if (_depth == 8)
            {
                fs.Write(ImageProcessing.ReduceTo8Bit(unpackedRaw));
            }
            else
            {
                // One 16-bit word per pixel, little-endian (the byte order of Windows on x86/x64/ARM64)
                ushort[] data = ImageProcessing.ReduceToDepth(unpackedRaw, _depth);
                fs.Write(MemoryMarshal.AsBytes(data.AsSpan()));
            }
        }

        // bgr8 holds tightly packed B, G, R bytes; Format24bppRgb uses the same
        // byte order but pads each row to a multiple of 4 bytes, so copy row by row
        private static Bitmap CreateBitmap(byte[] bgr8, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                int rowBytes = width * 3;
                for (int y = 0; y < height; y++)
                    Marshal.Copy(bgr8, y * rowBytes, bd.Scan0 + y * bd.Stride, rowBytes);
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return bmp;
        }


        public enum RawType
        {
            RawType_Packed,
            RawType_MIPI,
            RawType_Unpacked
        }

        private ushort[] unpackedRaw = Array.Empty<ushort>();
        private int width;
        private int height;
        private int stride;
        private int depth;
        private RawType type;
        private BayerPattern bayerPattern = BayerPattern.BGGR;
        private bool enableAwb = false;

        public bool EnableAwb
        {
            set => enableAwb = value;
        }

        private int ob = 0;
        public int OB
        {
            set 
            {
                // Since we use 16-bit processing in this class, we need to convert
                // the OB value from the input image's bit depth to 16-bit.
                int shift = 16 - depth;
                ob = value << shift;
            }
        }
    }
}
