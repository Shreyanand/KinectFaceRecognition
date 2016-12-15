//Copyright of Nayi Disha Studios Pvt. Ltd.
//Permission granted to Shrey Anand for submission of college applications.
//All rights reserved by Nayi Disha Studios Pvt. Ltd.
//Permissions for all dependencies covered by their separate respective license(s).




﻿using Emgu.CV;
using Microsoft.Kinect;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectFacialRecognition
{   
    /// <summary>
    /// Designed to Create utility methods to convert between OpenCv image format and Bitmap/ BitmapSource formats(WPF)
    /// </summary>
    public static class extensions
    {

        [System.Runtime.InteropServices.DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        static ColorBitmapGenerator _bitmapGenerator = new ColorBitmapGenerator();
        
        /// <summary>
        /// Converts a color frame to a System.Media.Imaging.BitmapSource.
        /// </summary>
        /// <param name="frame">The specified color frame.</param>
        /// <returns>The bitmap representation of the specified color frame.</returns>
        public static WriteableBitmap ToWriteableBitmap(this ColorFrame frame)
        {
            _bitmapGenerator.Update(frame);

            return _bitmapGenerator.Bitmap;
        }

        internal static Bitmap ToBitmap(this byte[] data, int width, int height
            , System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format32bppRgb)
        {
            var bitmap = new Bitmap(width, height, format);

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        /// <summary>
        /// Converts a kinects colorFrame to  bitmap from
        /// </summary>
        /// <param name="frame">The specofied color Frame</param>
        /// <returns>Converted bitmap </returns>
        public static Bitmap ToBitmap(this ColorFrame frame)
        {
            if (frame == null || frame.FrameDescription.LengthInPixels == 0)
                return null;

            var width = frame.FrameDescription.Width;
            var height = frame.FrameDescription.Height;

            var data = new byte[width * height * PixelFormats.Bgra32.BitsPerPixel / 8];
            frame.CopyConvertedFrameDataToArray(data, ColorImageFormat.Bgra);

            return data.ToBitmap(width, height);
        }
        /// <summary>
        /// Converts a Bitmap to BitmapSorce
        /// </summary>
        /// <param name="bitmap">The specified bitmap</param>
        /// <returns>The converted BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (bitmap == null) return null;
            IntPtr ptr = bitmap.GetHbitmap();
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            ptr,
            IntPtr.Zero,
            Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
            return source;
        }
        /// <summary>
        /// Converts ColorFrame to OpenCv Image
        /// </summary>
        /// <typeparam name="TColor">EMGU.CV.IColor Ex RGB, Gray  </typeparam>
        /// <typeparam name="TDepth">Int16, byte</typeparam>
        /// <param name="image">ColorFrame </param>
        /// <returns>Converted Open CV image</returns>
        public static Image<TColor, TDepth> ToOpenCVImage<TColor, TDepth>(this ColorFrame image)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            var bitmap = image.ToBitmap();
            return new Image<TColor, TDepth>(bitmap);
        }
        /// <summary>
        /// Converts bitmap to openCv image
        /// </summary>
        /// <typeparam name="TColor">EMGU.CV.IColor Ex RGB, Gray</typeparam>
        /// <typeparam name="TDepth">Int16, byte</typeparam>
        /// <param name="bitmap">bitmap Image</param>
        /// <returns>Converted Open CV Image</returns>
        public static Image<TColor, TDepth> ToOpenCVImage<TColor, TDepth>(this Bitmap bitmap)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            return new Image<TColor, TDepth>(bitmap);
        }
        /// <summary>
        /// Converts open cv image to BitmapSource image 
        /// </summary>
        /// <param name="image">Specified open Cv image</param>
        /// <returns>The BitmapSource representation of the open CV image</returns>
        public static BitmapSource ToBitmapSource(this IImage image)
        {
            var source = image.Bitmap.ToBitmapSource();
            return source;
        }

        /// <summary>
        /// Converts BitmapSource to Bitmap
        /// </summary>
        /// <param name="source"> The Specified BitmapSource </param>
        /// <returns> The bitmap representation of the BitmapSource</returns>

        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              System.Drawing.Imaging.ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }



    }
}
