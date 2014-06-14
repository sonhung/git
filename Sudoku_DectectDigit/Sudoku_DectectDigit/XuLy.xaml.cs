using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using Microsoft.Phone;
using Microsoft.Phone.Tasks;
using System.Text;

namespace Sudoku_DectectDigit
{
    public partial class XuLy : PhoneApplicationPage
    {
        BitmapImage bmImage;
        WriteableBitmap[,] bitmapDeleteBlank;
        int currentRow = 0, currentCol = 0;
        public XuLy()
        {
            InitializeComponent();
            BitmapImage b = new BitmapImage();

        }    
        //private void Post()
        //{
        //    string appId = "xxx";
        //    string[] extendedPermissions = new[] { "publish_stream", "offline_access" };
            
        //    var oauth = new FacebookOAuth { ClientId = appId };
            
        //    var parameters = new Dictionary<string, object>
        //            {
        //                { "response_type", "token" },
        //                { "display", "popup" }
        //            };

        //    if (extendedPermissions != null && extendedPermissions.Length > 0)
        //    {
        //        var scope = new StringBuilder();
        //        scope.Append(string.Join(",", extendedPermissions));
        //        parameters["scope"] = scope.ToString();
        //    }

        //    var loginUrl = oauth.GetLoginUrl(parameters);
          
        //}
        private void btnXuLy_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(@"image/2 (" + int.Parse(txtSource.Text) + ").png", UriKind.Relative);
            System.Windows.Resources.StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
            bmImage = new BitmapImage();
            bmImage.SetSource(resourceInfo.Stream);
            
            WriteableBitmap bitmapSource = new WriteableBitmap(bmImage);
            WriteableBitmap bina = ToAverageBinary(bitmapSource);
            Image.Source = bina;
            currentRow = 0;
            currentCol = 0;
            bitmapDeleteBlank = new DetectDigits().Detect(bitmapSource,-1);
            
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
           
            WriteableBitmap result=bitmapDeleteBlank[currentRow,currentCol];
            ImageResult.Source = result;

           // txbQualiti.Text = Recogition.RecognitionFrom(result);
            currentRow = currentCol == 8 ? (currentRow + 1) % 9 : currentRow;
            currentCol = (currentCol + 1) % 9;

            //currentRow = currentCol == 2 ? (currentRow + 1) % 3 : currentRow;
            //currentCol = (currentCol + 1) % 3;
        }
        WriteableBitmap ToAverageBinary(WriteableBitmap grayscale)
        {
            WriteableBitmap binary =
                new WriteableBitmap(grayscale.PixelWidth,
                    grayscale.PixelHeight);


            int[] histogramData = new int[256];
            int maxCount = 0;

            //first we will determine the histogram
            //for the grayscale image
            for (int pixelIndex = 0;
                pixelIndex < grayscale.Pixels.Length;
                pixelIndex++)
            {
                byte intensity = (byte)grayscale.Pixels[pixelIndex];

                //simply add another to the count
                //for that intensity
                histogramData[intensity]++;

                if (histogramData[intensity] > maxCount)
                {
                    maxCount = histogramData[intensity];
                }
            }

            //now we need to figure out the average intensity
            long average = 0;
            for (int intensity = 0; intensity < 256; intensity++)
            {
                average += intensity * histogramData[intensity];
            }

            //this is our threshold
            average /= grayscale.Pixels.Length;

            for (int pixelIndex = 0;
                pixelIndex < grayscale.Pixels.Length;
                pixelIndex++)
            {
                byte intensity = (byte)grayscale.Pixels[pixelIndex];

                //now we’re going to set the pixels
                //greater than or equal to the average
                //to white and everything else to black
                if (intensity >= average)
                {
                    intensity = 255;
                    unchecked { binary.Pixels[pixelIndex] = (int)0xFFFFFFFF; }
                }
                else
                {
                    intensity = 0;
                    unchecked { binary.Pixels[pixelIndex] = (int)0xFF000000; }
                }
            }

            return binary;
        }            
        public void SaveToMediaLibrary(WriteableBitmap bitmap, string name, int quality)
        {
            using (var stream = new MemoryStream())
            {
                
                // Save the picture to the WP7 media library
                bitmap.SaveJpeg(
                   stream,
                   bitmap.PixelWidth,
                   bitmap.PixelHeight,
                   0,
                   quality);
                stream.Seek(0, SeekOrigin.Begin);
                new Microsoft.Xna.Framework.Media.MediaLibrary().SavePicture(name, stream);
            }
        }



    }


}