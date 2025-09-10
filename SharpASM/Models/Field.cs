using SharpASM.Models.Type;

namespace SharpASM.Models;

public class Field
{
    public FieldAccessFlags AccessFlags { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Descriptor { get; set; } = string.Empty;
    public List<Attribute> Attributes { get; set; } = new();
}