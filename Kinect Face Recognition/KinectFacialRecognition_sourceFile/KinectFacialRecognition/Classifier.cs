//Copyright of Nayi Disha Studios Pvt. Ltd.
//Permission granted to Shrey Anand for submission of college applications.
//All rights reserved by Nayi Disha Studios Pvt. Ltd.
//Permissions for all dependencies covered by their separate respective license(s).


﻿using System;
using System.Collections.Generic;


using Emgu.CV;
using Emgu.CV.Structure;

using System.IO;
using System.Xml;
using Emgu.CV.Face;


namespace KinectFacialRecognition
{
    /// <summary>
    /// Desingned to separate training EigenObjectRecognizer , recognize logic from the main class.
    ///  Interacts internally with the open CV functionalities.
    /// the object of this class is passed as a parameter to the recognize function of the Facial Recognition class.
    /// </summary>
    internal class Classifier : IDisposable
    {
        
        #region Variables

       
        FaceRecognizer recognizer;

        
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> Names_List = new List<string>(); //labels
        List<int> Names_List_ID = new List<int>();
        int ContTrain, NumLabels;
        float Eigen_Distance = 0;
        string Eigen_label;
        int Eigen_threshold = 1500;

        //Class Variables
        bool _IsTrained = false;

        internal string Recognizer_Type = "EMGU.CV.LBPHFaceRecognizer";
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor, Looks in (Application.StartupPath + "\\TrainedFaces") for training data.
        /// </summary>
        internal Classifier()
        {
            _IsTrained = LoadTrainingData(System.AppDomain.CurrentDomain.BaseDirectory + "\\TrainedFaces");
        }

        /// <summary>
        /// Takes String input to a different location for training data
        /// </summary>
        /// <param name="Training_Folder"></param>
        internal Classifier(string Training_Folder)
        {
            _IsTrained = LoadTrainingData(Training_Folder);
        }
        #endregion

        #region Public

       /// <summary>
       /// gets the status of recognizer
       /// </summary>
        internal bool IsTrained
        {
            get { return _IsTrained; }
        }

        /// <summary>
        /// Recognise a Grayscale Image using the trained Eigen Recogniser
        /// </summary>
        
        internal string Recognise(Image<Gray, byte> Input_image, int Eigen_Thresh = -1)
        {
            if (_IsTrained)
            {
                FaceRecognizer.PredictionResult ER = recognizer.Predict(Input_image);

                if (ER.Label == -1)
                {
                    Eigen_label = "Unknown";
                    Eigen_Distance = 0;
                    return Eigen_label;
                }
                else
                {
                    Eigen_label = Names_List[ER.Label];
                    Eigen_Distance = (float)ER.Distance;
                    if (Eigen_Thresh > -1) Eigen_threshold = Eigen_Thresh;

                    //Only use the post threshold rule if we are using an Eigen Recognizer 
                    //since Fisher and LBHP threshold set during the constructor will work correctly 
                    switch (Recognizer_Type)
                    {
                        case ("EMGU.CV.EigenFaceRecognizer"):
                            if (Eigen_Distance > Eigen_threshold) return Eigen_label;
                            else return "Unknown";
                        case ("EMGU.CV.LBPHFaceRecognizer"):
                        case ("EMGU.CV.FisherFaceRecognizer"):
                        default:
                            return Eigen_label; //the threshold set in training controls unknowns
                    }




                }

            }
            else return "";
        }



        /// <summary>
        /// Returns a string containg the recognised persons name
        /// </summary>
        internal string Get_Eigen_Label
        {
            get
            {
                return Eigen_label;
            }
        }

        /// <summary>
        /// Returns a float confidence value for potential false clasifications
        /// </summary>
        internal float Get_Eigen_Distance
        {
            get
            {
                //get eigenDistance
                return Eigen_Distance;
            }
        }



        #endregion

