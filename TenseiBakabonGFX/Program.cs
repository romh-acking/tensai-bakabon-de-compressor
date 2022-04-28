using Libraries;
using Libraries.Compression;
using System;
using System.IO;
using System.Linq;

namespace Tensei_Bakabon_GFX
{
    class Program
    {
        public enum WriteArgs
        {
            action,
            uncompressedPath,
            compressedPath,
            interleveNumber,
        }

        public enum DumpArgs
        {
            action,
            romPath,
            addressStart,
            dumpPath,
            interleveNumber,
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new Exception($"Cannot have 0 arguments. {args[0]}");
            }

            var action = args[0];
            int requiredLength;
            int interleveNumber;

            switch (action)
            {
                case "Write":
                    Console.WriteLine($"Writing");

                    requiredLength = (int)Enum.GetValues(typeof(WriteArgs)).Cast<WriteArgs>().Max() + 1;
                    if (args.Length != requiredLength)
                    {
                        throw new Exception($"Required argument number: {requiredLength}. Received: {args.Length}");
                    }

                    var uncompressedPath = args[(int)WriteArgs.uncompressedPath];
                    var compressedPath = args[(int)WriteArgs.compressedPath];
                    interleveNumber = int.Parse(args[(int)WriteArgs.interleveNumber]);

                    WriteAllGeneral(uncompressedPath, compressedPath, interleveNumber);
                    break;
                case "Dump":
                    Console.WriteLine($"Dumping");

                    requiredLength = (int)Enum.GetValues(typeof(DumpArgs)).Cast<DumpArgs>().Max() + 1;
                    if (args.Length != requiredLength)
                    {
                        throw new Exception($"Required argument number: {requiredLength}. Received: {args.Length}");
                    }

                    var romPath = args[(int)DumpArgs.romPath];
                    var addressStart = MyMath.HexToDec(args[(int)DumpArgs.addressStart]);
                    var dumpPath = args[(int)DumpArgs.dumpPath];
                    interleveNumber = int.Parse(args[(int)DumpArgs.interleveNumber]);
                    DumpAllGeneral(romPath, (uint)addressStart, dumpPath, interleveNumber);
                    break;
                default:
                    throw new Exception($"Invalid first parameter: {action}");
            }

            Console.WriteLine($"Finished successfully.");
        }

        private static void WriteAllGeneral(string uncompressedPath, string compressedPath, int interleveNumber)
        {
            try
            {
                var uncompressed = File.ReadAllBytes(uncompressedPath);
                File.WriteAllBytes(compressedPath, RLEBakabon.Compress(uncompressed, interleveNumber));
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                System.Environment.Exit(1);
            }
        }

        private static void DumpAllGeneral(string romPath, uint start, string dumpPath, int interleveNumber)
        {
            var rom = File.ReadAllBytes(romPath);
            File.WriteAllBytes(dumpPath, RLEBakabon.Decompress(rom, start, interleveNumber));
        }
    }
}
