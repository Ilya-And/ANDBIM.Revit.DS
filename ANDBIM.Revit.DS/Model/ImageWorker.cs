using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ANDBIM.Revit.DS.Model
{
    public static class ImageWorker
    {
        public static BitmapImage ResizeBitmapImage(int newWidth, int newHeight, string imagePath)
        {
            string fileName = imagePath.Split('\\').Last();

            var arrayStr = imagePath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            //string folderPathToSave = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM.Revit.DS";

            //if (Directory.Exists(folderPathToSave) == false)
            //{
            //    Directory.CreateDirectory(folderPathToSave);
            //} 

            //if (File.Exists(Path.Combine(folderPathToSave, fileName)) == false)
            //{
            //    File.Copy(Path.Combine(folderPath, fileName), Path.Combine(folderPathToSave, fileName), true);
            //}

            BitmapImage original = new BitmapImage(new Uri(imagePath));

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

            if (pathToImage.ToLower().Contains(".jpeg") || pathToImage.ToLower().Contains(".jpg"))
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
