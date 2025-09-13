namespace SharpASM.Models;

public abstract class AttributeBase
{
    public string Name { get; set; } = string.Empty;
    
    public abstract byte[] ToBytes();
    public abstract void FromBytes(byte[] data);
}