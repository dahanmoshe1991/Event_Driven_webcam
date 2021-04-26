using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Camera.Interfaces
{
    public interface ICameraObserver
    {
        // Receive images from Camera
        void ImageCaptured(Mat image);
    }
}
