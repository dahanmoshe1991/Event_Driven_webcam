using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Camera.Interfaces
{
    interface IObservableCamera
    {
        // Attach an observer to the subject.
        void Attach(ICameraObserver observer);

        // Detach an observer from the subject.
        void Detach(ICameraObserver observer);

        // Notify all observers about an event.
        void Notify(Mat image);
    }
}
