namespace SharpASM.Models;

public class MethodInfo
{
    public ushort AccessFlags { get; set; }
    public ushort NameIndex { get; set; }
    public ushort DescriptorIndex { get; set; }
    public ushort AttributesCount { get; set; }
    public List<AttributeInfo> Attributes { get; set; } = [];
}