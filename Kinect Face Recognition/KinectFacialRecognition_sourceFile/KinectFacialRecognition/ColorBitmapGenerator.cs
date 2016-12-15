//Copyright of Nayi Disha Studios Pvt. Ltd.
//Permission granted to Shrey Anand for submission of college applications.
//All rights reserved by Nayi Disha Studios Pvt. Ltd.
//Permissions for all dependencies covered by their separate respective license(s).




﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectFacialRecognition
{
    /// <summary>
    /// Creates a bitmap representation of a Kinect color frame.
    /// </summary>
    internal class ColorBitmapGenerator
    {
        #region Members

        /// <summary>
        /// Returns the RGB pixel values.
        /// </summary>
        byte[] _pixels;

        /// <summary>
        /// Returns the width of the bitmap.
        /// </summary>
        int _width;

        /// <summary>
        /// Returns the height of the bitmap.
        /// </summary>
        int _height;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the actual bitmap.
        /// </summary>
        public WriteableBitmap Bitmap { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the bitmap with new frame data.
        /// </summary>
        /// <param name="frame">The specified Kinect color frame.</param>
        public void Update(ColorFrame frame)
        {
            if (Bitmap == null)
            {
                _width = frame.FrameDescription.Width;
                _height = frame.FrameDescription.Height;
                _pixels = new byte[_width * _height * 4];
                Bitmap = new WriteableBitmap(_width, _height, 96.0, 96.0, PixelFormats.Bgr32, null);
            }

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(_pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(_pixels, ColorImageFormat.Bgra);
            }

            Bitmap.Lock();

            Marshal.Copy(_pixels, 0, Bitmap.BackBuffer, _pixels.Length);
            Bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));

            Bitmap.Unlock();
        }

        #endregion
    }

}
