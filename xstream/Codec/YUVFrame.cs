namespace Xstream.Codec
{
    public class YUVFrame
    {
        public byte[][] FrameData;
        public int[] LineSizes;
        public YUVFrame(byte[][] frameData, int[] linesizes)
        {
            FrameData = frameData;
            LineSizes = linesizes;
        }
    }
}