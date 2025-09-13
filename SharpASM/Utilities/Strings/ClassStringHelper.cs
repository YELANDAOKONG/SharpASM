using System.Text;
using SharpASM.Helpers.Models.Type;
using SharpASM.Models;
using SharpASM.Models.Struct;
using SharpASM.Models.Struct.Attribute;
using SharpASM.Models.Type;
using SharpASM.Parsers;

namespace SharpASM.Utilities.Strings;

public class ClassStringHelper
{
    public static string FormatConstantPoolItem(ConstantPoolInfo cp)
    {
        if (cp == null) return "NULL";
        var constantStruct = cp.ToStruct().ToConstantStruct();
        return constantStruct switch
        {
            ConstantUtf8InfoStruct utf8 => $"(Utf8) \"{utf8}\"",
            ConstantIntegerInfoStruct integer => $"(Integer) {integer.GetValue()}",
            ConstantFloatInfoStruct floatVal => $"(Float) {floatVal.GetValue()}",
            ConstantLongInfoStruct longVal => $"(Long) {longVal.GetValue()}L",
            ConstantDoubleInfoStruct doubleVal => $"(Double) {doubleVal.GetValue()}D",
            ConstantClassInfoStruct classInfo => $"(Class) #{classInfo.NameIndex}",
            ConstantStringInfoStruct stringInfo => $"(String) #{stringInfo.NameIndex}",
            ConstantFieldrefInfoStruct fieldref => $"(Fieldref) #{fieldref.ClassIndex}.#{fieldref.NameAndTypeIndex}",
            ConstantMethodrefInfoStruct methodref => $"(Methodref) #{methodref.ClassIndex}.#{methodref.NameAndTypeIndex}",
            ConstantInterfaceMethodrefInfoStruct interfaceMethodref => 
                $"(InterfaceMethodref) #{interfaceMethodref.ClassIndex}.#{interfaceMethodref.NameAndTypeIndex}",
            ConstantNameAndTypeInfoStruct nameAndType => 
                $"(NameAndType) #{nameAndType.NameIndex}:#{nameAndType.DescriptorIndex}",
            ConstantMethodHandleInfoStruct methodHandle => 
                $"(MethodHandle) {methodHandle.ReferenceKind}.#{methodHandle.ReferenceIndex}",
            ConstantMethodTypeInfoStruct methodType => $"MethodType: #{methodType.DescriptorIndex}",
            ConstantDynamicInfoStruct dynamic => 
                $"(Dynamic) #{dynamic.BootstrapMethodAttrIndex}.#{dynamic.NameAndTypeIndex}",
            ConstantInvokeDynamicInfoStruct invokeDynamic => 
                $"(InvokeDynamic) #{invokeDynamic.BootstrapMethodAttrIndex}.#{invokeDynamic.NameAndTypeIndex}",
            ConstantModuleInfoStruct module => $"(Module) #{module.NameIndex}",
            ConstantPackageInfoStruct package => $"(Package) #{package.NameIndex}",
            _ => $"(Unknown) {constantStruct.GetType().Name}"
        };
    }

