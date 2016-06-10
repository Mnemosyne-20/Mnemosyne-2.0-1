using AForge.Imaging;
using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace Mnemosyne_Of_Mine
{
    /// <summary>
    /// Image comparison class to match and rate if bitmapped images are similar.
    /// will actually use to get reposts on /r/programmerhumor eventually, natm tho
    /// </summary>
    public static class ImageComparer
    {
        private static void DownloadImage(string uri)
        {
            var name = Path.GetFileName(new Uri(uri).LocalPath);
            string fileName = Environment.CurrentDirectory + "CheckImage.jpg";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download oit
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
            }
        }
        // The file extension for the generated Bitmap files
        private const string BitMapExtension = ".bmp";

        /// <summary>
        /// Compares the images.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="targetImage">The target image.</param>
        /// <param name="compareLevel">The compare level.</param>
        /// <param name="filepath">The filepath.</param>
        /// <param name="similarityThreshold">The similarity threshold.</param>
        /// <returns>Boolean result</returns>
        public static bool CompareImages(string image, string targetImage, double compareLevel, string filepath, float similarityThreshold)
        {
            // Load images into bitmaps
            var imageOne = new Bitmap(image);
            var imageTwo = new Bitmap(targetImage);

            var newBitmap1 = ChangePixelFormat(new Bitmap(imageOne), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var newBitmap2 = ChangePixelFormat(new Bitmap(imageTwo), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            newBitmap1 = SaveBitmapToFile(newBitmap1, filepath, image, BitMapExtension);
            newBitmap2 = SaveBitmapToFile(newBitmap2, filepath, targetImage, BitMapExtension);

            // Setup the AForge library
            var tm = new ExhaustiveTemplateMatching(similarityThreshold);

            // Process the images
            var results = tm.ProcessImage(newBitmap1, newBitmap2);

            // Compare the results, 0 indicates no match so return false
            if (results.Length <= 0)
            {
                return false;
            }

            // Return true if similarity score is equal or greater than the comparison level
            var match = results[0].Similarity >= compareLevel;

            return match;
        }

        /// <summary>
        /// Saves the bitmap automatic file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="filepath">The filepath.</param>
        /// <param name="name">The name.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>Bitmap image</returns>
        private static Bitmap SaveBitmapToFile(Bitmap image, string filepath, string name, string extension)
        {
            var savePath = string.Concat(filepath, "\\", Path.GetFileNameWithoutExtension(name), extension);

            image.Save(savePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return image;
        }

        /// <summary>
        /// Change the pixel format of the bitmap image
        /// </summary>
        /// <param name="inputImage">Bitmapped image</param>
        /// <param name="newFormat">Bitmap format - 24bpp</param>
        /// <returns>Bitmap image</returns>
        private static Bitmap ChangePixelFormat(Bitmap inputImage, System.Drawing.Imaging.PixelFormat newFormat)
        {
            return (inputImage.Clone(new Rectangle(0, 0, inputImage.Width, inputImage.Height), newFormat));
        }
    }
}
