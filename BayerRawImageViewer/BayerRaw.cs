using System;
using System.Collections.Generic;
using System.Formats.Asn1;
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

        public Bitmap ToBitmap()
        {
            using var bgr16BitMat = new Mat(height, width, MatType.CV_16UC3);
            using var bgr8BitMat = new Mat(height, width, MatType.CV_8UC3);

            Cv2.Demosaicing(unpackedRaw, bgr16BitMat, bayerPattern, 3);
            bgr16BitMat.ConvertTo(bgr8BitMat, MatType.CV_8UC3, 1 / 256.0);
            return BitmapConverter.ToBitmap(bgr8BitMat);
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
    }
}
