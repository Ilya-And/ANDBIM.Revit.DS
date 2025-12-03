using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ANDBIM.Revit.DS.Model
{
    public static class ImageWorker
    {
        public static BitmapImage ResizeBitmapImage(BitmapImage original, int newWidth, int newHeight, string imagePath)
        {
            var scaledBitmap = new TransformedBitmap();
            scaledBitmap.BeginInit();
            scaledBitmap.Source = original;

            // Создаем ScaleTransform
            ScaleTransform scale = new ScaleTransform(
                (double)newWidth / original.PixelWidth,
                (double)newHeight / original.PixelHeight,
                0, 0);

            scaledBitmap.Transform = scale;
            scaledBitmap.EndInit();

            // Конвертируем обратно в BitmapImage
            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.StreamSource = BitmapSourceToStream(scaledBitmap, imagePath);
            result.EndInit();

            return result;
        }

        private static MemoryStream BitmapSourceToStream(BitmapSource bitmapSource, string pathToImage)
        {
            PngBitmapEncoder encoderPNG = new PngBitmapEncoder();
            JpegBitmapEncoder encoderJPEG = new JpegBitmapEncoder();
            var stream = new MemoryStream();

            if (pathToImage.ToLower().Contains(".jpeg"))
            {
                encoderJPEG.Frames.Add(BitmapFrame.Create(bitmapSource));

                encoderJPEG.Save(stream);
                stream.Position = 0;
            }
            else if (pathToImage.ToLower().Contains(".png"))
            {
                encoderPNG.Frames.Add(BitmapFrame.Create(bitmapSource));

                encoderPNG.Save(stream);
                stream.Position = 0;
            }

            return stream;
        }
    }
}
