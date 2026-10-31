using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace BayerRawImageViewer.Tests
{
    /// <summary>
    /// Reference pipeline built on OpenCV. It reproduces the original OpenCV-based
    /// BayerRaw implementation and serves as the expected output in tests.
    /// </summary>
    internal static class OpenCvReference
    {
        // UI label -> OpenCV code, exactly as MainForm.getBayerSelection() maps them
        public static ColorConversionCodes ToOpenCvCode(string uiLabel) => uiLabel switch
        {
            "BG" => ColorConversionCodes.BayerBG2RGB,
            "RG" => ColorConversionCodes.BayerRG2RGB,
            "GR" => ColorConversionCodes.BayerGR2RGB,
            "GB" => ColorConversionCodes.BayerGB2RGB,
            _ => throw new ArgumentException($"Unknown Bayer label {uiLabel}"),
        };

        /// <param name="raw16">Unpacked raw, MSB-aligned to 16 bits</param>
        /// <param name="ob16">Black level, already scaled to 16 bits</param>
        public static Bitmap ToBitmap(ushort[] raw16, int width, int height, string uiLabel, bool enableAwb, int ob16)
        {
            using var unpackedRaw = ToMat(raw16, width, height);
            using var bgr16BitMat = new Mat(height, width, MatType.CV_16UC3);
            using var bgr8BitMat = new Mat(height, width, MatType.CV_8UC3);

            using var unpackedRawMinusOb = unpackedRaw - ob16;
            Cv2.Demosaicing(unpackedRawMinusOb, bgr16BitMat, ToOpenCvCode(uiLabel), 3);
            if (enableAwb)
            {
                Scalar sums = Cv2.Sum(bgr16BitMat);
                double rGain = sums[1] / (double)sums[2];
                double bGain = sums[1] / (double)sums[0];

                Cv2.Split(bgr16BitMat, out var bgrSplit16Bit);

                using var bPlane = bgrSplit16Bit[0] * bGain;
                using var rPlane = bgrSplit16Bit[2] * rGain;
                Mat[] planes = { bPlane, bgrSplit16Bit[1], rPlane };
                using var bgr16BitWBMat = new Mat();
                Cv2.Merge(planes, bgr16BitWBMat);

                bgr16BitWBMat.ConvertTo(bgr8BitMat, MatType.CV_8UC3, 1 / 256.0);
                foreach (var plane in bgrSplit16Bit)
                    plane.Dispose();
            }
            else
            {
                bgr16BitMat.ConvertTo(bgr8BitMat, MatType.CV_8UC3, 1 / 256.0);
            }

            return BitmapConverter.ToBitmap(bgr8BitMat);
        }

        /// <summary>Expected file content of BayerRaw.saveUnpackRaw().</summary>
        public static byte[] UnpackedRawFile(ushort[] raw16, int width, int height, int outputDepth)
        {
            using var unpackedRaw = ToMat(raw16, width, height);
            using var convertedRaw = new Mat();
            if (outputDepth == 8)
            {
                unpackedRaw.ConvertTo(convertedRaw, MatType.CV_8UC1, 1 / 256.0);
                convertedRaw.GetArray(out byte[] data);
                return data;
            }

            unpackedRaw.ConvertTo(convertedRaw, MatType.CV_16UC1, 1.0 / (1 << (16 - outputDepth)));
            convertedRaw.GetArray(out ushort[] words);
            var bytes = new byte[words.Length * 2];
            Buffer.BlockCopy(words, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static Mat ToMat(ushort[] raw16, int width, int height)
        {
            var mat = new Mat(height, width, MatType.CV_16UC1);
            mat.SetArray(raw16);
            return mat;
        }
    }
}
