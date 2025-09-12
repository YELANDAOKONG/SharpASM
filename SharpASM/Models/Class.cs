using SharpASM.Models.Struct;
using SharpASM.Models.Type;
using SharpASM.Utilities;
using System.Text;

namespace SharpASM.Models;

public class Class
{
    public uint Magic { get; set; } = 0xCAFEBABE;
    public ushort MinorVersion { get; set; } = 0;
    public ClassFileVersion MajorVersion { get; set; } = ClassFileVersion.V17;
    
    public List<ConstantPoolInfo> ConstantPool { get; set; } = [];
    public ushort ConstantPoolCount => CalculateConstantPoolCount();
    public ushort ConstantPoolIndexCount => CalculateConstantPoolIndexCount();
    
    public ClassAccessFlags AccessFlags { get; set; }
    
    public string ThisClass { get; set; } = string.Empty;
    public string SuperClass { get; set; } = "java/lang/Object";
    
    public ushort InterfacesCount => (ushort)Interfaces.Count;
    public List<string> Interfaces { get; set; } = new();
    
    public List<Field> Fields { get; set; } = new();
    public List<Method> Methods { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();

    public ushort CalculateConstantPoolCount()
    {
        return (ushort)(ConstantPool.Count + 1);
    }

    public ushort CalculateConstantPoolIndexCount()
    {
        ushort count = 1;
        
        foreach (var constant in ConstantPool)
        {
            if (constant.Tag == ConstantPoolTag.Long || 
                constant.Tag == ConstantPoolTag.Double)
            {
                count += 2;
            }
            else
            {
                count += 1;
            }
        }
        
        return count;
    }

    #region Functions

    public ConstantPoolHelper GetConstantPoolHelper()
    {
        return new ConstantPoolHelper(ConstantPool);
    }

    #endregion

    #region Functions (Struct)

    public static Class FromStruct(ClassStruct classStruct)
    {
        var clazz = new Class
        {
            Magic = classStruct.Magic,
            MinorVersion = classStruct.MinorVersion,
            MajorVersion = (ClassFileVersion)classStruct.MajorVersion,
            AccessFlags = (ClassAccessFlags)classStruct.AccessFlags
        };

        // Store the complete constant pool
        clazz.ConstantPool = new List<ConstantPoolInfo>();
        for (int i = 0; i < classStruct.ConstantPool.Length; i++)
        {
            var cpInfo = classStruct.ConstantPool[i];
            if (cpInfo != null)
            {
                clazz.ConstantPool.Add(ConstantPoolInfo.FromStruct(cpInfo));
            }
        }

        // ThisClass & SuperClass
        clazz.ThisClass = GetStringFromConstantPool(clazz, classStruct.ThisClass) ?? throw new NullReferenceException();
        clazz.SuperClass = GetStringFromConstantPool(clazz, classStruct.SuperClass) ?? throw new NullReferenceException();

        // Interfaces
        clazz.Interfaces = new List<string>();
        for (int i = 0; i < classStruct.InterfacesCount; i++)
        {
            var interfaceIndex = classStruct.Interfaces[i];
            var interfaceName = GetStringFromConstantPool(clazz, interfaceIndex);
            if (interfaceName != null)
                clazz.Interfaces.Add(interfaceName);
        }

        // Fields
        clazz.Fields = new List<Field>();
        foreach (var fieldStruct in classStruct.Fields)
        {
            var field = new Field
            {
                AccessFlags = (FieldAccessFlags)fieldStruct.AccessFlags,
                Name = GetStringFromConstantPool(clazz, fieldStruct.NameIndex) ?? throw new NullReferenceException(),
                Descriptor = GetStringFromConstantPool(clazz, fieldStruct.DescriptorIndex) ?? throw new NullReferenceException()
            };

            // Field Attributes
            field.Attributes = ParseAttributes(clazz, fieldStruct.Attributes);
            clazz.Fields.Add(field);
        }

        // Methods
        clazz.Methods = new List<Method>();
        foreach (var methodStruct in classStruct.Methods)
        {
            var method = new Method
            {
                AccessFlags = (MethodAccessFlags)methodStruct.AccessFlags,
                Name = GetStringFromConstantPool(clazz, methodStruct.NameIndex) ?? throw new NullReferenceException(),
                Descriptor = GetStringFromConstantPool(clazz, methodStruct.DescriptorIndex) ?? throw new NullReferenceException()
            };

            // Method Attributes
            method.Attributes = ParseAttributes(clazz, methodStruct.Attributes);
            clazz.Methods.Add(method);
        }

        // Class Attributes
        clazz.Attributes = ParseAttributes(clazz, classStruct.Attributes);

        return clazz;
    }

    public ClassStruct ToStruct()
    {
        var classStruct = new ClassStruct
        {
            Magic = Magic,
            MinorVersion = MinorVersion,
            MajorVersion = (ushort)MajorVersion,
            AccessFlags = (ushort)AccessFlags
        };

        var constantPoolHelper = new ConstantPoolHelper(new List<ConstantPoolInfo>(ConstantPool));

        // Add new constant pool entries for any new strings
        constantPoolHelper.NewUtf8(ThisClass);
        constantPoolHelper.NewUtf8(SuperClass);
        
        foreach (var interfaceName in Interfaces)
        {
            constantPoolHelper.NewUtf8(interfaceName);
        }
        
        foreach (var field in Fields)
        {
            constantPoolHelper.NewUtf8(field.Name);
            constantPoolHelper.NewUtf8(field.Descriptor);
            foreach (var attr in field.Attributes)
            {
                constantPoolHelper.NewUtf8(attr.Name);
            }
        }
        
        foreach (var method in Methods)
        {
            constantPoolHelper.NewUtf8(method.Name);
            constantPoolHelper.NewUtf8(method.Descriptor);
            foreach (var attr in method.Attributes)
            {
                constantPoolHelper.NewUtf8(attr.Name);
            }
        }
        
        foreach (var attr in Attributes)
        {
            constantPoolHelper.NewUtf8(attr.Name);
        }

        // ThisClass & SuperClass Index
        classStruct.ThisClass = constantPoolHelper.NewClass(ThisClass);
        classStruct.SuperClass = constantPoolHelper.NewClass(SuperClass);

        // Interfaces
        classStruct.InterfacesCount = (ushort)Interfaces.Count;
        classStruct.Interfaces = new ushort[Interfaces.Count];
        for (int i = 0; i < Interfaces.Count; i++)
        {
            classStruct.Interfaces[i] = constantPoolHelper.NewClass(Interfaces[i]);
        }

        // Fields
        classStruct.FieldsCount = (ushort)Fields.Count;
        classStruct.Fields = new FieldInfoStruct[Fields.Count];
        for (int i = 0; i < Fields.Count; i++)
        {
            var field = Fields[i];
            classStruct.Fields[i] = new FieldInfoStruct
            {
                AccessFlags = (ushort)field.AccessFlags,
                NameIndex = constantPoolHelper.NewUtf8(field.Name),
                DescriptorIndex = constantPoolHelper.NewUtf8(field.Descriptor),
                AttributesCount = (ushort)field.Attributes.Count,
                Attributes = ConvertAttributes(field.Attributes, constantPoolHelper)
            };
        }

        // Methods
        classStruct.MethodsCount = (ushort)Methods.Count;
        classStruct.Methods = new MethodInfoStruct[Methods.Count];
        for (int i = 0; i < Methods.Count; i++)
        {
            var method = Methods[i];
            classStruct.Methods[i] = new MethodInfoStruct
            {
                AccessFlags = (ushort)method.AccessFlags,
                NameIndex = constantPoolHelper.NewUtf8(method.Name),
                DescriptorIndex = constantPoolHelper.NewUtf8(method.Descriptor),
                AttributesCount = (ushort)method.Attributes.Count,
                Attributes = ConvertAttributes(method.Attributes, constantPoolHelper)
            };
        }

        // Class Attributes
        classStruct.AttributesCount = (ushort)Attributes.Count;
        classStruct.Attributes = ConvertAttributes(Attributes, constantPoolHelper);

        // Constant Pool
        classStruct.ConstantPool = constantPoolHelper.ToArray();
        classStruct.ConstantPoolCount = constantPoolHelper.ConstantPoolIndexCount;

        return classStruct;
    }

    private static string? GetStringFromConstantPool(Class clazz, ushort index)
    {
        if (index == 0 || index >= clazz.ConstantPoolIndexCount) return null;

        int currentIndex = 1;
        for (int i = 0; i < clazz.ConstantPool.Count; i++)
        {
            var constant = clazz.ConstantPool[i];
            if (currentIndex == index)
            {
                var constantStruct = constant.ToStruct().ToConstantStruct();
                return constantStruct switch
                {
                    ConstantUtf8InfoStruct utf8 => utf8.ToString(),
                    ConstantClassInfoStruct classInfo => GetStringFromConstantPool(clazz, classInfo.NameIndex),
                    ConstantStringInfoStruct stringInfo => GetStringFromConstantPool(clazz, stringInfo.NameIndex),
                    ConstantNameAndTypeInfoStruct nameAndType => 
                        $"{GetStringFromConstantPool(clazz, nameAndType.NameIndex)}:{GetStringFromConstantPool(clazz, nameAndType.DescriptorIndex)}",
                    _ => null
                };
            }

            currentIndex++;
            if (constant.Tag == ConstantPoolTag.Long || constant.Tag == ConstantPoolTag.Double)
            {
                currentIndex++;
            }
        }

        return null;
    }

    private static List<Attribute> ParseAttributes(Class clazz, AttributeInfoStruct[] attributes)
    {
        var result = new List<Attribute>();
        foreach (var attr in attributes)
        {
            var attribute = new Attribute
            {
                Name = GetStringFromConstantPool(clazz, attr.AttributeNameIndex) ?? throw new NullReferenceException(),
                Info = attr.Info
            };
            result.Add(attribute);
        }
        return result;
    }

    private static AttributeInfoStruct[] ConvertAttributes(List<Attribute> attributes, ConstantPoolHelper constantPoolHelper)
    {
        var result = new AttributeInfoStruct[attributes.Count];
        for (int i = 0; i < attributes.Count; i++)
        {
            var attr = attributes[i];
            result[i] = new AttributeInfoStruct
            {
                AttributeNameIndex = constantPoolHelper.NewUtf8(attr.Name),
                AttributeLength = (uint)attr.Info.Length,
                Info = attr.Info
            };
        }
        return result;
    }

    #endregion
}
