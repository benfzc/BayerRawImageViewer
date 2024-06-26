﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;

namespace BayerRawImageViewer
{
    internal class BayerRaw: IDisposable
    {
        private bool LoadPackedRaw14(string pathname)
        {
            byte[] data = System.IO.File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * height * 7 / 4)
                return false;

            unpackedRaw = new Mat(height, width, MatType.CV_16UC1);
            var indexer = unpackedRaw.GetGenericIndexer<ushort>();
            for (int row = 0; row < height; row++)
            {
                /* convert packed raw14 to unpacked raw16 */
                int offset = row * stride;
                for (int col = 0; col < width;)
                {
                    ushort value = 0;

                    value = (ushort)data[offset + 0];
                    value |= (ushort)((data[offset + 1] & 0x3f) << 8);
                    indexer[row, col++] = (ushort)(value << 2);

                    value = (ushort)((data[offset + 1] & 0xc0) >> 6);
                    value |= (ushort)((data[offset + 2] & 0xff) << 2);
                    value |= (ushort)((data[offset + 3] & 0x0f) << 10);
                    indexer[row, col++] = (ushort)(value << 2);

                    value = (ushort)((data[offset + 3] & 0xf0) >> 4);
                    value |= (ushort)((data[offset + 4] & 0xff) << 4);
                    value |= (ushort)((data[offset + 5] & 0x03) << 12);
                    indexer[row, col++] = (ushort)(value << 2);

                    value = (ushort)((data[offset + 5] & 0xfc) >> 2);
                    value |= (ushort)((data[offset + 6] & 0xff) << 6);
                    indexer[row, col++] = (ushort)(value << 2);


                    offset += 7;
                }
            }

            return true;
        }

        private bool LoadPackedRaw12(string pathname)
        {
            byte[] data = System.IO.File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * height * 3 / 2)
                return false;

            unpackedRaw = new Mat(height, width, MatType.CV_16UC1);
            var indexer = unpackedRaw.GetGenericIndexer<ushort>();
            for (int row = 0; row < height; row++)
            {
                /* convert packed raw12 to unpacked raw16 */
                int offset = row * stride;
                for (int col = 0; col < width;)
                {
                    ushort value = 0;

                    value = (ushort)data[offset + 0];
                    value |= (ushort)((data[offset + 1] & 0x0f) << 8);
                    indexer[row, col++] = (ushort)(value << 4);

                    value = (ushort)((data[offset + 1] & 0xf0)  >> 4);
                    value |= (ushort)((data[offset + 2] & 0xff) << 4);
                    indexer[row, col++] = (ushort)(value << 4);
                    offset += 3;
                }
            }

