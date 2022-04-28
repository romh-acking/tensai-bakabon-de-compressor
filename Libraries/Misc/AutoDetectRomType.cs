using Libraries;
using System.IO;
using System.Text;

namespace LunarAddress.Conversion
{
    public static class AutoDetectRomType
    {
        public static RomType Detect(byte[] RomData)
        {
            Stream s = new MemoryStream(RomData);
            RomType Type = RomType.Invalid;

            byte ptr;

            using (BinaryReader b = new BinaryReader(s, Encoding.ASCII, true))
            {
                s.Position = 0x7FDC;

                // Read the header for rom type info
                // The header location differs between HiROM and LoROM.

                // LoRom
                if ((b.ReadUInt16() ^ b.ReadUInt16()) == 0xFFFF)
                {
                    s.Position = 0x7FD5;
                    ptr = b.ReadByte();
                }
                else
                {
                    // HiRom
                    s.Position = 0xFFDC;

                    if ((b.ReadUInt16() ^ b.ReadUInt16()) != 0xFFFF)
                    {
                        throw new System.Exception("Cannot determine ROM memory map type.");
                    }
                    else
                    {
                        s.Position = 0xFFD5;
                        ptr = b.ReadByte();
                    }
                }

                // Some of these settings appear to contradict this documentation
                // https://problemkaputt.de/fullsnes.htm#snescartridgeromheader
                // But this code matches Lunar Address, so I'll leave it be for now.

                if ((ptr & 0xF) == 5)
                {
                    Type = RomType.ExHiROM;
                }
                else if ((ptr & 0xF) == 3)
                {
                    Type = RomType.HiROM;
                }
                else if ((ptr & 1) == 1)
                {
                    if (RomData.Length <= 0x400000)
                    {
                        Type = RomType.HiROM;
                    }
                    else
                    {
                        Type = RomType.ExHiROM;
                    }
                }
                else if (RomData.Length <= 0x400000)
                {
                    Type = (ptr >> 4) >= 3 ? RomType.LoROM2 : RomType.LoROM1;
                }
                else
                {
                    Type = RomType.ExLoROM;
                }
            }

            return Type;
        }
    }
}