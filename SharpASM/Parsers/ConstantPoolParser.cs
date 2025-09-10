using SharpASM.Models.Structs;
using SharpASM.Models.Type;
using System;
using System.Collections.Generic;

namespace SharpASM.Parsers;

public static class ConstantPoolParser
{
    public static List<object> ParseConstantPool(byte[] data, ref int offset, ushort constantPoolCount)
    {
        var constantPool = new List<object>();
            
        for (int i = 1; i < constantPoolCount; i++)
        {
            byte tag = data[offset++];
                
            object constantPoolItem;
            switch ((ConstantPoolTag)tag)
            {
                case ConstantPoolTag.Class:
                    constantPoolItem = ConstantClassInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Fieldref:
                    constantPoolItem = ConstantFieldrefInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Methodref:
                    constantPoolItem = ConstantMethodrefInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.InterfaceMethodref:
                    constantPoolItem = ConstantInterfaceMethodrefInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.String:
                    constantPoolItem = ConstantStringInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Integer:
                    constantPoolItem = ConstantIntegerInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Float:
                    constantPoolItem = ConstantFloatInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Long:
                    constantPoolItem = ConstantLongInfoStruct.FromBytes(data, ref offset);
                    i++; // Long and Double take two slots
                    break;
                case ConstantPoolTag.Double:
                    constantPoolItem = ConstantDoubleInfoStruct.FromBytes(data, ref offset);
                    i++; // Long and Double take two slots
                    break;
                case ConstantPoolTag.NameAndType:
                    constantPoolItem = ConstantNameAndTypeInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Utf8:
                    constantPoolItem = ConstantUtf8InfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.MethodHandle:
                    constantPoolItem = ConstantMethodHandleInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.MethodType:
                    constantPoolItem = ConstantMethodTypeInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Dynamic:
                    constantPoolItem = ConstantDynamicInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.InvokeDynamic:
                    constantPoolItem = ConstantInvokeDynamicInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Module:
                    constantPoolItem = ConstantModuleInfoStruct.FromBytes(data, ref offset);
                    break;
                case ConstantPoolTag.Package:
                    constantPoolItem = ConstantPackageInfoStruct.FromBytes(data, ref offset);
                    break;
                default:
                    throw new NotSupportedException($"Unknown constant pool tag: {tag}");
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