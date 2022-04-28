namespace Libraries.Compression
{
    public interface ICompressor
    {
        byte[] Compress(byte[] data);
    }
}
