using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using OpenCvSharp;

namespace Camera
{
    public class CameraWrapper : IDisposable, ICameraWrapper
    {
        private VideoCapture _camera;
        private readonly CameraSettings _settings;
        private readonly Queue<Mat> _camImagesQueue;
        //private Window _window;
        private System.Timers.Timer _intervalCaptureTimer;
        private const int IntervalTimeMs = 10000;
        public event EventHandler StartRollingEvent;
        public event EventHandler StopRollingEvent;

        public AutoResetEvent FinishedRollingEvent = new AutoResetEvent(false); //for upper class maybe to remove in future

        public CameraState State { get; private set; }

        public CameraWrapper(CameraSettings cameraSettings)
        {
            _settings = cameraSettings;
            if (!ConnectCam()) return;

            State = CameraState.Idle;
            _camImagesQueue = new Queue<Mat>();
            ApplyConfigurations(cameraSettings);
            StartRollingEvent += StartRolling;
        }

        public bool ConnectCam()
        {
            _camera = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
            if (!_camera.IsOpened())
            {
                Console.WriteLine("Failed to connect and open camera.");
                State = CameraState.Error;
                return false;
            }

            return true;
        }

        private void ApplyConfigurations(CameraSettings cameraSettings)
        {
            //todo: check if needed more configurations
            _camera.FrameWidth = cameraSettings.FrameWidth; //1280
            _camera.FrameHeight = cameraSettings.FrameHeight; //720
        }

        /// <summary>
        /// Activated on StartRollingEvent.
        /// Initiate interval timer which will capture an image every and add this image to a queue.
        /// This operation will continue until stop event will occur.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartRolling(object sender, EventArgs e)
        {
            //check valid state
            if (State != CameraState.Idle)
            {
                Console.WriteLine($"State is not valid for Rolling. State is {State}.");
                return;
            }

            State = CameraState.Rolling;
            _camImagesQueue.Clear();
            ActivateTimer();
            StartRollingEvent -= StartRolling;
            StopRollingEvent += StopRolling;
            //_window = new Window("capture");

            Console.WriteLine("Rolling...");

        }

        /// <summary>
        /// Activated on StopRollingEvent.
        /// Stops interval timer which will stop taking images.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StopRolling(object sender, EventArgs e)
        {
            //check operation is valid
            if (State != CameraState.Rolling)
            {
                Console.WriteLine($"Received StopRolling while state is not rolling. State is {State}.");
                return;
            }

            //reset to idle state
            Console.WriteLine("Stop Rolling.");
            _intervalCaptureTimer.Stop();
            StopRollingEvent -= StopRolling;
            StartRollingEvent += StartRolling;
            State = CameraState.Idle;
            //_window.Close();

            //notify finished
            FinishedRollingEvent.Set();
        }

        private void ActivateTimer()
        {
            // Create a timer and set a ms value interval.
            _intervalCaptureTimer = new System.Timers.Timer
            {
                Interval = IntervalTimeMs
            };

            // Hook up the Elapsed event for the timer. 
            _intervalCaptureTimer.Elapsed += OnPhotoIntervalEvent;

            // Have the timer fire repeated events (true is the default)
            _intervalCaptureTimer.AutoReset = true;

            // Start the timer
            _intervalCaptureTimer.Enabled = true;
        }

        private void OnPhotoIntervalEvent(object sender, ElapsedEventArgs e)
        {
            if (State != CameraState.Rolling)
                return;

            ProcessImage();
        }

        private void ProcessImage()
        {
            var image = Capture();
            if (image != null)
            {
                _camImagesQueue.Enqueue(image);
               //Task.Run((() => _window.ShowImage(image)));
                
               var date = $"{DateTime.Now:ddMyyyy_hhmmsstt}";
                var res = image.SaveImage(_settings.FilePath + "test_" + date + ".jpg");
                Console.WriteLine($"Image save time: {date} save operation successful: {res}");
            }
            else
                Console.WriteLine("failed to capture an image");
        }

        public Mat Capture()
        {
            var image = new Mat();
            return _camera.Read(image) ? image : null;
        }

        public void Dispose()
        {
            _intervalCaptureTimer.Stop();
            StopRollingEvent -= StopRolling;
            StartRollingEvent -= StartRolling;
            _camera.Dispose();
        }

        public void RaiseStartRollingEvent()
        {
            StartRollingEvent?.Invoke(this, EventArgs.Empty);
        }
        public void RaiseStopRollingEvent()
        {
            StopRollingEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
