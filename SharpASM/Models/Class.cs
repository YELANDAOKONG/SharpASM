using SharpASM.Models.Type;

namespace SharpASM.Models;

public class Class
{
    
    public uint Magic { get; set; } = 0xCAFEBABE;
    public ushort MinorVersion { get; set; } = 0;
    public ClassFileVersion MajorVersion { get; set; } = ClassFileVersion.V17;
    
    public ushort ConstantPoolCount => (ushort)(ConstantPool.Count + 1);
    public List<ConstantPoolInfo> ConstantPool { get; set; } = new();
    public ClassAccessFlags AccessFlags { get; set; }
    
    public ushort ThisClass { get; set; }
    public ushort SuperClass { get; set; }
    
    public ushort InterfacesCount => (ushort)Interfaces.Count;
    public List<ushort> Interfaces { get; set; } = new();
    
    public ushort FieldsCount => (ushort)Fields.Count;
    
    public List<Field> Fields { get; set; } = new();
    
    public ushort MethodsCount => (ushort)Methods.Count;
    
    public List<Method> Methods { get; set; } = new();
    
    public ushort AttributesCount => (ushort)Attributes.Count;
    
    public List<Attribute> Attributes { get; set; } = new();
    
}