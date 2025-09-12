// ClassParser.cs
using SharpASM.Models.Struct;
using SharpASM.Models.Type;
using SharpASM.Parsers;
using SharpASM.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpASM.Parsers;

public static class ClassParser
{
    public static ClassStruct Parse(byte[] data)
    {
        int offset = 0;
        var classStruct = new ClassStruct();
        
        // Read Magic
        classStruct.Magic = ByteUtils.ReadUInt32(data, ref offset);
        if (classStruct.Magic != ClassStruct.ClassMagic)
            throw new InvalidDataException("Invalid class file magic number");
        
        // Read Version
        classStruct.MinorVersion = ByteUtils.ReadUInt16(data, ref offset);
        classStruct.MajorVersion = ByteUtils.ReadUInt16(data, ref offset);
        
        // Read Constant Pool
        classStruct.ConstantPoolCount = ByteUtils.ReadUInt16(data, ref offset);
        var constantPool = ConstantPoolParser.ParseConstantPool(data, ref offset, classStruct.ConstantPoolCount);
        
        classStruct.ConstantPool = new ConstantPoolInfoStruct[constantPool.Count];
        
        for (int i = 0; i < constantPool.Count; i++)
        {
            var item = constantPool[i];
            classStruct.ConstantPool[i] = new ConstantPoolInfoStruct
            {
                Tag = GetTagFromConstantPoolItem(item),
                Info = GetInfoFromConstantPoolItem(item) ?? []
            };
        }
        
        // Read Access Flags
        classStruct.AccessFlags = ByteUtils.ReadUInt16(data, ref offset);
        
        // Read Class & Super Class
        classStruct.ThisClass = ByteUtils.ReadUInt16(data, ref offset);
        classStruct.SuperClass = ByteUtils.ReadUInt16(data, ref offset);
        
        // Read Interfaces
        classStruct.InterfacesCount = ByteUtils.ReadUInt16(data, ref offset);
        classStruct.Interfaces = new ushort[classStruct.InterfacesCount];
        for (int i = 0; i < classStruct.InterfacesCount; i++)
        {
            classStruct.Interfaces[i] = ByteUtils.ReadUInt16(data, ref offset);
        }
        
        // Read Fields
        classStruct.FieldsCount = ByteUtils.ReadUInt16(data, ref offset);
        classStruct.Fields = new FieldInfoStruct[classStruct.FieldsCount];
        for (int i = 0; i < classStruct.FieldsCount; i++)
        {
            classStruct.Fields[i] = FieldInfoStruct.FromBytes(data, ref offset);
        }
        
        // Read Methods
        classStruct.MethodsCount = ByteUtils.ReadUInt16(data, ref offset);
        classStruct.Methods = new MethodInfoStruct[classStruct.MethodsCount];
        for (int i = 0; i < classStruct.MethodsCount; i++)
        {
            classStruct.Methods[i] = MethodInfoStruct.FromBytes(data, ref offset);
        }
        
        // Read Class Attributes
        classStruct.AttributesCount = ByteUtils.ReadUInt16(data, ref offset);
        classStruct.Attributes = new AttributeInfoStruct[classStruct.AttributesCount];
        for (int i = 0; i < classStruct.AttributesCount; i++)
        {
            classStruct.Attributes[i] = AttributeInfoStruct.FromBytes(data, ref offset);
        }
        
        return classStruct;
    }
    
