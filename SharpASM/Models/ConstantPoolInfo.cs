using SharpASM.Models.Type;

namespace SharpASM.Models;

public abstract class ConstantPoolInfo
{
    public abstract ConstantPoolTag Tag { get; }
    
    public abstract byte[] ToBytes();
    public abstract override string ToString();
}