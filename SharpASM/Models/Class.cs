using SharpASM.Models.Struct;
using SharpASM.Models.Type;
using SharpASM.Utilities;

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

        var constantPoolStrings = new Dictionary<ushort, string>();
        BuildConstantPoolStrings(classStruct, constantPoolStrings);

        // ThisClass & SuperClass
        clazz.ThisClass = GetStringFromConstantPool(classStruct, classStruct.ThisClass, constantPoolStrings) ?? throw new NullReferenceException();
        clazz.SuperClass = GetStringFromConstantPool(classStruct, classStruct.SuperClass, constantPoolStrings) ?? throw new NullReferenceException();

        // Interfaces
        clazz.Interfaces = new List<string>();
        for (int i = 0; i < classStruct.InterfacesCount; i++)
        {
            var interfaceIndex = classStruct.Interfaces[i];
            var interfaceName = GetStringFromConstantPool(classStruct, interfaceIndex, constantPoolStrings);
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
                Name = GetStringFromConstantPool(classStruct, fieldStruct.NameIndex, constantPoolStrings) ?? throw new NullReferenceException(),
                Descriptor = GetStringFromConstantPool(classStruct, fieldStruct.DescriptorIndex, constantPoolStrings) ?? throw new NullReferenceException()
            };

            // Field Attributes
            field.Attributes = ParseAttributes(classStruct, fieldStruct.Attributes, constantPoolStrings);
            clazz.Fields.Add(field);
        }

        // Methods
        clazz.Methods = new List<Method>();
        foreach (var methodStruct in classStruct.Methods)
        {
            var method = new Method
            {
                AccessFlags = (MethodAccessFlags)methodStruct.AccessFlags,
                Name = GetStringFromConstantPool(classStruct, methodStruct.NameIndex, constantPoolStrings) ?? throw new NullReferenceException(),
                Descriptor = GetStringFromConstantPool(classStruct, methodStruct.DescriptorIndex, constantPoolStrings) ?? throw new NullReferenceException()
            };

            // Method Attributes
            method.Attributes = ParseAttributes(classStruct, methodStruct.Attributes, constantPoolStrings);
            clazz.Methods.Add(method);
        }

        // Class Attributes
        clazz.Attributes = ParseAttributes(classStruct, classStruct.Attributes, constantPoolStrings);

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

        var constantPoolHelper = new ConstantPoolHelper(ConstantPool);

        AddConstantPoolEntries(constantPoolHelper);

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
        classStruct.ConstantPoolCount = (ushort)(constantPoolHelper.ConstantPoolIndexCount);

        return classStruct;
    }

    private static void BuildConstantPoolStrings(ClassStruct classStruct, Dictionary<ushort, string> constantPoolStrings)
    {
        ushort currentIndex = 1;
        for (int i = 0; i < classStruct.ConstantPool.Length; i++)
        {
            var cpInfo = classStruct.ConstantPool[i];
            if (cpInfo == null) continue;

            var constantStruct = cpInfo.ToConstantStruct();
            switch (constantStruct)
            {
                case ConstantUtf8InfoStruct utf8:
                    constantPoolStrings[currentIndex] = utf8.ToString();
                    break;
                case ConstantClassInfoStruct classInfo:
                    // 类名需要递归解析，但这里先存储索引，后续按需解析
                    break;
                case ConstantStringInfoStruct stringInfo:
                    // 字符串需要递归解析，但这里先存储索引，后续按需解析
                    break;
            }

            currentIndex++;
            if (constantStruct is ConstantLongInfoStruct || constantStruct is ConstantDoubleInfoStruct)
            {
                currentIndex++;
            }
        }
    }

    private static string? GetStringFromConstantPool(ClassStruct classStruct, ushort index, Dictionary<ushort, string> constantPoolStrings)
    {
        if (constantPoolStrings.TryGetValue(index, out string? value))
        {
            return value;
        }

        if (index == 0 || index >= classStruct.ConstantPoolCount) return null;

        ushort currentIndex = 1;
        for (int i = 0; i < classStruct.ConstantPool.Length; i++)
        {
            var cpInfo = classStruct.ConstantPool[i];
            if (cpInfo == null) continue;

            if (currentIndex == index)
            {
                var constantStruct = cpInfo.ToConstantStruct();
                return constantStruct switch
                {
                    ConstantUtf8InfoStruct utf8 => utf8.ToString(),
                    ConstantClassInfoStruct classInfo => GetStringFromConstantPool(classStruct, classInfo.NameIndex, constantPoolStrings),
                    ConstantStringInfoStruct stringInfo => GetStringFromConstantPool(classStruct, stringInfo.NameIndex, constantPoolStrings),
                    ConstantNameAndTypeInfoStruct nameAndType => 
                        $"{GetStringFromConstantPool(classStruct, nameAndType.NameIndex, constantPoolStrings)}:{GetStringFromConstantPool(classStruct, nameAndType.DescriptorIndex, constantPoolStrings)}",
                    _ => null
                };
            }

            currentIndex++;
            if (cpInfo.Tag == (byte)ConstantPoolTag.Long || cpInfo.Tag == (byte)ConstantPoolTag.Double)
            {
                currentIndex++;
            }
        }

        return null;
    }

    private static List<Attribute> ParseAttributes(ClassStruct classStruct, AttributeInfoStruct[] attributes, Dictionary<ushort, string> constantPoolStrings)
    {
        var result = new List<Attribute>();
        foreach (var attr in attributes)
        {
            var attribute = new Attribute
            {
                Name = GetStringFromConstantPool(classStruct, attr.AttributeNameIndex, constantPoolStrings) ?? throw new NullReferenceException(),
                Info = attr.Info
            };
            result.Add(attribute);
        }
        return result;
    }

    private void AddConstantPoolEntries(ConstantPoolHelper constantPoolHelper)
    {
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