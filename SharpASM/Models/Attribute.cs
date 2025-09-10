namespace SharpASM.Models;

public class Attribute
{
    public string Name { get; set; } = string.Empty;
    public byte[] Info { get; set; } = Array.Empty<byte>();
}