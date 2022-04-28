namespace Libraries
{
    public class AddressVRAM
    {
        public static uint PcToSnes(uint PcAddress, out bool IsInvalidPcAddress)
        {
            uint SnesAddress = 0;

            if (!(PcAddress >= 0x20C13 && PcAddress < 0x30C13))
            {
                IsInvalidPcAddress = false;
            }
            else
            {
                IsInvalidPcAddress = true;
                SnesAddress = (PcAddress - 0x20C13) >> 1;
            }
            return SnesAddress;
        }
        public static uint SnesToPc(uint SnesAddress, out bool IsValidSnesAddress)
        {
            uint PcAddress = 0;
            if (SnesAddress >= 0x0000 && SnesAddress <= 0x7fff)
            {
                PcAddress = (SnesAddress << 1) + 0x20C13;
                IsValidSnesAddress = true;
            }
            else
            {
                IsValidSnesAddress = false;
            }
            return PcAddress;
        }
    }
}