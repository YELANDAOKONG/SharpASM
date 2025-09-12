using SharpASM.Models.Type;

namespace SharpASM.Models.Struct.Interfaces;

public interface IConstantStruct
{
    public byte GetTag();
    
    public byte[] ToBytes();
    public byte[] ToBytesWithoutTag();
    
    public ConstantPoolInfoStruct ToStructInfo();
}