    public static string FormatToString(Class clazz)
    {
        var helper = clazz.GetConstantPoolHelper();
        var builder = new StringBuilder();

        builder.AppendLine($"[@] Magic: 0x{clazz.Magic:X}");
        builder.AppendLine($"[@] Minor Version: {clazz.MinorVersion}");
        builder.AppendLine($"[@] Major Version: {clazz.MajorVersion.ToString()}");
        builder.AppendLine($"[@] Constant Pool: {clazz.ConstantPoolCount} Length");
        builder.AppendLine($"[@] Constant Pool: {clazz.ConstantPoolIndexCount} Index");
        builder.AppendLine($"[@] Access Flags: {ClassAccessFlagsHelper.GetFlagsString((ushort)clazz.AccessFlags)}");
        builder.AppendLine($"[@] This Class: {clazz.ThisClass}");
        builder.AppendLine($"[@] Super Class: {clazz.SuperClass}");
        
        if (clazz.ConstantPool.Count != 0)
        {
            builder.AppendLine($"[@] Constant Pool: ");
        }

        foreach (var cpi in clazz.ConstantPool)
        {
            builder.AppendLine($"[@] - Constant: {FormatConstantPoolItem(cpi)}");
        }
        
        
        if (clazz.Attributes.Count != 0)
        {
            builder.AppendLine($"[@] Attributes: ");
        }

        foreach (var attribute in clazz.Attributes)
        {
            builder.AppendLine($"[@] - Attribute: {attribute.Name} ({attribute.Info.Length} Bytes)");
        }

        if (clazz.Interfaces.Count != 0)
        {
            builder.AppendLine($"[@] Interfaces: ");
        }

        foreach (var clazzInterface in clazz.Interfaces)
        {
            builder.AppendLine($"[@] - Interface: {clazzInterface}");
        }

        if (clazz.Fields.Count != 0)
        {
            builder.AppendLine($"[@] Fields: ");
        }

        foreach (var field in clazz.Fields)
        {
            builder.AppendLine(
                $"[@] - Field: {field.Name} {field.Descriptor} [ {FieldAccessFlagsHelper.GetFlagsString((ushort)field.AccessFlags)} ]");
            foreach (var attribute in field.Attributes)
            {
                builder.AppendLine($"[@] -  - Attribute: {attribute.Name} ({attribute.Info.Length} Bytes)");
                if (attribute.Name == "ConstantValue")
                {
                    AttributeInfoStruct attributeInfoStruct = new AttributeInfoStruct()
                    {
                        AttributeLength = 0,
                        AttributeNameIndex = 0,
                        Info = attribute.Info,
                    };
                    ConstantValueAttributeStruct attributeStruct =
                        ConstantValueAttributeStruct.FromStructInfo(attributeInfoStruct);
                    var index = attributeStruct.ConstantValueIndex;
                    var cp = helper.ByIndex(index);
                    if (cp == null) continue;
                    var icp = cp.ToStruct().ToConstantStruct();

                    var formattedValue = FormatConstantPoolItem(cp);
                    builder.AppendLine($"[@] -  -  - ConstantValue: {formattedValue}");

                    if (cp.Tag == ConstantPoolTag.String)
                    {
                        var stringCp =
                            helper.ByIndex(((ConstantStringInfoStruct)cp.ToStruct().ToConstantStruct()).NameIndex);
                        if (stringCp != null && stringCp.Tag == ConstantPoolTag.Utf8)
                        {
                            var utf8Value = ((ConstantUtf8InfoStruct)stringCp.ToStruct().ToConstantStruct()).ToString();
                            builder.AppendLine($"[@] -  -  -  - String Value: \"{utf8Value}\"");
                        }
                    }
                }
            }
        }

        if (clazz.Methods.Count != 0)
        {
            builder.AppendLine($"[@] Methods: ");
        }

        foreach (var method in clazz.Methods)
        {

            builder.AppendLine(
                $"[@] - Method: {method.Name} {method.Descriptor} [ {MethodAccessFlagsHelper.GetFlagsString((ushort)method.AccessFlags)} ]");
            foreach (var attribute in method.Attributes)
            {
                builder.AppendLine($"[@] -  - Attribute: {attribute.Name} ({attribute.Info.Length} Bytes)");
                if (attribute.Name == "Code")
                {
                    AttributeInfoStruct attributeInfoStruct = new AttributeInfoStruct()
                    {
                        AttributeLength = 0,
                        AttributeNameIndex = 0,
                        Info = attribute.Info,
                    };
                    CodeAttributeStruct attributeStruct = CodeAttributeStruct.FromStructInfo(attributeInfoStruct);
                    var codes = ByteCodeParser.Parse(attributeStruct.Code);
                    foreach (var code in codes)
                    {
                        builder.AppendLine($"[@] -  -  - {code}");
                    }
                }
            }
        }
        
        return builder.ToString();
    }
}