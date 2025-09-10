using SharpASM.Models.Type;

namespace SharpASM.Models;

public class Method
{
    public MethodAccessFlags AccessFlags { get; set; }
    public ushort NameIndex { get; set; }
    public ushort DescriptorIndex { get; set; }
    public List<Attribute> Attributes { get; set; } = new();
}