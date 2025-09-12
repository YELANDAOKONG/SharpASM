namespace SharpASM.Models.Struct.Interfaces;

public interface IAttributeStruct
{
    public byte[] ToBytes();
    public byte[] ToBytesWithoutIndexAndLength();
    
     public AttributeInfoStruct ToStructInfo();
}