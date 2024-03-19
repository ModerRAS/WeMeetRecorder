using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = System.Drawing.Point;
using System.Runtime.Serialization;
namespace WeMeetRecorder.Utils {
    public class Detector {
        public static PaddleOCR PaddleOCR = new PaddleOCR();
        static Mat BitmapToMat(Bitmap bitmap) {
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            Mat mat = new Mat(bitmap.Height, bitmap.Width, MatType.CV_8UC4, bmpData.Scan0);
            bitmap.UnlockBits(bmpData);
            return mat;
        }
        public static Point MatchTemplateOnScreen(string file) {
            return MatchTemplateOnScreen(Cv2.ImRead(file));

        }
        public static Point MatchTemplateOnScreen(Mat templateImage) {
            Mat grayTemplateImage = new Mat();
            Cv2.CvtColor(templateImage, grayTemplateImage, ColorConversionCodes.BGR2HSV);
            // 获取屏幕截图
            Bitmap screenCapture = NativeMethod.GetScreenCapture();
            screenCapture.Save("screen.png", ImageFormat.Png);

            // 将 Bitmap 转换为 Mat
            Mat screenMat = BitmapToMat(screenCapture);
            Mat grayscreenMat = new Mat();

            Cv2.CvtColor(screenMat, grayscreenMat, ColorConversionCodes.BGR2HSV);
            // 创建结果图像
            Mat resultImage = new Mat();

            // 使用模板匹配方法
            Cv2.MatchTemplate(grayscreenMat, grayTemplateImage, resultImage, TemplateMatchModes.CCoeffNormed);

            // 获取最佳匹配位置
            double minVal, maxVal;
            OpenCvSharp.Point minLoc, maxLoc;
            Cv2.MinMaxLoc(resultImage, out minVal, out maxVal, out minLoc, out maxLoc);

            var dpi = NativeMethod.GetDPIScaling();


            return new Point() { X = (int)((maxLoc.X + templateImage.Cols / 2.0) / dpi), Y = (int)((maxLoc.Y + templateImage.Rows / 2.0) / dpi) };
        }

        public static Point GetTextFromScreen(string text) {
            var cap = NativeMethod.GetScreenCapture();
            var result = PaddleOCR.GetOcrResult(cap);
            foreach (var e in result.Regions) {
                if (e.Text.Equals(text)) {
                    return new Point((int)e.Rect.Center.X, (int)e.Rect.Center.Y);
                }
            }
            throw new NotFoundException("Not Found");
        }

        [Serializable]
        public class NotFoundException : Exception {
            public NotFoundException() {
            }

            public NotFoundException(string? message) : base(message) {
            }

            public NotFoundException(string? message, Exception? innerException) : base(message, innerException) {
            }

            protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
            }
        }
    }
}
