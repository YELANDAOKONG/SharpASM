using SharpASM.Models.Type;

namespace SharpASM.Models;

public class ConstantPoolInfo
{
    public ConstantPoolTag Tag { get; set; }
    public byte[] Info { get; set; } = [];
    
    // public abstract byte[] ToBytes();
    // public abstract override string ToString();
}