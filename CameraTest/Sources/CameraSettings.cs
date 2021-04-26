namespace Camera.Sources
{
    public struct CameraSettings
    {
        public int FrameWidth { get; }
        public int FrameHeight { get; }
        public string FilePath { get; }

        public CameraSettings(int frameWidth, int frameHeight, string filePath)
        {
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            FilePath = filePath;
        }
    }
}