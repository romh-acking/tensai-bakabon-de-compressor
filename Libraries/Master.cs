using Libraries.Compression;
using System;

namespace Libraries
{
    public static class Master
    {
        public static uint SnestoPC(uint SnesAddress, RomType Type, out bool IsValidAddress)
        {
            var PcAddress = Type switch
            {
                RomType.LoROM1 => AddressLoROM1.SnesToPc(SnesAddress, out IsValidAddress),
                RomType.LoROM2 => AddressLoROM2.SnesToPc(SnesAddress, out IsValidAddress),
                RomType.HiROM => AddressHiRom.SnesToPc(SnesAddress, out IsValidAddress),
                RomType.ExLoROM => AddressExLoROM.SnesToPc(SnesAddress, out IsValidAddress),
                RomType.ExHiROM => AddressExHiROM.SnesToPc(SnesAddress, out IsValidAddress),
                _ => throw new Exception("Invalid rom type."),
            };
            return PcAddress;
        }

        public static uint PcToSnes(uint PcAddress, RomType Type, out bool IsValidAddress)
        {
            var SnesAddress = Type switch
            {
                RomType.LoROM1 => AddressLoROM1.PcToSnes(PcAddress, out IsValidAddress),
                RomType.LoROM2 => AddressLoROM2.PcToSnes(PcAddress, out IsValidAddress),
                RomType.HiROM => AddressHiRom.PcToSnes(PcAddress, out IsValidAddress),
                RomType.ExLoROM => AddressExLoROM.PcToSnes(PcAddress, out IsValidAddress),
                RomType.ExHiROM => AddressExHiROM.PcToSnes(PcAddress, out IsValidAddress),
                _ => throw new Exception("Invalid rom type."),
            };
            return SnesAddress;
        }

        public static byte[] Decompress(byte[] RomData, uint Start, CompressionType Type)
        {
            byte[] Uncompresed = Type switch
            {
                CompressionType.LZ1 => Lz1AndLz2.Decompress(RomData, Start, Type),
                CompressionType.LZ2 => Lz1AndLz2.Decompress(RomData, Start, Type),
                CompressionType.MarioPicross => MarioPicross.Decompress(RomData, Start),
                _ => throw new Exception("Not a valid compression type."),
            };
            return Uncompresed;
        }

        public static byte[] Compress(byte[] Uncompresed, CompressionType Type)
        {
            byte[] Compressed = Type switch
            {
                CompressionType.LZ1 => Lz1AndLz2.Compress(Uncompresed, Type),
                CompressionType.LZ2 => Lz1AndLz2.Compress(Uncompresed, Type),
                CompressionType.MarioPicross => MarioPicross.Compress(Uncompresed),
                _ => throw new Exception("Not a valid compression type."),
            };
            return Compressed;
        }
    }
}