namespace SharpASM.Models.Attributes;

public class UnknownAttribute : AttributeBase
{
    public byte[] Data { get; set; } = Array.Empty<byte>();

    public override byte[] ToBytes()
    {
        return Data;
    }

    public override void FromBytes(byte[] data)
    {
        Data = data;
    }
}