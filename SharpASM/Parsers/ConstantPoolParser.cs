using SharpASM.Models.Type;
using System;
using System.Collections.Generic;
using SharpASM.Models.Struct;

namespace SharpASM.Parsers;

public static class ConstantPoolParser
{
    public static List<object> ParseConstantPool(byte[] data, ref int offset, ushort constantPoolCount)
    {
        var constantPool = new List<object>();
        
        for (int i = 1; i < constantPoolCount; i++)
        {
            if (offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset exceeds data length");
                
            byte tag = data[offset++]; // Read tag once
                
            object constantPoolItem;
            switch ((ConstantPoolTag)tag)
            {
                case ConstantPoolTag.Class:
                    constantPoolItem = ConstantClassInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Fieldref:
                    constantPoolItem = ConstantFieldrefInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Methodref:
                    constantPoolItem = ConstantMethodrefInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.InterfaceMethodref:
                    constantPoolItem = ConstantInterfaceMethodrefInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.String:
                    constantPoolItem = ConstantStringInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Integer:
                    constantPoolItem = ConstantIntegerInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Float:
                    constantPoolItem = ConstantFloatInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Long:
                    constantPoolItem = ConstantLongInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    i++; // Long takes two slots
                    break;
                case ConstantPoolTag.Double:
                    constantPoolItem = ConstantDoubleInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    i++; // Double takes two slots
                    break;
                case ConstantPoolTag.NameAndType:
                    constantPoolItem = ConstantNameAndTypeInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Utf8:
                    constantPoolItem = ConstantUtf8InfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.MethodHandle:
                    constantPoolItem = ConstantMethodHandleInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.MethodType:
                    constantPoolItem = ConstantMethodTypeInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Dynamic:
                    constantPoolItem = ConstantDynamicInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.InvokeDynamic:
                    constantPoolItem = ConstantInvokeDynamicInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Module:
                    constantPoolItem = ConstantModuleInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                case ConstantPoolTag.Package:
                    constantPoolItem = ConstantPackageInfoStruct.FromBytesWithTag(tag, data, ref offset);
                    break;
                default:
                    int start = Math.Max(0, offset - 10);
                    int end = Math.Min(data.Length, offset + 10);
                    string context = BitConverter.ToString(data, start, end - start);
                    throw new NotSupportedException($"Unknown constant pool tag: {tag} at offset {offset-1}. Context: {context}");
            }
                
            constantPool.Add(constantPoolItem);
        }
            
        return constantPool;
    }
        
    public static byte[] SerializeConstantPool(List<object> constantPool)
    {
        using (var stream = new System.IO.MemoryStream())
        {
            foreach (var item in constantPool)
            {
                byte[] bytes;
                    
                switch (item)
                {
                    case ConstantClassInfoStruct classInfo:
                        bytes = classInfo.ToBytes();
                        break;
                    case ConstantFieldrefInfoStruct fieldrefInfo:
                        bytes = fieldrefInfo.ToBytes();
                        break;
                    case ConstantMethodrefInfoStruct methodrefInfo:
                        bytes = methodrefInfo.ToBytes();
                        break;
                    case ConstantInterfaceMethodrefInfoStruct interfaceMethodrefInfo:
                        bytes = interfaceMethodrefInfo.ToBytes();
                        break;
                    case ConstantStringInfoStruct stringInfo:
                        bytes = stringInfo.ToBytes();
                        break;
                    case ConstantIntegerInfoStruct integerInfo:
                        bytes = integerInfo.ToBytes();
                        break;
                    case ConstantFloatInfoStruct floatInfo:
                        bytes = floatInfo.ToBytes();
                        break;
                    case ConstantLongInfoStruct longInfo:
                        bytes = longInfo.ToBytes();
                        break;
                    case ConstantDoubleInfoStruct doubleInfo:
                        bytes = doubleInfo.ToBytes();
                        break;
                    case ConstantNameAndTypeInfoStruct nameAndTypeInfo:
                        bytes = nameAndTypeInfo.ToBytes();
                        break;
                    case ConstantUtf8InfoStruct utf8Info:
                        bytes = utf8Info.ToBytes();
                        break;
                    case ConstantMethodHandleInfoStruct methodHandleInfo:
                        bytes = methodHandleInfo.ToBytes();
                        break;
                    case ConstantMethodTypeInfoStruct methodTypeInfo:
                        bytes = methodTypeInfo.ToBytes();
                        break;
                    case ConstantDynamicInfoStruct dynamicInfo:
                        bytes = dynamicInfo.ToBytes();
                        break;
                    case ConstantInvokeDynamicInfoStruct invokeDynamicInfo:
                        bytes = invokeDynamicInfo.ToBytes();
                        break;
                    case ConstantModuleInfoStruct moduleInfo:
                        bytes = moduleInfo.ToBytes();
                        break;
                    case ConstantPackageInfoStruct packageInfo:
                        bytes = packageInfo.ToBytes();
                        break;
                    default:
                        throw new NotSupportedException($"Unknown constant pool item type: {item.GetType()}");
                }
                    
                stream.Write(bytes, 0, bytes.Length);
            }
                
            return stream.ToArray();
        }
    }
}
