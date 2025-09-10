namespace SharpASM.Models.Data;

public struct U3 : IU
{
    public byte Byte1 { get; set; }
    public byte Byte2 { get; set; }
    public byte Byte3 { get; set; }
    
    public byte[] ToBytes(bool bigEndian = true)
    {
        return bigEndian ?
            new byte[] { Byte1, Byte2, Byte3 } :
            new byte[] { Byte3, Byte2, Byte1 };
    }
    
    public static U3 FromBytes(byte[] bytes, bool bigEndian = true)
    {
        if (bytes.Length < 3)
            throw new ArgumentException("Insufficient byte array length");
        return bigEndian ?
            new U3 { Byte1 = bytes[0], Byte2 = bytes[1], Byte3 = bytes[2] } :
            new U3 { Byte1 = bytes[2], Byte2 = bytes[1], Byte3 = bytes[0] };
    }
}