using SharpASM.Models.Struct;
using SharpASM.Models.Type;

namespace SharpASM.Models;

public class Class
{
    
    public uint Magic { get; set; } = 0xCAFEBABE;
    public ushort MinorVersion { get; set; } = 0;
    public ClassFileVersion MajorVersion { get; set; } = ClassFileVersion.V17;
    
    public ClassAccessFlags AccessFlags { get; set; }
    
    public ushort ThisClass { get; set; }
    public ushort SuperClass { get; set; }
    
    public ushort InterfacesCount => (ushort)Interfaces.Count;
    public List<string> Interfaces { get; set; } = new();
    
    public List<Field> Fields { get; set; } = new();
    public List<Method> Methods { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();
}