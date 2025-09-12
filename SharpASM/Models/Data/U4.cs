namespace SharpASM.Models.Data;

public struct U4 : IU
{
    public byte Byte1 { get; set; }
    public byte Byte2 { get; set; }
    public byte Byte3 { get; set; }
    public byte Byte4 { get; set; }
    
    public byte[] ToBytes(bool bigEndian = true)
    {
        return bigEndian ?
            new byte[] { Byte1, Byte2, Byte3, Byte4 } :
            new byte[] { Byte4, Byte3, Byte2, Byte1 };
    }
    
    public static U4 FromBytes(byte[] bytes, bool bigEndian = true)
    {
        if (bytes.Length < 4)
            throw new ArgumentException("Insufficient byte array length");
        return bigEndian ?
            new U4 { Byte1 = bytes[0], Byte2 = bytes[1], Byte3 = bytes[2], Byte4 = bytes[3] } :
            new U4 { Byte1 = bytes[3], Byte2 = bytes[2], Byte3 = bytes[1], Byte4 = bytes[0] };
    }

    public uint ToUInt32(bool bigEndian = true)
    {
        if (bigEndian)
        {
            return (uint)((Byte1 << 24) | (Byte2 << 16) | (Byte3 << 8) | Byte4);
        }
        else
        {
            return (uint)((Byte4 << 24) | (Byte3 << 16) | (Byte2 << 8) | Byte1);
        }
    }

    public static U4 FromUInt32(uint data, bool bigEndian = true)
    {
        if (bigEndian)
        {
            return new U4 
            { 
                Byte1 = (byte)(data >> 24),
                Byte2 = (byte)(data >> 16),
                Byte3 = (byte)(data >> 8),
                Byte4 = (byte)data
            };
        }
        else
        {
            return new U4 
            { 
                Byte1 = (byte)data,
                Byte2 = (byte)(data >> 8),
                Byte3 = (byte)(data >> 16),
                Byte4 = (byte)(data >> 24)
            };
        }
    }
}