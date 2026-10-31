using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BayerRawImageViewer
{
    /// <summary>
    /// A Bayer raw image loaded from file. Pixels are kept as 16-bit values,
    /// MSB-aligned (a 10-bit value v is stored as v &lt;&lt; 6), one per pixel, row-major.
    ///
    /// Each loader decodes one group of pixels at a time. Bit diagrams use letters
    /// for pixels in file order (A is the 1st pixel of a group, B the 2nd, ...);
    /// A0 is the LSB of pixel A.
    /// </summary>
    internal class BayerRaw
    {
        /// <summary>
        /// Packed RAW14: all pixels form one continuous LSB-first bit stream.
        /// 4 pixels fill 7 bytes exactly.
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
                int rowStart = row * width;
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    // 7 bytes = one 56-bit little-endian word; pixel i is bits [14*i, 14*i + 13]
                    ulong word = LittleEndian(data, offset, 7);
                    int pixelA = (int)(word >> 0) & 0x3fff;
                    int pixelB = (int)(word >> 14) & 0x3fff;
                    int pixelC = (int)(word >> 28) & 0x3fff;
                    int pixelD = (int)(word >> 42) & 0x3fff;

                    unpackedRaw[rowStart + col] = MsbAlign(pixelA);
                    unpackedRaw[rowStart + col + 1] = MsbAlign(pixelB);
                    unpackedRaw[rowStart + col + 2] = MsbAlign(pixelC);
                    unpackedRaw[rowStart + col + 3] = MsbAlign(pixelD);

                    offset += 7;
                }
            }

            return true;
        }

        /// <summary>
        /// Packed RAW12: all pixels form one continuous LSB-first bit stream.
        /// 2 pixels fill 3 bytes exactly.
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
                int rowStart = row * width;
                int offset = row * stride;
                for (int col = 0; col < width; col += 2)
                {
                    // 3 bytes = one 24-bit little-endian word; pixel i is bits [12*i, 12*i + 11]
                    ulong word = LittleEndian(data, offset, 3);
                    int pixelA = (int)(word >> 0) & 0xfff;
                    int pixelB = (int)(word >> 12) & 0xfff;

                    unpackedRaw[rowStart + col] = MsbAlign(pixelA);
                    unpackedRaw[rowStart + col + 1] = MsbAlign(pixelB);

                    offset += 3;
                }
            }

            return true;
        }

        /// <summary>
        /// Packed RAW10: all pixels form one continuous LSB-first bit stream.
        /// 4 pixels fill 5 bytes exactly.
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
                int rowStart = row * width;
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    // 5 bytes = one 40-bit little-endian word; pixel i is bits [10*i, 10*i + 9]
                    ulong word = LittleEndian(data, offset, 5);
                    int pixelA = (int)(word >> 0) & 0x3ff;
                    int pixelB = (int)(word >> 10) & 0x3ff;
                    int pixelC = (int)(word >> 20) & 0x3ff;
                    int pixelD = (int)(word >> 30) & 0x3ff;

                    unpackedRaw[rowStart + col] = MsbAlign(pixelA);
                    unpackedRaw[rowStart + col + 1] = MsbAlign(pixelB);
                    unpackedRaw[rowStart + col + 2] = MsbAlign(pixelC);
                    unpackedRaw[rowStart + col + 3] = MsbAlign(pixelD);

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
                int rowStart = row * width;
                int offset = row * stride;
                if (depth == 8)
                {
                    for (int col = 0; col < width; col++)
                        unpackedRaw[rowStart + col] = MsbAlign(data[offset + col]);
                }
                else if (depth > 8 && depth <= 16)
                {
                    for (int col = 0; col < width; col++)
                    {
                        int pixel = (int)LittleEndian(data, offset + 2 * col, 2);
                        unpackedRaw[rowStart + col] = MsbAlign(pixel);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// MIPI CSI-2 RAW10: 4 pixels in 5 bytes. Bytes 0-3 hold the 8 MSBs of
        /// pixels A-D, byte 4 packs the 2 LSBs of all four.
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
                int rowStart = row * width;
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    // 2 LSBs of pixel i are bits [2*i, 2*i + 1] of byte 4
                    int lsbs = data[offset + 4];
                    int pixelA = (data[offset + 0] << 2) | ((lsbs >> 0) & 0x3);
                    int pixelB = (data[offset + 1] << 2) | ((lsbs >> 2) & 0x3);
                    int pixelC = (data[offset + 2] << 2) | ((lsbs >> 4) & 0x3);
                    int pixelD = (data[offset + 3] << 2) | ((lsbs >> 6) & 0x3);

                    unpackedRaw[rowStart + col] = MsbAlign(pixelA);
                    unpackedRaw[rowStart + col + 1] = MsbAlign(pixelB);
                    unpackedRaw[rowStart + col + 2] = MsbAlign(pixelC);
                    unpackedRaw[rowStart + col + 3] = MsbAlign(pixelD);

                    offset += 5;
                }
            }

            return true;
        }

        /// <summary>
        /// MIPI CSI-2 RAW12: 2 pixels in 3 bytes. Bytes 0-1 hold the 8 MSBs of
        /// pixels A-B, byte 2 packs the 4 LSBs of both.
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
                int rowStart = row * width;
                int offset = row * stride;
                for (int col = 0; col < width; col += 2)
                {
                    // 4 LSBs of pixel i are bits [4*i, 4*i + 3] of byte 2
                    int lsbs = data[offset + 2];
                    int pixelA = (data[offset + 0] << 4) | ((lsbs >> 0) & 0xf);
                    int pixelB = (data[offset + 1] << 4) | ((lsbs >> 4) & 0xf);

                    unpackedRaw[rowStart + col] = MsbAlign(pixelA);
                    unpackedRaw[rowStart + col + 1] = MsbAlign(pixelB);

                    offset += 3;
                }
            }

            return true;
        }

        /// <summary>
        /// MIPI CSI-2 RAW14: 4 pixels in 7 bytes. Bytes 0-3 hold the 8 MSBs of
        /// pixels A-D, bytes 4-6 pack the 6 LSBs of all four.
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
                int rowStart = row * width;
                int offset = row * stride;
                for (int col = 0; col < width; col += 4)
                {
                    // Bytes 4-6 = one 24-bit little-endian word; 6 LSBs of pixel i are bits [6*i, 6*i + 5]
                    int lsbs = (int)LittleEndian(data, offset + 4, 3);
                    int pixelA = (data[offset + 0] << 6) | ((lsbs >> 0) & 0x3f);
                    int pixelB = (data[offset + 1] << 6) | ((lsbs >> 6) & 0x3f);
                    int pixelC = (data[offset + 2] << 6) | ((lsbs >> 12) & 0x3f);
                    int pixelD = (data[offset + 3] << 6) | ((lsbs >> 18) & 0x3f);

                    unpackedRaw[rowStart + col] = MsbAlign(pixelA);
                    unpackedRaw[rowStart + col + 1] = MsbAlign(pixelB);
                    unpackedRaw[rowStart + col + 2] = MsbAlign(pixelC);
                    unpackedRaw[rowStart + col + 3] = MsbAlign(pixelD);

                    offset += 7;
                }
            }

            return true;
        }

        // Combine count bytes starting at offset into one integer, first byte as the least significant
        private static ulong LittleEndian(byte[] data, int offset, int count)
        {
            ulong word = 0;
            for (int i = count - 1; i >= 0; i--)
                word = (word << 8) | data[offset + i];
            return word;
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
