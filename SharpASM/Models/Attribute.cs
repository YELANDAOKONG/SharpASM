namespace SharpASM.Models;

public class Attribute
{
    // public string Name { get; set; } = string.Empty;
    // public byte[] Data { get; set; } = Array.Empty<byte>();
    public ushort AttributeNameIndex { get; set; }
    public byte[] Info { get; set; } = Array.Empty<byte>();
}