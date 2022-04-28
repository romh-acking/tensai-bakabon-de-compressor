using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries.Compression
{
    public class RLEBakabon
    {
        // https://www.smspower.org/forums/11498-TensaiBakabonTranslation
        // https://www.smspower.org/Development/Compression

        // This is an RLE variant. It is used to decode both tiles and tilemap data. 
        // Interleaving factor (4 for tiles, 2 for tilemap)

        public enum WriteMode
        {
            None = -1,
            Fill = 0b0,
            Write = 0b1,
        }

        private static byte maxCount = 0b01111111 - 1;

        public static byte[] Decompress(byte[] compressedData, uint start, int interleveFactor)
        {
            var hl = start;

            byte[] buffer = new byte[0x4000];
            int bufferI;

            int maxI = 0;

            // start going through each color plane
            int loops = 0;
            while (loops < interleveFactor)
            {
                bufferI = loops;
                while (true)
                {
                    var b = compressedData[hl++];

                    if (b == 0)
                    {
                        break;
                    }

                    WriteMode writemode = (WriteMode)((b & 0b10000000) >> 7);
                    var count = (b & 0b01111111);

                    while (count-- > 0)
                    {
                        buffer[bufferI++] = compressedData[hl];

                        if (writemode == WriteMode.Write)
                        {
                            hl++;
                        }

                        for (int i = 0; i < interleveFactor - 1; i++)
                        {
                            bufferI++;
                        }
                    }

                    if (writemode == WriteMode.Fill)
                    {
                        hl++;
                    }
                }

                maxI = Math.Max(maxI, bufferI - (int)interleveFactor + 1);

                loops++;
            }

            byte[] trimmedBuffer = new byte[maxI];
            Array.Copy(buffer, 0, trimmedBuffer, 0, trimmedBuffer.Length);

            return trimmedBuffer;
        }

        public static byte[] Compress(byte[] uncompressed, int interleveFactor)
        {
            var deinterleved = Deinterleave(uncompressed, interleveFactor);

            List<byte> compressed = new();

            foreach (byte[] plane in deinterleved)
            {
                int i = 0;

                while (i < plane.Length)
                {
                    int count = 0;
                    List<byte> writeValue = new();

                    WriteMode w = WriteMode.None;

                    while (i + 1 < plane.Length && count < maxCount && plane[i] == plane[i + 1])
                    {
                        if (w == WriteMode.None)
                        {
                            w = WriteMode.Fill;
                            writeValue.Add(plane[i]);
                        }

                        count++;
                        i++;
                    }

                    if (w == WriteMode.None)
                    {
                        bool CheckIfNextThreeAreSame(int indexer)
                        {
                            int c = 1;
                            for (int k = indexer + 1; k < plane.Length && k - indexer < 3; k++)
                            {
                                if (plane[indexer] != plane[k])
                                {
                                    return false;
                                }
                                c++;
                            }

                            return c == 3;
                        }

                        w = WriteMode.Write;

                        while (count < maxCount)
                        {
                            if (CheckIfNextThreeAreSame(i))
                            {
                                break;
                            }

                            if (i == plane.Length)
                            {
                                break;
                            }

                            writeValue.Add(plane[i]);

                            count++;
                            i++;
                        }
                    }

                    // Handle the weird quirks in the compression implementation
                    if (w == WriteMode.Fill)
                    {
                        i++;
                    }

                    if (w == WriteMode.Write)
                    {
                        count--;
                    }

                    if (w == WriteMode.None)
                    {
                        throw new Exception();
                    }

                    byte command = (byte)(((int)w << 7) | count + 1);
                    compressed.Add(command);

                    foreach (byte b in writeValue)
                    {
                        compressed.Add(b);
                    }

                    if (i >= plane.Length)
                    {
                        compressed.Add(0x0);
                        break;
                    }
                }
            }

            return compressed.ToArray();
        }



        public static byte[] TrimTrailingZeros(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        private static byte[][] Deinterleave(byte[] buf, int totalPlanes)
        {
            if (buf.Length % totalPlanes != 0)
            {
                throw new Exception($"Error: size of file {buf.Length} is not a multiple of {totalPlanes}");
            }

            byte[][] deinterleved = new byte[totalPlanes][];

            for (int planeNo = 0; planeNo < totalPlanes; planeNo++)
            {
                deinterleved[planeNo] = new byte[buf.Length / totalPlanes];
            }

            for (int i = 0; i < buf.Length; i++)
            {
                int planeNo = i % totalPlanes;
                deinterleved[planeNo][i / totalPlanes] = buf[i];
            }

            return deinterleved;
        }
    }
}