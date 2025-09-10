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
}