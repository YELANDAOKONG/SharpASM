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
}