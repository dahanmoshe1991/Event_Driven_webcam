using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Camera.Interfaces;
using OpenCvSharp;

namespace Camera.Sources
{
    public class CameraWrapper : IDisposable, ICameraWrapper, IObservableCamera
    {
        private VideoCapture _camera;
        private readonly CameraSettings _settings;
        private readonly bool _debugMode;
        private readonly Queue<Mat> _camImagesQueue;
        private readonly List<ICameraObserver> _observers;
        //private Window _window;
        private System.Timers.Timer _intervalCaptureTimer;
        private const int IntervalTimeMs = 10000;
        public event EventHandler StartRollingEvent;
        public event EventHandler StopRollingEvent;

        public AutoResetEvent FinishedRollingEvent = new AutoResetEvent(false); //for upper class maybe to remove in future

        public CameraState State { get; private set; }

        public CameraWrapper(CameraSettings cameraSettings, bool debugMode)
        {
            _settings = cameraSettings;
            _debugMode = debugMode;
            if (!ConnectCam()) return;

            State = CameraState.Idle;
            _camImagesQueue = new Queue<Mat>();
            _observers = new List<ICameraObserver>();
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

            Console.WriteLine("\nStarted Rolling...");

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
            Console.WriteLine("\nStop Rolling.");
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
                //using observer interface to pass images to Camera observers
                Notify(image);

                if (_debugMode)
                {
                    _camImagesQueue.Enqueue(image);
                    SaveImage(image);
                }
            }
            else
                Console.WriteLine("failed to capture an image");
        }

        private void SaveImage(Mat image)
        {
            var date = $"{DateTime.Now:ddMyyyy_hhmmsstt}";
            var res = image.SaveImage(_settings.FilePath + "test_" + date + ".jpg");
            Console.WriteLine($"Image save time: {date} save operation successful: {res}");
        }

        public Mat Capture()
        {
            var image = new Mat();
            return _camera.Read(image) ? image : null;
        }

        public void Dispose()
        {
            CloseCamera();
            _camera.Dispose();
        }

        private void CloseCamera()
        {
            _intervalCaptureTimer.Stop();
            StopRollingEvent -= StopRolling;
            StartRollingEvent -= StartRolling;
            _observers.Clear();
        }

        public void RaiseStartRollingEvent()
        {
            StartRollingEvent?.Invoke(this, EventArgs.Empty);
        }
        public void RaiseStopRollingEvent()
        {
            StopRollingEvent?.Invoke(this, EventArgs.Empty);
        }

        // The subscription management methods.
        public void Attach(ICameraObserver observer)
        {
            _observers.Add(observer);
            Console.WriteLine("Camera: Attached an observer.");
        }
        //unsubscribe
        public void Detach(ICameraObserver observer)
        {
            _observers.Remove(observer);
            Console.WriteLine("Camera: Detached an observer.");
        }

        // Trigger an update in each subscriber.
        public void Notify(Mat image)
        {
            Console.WriteLine("\nCamera: Notifying observers...");

            foreach (var observer in _observers)
            {
                observer.ImageCaptured(image);
            }
        }
    }
}
