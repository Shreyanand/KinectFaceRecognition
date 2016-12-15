//Copyright of Nayi Disha Studios Pvt. Ltd.
//Permission granted to Shrey Anand for submission of college applications.
//All rights reserved by Nayi Disha Studios Pvt. Ltd.
//Permissions for all dependencies covered by their separate respective license(s).




using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Xml;
using System.Threading;

namespace KinectFacialRecognition

{   

    /// <summary>
    /// Kinect Facial recognition namespace. Defines two classes: extensions and FacialRecognition 
    ///  <para>Steps to USE</para>
    /// <para>1) Create a wpf application and add a reference to EMGU.CV.dll , Emgu.CV.DebuggerVisualizers.VS20XX, EMGU.util.dll , KinectFacialREcognition.dll given in lib folder</para>
    /// <para>2) Add cvextern.dll, msvcp120.dll, msvcr120.dll, opencv_ffmpeg300_64.dll to your project by "ADD existing item" option. These dlls are given in lib folder</para>
    /// <para>3) Add reference to Microsoft.Kinect.Face, Microsoft.Kinect</para>
    /// <para>5) Copy this command in the post build event of your project. If the command gives an error then find the path to the "NuiDatabase"folder of your installed kinect sdk and update the command.</para>
    /// %systemroot%\System32\xcopy "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.Kinect.Face\2.0\Redist\CommonConfiguration\x86\NuiDatabase" "NuiDatabase" /e /y /i /r
    /// <para>6) Declare, Initialize kinect FaceFrame, ColorFrame, BodyFrame, ColorBitmap, FacialRecognition class object</para>
    /// <para>7) Declare face frame events to update variables</para>
    /// <para>8) Use functions of FacialRecogniton through its object</para>
    /// <para>Check  demo app for the complete code </para>
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    public static class NamespaceDoc { }
    /// <summary>
    /// Provides access to methods requried to train and recognize faces
    /// </summary>
    public class FacialRecognition
    {   
        /// <summary>
        /// Variables needed to save Jpg and xml
        /// </summary>
        #region Saving Jpg and xaml
        List<Image<Gray, byte>> ImagestoWrite = new List<Image<Gray, byte>>();
        EncoderParameters ENC_Parameters = new EncoderParameters(1);
        EncoderParameter ENC = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100);
        ImageCodecInfo Image_Encoder_JPG;

        //Saving XAML Data file
        List<string> NamestoWrite = new List<string>();
        List<string> NamesforFile = new List<string>();
        XmlDocument docu = new XmlDocument();
        #endregion

        //Declaring classifier recognizer
        private Classifier Eigen_Recog;

        /// <summary>
        /// Constructor to intialize Facial Recognition internal variables 
        /// </summary>
        public FacialRecognition()
        {
            this.Eigen_Recog = new Classifier();
            this.ENC_Parameters.Param[0] = ENC;
            this.Image_Encoder_JPG = GetEncoder(ImageFormat.Jpeg);
            
        }

        # region dll public functions
        
