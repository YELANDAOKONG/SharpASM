namespace SharpASM.Models.Data;

public struct U2 : IU
{
    public byte Byte1 { get; set; }
    public byte Byte2 { get; set; }
    
    public byte[] ToBytes(bool bigEndian = true)
    {
        return bigEndian ? 
            new byte[] { Byte1, Byte2 } : 
            new byte[] { Byte2, Byte1 };
    }
    
    public static U2 FromBytes(byte[] bytes, bool bigEndian = true)
    {
        if (bytes.Length < 2) 
            throw new ArgumentException("Insufficient byte array length");
        return bigEndian ?
            new U2 { Byte1 = bytes[0], Byte2 = bytes[1] } :
            new U2 { Byte1 = bytes[1], Byte2 = bytes[0] };
    }

    public ushort ToUInt16(bool bigEndian = true)
    {
        if (bigEndian)
        {
            return (ushort)((Byte1 << 8) | Byte2);
        }
        else
        {
            return (ushort)((Byte2 << 8) | Byte1);
        }
    }

    public static U2 FromUInt16(ushort data, bool bigEndian = true)
    {
        if (bigEndian)
        {
            return new U2 
            { 
                Byte1 = (byte)(data >> 8), 
                Byte2 = (byte)data 
            };
        }
        else
        {
            return new U2 
            { 
                Byte1 = (byte)data, 
                Byte2 = (byte)(data >> 8) 
            };
        }
    }
}