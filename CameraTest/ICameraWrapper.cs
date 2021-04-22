using System;
using OpenCvSharp;

namespace Camera
{
    public interface ICameraWrapper
    {
        CameraState State { get; }
        bool ConnectCam();
        void StartRolling(object sender, EventArgs e);
        void StopRolling(object sender, EventArgs e);
        Mat Capture();
    }

    public enum CameraState
    {
        Error = 0,
        Init,
        Idle,
        Rolling,
    }
}