namespace DiscImageChef.VideoNow
{
    public static class Swapping
    {
        public static byte[] SwapBuffer(byte[] buffer)
        {
            byte[] tmp = new byte[buffer.Length];

            for(int i = 0; i < buffer.Length; i += 2)
            {
                tmp[i] = buffer[i + 1];
                tmp[i             + 1] = buffer[i];
            }

            return tmp;
        }
    }
}