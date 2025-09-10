namespace SharpASM.Utilities
{
    public static class ByteUtils
    {
        public static ushort ReadUInt16(byte[] data, ref int offset)
        {
            ushort value = (ushort)((data[offset] << 8) | data[offset + 1]);
            offset += 2;
            return value;
        }

        public static uint ReadUInt32(byte[] data, ref int offset)
        {
            uint value = (uint)((data[offset] << 24) | (data[offset + 1] << 16) | 
                                (data[offset + 2] << 8) | data[offset + 3]);
            offset += 4;
            return value;
        }

        public static void WriteUInt16(ushort value, MemoryStream stream)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        public static void WriteUInt32(uint value, MemoryStream stream)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        public static byte[] ReadBytes(byte[] data, ref int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, offset, result, 0, length);
            offset += length;
            return result;
        }
    }
}