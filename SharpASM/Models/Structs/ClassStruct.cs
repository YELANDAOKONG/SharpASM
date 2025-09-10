using System.Text;
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

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append("@Class {\n");
        builder.Append($"    Magic: 0x{Magic:X},\n");
        builder.Append($"    MinorVersion: {MinorVersion},\n");
        builder.Append($"    MajorVersion: {MajorVersion},\n");
        builder.Append($"    ConstantPoolCount: {ConstantPoolCount},\n");
        builder.Append($"    ConstantPool: [\n");
        foreach (var poolInfoStruct in ConstantPool)
        {
            if (poolInfoStruct == null)
            {
                continue;
            }
            builder.Append($"        {{\n");
            builder.Append($"            Tag: {poolInfoStruct.Tag},\n");
            builder.Append($"            Info: 0x{BitConverter.ToString(poolInfoStruct.Info).Replace("-", "")},\n");
            builder.Append($"        }},\n");
        }
        builder.Append($"    ],\n");
        builder.Append($"    AccessFlags: {AccessFlags},\n");
        builder.Append($"    ThisClass: {ThisClass},\n");
        builder.Append($"    SuperClass: {SuperClass},\n");
        builder.Append($"    InterfacesCount: {InterfacesCount},\n");
        builder.Append($"    Interfaces: [\n");
        foreach (var @interface in Interfaces)
        {
            builder.Append($"        {@interface},\n");
        }
        builder.Append($"    ],\n");
        builder.Append($"    FieldsCount: {FieldsCount},\n");
        builder.Append($"    Fields: [\n");
        foreach (var field in Fields)
        {
            if (field == null)
            {
                continue;
            }
            builder.Append($"        {{\n");
            builder.Append($"            AccessFlags: {field.AccessFlags},\n");
            builder.Append($"            NameIndex: {field.NameIndex},\n");
            builder.Append($"            DescriptorIndex: {field.DescriptorIndex},\n");
            builder.Append($"            AttributesCount: {field.AttributesCount},\n");
            builder.Append($"            Attributes: [\n");
            foreach (var attribute in field.Attributes)
            {
                if (attribute == null)
                {
                    continue;
                }
                builder.Append($"                {{\n");
                builder.Append($"                    AttributeNameIndex: {attribute.AttributeNameIndex},\n");
                builder.Append($"                    AttributeLength: {attribute.AttributeLength},\n");
                builder.Append($"                    Info: 0x{BitConverter.ToString(attribute.Info).Replace("-", "")},\n");
                builder.Append($"                }}\n");
            }
            builder.Append($"            ],\n");
            builder.Append($"        }},\n");
        }
        builder.Append($"    ],\n");
        builder.Append($"    MethodsCount: {MethodsCount},\n");
        builder.Append($"    Methods: [\n");
        foreach (var method in Methods)
        {
            if (method == null)
            {
                continue;
            }
            builder.Append($"        {{\n");
            builder.Append($"            AccessFlags: {method.AccessFlags},\n");
            builder.Append($"            NameIndex: {method.NameIndex},\n");
            builder.Append($"            DescriptorIndex: {method.DescriptorIndex},\n");
            builder.Append($"            AttributesCount: {method.AttributesCount},\n");
            builder.Append($"            Attributes: [\n");
            foreach (var attribute in method.Attributes)
            {
                if (attribute == null)
                {
                    continue;
                }
                builder.Append($"                {{\n");
                builder.Append($"                    AttributeNameIndex: {attribute.AttributeNameIndex},\n");
                builder.Append($"                    AttributeLength: {attribute.AttributeLength},\n");
                builder.Append($"                    Info: 0x{BitConverter.ToString(attribute.Info).Replace("-", "")},\n");
                builder.Append($"                }}\n");
            }
            builder.Append($"            ],\n");
            builder.Append($"        }},\n");
        }
        builder.Append($"    ],\n");
        builder.Append($"    AttributesCount: {AttributesCount},\n");
        builder.Append($"    Attributes: [\n");
        foreach (var attribute in Attributes)
        {
            if (attribute == null)
            {
                continue;
            }
            builder.Append($"        {{\n");
            builder.Append($"            AttributeNameIndex: {attribute.AttributeNameIndex}\n");
            builder.Append($"            AttributeLength: {attribute.AttributeLength},\n");
            builder.Append($"            Info: 0x{BitConverter.ToString(attribute.Info).Replace("-", "")},\n");
            builder.Append($"        }},\n");
        }
        builder.Append($"    ],\n");
        builder.Append("}");
        return builder.ToString();
    }
}