        /// <summary>
        /// Gets status of the recognizer.
        /// </summary>
        /// <returns> 
        /// True if recognizer is trained , False otherwise</returns>
        public bool IsTrained()
        {
            return this.Eigen_Recog.IsTrained; 
        }
        /// <summary>
        /// Assigns a tracking ID to face from Body tracking ID
        /// </summary>
        /// <param name="bodyFrame"> The body frame of kinect class </param>
        /// <param name="_bodies"> list of bodies tracked </param>
        /// <param name="_faceSource"> The specified and initialized faceSource </param>
        public void assignIDtoface(BodyFrame bodyFrame, IList<Body> _bodies, FaceFrameSource _faceSource)
        {
            bodyFrame.GetAndRefreshBodyData(_bodies);

            Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();

            if (!_faceSource.IsTrackingIdValid)
            {
                if (body != null)
                {
                    // 4) Assign a tracking ID to the face source
                    _faceSource.TrackingId = body.TrackingId;
                }
            }
        }
        /// <summary>
        /// Draws a rectangle on the face in the bitmap
        /// </summary>
        /// <param name="colorBitmap"> The specified colorBitmap on which the face reactangle is drawn </param>
        /// <param name="faceFrameResult"> The specified face frame result, obtained each time a face frame event is called </param>
        /// <returns>True if face is found and drawn on the bitmap, false otherwise</returns>
        public bool drawRect(WriteableBitmap colorBitmap, FaceFrameResult faceFrameResult)
        {
            var faceDetectedrect = getfaceRectangle(faceFrameResult);
            if (!faceDetectedrect.IsEmpty)
            {
                var x2 = faceDetectedrect.X + faceDetectedrect.Width;
                var y2 = faceDetectedrect.Y + faceDetectedrect.Height;
                colorBitmap.DrawRectangle(faceDetectedrect.X - 40, faceDetectedrect.Y - 60, x2 + 40, y2 + 20, Colors.Black);
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Takes a snapshot of the face present in the given Bitmap, preprocesses it and converts it to open CV image
        /// </summary>
        /// <param name="colorBitmap">The specified Bitmap from which the face Snapshot will be taken  </param>
        /// <param name="faceFrameResult">The specified face frame result which gives the bounding face rectangle</param>
        /// <returns> Open CV Image of face.
        /// Returns null if face is  not found in the frame</returns>
        public Image<Gray, Byte> takefacesnapshot(WriteableBitmap colorBitmap, FaceFrameResult faceFrameResult)
        {

            var faceDetectedrect = getfaceRectangle(faceFrameResult);
            if (faceDetectedrect.IsEmpty) { Image<Gray, Byte> blank = null; return blank; }

            else
            {

                var faceBitmap = colorBitmap.Crop(faceDetectedrect.X, faceDetectedrect.Y - 10, faceDetectedrect.Width + 5, faceDetectedrect.Height + 20);

                Bitmap bitmap = extensions.GetBitmap(faceBitmap);
                
                var cvImage = extensions.ToOpenCVImage<Bgr, byte>(bitmap);


                var result = cvImage.Convert<Gray, Byte>().Resize(200, 200, Emgu.CV.CvEnum.Inter.Cubic);
                result._EqualizeHist();
                //CvInvoke.CLAHE(this.result,40,new System.Drawing.Size(8,8),this.result);
                //this.imageBox1.Source = extensions.ToBitmapSource(this.result);
                return result;
            }
        }
        
        /// <summary>
        /// Recognizes  the face present in the colorBitmap 
        /// </summary>
        /// <param name="colorBitmap">The color bitmap which contains the face to be recognized</param>
        /// <param name="faceFrameresult">The specified face frame result which gives the bounding face rectangle </param>
        /// <returns>Name and confidence separated by space.
        /// Returns "" if faceSnapshot is null or no data loaded</returns> 
       
        public string Recognize(WriteableBitmap colorBitmap, FaceFrameResult faceFrameresult)
        {
            
            if (this.Eigen_Recog.IsTrained)
            {
                var faceSnapshot = takefacesnapshot(colorBitmap, faceFrameresult);
                if (faceSnapshot != null)
                {
                    string name = this.Eigen_Recog.Recognise(faceSnapshot);
                    int match_value = (int)this.Eigen_Recog.Get_Eigen_Distance;
                    return name + "" + match_value;
                }
                else { return ""; }
            }
            else { return ""; }
        }
        /// <summary>
        /// Recognizes  the face present in the faceSnapshot and returns a string with name and confidence separated by space
        /// </summary>
        /// <param name="faceSnapshot">The exact face in openCV image format that will  be recognized</param>
        /// <returns>Name and confidence separated by a space.  
        /// Returns "" if faceSnapshot is null or no data loaded</returns> 
        public string Recognize(Image<Gray, Byte> faceSnapshot)
        {
            if (this.Eigen_Recog.IsTrained)
            {
                if (faceSnapshot != null)
                {
                    string name = this.Eigen_Recog.Recognise(faceSnapshot);
                    int match_value = (int)this.Eigen_Recog.Get_Eigen_Distance;
                    return name + " " + match_value;

                }
                else { return ""; }
            }
            else return "";
        }
        /// <summary>
        /// Trains  the face present in the color bitmap 
        /// </summary>
        /// <param name="colorBitmap">The color bitmap which contains the face to be trained</param>
        /// <param name="faceFrameresult">The specified face frame result which gives the bounding face rectangle</param>
        /// <param name="name">The name of the person who will be trained</param>
        /// <returns> True if training succesful, false otherwise</returns>
        public bool train(WriteableBitmap colorBitmap, FaceFrameResult faceFrameresult, String name)
        {

            var result = takefacesnapshot(colorBitmap, faceFrameresult);
            if (result != null)
            {
                if (save_training_data(result.ToBitmap(), name))
                {
                    return true;
                }
                else return false;
            }
            else return false;

        }
        /// <summary>
        /// Trains  the face present in the faceSnapshot
        /// </summary>
        /// <param name="faceSnapshot">The exact face in openCV image format that will  be recognized</param>
        /// <param name="name">The name of the person who will be trained</param>
        /// <returns>True if training succesful, false otherwise </returns>
        public bool train(Image<Gray, Byte> faceSnapshot, String name)
        {
            if (faceSnapshot != null)
            {
                if (save_training_data(faceSnapshot.ToBitmap(), name)) { return true; }
                else return false;
            }
            else return false;
        }
        /// <summary>
        /// Reloads the  training data and updates the recognizer
        /// </summary>
        /// <returns>True if Recognizer trained</returns>
        public bool reload()
        {

            this.Eigen_Recog = new Classifier();
            return (this.Eigen_Recog.IsTrained);

        }

        #endregion

        #region dll private Functions

        /// <summary>
        /// Returns the face coordinates from the faceFrameResult
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private Int32Rect getfaceRectangle(FaceFrameResult result)
        {

            if (result != null)
            {


                var facesDetected = result.FaceBoundingBoxInColorSpace;
                int x = facesDetected.Left;
                int y = facesDetected.Top;
                int height = facesDetected.Bottom - facesDetected.Top;
                int width = facesDetected.Right - facesDetected.Left;
                if (facesDetected != null)
                {

                    var faceDetectedrect = new Int32Rect(x, y, height, width);
                    return faceDetectedrect;
                }
                else { return Int32Rect.Empty; }
            }
            else { return Int32Rect.Empty; }

        }
        /// <summary>
        /// Returns the codec for the image format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates/Updates the face trained in the folder, their Jpg and the XML file associated with them.
        /// </summary>
        /// <param name="face_data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool save_training_data(System.Drawing.Image face_data, String name)
        {
            try
            {
                Random rand = new Random();
                bool file_create = true;
                string facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";
                while (file_create)
                {

                    if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/" + facename))
                    {
                        file_create = false;
                    }
                    else
                    {
                        facename = "face_" + name + "_" + rand.Next().ToString() + ".jpg";
                    }
                }


                if (Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/"))
                {
                    face_data.Save(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/" + facename, ImageFormat.Jpeg);
                }
                else
                {
                    Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/");
                    face_data.Save(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/" + facename, ImageFormat.Jpeg);
                }
                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedLabels.xml"))
                {
                    //File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", NAME_PERSON.Text + "\n\r");
                    bool loading = true;
                    while (loading)
                    {
                        try
                        {
                            docu.Load(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedLabels.xml");
                            loading = false;
                        }
                        catch
                        {
                            docu = null;
                            docu = new XmlDocument();
                            Thread.Sleep(10);
                        }
                    }

                    //Get the root element
                    XmlElement root = docu.DocumentElement;

                    XmlElement face_D = docu.CreateElement("FACE");
                    XmlElement name_D = docu.CreateElement("NAME");
                    XmlElement file_D = docu.CreateElement("FILE");

                    //Add the values for each nodes
                    //name.Value = textBoxName.Text;
                    //age.InnerText = textBoxAge.Text;
                    //gender.InnerText = textBoxGender.Text;
                    name_D.InnerText = name;
                    file_D.InnerText = facename;

                    //Construct the Person element
                    //person.Attributes.Append(name);
                    face_D.AppendChild(name_D);
                    face_D.AppendChild(file_D);

                    //Add the New person element to the end of the root element
                    root.AppendChild(face_D);

                    //Save the document
                    docu.Save(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedLabels.xml");
                    //XmlElement child_element = docu.CreateElement("FACE");
                    //docu.AppendChild(child_element);
                    //docu.Save("TrainedLabels.xml");
                }
                else
                {
                    FileStream FS_Face = File.OpenWrite(System.AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedLabels.xml");
                    using (XmlWriter writer = XmlWriter.Create(FS_Face))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Faces_For_Training");

                        writer.WriteStartElement("FACE");
                        writer.WriteElementString("NAME", name);
                        writer.WriteElementString("FILE", facename);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    FS_Face.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        #endregion


    }
}
