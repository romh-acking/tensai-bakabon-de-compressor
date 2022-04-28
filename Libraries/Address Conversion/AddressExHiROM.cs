namespace Libraries
{
    public class AddressExHiROM
    {
        public static uint PcToSnes(uint PcAddress, out bool IsValidPcAddress)
        {
            uint SnesAddress = 0;

            if (PcAddress >= 0x7E0000)
            {
                IsValidPcAddress = false;
            }
            else
            {
                IsValidPcAddress = true;

                SnesAddress = PcAddress;
                if (PcAddress < 0x400000)
                {
                    SnesAddress |= 0xC00000;
                }

                if (PcAddress >= 0x7E0000)
                {
                    SnesAddress -= 0x400000;
                }
            }
            return SnesAddress;
        }
        public static uint SnesToPc(uint SnesAddress, out bool IsValidSnesAddress)
        {
            uint PcAddress = 0;
            if ((SnesAddress >= 0xC00000 && SnesAddress <= 0xFFFFFF) || (SnesAddress >= 0x400000 && SnesAddress <= 0x7DFFFF))
            {
                IsValidSnesAddress = true;
                PcAddress = SnesAddress & 0x3FFFFF;

                if (SnesAddress < 0xC00000)
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
