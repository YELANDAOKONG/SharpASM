namespace SharpASM.Models;

public class Attribute
{
    public string Name { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}