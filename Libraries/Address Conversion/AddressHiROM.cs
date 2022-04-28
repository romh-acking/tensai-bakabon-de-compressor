namespace Libraries
{
    public class AddressHiRom
    {
        public static uint PcToSnes(uint PcAddress, out bool IsValidPcAddress)
        {
            uint SnesAddress = 0;

            if (PcAddress >= 0x400000)
            {
                IsValidPcAddress = false;
            }
            else
            {
                IsValidPcAddress = true;
                SnesAddress = PcAddress | 0xC00000;
            }
            return SnesAddress;
        }
        public static uint SnesToPc(uint SnesAddress, out bool IsValidSnesAddress)
        {
            uint PcAddress = 0;
            if (SnesAddress >= 0xC00000 && SnesAddress <= 0xFFFFFF)
            {
                IsValidSnesAddress = true;
                PcAddress = (SnesAddress & 0x3FFFFF);
            }
            else
            {
                IsValidSnesAddress = false;
            }

            return PcAddress;
        }
    }
}