    public static byte[] Serialize(ClassStruct classStruct)
    {
        using (var stream = new MemoryStream())
        {
            // Write Magic
            ByteUtils.WriteUInt32(classStruct.Magic, stream);
            
            // Write Version
            ByteUtils.WriteUInt16(classStruct.MinorVersion, stream);
            ByteUtils.WriteUInt16(classStruct.MajorVersion, stream);
            
            // Calculate actual constant pool slot count including double-width entries
            ushort constantPoolSlotCount = 0;
            foreach (var cpInfo in classStruct.ConstantPool)
            {
                constantPoolSlotCount++;
                if (cpInfo.Tag == (byte)ConstantPoolTag.Long || cpInfo.Tag == (byte)ConstantPoolTag.Double)
                {
                    constantPoolSlotCount++; // Double-width entries take two slots
                }
            }
        
            // Write Constant Pool Count (number of slots + 1 for the zero index)
            ByteUtils.WriteUInt16((ushort)(constantPoolSlotCount + 1), stream);
            
            // Convert ConstantPoolInfoStruct To Objects List
            var constantPoolItems = new List<object>();
            foreach (var cpInfo in classStruct.ConstantPool)
            {
                if (cpInfo.Info == null)
                {
                    throw new ArgumentException("Constant pool item info is null");
                }
                constantPoolItems.Add(ConvertToConstantPoolItem(cpInfo));
            }
            
            // Serialize Constant Pool
            var constantPoolBytes = ConstantPoolParser.SerializeConstantPool(constantPoolItems);
            stream.Write(constantPoolBytes, 0, constantPoolBytes.Length);
            
            // Write Access Flags
            ByteUtils.WriteUInt16(classStruct.AccessFlags, stream);
            
            // Write Class & Super Class
            ByteUtils.WriteUInt16(classStruct.ThisClass, stream);
            ByteUtils.WriteUInt16(classStruct.SuperClass, stream);
            
            // Write Interfaces Count & Interfaces
            ByteUtils.WriteUInt16(classStruct.InterfacesCount, stream);
            foreach (var interfaceIndex in classStruct.Interfaces)
            {
                ByteUtils.WriteUInt16(interfaceIndex, stream);
            }
            
            // Write Fields Count & Fields
            ByteUtils.WriteUInt16(classStruct.FieldsCount, stream);
            foreach (var field in classStruct.Fields)
            {
                var bytes = field.ToBytes();
                stream.Write(bytes, 0, bytes.Length);
            }
            
            // Write Methods Count & Methods
            ByteUtils.WriteUInt16(classStruct.MethodsCount, stream);
            foreach (var method in classStruct.Methods)
            {
                var bytes = method.ToBytes();
                stream.Write(bytes, 0, bytes.Length);
            }
            
            // Write Attributes Count & Attributes
            ByteUtils.WriteUInt16(classStruct.AttributesCount, stream);
            foreach (var attribute in classStruct.Attributes)
            {
                var bytes = attribute.ToBytes();
                stream.Write(bytes, 0, bytes.Length);
            }
            
            return stream.ToArray();
        }
    }
    
    private static byte GetTagFromConstantPoolItem(object item)
    {
        return item switch
        {
            ConstantClassInfoStruct => (byte)ConstantPoolTag.Class,
            ConstantFieldrefInfoStruct => (byte)ConstantPoolTag.Fieldref,
            ConstantMethodrefInfoStruct => (byte)ConstantPoolTag.Methodref,
            ConstantInterfaceMethodrefInfoStruct => (byte)ConstantPoolTag.InterfaceMethodref,
            ConstantStringInfoStruct => (byte)ConstantPoolTag.String,
            ConstantIntegerInfoStruct => (byte)ConstantPoolTag.Integer,
            ConstantFloatInfoStruct => (byte)ConstantPoolTag.Float,
            ConstantLongInfoStruct => (byte)ConstantPoolTag.Long,
            ConstantDoubleInfoStruct => (byte)ConstantPoolTag.Double,
            ConstantNameAndTypeInfoStruct => (byte)ConstantPoolTag.NameAndType,
            ConstantUtf8InfoStruct => (byte)ConstantPoolTag.Utf8,
            ConstantMethodHandleInfoStruct => (byte)ConstantPoolTag.MethodHandle,
            ConstantMethodTypeInfoStruct => (byte)ConstantPoolTag.MethodType,
            ConstantDynamicInfoStruct => (byte)ConstantPoolTag.Dynamic,
            ConstantInvokeDynamicInfoStruct => (byte)ConstantPoolTag.InvokeDynamic,
            ConstantModuleInfoStruct => (byte)ConstantPoolTag.Module,
            ConstantPackageInfoStruct => (byte)ConstantPoolTag.Package,
            _ => throw new NotSupportedException($"Unknown constant pool item type: {item.GetType()}")
        };
    }
    
