/*
 * Terra Compress Lz2
 * Version 1.0
 * https://github.com/Smallhacker/TerraCompress
*/

using System;
using System.Collections.Generic;

namespace Libraries.Compression
{
    public class Lz1AndLz2 //: ICompressor, IDecompressor
    {
        private enum LzCommands
        {
            DirectCopy = 0b000,
            ByteFill = 0b001,
            WordFill = 0b010,
            IncreaseFill = 0b011,
            Repeat = 0b100,
            LongCommand = 0b111
        }

        // How many bytes each command must encode to outdo Direct Copy
        private static readonly byte[] COMMAND_WEIGHT =
            {
                0,  // Direct Copy
                3,  // Byte Fill
                4,  // Word Fill
                3,  // Increasing Fill
                4   // Repeat
            };

        public static byte[] Compress(byte[] data, CompressionType CompressionType)
        {
            // Greedy implementation

            if (data == null)
            {
                throw new Exception("Data is null.");
            }

            List<byte> output = new();
            int position = 0;
            int length = data.Length;

            List<byte> directCopyBuffer = null;

            while (position < length)
            {
                byte currentByte = data[position++];
                byte nextByte = 0;
                ushort repeatAddress = 0;

                int[] byteCount = new int[(int)LzCommands.Repeat + 1];

                // Evaluate Byte Fill
                byteCount[(int)LzCommands.ByteFill] = 1;
                {
                    for (int i = position; i < length && WithinSizeLimit(i); i++)
                    {
                        if (data[i] != currentByte)
                        {
                            break;
                        }
                        byteCount[(int)LzCommands.ByteFill]++;
                    }
                }

                // Evaluate Word Fill
                byteCount[(int)LzCommands.WordFill] = 1;
                {
                    if (position < length)
                    {
                        byteCount[(int)LzCommands.WordFill]++;
                        nextByte = data[position];
                        int oddEven = 0;

                        for (int i = position + 1; i < length && WithinSizeLimit(byteCount[(int)LzCommands.WordFill] + 1); i++, oddEven++)
                        {
                            byte currentOddEvenByte = (oddEven & 1) == 0 ? currentByte : nextByte;
                            if (data[i] != currentOddEvenByte)
                            {
                                break;
                            }
                            byteCount[(int)LzCommands.WordFill]++;
                        }
                    }
                }

                // Evaluate Increasing Fill
                byteCount[(int)LzCommands.IncreaseFill] = 1;
                {
                    byte increaseByte = (byte)(currentByte + 1);
                    for (int i = position; i < length && WithinSizeLimit(byteCount[(int)LzCommands.IncreaseFill] + 1); i++)
                    {
                        if (data[i] != increaseByte++)
                        {
                            break;
                        }
                        byteCount[(int)LzCommands.IncreaseFill]++;
                    }
                }

                // Evaluate Repeat
                byteCount[(int)LzCommands.Repeat] = 0;
                {
                    //Slow O(n^2) brute force algorithm for now
                    int maxAddressInt = Math.Min(0xFFFF, position - 2);
                    if (maxAddressInt >= 0)
                    {
                        ushort maxAddress = (ushort)maxAddressInt;
                        for (int start = 0; start <= maxAddress; start++)
                        {
                            int chunkSize = 0;

                            for (int pos = position - 1; pos < length && WithinSizeLimit(chunkSize); pos++)
                            {
                                if (data[pos] != data[start + chunkSize])
                                {
                                    break;
                                }
                                chunkSize++;
                            }

                            if (chunkSize > byteCount[(int)LzCommands.Repeat] && WithinSizeLimit(chunkSize))
                            {
                                repeatAddress = (ushort)start;
                                byteCount[(int)LzCommands.Repeat] = chunkSize;
                            }
                        }
                    }
                }

                // Choose next command
                LzCommands nextCommand = LzCommands.DirectCopy; // Default command unless anything better is found
                int nextCommandByteCount = 1;
                for (byte commandSuggestion = 1; commandSuggestion < byteCount.Length; commandSuggestion++)
                {
                    // Would this command save any space?
                    if (byteCount[commandSuggestion] >= COMMAND_WEIGHT[commandSuggestion])
                    {
                        // Is it better than what we already have?
                        if (byteCount[commandSuggestion] > nextCommandByteCount)
                        {
                            if (!WithinSizeLimit(byteCount[commandSuggestion]))
                            {
                                throw new Exception("Internal error: Length assertion failed.");
                            }
                            nextCommand = (LzCommands)commandSuggestion;
                            nextCommandByteCount = byteCount[commandSuggestion];
                        }
                    }
                }

                // Direct Copy commands are incrementally built.
                // Output or add to as needed.
                if (nextCommand == (int)LzCommands.DirectCopy)
                {
                    if (directCopyBuffer == null)
                    {
                        directCopyBuffer = new List<byte>();
                    }
                    directCopyBuffer.Add(currentByte);

                    if (directCopyBuffer.Count >= 1023)
                    {
                        // Direct Copy has a maximum length of 1023 bytes
                        OutputCommand(LzCommands.DirectCopy, directCopyBuffer.Count, output);
                        output.AddRange(directCopyBuffer);
                        directCopyBuffer = null;
                    }
                }
                else
                {
                    if (directCopyBuffer != null)
                    {
                        // Direct Copy command in progress. Write it to output before proceeding
                        OutputCommand(LzCommands.DirectCopy, directCopyBuffer.Count, output);
                        output.AddRange(directCopyBuffer);
                        directCopyBuffer = null;
                    }
                }

                // Output command
                switch (nextCommand)
                {
                    case (int)LzCommands.DirectCopy:
                        // Already handled above
                        break;
                    case LzCommands.ByteFill:
                        OutputCommand(nextCommand, nextCommandByteCount, output);
                        output.Add(currentByte);
                        break;
                    case LzCommands.WordFill:
                        OutputCommand(nextCommand, nextCommandByteCount, output);
                        output.Add(currentByte);
                        output.Add(nextByte);
                        break;
                    case LzCommands.IncreaseFill:
                        OutputCommand(nextCommand, nextCommandByteCount, output);
                        output.Add(currentByte);
                        break;
                    case LzCommands.Repeat:
                        OutputCommand(nextCommand, nextCommandByteCount, output);

                        switch (CompressionType)
                        {
                            case CompressionType.LZ1:
                                output.Add((byte)repeatAddress);
                                output.Add((byte)(repeatAddress >> 8));
                                break;
                            case CompressionType.LZ2:
                                output.Add((byte)(repeatAddress >> 8));
                                output.Add((byte)repeatAddress);
                                break;
                            default:
                                throw new Exception("Not a valid compression type.");
                        }

                        break;
                    default:
                        throw new Exception("Internal error: Unknown command chosen.");
                }

                position += (nextCommandByteCount) - 1;
            }

            // Output Direct Copy buffer if it exists
            if (directCopyBuffer != null)
            {
                OutputCommand((int)LzCommands.DirectCopy, directCopyBuffer.Count, output);
                output.AddRange(directCopyBuffer);
            }

            output.Add(0xFF);
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] CompressedData, uint Start, CompressionType CompressionType)
        {
            if (CompressedData == null)
            {
                throw new Exception("Compressed data is null.");
            }
            try
            {
                List<byte> output = new();
                uint position = Start;

                while (true)
                {
                    byte commandLength = CompressedData[position++];
                    if (commandLength == 0xFF)
                    {
                        break;
                    }

                    if (output.Count == 0x1676)
                    {

                    }

                    LzCommands command = (LzCommands)(commandLength >> 5);
                    int length;
                    if (command == LzCommands.LongCommand) // Long command
                    {
                        length = CompressedData[position++];
                        length |= ((commandLength & 3) << 8);
                        length++;
                        command = (LzCommands)((commandLength >> 2) & 7);
                    }
                    else
                    {
                        length = (commandLength & 0x1F) + 1;
                    }

                    switch (command)
                    {
                        case LzCommands.DirectCopy: // Direct Copy
                            for (int i = 0; i < length; i++)
                            {
                                output.Add(CompressedData[position++]);
                            }
                            break;
                        case LzCommands.ByteFill: // Byte Fill
                            byte fillByte = CompressedData[position++];
                            for (int i = 0; i < length; i++)
                            {
                                output.Add(fillByte);
                            }
                            break;
                        case LzCommands.WordFill: // Word Fill
                            byte fillByteEven = CompressedData[position++];
                            byte fillByteOdd = CompressedData[position++];
                            for (int i = 0; i < length; i++)
                            {
                                byte thisByte = (i & 1) == 0 ? fillByteEven : fillByteOdd;
                                output.Add(thisByte);
                            }
                            break;
                        case LzCommands.IncreaseFill: // Increasing Fill
                            byte increaseFillByte = CompressedData[position++];
                            for (int i = 0; i < length; i++)
                            {
                                output.Add(increaseFillByte++);
                            }
                            break;
                        case LzCommands.Repeat: // Repeat
                            ushort origin;
                            switch (CompressionType)
                            {
                                case CompressionType.LZ1:
                                    origin = (ushort)((CompressedData[position + 1] << 8) | CompressedData[position]);
                                    position += 2;
                                    break;
                                case CompressionType.LZ2:
                                    origin = (ushort)((CompressedData[position] << 8) | CompressedData[position + 1]);
                                    position += 2;
                                    break;
                                default:
                                    throw new Exception("Not a valid compression type.");
                            }

                            for (int i = 0; i < length; i++)
                            {
                                output.Add(output[origin++]);
                            }
                            break;

                        default:
                            throw new Exception("Invalid Lz command: " + command);
                    }
                }

                return output.ToArray();
            }
            catch (IndexOutOfRangeException)
            {
                throw new Exception("Reached unexpected end of compressed data.");
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception("Compressed data contains invalid Lz Repeat command.");
            }
        }


        private static bool WithinSizeLimit(int length)
        {
            return !(length < 1 || length >= 1024);
        }

        private static void OutputCommand(LzCommands command, int length, List<byte> output)
        {
            if (!WithinSizeLimit(length))
            {
                throw new Exception("Internal error: Length assertion failed.");
            }

            if (length > 32)
            {
                // Long command
                length--;
                byte firstByte = (byte)(0xE0 | ((int)command << 2) | (length >> 8));
                byte secondByte = (byte)length;
                output.Add(firstByte);
                output.Add(secondByte);
            }
            else
            {
                // Short command
                length--;
                byte commandLength = (byte)((int)command << 5 | length);
                output.Add(commandLength);
            }
        }
    }
}
