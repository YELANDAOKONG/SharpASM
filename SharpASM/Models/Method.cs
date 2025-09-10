using SharpASM.Models.Type;

namespace SharpASM.Models;

public class Method
{
    public MethodAccessFlags AccessFlags { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Descriptor { get; set; } = string.Empty;
    public List<Attribute> Attributes { get; set; } = new();
}