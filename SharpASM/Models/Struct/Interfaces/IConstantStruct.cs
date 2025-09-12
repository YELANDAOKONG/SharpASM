namespace SharpASM.Models.Struct.Interfaces;

public interface IConstantStruct
{
    public byte[] ToBytes();
    public byte[] ToBytesWithoutTag();
}