        #region Private
        /// <summary>
        /// Loads the traing data given a (string) folder location
        /// </summary>
        /// <param name="Folder_location"></param>
        /// <returns></returns>
        private bool LoadTrainingData(string Folder_location)
        {
            if (File.Exists(Folder_location + "\\TrainedLabels.xml"))
            {
                try
                {
                    //message_bar.Text = "";
                    Names_List.Clear();
                    Names_List_ID.Clear();
                    trainingImages.Clear();
                    FileStream filestream = File.OpenRead(Folder_location + "\\TrainedLabels.xml");
                    long filelength = filestream.Length;
                    byte[] xmlBytes = new byte[filelength];
                    filestream.Read(xmlBytes, 0, (int)filelength);
                    filestream.Close();

                    MemoryStream xmlStream = new MemoryStream(xmlBytes);

                    using (XmlReader xmlreader = XmlTextReader.Create(xmlStream))
                    {
                        while (xmlreader.Read())
                        {
                            if (xmlreader.IsStartElement())
                            {
                                switch (xmlreader.Name)
                                {
                                    case "NAME":
                                        if (xmlreader.Read())
                                        {
                                            Names_List_ID.Add(Names_List.Count); //0, 1, 2, 3....
                                            Names_List.Add(xmlreader.Value.Trim());
                                            NumLabels += 1;
                                        }
                                        break;
                                    case "FILE":
                                        if (xmlreader.Read())
                                        {
                                            //PROBLEM HERE IF TRAININGG MOVED
                                            trainingImages.Add(new Image<Gray, byte>(System.AppDomain.CurrentDomain.BaseDirectory + "\\TrainedFaces\\" + xmlreader.Value.Trim()));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    ContTrain = NumLabels;

                    if (trainingImages.ToArray().Length != 0)
                    {

                        //Eigen face recognizer
                        //Parameters:	
                        //      num_components – The number of components (read: Eigenfaces) kept for this Prinicpal 
                        //          Component Analysis. As a hint: There’s no rule how many components (read: Eigenfaces) 
                        //          should be kept for good reconstruction capabilities. It is based on your input data, 
                        //          so experiment with the number. Keeping 80 components should almost always be sufficient.
                        //
                        //      threshold – The threshold applied in the prediciton. This still has issues as it work inversly to LBH and Fisher Methods.
                        //          if you use 0.0 recognizer.Predict will always return -1 or unknown if you use 5000 for example unknow won't be reconised.
                        //          As in previous versions I ignore the built in threhold methods and allow a match to be found i.e. double.PositiveInfinity
                        //          and then use the eigen distance threshold that is return to elliminate unknowns. 
                        //
                        //NOTE: The following causes the confusion, sinc two rules are used. 
                        //--------------------------------------------------------------------------------------------------------------------------------------
                        //Eigen Uses
                        //          0 - X = unknown
                        //          > X = Recognised
                        //
                        //Fisher and LBPH Use
                        //          0 - X = Recognised
                        //          > X = Unknown
                        //
                        // Where X = Threshold value


                        switch (Recognizer_Type)
                        {
                            case ("EMGU.CV.LBPHFaceRecognizer"):
                                recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 100);//50
                                break;
                            case ("EMGU.CV.FisherFaceRecognizer"):
                                recognizer = new FisherFaceRecognizer(0, 3500);//4000
                                break;
                            case ("EMGU.CV.EigenFaceRecognizer"):
                            default:
                                recognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
                                break;
                        }

                        recognizer.Train(trainingImages.ToArray(), Names_List_ID.ToArray());
                        // Recognizer_Type = recognizer.GetType();
                        // string v = recognizer.ToString(); //EMGU.CV.FisherFaceRecognizer || EMGU.CV.EigenFaceRecognizer || EMGU.CV.LBPHFaceRecognizer

                        return true;
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else return false;
        }


        #endregion

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

/// Acknowledgement: https://www.codeproject.com/articles/261550/emgu-multiple-face-recognition-using-pca-and-paral