            return true;
        }

        private bool LoadPackedRaw10(string pathname)
        {
            byte[] data = System.IO.File.ReadAllBytes(pathname);
            if (data.Length < stride * height || data.Length < width * 5 / 4 * height)
                return false;

            unpackedRaw = new Mat(height, width, MatType.CV_16UC1);
            var indexer = unpackedRaw.GetGenericIndexer<ushort>();
            for (int row = 0; row < height; row++)
            {
                /* convert packed raw10 to unpacked raw16 */
                int offset = row * stride;
                for (int col = 0; col < width;)
                {
                    ushort value = 0;

                    value = (ushort)data[offset + 0];
                    value |= (ushort)((data[offset + 1] & 0x03) << 8);
                    indexer[row, col++] = (ushort)(value << 6);

                    value = (ushort)(data[offset + 1] >> 2);
                    value |= (ushort)((data[offset + 2] & 0x0f) << 6);
                    indexer[row, col++] = (ushort)(value << 6);

                    value = (ushort)(data[offset + 2] >> 4);
                    value |= (ushort)((data[offset + 3] & 0x3f) << 4);
                    indexer[row, col++] = (ushort)(value << 6);

                    value = (ushort)(data[offset + 3] >> 6);
                    value |= (ushort)((data[offset + 4] & 0xff) << 2);
                    indexer[row, col++] = (ushort)(value << 6);

                    offset += 5;
                }
            }

            return true;
        }

        private bool LoadUnpackedRaw(string pathname)
        {
            byte[] data = System.IO.File.ReadAllBytes(pathname);
            if (data.Length < stride * height)
                return false;

            unpackedRaw = new Mat(height, width, MatType.CV_16UC1);
            if (depth == 8)
            {
                var indexer = unpackedRaw.GetGenericIndexer<ushort>();
                for (int row = 0; row < height; row++)
                {
                    int offset = row * stride;
                    for (int col = 0; col < width;)
                    {
                        indexer[row, col++] = (ushort)(data[offset] << 8);
                        offset++;
                    }
                }
            }
            else if (depth > 8 && depth <= 16)
            {
                int shift = 16 - depth;
                var indexer = unpackedRaw.GetGenericIndexer<ushort>();
                for (int row = 0; row < height; row++)
                {
                    int offset = row * stride;
                    for (int col = 0; col < width;)
                    {
                        ushort value = 0;

                        value = (ushort)(data[offset + 0] | (data[offset + 1] << 8));
                        indexer[row, col++] = (ushort)(value << shift);

                        offset += 2;
                    }
                }
            }

            return true;
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
            }

            if (!readSuccess)
            {
                throw new ArgumentException("The depth or resolution settings might be incorrect.");
            }
        }

        public void SetBayerPattern(ColorConversionCodes pattern)
        {
            bayerPattern = pattern;
        }

        public Bitmap ToBitmap()
        {
            using var bgr16BitMat = new Mat(height, width, MatType.CV_16UC3);
            using var bgr8BitMat = new Mat(height, width, MatType.CV_8UC3);

            using var unpackedRawMinusOb = unpackedRaw - ob;
            Cv2.Demosaicing(unpackedRawMinusOb, bgr16BitMat, bayerPattern, 3);
            if (enableAwb)
            {
                // calculate WB gain
                Scalar sums = Cv2.Sum(bgr16BitMat);
                double rGain = sums[1] / (double)sums[2];
                double bGain = sums[1] / (double)sums[0];

                // apply WB gain
                Cv2.Split(bgr16BitMat, out var bgrSplit16Bit);

                using var bPlane = bgrSplit16Bit[0] * bGain;
                using var rPlane = bgrSplit16Bit[2] * rGain;
                Mat[] planes = { bPlane, bgrSplit16Bit[1], rPlane };
                using var bgr16BitWBMat = new Mat();
                Cv2.Merge(planes, bgr16BitWBMat);

                bgr16BitWBMat.ConvertTo(bgr8BitMat, MatType.CV_8UC3, 1 / 256.0);
            }
            else {
                bgr16BitMat.ConvertTo(bgr8BitMat, MatType.CV_8UC3, 1 / 256.0);
            }

            return BitmapConverter.ToBitmap(bgr8BitMat);
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
            using var convertedRaw = new Mat();
            if (_depth == 8)
            {
                unpackedRaw.ConvertTo(convertedRaw, MatType.CV_8UC1, 1 / 256.0);
                convertedRaw.GetArray(out byte[] data);
                fs.Write(data);
            }
            else
            {
                unpackedRaw.ConvertTo(convertedRaw, MatType.CV_16UC1, 1.0 / (1 << (16 - _depth)));
                convertedRaw.GetArray(out short[] data);
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    foreach (short value in data)
                        writer.Write(value);
                }
            }
        }


        ~BayerRaw()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && unpackedRaw != null)
            {
                unpackedRaw.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public enum RawType
        {
            RawType_Packed,
            RawType_MIPI,
            RawType_Unpacked
        }

        Mat unpackedRaw = null!;
        private int width;
        private int height;
        private int stride;
        private int depth;
        private RawType type;
        private ColorConversionCodes bayerPattern = ColorConversionCodes.BayerBG2RGB;
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
