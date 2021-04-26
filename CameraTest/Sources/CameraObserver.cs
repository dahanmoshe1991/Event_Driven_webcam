using System;
using Camera.Interfaces;
using OpenCvSharp;

namespace Camera.Sources
{
    public class CameraObserver : ICameraObserver
    {
        private readonly string _name;

        public CameraObserver(string name)
        {
            _name = name;
        }

        public void ImageCaptured(Mat image)
        {
            Console.WriteLine($"Received image on {_name}.");
        }
    }
}