using System;
using System.Threading.Tasks;

namespace Camera.Sources
{
    /// <summary>
    /// use this class as a standalone console app to test the CameraWrapper functionality. 
    /// </summary>
    public class CamApp
    {
        private static CameraWrapper _camera;
        static void Main(string[] args)
        {
            _camera = new CameraWrapper(new CameraSettings(1280, 720, @"..\..\..\OutputImages\"),false);
            var cameraObserver1 = new CameraObserver("cameraObserver1");
            var cameraObserver2 = new CameraObserver("cameraObserver2");

            do
            {
                Console.WriteLine("\n**Write s for start filming.**");
                Task.Run((() =>
                {
                    var stay = true;
                    while (stay)
                    {
                        var input = Console.ReadLine()?.ToLower();
                        switch (input)
                        {
                            case "s":
                                if (_camera.State == CameraState.Idle)
                                {
                                    //subscribe to camera
                                    _camera.Attach(cameraObserver1);
                                    _camera.Attach(cameraObserver2);
                                    //imitate outside thread/process starting filming operation  
                                    Task.Run((() => _camera.RaiseStartRollingEvent()));
                                }
                                Console.WriteLine("\n**Write p for stop filming.**");
                                break;
                            case "p":
                                if (_camera.State == CameraState.Rolling)
                                {
                                    Task.Run((() => _camera.RaiseStopRollingEvent()));
                                    stay = false;
                                }
                                break;
                        }
                    }

                }));


                _camera.FinishedRollingEvent.WaitOne();

                Console.WriteLine("\nWrite q for closing, anything else for restarting.");
            } while (Console.ReadLine()?.ToLower() != "q");
            
            //close program
            _camera.Dispose();
        }

    }
}