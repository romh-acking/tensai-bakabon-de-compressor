namespace Libraries
{
    public class AddressExLoROM
    {
        public static uint PcToSnes(uint PcAddress, out bool IsValidPcAddress)
        {
            uint SnesAddress = 0;

            if (PcAddress >= 0x7F0000)
            {
                IsValidPcAddress = false;
            }
            else
            {
                IsValidPcAddress = true;
                SnesAddress = ((PcAddress | 0x8000) & 0xFFFF) | ((PcAddress << 1) & 0x7F0000);

                if (PcAddress < 0x400000)
                {
                    SnesAddress += 0x800000;
                }
            }
            return SnesAddress;
        }
        public static uint SnesToPc(uint SnesAddress, out bool IsValidSnesAddress)
        {
            uint PcAddress = 0;
            if ((SnesAddress >= 0x808000 && SnesAddress <= 0xFFFFFF) || (SnesAddress >= 0x008000 && SnesAddress <= 0x7dffff))
            {
                IsValidSnesAddress = true;

                PcAddress = (SnesAddress & 0x7FFF | ((SnesAddress & 0x7F0000) >> 1));

                if (SnesAddress < 0x800000)
                {
                    PcAddress += 0x400000;
                }
            }
            else
            {
                IsValidSnesAddress = false;
            }

            return PcAddress;
        }
    }
}