using SharpASM.Models.Struct;
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

    public static Class FromStruct(ClassStruct classStruct)
    {
        var cls = new Class
        {
            Magic = classStruct.Magic,
            MinorVersion = classStruct.MinorVersion,
            MajorVersion = (ClassFileVersion)classStruct.MajorVersion,
            AccessFlags = (ClassAccessFlags)classStruct.AccessFlags,
            ThisClass = classStruct.ThisClass,
            SuperClass = classStruct.SuperClass
        };
        
        foreach (var cpStruct in classStruct.ConstantPool)
        {
            var cpInfo = new ConstantPoolInfo
            {
                Tag = (ConstantPoolTag)cpStruct.Tag,
                Info = cpStruct.Info
            };
            cls.ConstantPool.Add(cpInfo);
        }
        
        cls.Interfaces.AddRange(classStruct.Interfaces);
        
        foreach (var fieldStruct in classStruct.Fields)
        {
            var field = new Field
            {
                AccessFlags = (FieldAccessFlags)fieldStruct.AccessFlags,
                NameIndex = fieldStruct.NameIndex,
                DescriptorIndex = fieldStruct.DescriptorIndex
            };
            
            foreach (var attrStruct in fieldStruct.Attributes)
            {
                var attribute = new Attribute
                {
                    AttributeNameIndex = attrStruct.AttributeNameIndex,
                    Info = attrStruct.Info
                };
                field.Attributes.Add(attribute);
            }
            
            cls.Fields.Add(field);
        }
        
        foreach (var methodStruct in classStruct.Methods)
        {
            var method = new Method
            {
                AccessFlags = (MethodAccessFlags)methodStruct.AccessFlags,
                NameIndex = methodStruct.NameIndex,
                DescriptorIndex = methodStruct.DescriptorIndex
            };
            
            foreach (var attrStruct in methodStruct.Attributes)
            {
                var attribute = new Attribute
                {
                    AttributeNameIndex = attrStruct.AttributeNameIndex,
                    Info = attrStruct.Info
                };
                method.Attributes.Add(attribute);
            }
            
            cls.Methods.Add(method);
        }
        
        foreach (var attrStruct in classStruct.Attributes)
        {
            var attribute = new Attribute
            {
                AttributeNameIndex = attrStruct.AttributeNameIndex,
                Info = attrStruct.Info
            };
            cls.Attributes.Add(attribute);
        }
        
        return cls;
    }
    
    public ClassStruct ToStruct()
    {
        var classStruct = new ClassStruct
        {
            Magic = Magic,
            MinorVersion = MinorVersion,
            MajorVersion = (ushort)MajorVersion,
            AccessFlags = (ushort)AccessFlags,
            ThisClass = ThisClass,
            SuperClass = SuperClass,
            InterfacesCount = InterfacesCount,
            Interfaces = Interfaces.ToArray(),
            FieldsCount = FieldsCount,
            MethodsCount = MethodsCount,
            AttributesCount = AttributesCount
        };
        
        classStruct.ConstantPoolCount = ConstantPoolCount;
        classStruct.ConstantPool = new ConstantPoolInfoStruct[ConstantPool.Count];
        for (int i = 0; i < ConstantPool.Count; i++)
        {
            classStruct.ConstantPool[i] = new ConstantPoolInfoStruct
            {
                Tag = (byte)ConstantPool[i].Tag,
                Info = ConstantPool[i].Info
            };
        }
        
        classStruct.Fields = new FieldInfoStruct[Fields.Count];
        for (int i = 0; i < Fields.Count; i++)
        {
            var field = Fields[i];
            var fieldStruct = new FieldInfoStruct
            {
                AccessFlags = (ushort)field.AccessFlags,
                NameIndex = field.NameIndex,
                DescriptorIndex = field.DescriptorIndex,
                AttributesCount = (ushort)field.Attributes.Count
            };
            
            fieldStruct.Attributes = new AttributeInfoStruct[field.Attributes.Count];
            for (int j = 0; j < field.Attributes.Count; j++)
            {
                var attribute = field.Attributes[j];
                fieldStruct.Attributes[j] = new AttributeInfoStruct
                {
                    AttributeNameIndex = attribute.AttributeNameIndex,
                    AttributeLength = (uint)attribute.Info.Length,
                    Info = attribute.Info
                };
            }
            
            classStruct.Fields[i] = fieldStruct;
        }
        
        classStruct.Methods = new MethodInfoStruct[Methods.Count];
        for (int i = 0; i < Methods.Count; i++)
        {
            var method = Methods[i];
            var methodStruct = new MethodInfoStruct
            {
                AccessFlags = (ushort)method.AccessFlags,
                NameIndex = method.NameIndex,
                DescriptorIndex = method.DescriptorIndex,
                AttributesCount = (ushort)method.Attributes.Count
            };
            
            methodStruct.Attributes = new AttributeInfoStruct[method.Attributes.Count];
            for (int j = 0; j < method.Attributes.Count; j++)
            {
                var attribute = method.Attributes[j];
                methodStruct.Attributes[j] = new AttributeInfoStruct
                {
                    AttributeNameIndex = attribute.AttributeNameIndex,
                    AttributeLength = (uint)attribute.Info.Length,
                    Info = attribute.Info
                };
            }
            
            classStruct.Methods[i] = methodStruct;
        }
        
        classStruct.Attributes = new AttributeInfoStruct[Attributes.Count];
        for (int i = 0; i < Attributes.Count; i++)
        {
            var attribute = Attributes[i];
            classStruct.Attributes[i] = new AttributeInfoStruct
            {
                AttributeNameIndex = attribute.AttributeNameIndex,
                AttributeLength = (uint)attribute.Info.Length,
                Info = attribute.Info
            };
        }
        
        return classStruct;
    }
    
    
    
}