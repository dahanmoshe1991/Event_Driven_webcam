using System;

namespace Camera
{
    public enum CameraInput
    {
        Exception = 0,
        Create,
        Idle,
        StartRolling,
        StopRolling,
        Exit
    }


    public class CameraStateMachine
    {
        public CameraState CurrentState { get; }
        public CameraStateMachine()
        {
            CurrentState = CameraState.Init;
        }

        public CameraInput ChangeState(CameraState intendedState)
        {
            CameraInput nextOperation = CameraInput.Exception;

            switch (intendedState)
            {
                case (CameraState.Idle):
                    switch (CurrentState)
                    {
                        case CameraState.Init:
                            nextOperation = CameraInput.Create;
                            break;
                        case CameraState.Rolling:
                            nextOperation = CameraInput.StopRolling;
                            break;
                    }
                    break;

                case (CameraState.Rolling):
                    if (CurrentState == CameraState.Idle)
                        nextOperation = CameraInput.StartRolling;
                    break;

                case (CameraState.Terminated):
                    if (CurrentState == CameraState.Idle)
                        nextOperation = CameraInput.Exit;
                    break;

                case CameraState.Error:
                    nextOperation = CameraInput.Exception;
                    break;

                default:
                    nextOperation = CameraInput.Exception;
                    break;
            };
            Console.WriteLine($"Current state: {CurrentState}, Intended State: {intendedState}, next Operation: {nextOperation}");
            return nextOperation;
        }
    }
}
