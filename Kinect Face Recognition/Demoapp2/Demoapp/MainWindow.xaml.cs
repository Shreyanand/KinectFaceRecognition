//Copyright of Nayi Disha Studios Pvt. Ltd.
//Permission granted to Shrey Anand for submission of college applications.
//All rights reserved by Nayi Disha Studios Pvt. Ltd.
//Permissions for all dependencies covered by their separate respective license(s).

﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;

using System.IO;
using System.ComponentModel;
using KinectFacialRecognition;

namespace Demoapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        #region variables
        private KinectSensor sensor;
        private ColorFrameReader colorFrameReader = null;
        private BodyFrameReader _bodyReader = null;
        private IList<Body> _bodies = null;

        // 1) Specify a face frame source and a face frame reader
        FaceFrameSource _faceSource = null;
        FaceFrameReader _faceReader = null;

        private WriteableBitmap colorBitmap = null;
        private FaceFrameResult faceFrameResult = null;
        FacialRecognition kfr = null;
        
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            this.kfr = new FacialRecognition();
           
            this.sensor = KinectSensor.GetDefault();
           

            _bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
            _bodyReader = this.sensor.BodyFrameSource.OpenReader();
            _bodyReader.FrameArrived += BodyReader_FrameArrived;

            // 2) Initialize the face source with the desired features
            _faceSource = new FaceFrameSource(this.sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace);
            _faceReader = _faceSource.OpenReader();
            _faceReader.FrameArrived += FaceReader_FrameArrived;

            // open the reader for the color frames
            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();
            
            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            try
            {

                this.sensor.Open();
            }
            catch (IOException)
            {
                this.sensor = null;
            }
            // create the colorFrameDescription from the ColorFrameSource using Bgra format

            FrameDescription colorFrameDescription = this.sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            if (this.kfr.IsTrained())
            {
                this.RECOGNIZED_PERSON.Text = "Training Data loaded";
            }
            else
            {
                this.RECOGNIZED_PERSON.Text = "No training data found, please train program using Train menu option";
            }
        }

        #region farmeEvents
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        { 
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    //ToWriteableBitmap(colorFrame, this.colorBitmap);
                    this.colorBitmap = colorFrame.ToWriteableBitmap();
                    //var faceDetectedrect = getfaceRectangle(this.faceFrameResult);

                    if (kfr.drawRect(this.colorBitmap, this.faceFrameResult)) { this.text.Text = "face Detected"; }
                    else { this.text.Text = "face not in frame"; }
                    this.Image.Source = colorBitmap;
                }
            }

        }






        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    kfr.assignIDtoface(frame, _bodies, this._faceSource);
                }
            }
        }




        void FaceReader_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    FaceFrameResult result = frame.FaceFrameResult;
                    if (result != null)
                    {

                        this.faceFrameResult = result;

                    }
                }
            }
        }





        #endregion


        #region buttonClickEvents
        private void Recognize_Click(object sender, RoutedEventArgs e)
        {


            var faceSnapshot = kfr.takefacesnapshot(this.colorBitmap, this.faceFrameResult);
            if (faceSnapshot != null)
            {
                //resultImages.Add(result);
                this.imageBox2.Source = extensions.ToBitmapSource( faceSnapshot);

                this.RECOGNIZED_PERSON.Text = kfr.Recognize(faceSnapshot);

            }
            // The other this.RECOGNIZED_PERSON.Text = kfr.Recognize(this.colorBitmap,this.faceFrameResult, kfr.get_EigenRecog());
            


        }


        private void RELOAD_Click(object sender, RoutedEventArgs e)
        {
            if (kfr.reload()) { this.RECOGNIZED_PERSON.Text = "training data loaded"; }
            else { this.RECOGNIZED_PERSON.Text = "No training data found, please train program using Train menu option"; }
        }


        private void Train_Click(object sender, RoutedEventArgs e)
        {

            var faceSnapshot = kfr.takefacesnapshot(this.colorBitmap, this.faceFrameResult);
            if (faceSnapshot != null)
            {
                this.imageBox1.Source = extensions.ToBitmapSource( faceSnapshot);

                //this.imageBox1.Source = extensions.ToBitmapSource(result);
                //resultImages.Add(result);
                MessageBoxResult yesno = MessageBox.Show("Would you like to save the image?", "TRAIN", MessageBoxButton.YesNo);
                switch (yesno)
                {
                    case MessageBoxResult.Yes:
                        if (!kfr.train(faceSnapshot, this.NAME_PERSON.Text)) { this.text.Text = "Error"; }
                        this.imageBox1.Source = null;
                        break;
                    case MessageBoxResult.No:
                        // MessageBox.Show("Oh well, too bad!", "My App");
                        this.imageBox1.Source = null;
                        break;

                }
            }
        }

        #endregion

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
            if (_bodyReader != null)
            {
                _bodyReader.Dispose();
                _bodyReader = null;
            }

            if (_faceReader != null)
            {
                _faceReader.Dispose();
                _faceReader = null;
            }

            if (_faceSource != null)
            {
                _faceSource.Dispose();
                _faceSource = null;
            }
        }
    }
}