    private static byte[] GetInfoFromConstantPoolItem(object item)
    {
        try
        {
            return item switch
            {
                ConstantClassInfoStruct classInfo => classInfo.ToBytesWithoutTag(),
                ConstantFieldrefInfoStruct fieldrefInfo => fieldrefInfo.ToBytesWithoutTag(),
                ConstantMethodrefInfoStruct methodrefInfo => methodrefInfo.ToBytesWithoutTag(),
                ConstantInterfaceMethodrefInfoStruct interfaceMethodrefInfo =>
                    interfaceMethodrefInfo.ToBytesWithoutTag(),
                ConstantStringInfoStruct stringInfo => stringInfo.ToBytesWithoutTag(),
                ConstantIntegerInfoStruct integerInfo => integerInfo.ToBytesWithoutTag(),
                ConstantFloatInfoStruct floatInfo => floatInfo.ToBytesWithoutTag(),
                ConstantLongInfoStruct longInfo => longInfo.ToBytesWithoutTag(),
                ConstantDoubleInfoStruct doubleInfo => doubleInfo.ToBytesWithoutTag(),
                ConstantNameAndTypeInfoStruct nameAndTypeInfo => nameAndTypeInfo.ToBytesWithoutTag(),
                ConstantUtf8InfoStruct utf8Info => utf8Info.ToBytesWithoutTag(),
                ConstantMethodHandleInfoStruct methodHandleInfo => methodHandleInfo.ToBytesWithoutTag(),
                ConstantMethodTypeInfoStruct methodTypeInfo => methodTypeInfo.ToBytesWithoutTag(),
                ConstantDynamicInfoStruct dynamicInfo => dynamicInfo.ToBytesWithoutTag(),
                ConstantInvokeDynamicInfoStruct invokeDynamicInfo => invokeDynamicInfo.ToBytesWithoutTag(),
                ConstantModuleInfoStruct moduleInfo => moduleInfo.ToBytesWithoutTag(),
                ConstantPackageInfoStruct packageInfo => packageInfo.ToBytesWithoutTag(),
                _ => throw new NotSupportedException($"Unknown constant pool item type: {item.GetType()}")
            };
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }
    
    private static object ConvertToConstantPoolItem(ConstantPoolInfoStruct cpInfo)
    {
        if (cpInfo.Info == null)
        {
            throw new ArgumentException("Constant pool item info is null");
        }
        
        int tempOffset = 0;
        
        switch ((ConstantPoolTag)cpInfo.Tag)
        {
            case ConstantPoolTag.Class:
                var classInfo = new ConstantClassInfoStruct();
                classInfo.Tag = cpInfo.Tag;
                classInfo.NameIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return classInfo;
                
            case ConstantPoolTag.Fieldref:
                var fieldrefInfo = new ConstantFieldrefInfoStruct();
                fieldrefInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                fieldrefInfo.ClassIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                fieldrefInfo.NameAndTypeIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return fieldrefInfo;
                
            case ConstantPoolTag.Methodref:
                var methodrefInfo = new ConstantMethodrefInfoStruct();
                methodrefInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                methodrefInfo.ClassIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                methodrefInfo.NameAndTypeIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return methodrefInfo;
                
            case ConstantPoolTag.InterfaceMethodref:
                var interfaceMethodrefInfo = new ConstantInterfaceMethodrefInfoStruct();
                interfaceMethodrefInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                interfaceMethodrefInfo.ClassIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                interfaceMethodrefInfo.NameAndTypeIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return interfaceMethodrefInfo;
                
            case ConstantPoolTag.String:
                var stringInfo = new ConstantStringInfoStruct();
                stringInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                stringInfo.NameIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return stringInfo;
            
            case ConstantPoolTag.Integer:
                var integerInfo = new ConstantIntegerInfoStruct();
                integerInfo.Tag = cpInfo.Tag;
                tempOffset = 0;
                integerInfo.Bytes = ByteUtils.ReadUInt32(cpInfo.Info, ref tempOffset);
                return integerInfo;
                
            case ConstantPoolTag.Float:
                var floatInfo = new ConstantFloatInfoStruct();
                floatInfo.Tag = cpInfo.Tag;
                tempOffset = 0;
                floatInfo.Bytes = ByteUtils.ReadUInt32(cpInfo.Info, ref tempOffset);
                return floatInfo;
                
            case ConstantPoolTag.Long:
                var longInfo = new ConstantLongInfoStruct();
                longInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                longInfo.HighBytes = ByteUtils.ReadUInt32(cpInfo.Info, ref tempOffset);
                longInfo.LowBytes = ByteUtils.ReadUInt32(cpInfo.Info, ref tempOffset);
                return longInfo;
                
            case ConstantPoolTag.Double:
                var doubleInfo = new ConstantDoubleInfoStruct();
                doubleInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                doubleInfo.HighBytes = ByteUtils.ReadUInt32(cpInfo.Info, ref tempOffset);
                doubleInfo.LowBytes = ByteUtils.ReadUInt32(cpInfo.Info, ref tempOffset);
                return doubleInfo;
                
            case ConstantPoolTag.NameAndType:
                var nameAndTypeInfo = new ConstantNameAndTypeInfoStruct();
                nameAndTypeInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                nameAndTypeInfo.NameIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                nameAndTypeInfo.DescriptorIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return nameAndTypeInfo;
                
            case ConstantPoolTag.Utf8:
                var utf8Info = new ConstantUtf8InfoStruct();
                utf8Info.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                utf8Info.Length = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                utf8Info.Bytes = new byte[utf8Info.Length];
                Array.Copy(cpInfo.Info, tempOffset, utf8Info.Bytes, 0, utf8Info.Length);
                return utf8Info;
                
            case ConstantPoolTag.MethodHandle:
                var methodHandleInfo = new ConstantMethodHandleInfoStruct();
                methodHandleInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                methodHandleInfo.ReferenceKind = cpInfo.Info[tempOffset++];
                methodHandleInfo.ReferenceIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return methodHandleInfo;
                
            case ConstantPoolTag.MethodType:
                var methodTypeInfo = new ConstantMethodTypeInfoStruct();
                methodTypeInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                methodTypeInfo.DescriptorIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return methodTypeInfo;
                
            case ConstantPoolTag.Dynamic:
                var dynamicInfo = new ConstantDynamicInfoStruct();
                dynamicInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                dynamicInfo.BootstrapMethodAttrIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                dynamicInfo.NameAndTypeIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return dynamicInfo;
                
            case ConstantPoolTag.InvokeDynamic:
                var invokeDynamicInfo = new ConstantInvokeDynamicInfoStruct();
                invokeDynamicInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                invokeDynamicInfo.BootstrapMethodAttrIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                invokeDynamicInfo.NameAndTypeIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return invokeDynamicInfo;
                
            case ConstantPoolTag.Module:
                var moduleInfo = new ConstantModuleInfoStruct();
                moduleInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                moduleInfo.NameIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return moduleInfo;
                
            case ConstantPoolTag.Package:
                var packageInfo = new ConstantPackageInfoStruct();
                packageInfo.Tag = cpInfo.Tag;
                tempOffset = 0; // 修复：重置偏移量
                packageInfo.NameIndex = ByteUtils.ReadUInt16(cpInfo.Info, ref tempOffset);
                return packageInfo;
                
            default:
                throw new NotSupportedException($"Unknown constant pool tag: {cpInfo.Tag}");
        }
    }
}
