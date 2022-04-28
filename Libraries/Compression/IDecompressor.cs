namespace Libraries.Compression
{
    public interface IDecompressor
    {
        byte[] Decompress(byte[] CompressedData, uint Start);
    }
}
