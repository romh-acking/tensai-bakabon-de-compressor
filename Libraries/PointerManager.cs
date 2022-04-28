using System;

namespace Libraries
{
    public static class PointerManager
    {
        public static byte[] CreateAddressCustomFormat(long pcAddress, string[] format)
        {
            byte[] pointerAsBytes = BitConverter.GetBytes(pcAddress);

            byte[] newPointer = new byte[format.Length];

            for (int i = 0; i < format.Length; i++)
            {
                string Val = format[i];

                if (Val.Contains('('))
                {
                    Val = Val.Replace("(", "").Replace(")", "");

                    //get X value in BX
                    int j = int.Parse(Val[1].ToString());

                    //because we reversed the order, we d
                    newPointer[i] = pointerAsBytes[j];
                }
                else
                {
                    newPointer[i] = (byte)MyMath.HexToDec(Val);
                }
            }
            return newPointer;
        }

        public static void ReadLittleEndian(byte[] b, out long PointerAddress)
        {
            byte[] lb = new byte[8];
            Array.Copy(b, 0, lb, 0, b.Length);
            PointerAddress = (long)BitConverter.ToUInt64(lb, 0);
        }

        public static byte[] CreatePointer_LittleEndian(long Address, long Length)
        {
            byte[] Converted = BitConverter.GetBytes(Address);
            byte[] Done = new byte[Length];

            Array.Copy(Converted, 0, Done, 0, Length);

            return Done;
        }
    }
}