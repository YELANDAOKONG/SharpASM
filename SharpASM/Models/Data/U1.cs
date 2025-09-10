namespace SharpASM.Models.Data;

public struct U1 : IU
{
    public byte Byte1 { get; set; }
    
    public byte[] ToBytes(bool bigEndian = true)
    {
        return new byte[] { Byte1 };
    }
    
    public static U1 FromBytes(byte[] bytes, bool bigEndian = true)
    {
        if (bytes.Length < 1)
            throw new ArgumentException("Insufficient byte array length");
        return new U1 { Byte1 = bytes[0] };
    }
}