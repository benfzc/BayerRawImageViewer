using System;
using System.Collections.Generic;
using System.Drawing;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace BayerRawImageViewer
{
    internal class BayerRaw: IDisposable
    {
        public BayerRaw(string pathname, int _width, int _height, int _stride, int _depth, int _type)
        {
            width = _width; height = _height; stride = _stride; depth = _depth; type = _type;

            byte[] data = System.IO.File.ReadAllBytes(pathname);
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
        }

        public void SetBayerPattern(ColorConversionCodes pattern)
        {
            bayerPattern = pattern;
        }

        public void toggleAWB(bool _enableAwb, int _ob)
        {
            if (_enableAwb)
            {
                ob = _ob;
            }
            else
            {
                ob = 0;
            }
            enableAwb = _enableAwb;

        }

        public Bitmap ToBitmap()
        {
            using var bgr16BitMat = new Mat(height, width, MatType.CV_16UC3);
            using var bgr8BitMat = new Mat(height, width, MatType.CV_8UC3);

            if (enableAwb)
            {
                using var unpackedRawMinusOb = unpackedRaw - ob;
                Cv2.Demosaicing(unpackedRawMinusOb, bgr16BitMat, bayerPattern, 3);

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
                Cv2.Demosaicing(unpackedRaw, bgr16BitMat, bayerPattern, 3);
                bgr16BitMat.ConvertTo(bgr8BitMat, MatType.CV_8UC3, 1 / 256.0);
            }

            return BitmapConverter.ToBitmap(bgr8BitMat);
        }

        public void saveUnpackRaw(int _depth, string _pathname)
        {
            if (unpackedRaw == null) return;
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
                unpackedRaw = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }




        Mat? unpackedRaw;
        private int width;
        private int height;
        private int stride;
        private int depth;
        private int type;
        private ColorConversionCodes bayerPattern = ColorConversionCodes.BayerBG2RGB;
        private bool enableAwb = false;
        private int ob = 0;
    }
}
