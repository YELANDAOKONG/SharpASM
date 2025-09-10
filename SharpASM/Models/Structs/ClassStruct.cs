using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ClassStruct
{

    public const uint ClassMagic = 0xCAFEBABE;
    
    /*
     * ClassFile {
           u4             magic;
           u2             minor_version;
           u2             major_version;
           u2             constant_pool_count;
           cp_info        constant_pool[constant_pool_count-1];
           u2             access_flags;
           u2             this_class;
           u2             super_class;
           u2             interfaces_count;
           u2             interfaces[interfaces_count];
           u2             fields_count;
           field_info     fields[fields_count];
           u2             methods_count;
           method_info    methods[methods_count];
           u2             attributes_count;
           attribute_info attributes[attributes_count];
       }
     */
    
    public uint Magic { get; set; } = 0xCAFEBABE;
    public ushort MinorVersion { get; set; } = 0;
    public ushort MajorVersion { get; set; } = (ushort)ClassFileVersion.V17;
    
    public ushort ConstantPoolCount { get; set; }
    public ConstantPoolInfoStruct[] ConstantPool { get; set; } = [];
    public ushort AccessFlags { get; set; } 
    public ushort ThisClass { get; set; }
    public ushort SuperClass { get; set; }
    public ushort InterfacesCount { get; set; }
    public ushort[] Interfaces { get; set; } = [];
    public ushort FieldsCount { get; set; }
    public FieldInfoStruct[] Fields { get; set; } = [];
    public ushort MethodsCount { get; set; }
    public MethodInfoStruct[] Methods { get; set; } = [];
    public ushort AttributesCount { get; set; }
    public AttributeInfoStruct[] Attributes { get; set; } = [];
}