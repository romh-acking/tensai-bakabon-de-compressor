namespace Libraries
{
    public class AddressRAM
    {
        public static uint PcToSnes(uint PcAddress, out bool IsValidPcAddress)
        {
            uint SnesAddress = 0;

            if (PcAddress < 0xC13 || PcAddress >= 0x20C13)
            {
                IsValidPcAddress = false;
            }
            else
            {
                IsValidPcAddress = true;
                SnesAddress = PcAddress - 0xC13 + 0x7E0000;

            }
            return SnesAddress;
        }
        public static uint SnesToPc(uint SnesAddress, out bool IsValidSnesAddress)
        {
            uint PcAddress = 0;

            if (SnesAddress >= 0x7E0000 && SnesAddress <= 0x7FFFFF)
            {
                IsValidSnesAddress = true;
                PcAddress = (0x1FFFF & SnesAddress) + 0xC13;
            }
            else
            {
                IsValidSnesAddress = false;
            }

            return PcAddress;
        }
    }
}
