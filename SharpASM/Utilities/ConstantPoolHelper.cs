using System.Text;
using SharpASM.Models;
using SharpASM.Models.Struct;
using SharpASM.Models.Type;

namespace SharpASM.Utilities;

public class ConstantPoolHelper
{
    public List<ConstantPoolInfo> ConstantPool { get; set; }
    public ushort ConstantPoolCount => CalculateConstantPoolCount();
    public ushort ConstantPoolIndexCount => CalculateConstantPoolIndexCount();

    public ConstantPoolHelper(List<ConstantPoolInfo> constantPool)
    {
        ConstantPool = constantPool;
    }
    
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

    public ushort NewUtf8(string text)
    {
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
        
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Utf8)
            {
                var cpi = (ConstantUtf8InfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.Length == utf8Bytes.Length)
                {
                    if (cpi.Bytes.SequenceEqual(utf8Bytes))
                    {
                        return index;
                    }
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newUtf8 = new ConstantUtf8InfoStruct
        {
            Tag = (byte) ConstantPoolTag.Utf8,
            Length = (ushort)utf8Bytes.Length,
            Bytes = utf8Bytes
        };
        var newConstant = ConstantPoolInfo.FromStruct(newUtf8.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewInteger(int value)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Integer)
            {
                var cpi = (ConstantIntegerInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.GetValue() == value)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newInt = new ConstantIntegerInfoStruct()
        {
            Tag = (byte) ConstantPoolTag.Integer,
        };
        newInt.SetValue(value);
        var newConstant = ConstantPoolInfo.FromStruct(newInt.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewFloat(float value)
    {
        var newFloat = new ConstantFloatInfoStruct()
        {
            Tag = (byte) ConstantPoolTag.Float,
        };
        newFloat.SetValue(value);
        
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Float)
            {
                var cpi = (ConstantFloatInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.Bytes == newFloat.Bytes)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newConstant = ConstantPoolInfo.FromStruct(newFloat.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewLong(long value)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Long)
            {
                var cpi = (ConstantLongInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.GetValue() == value)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newLong = new ConstantLongInfoStruct()
        {
            Tag = (byte) ConstantPoolTag.Long,
        };
        newLong.SetValue(value);
        var newConstant = ConstantPoolInfo.FromStruct(newLong.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewDouble(double value)
    {
        var newDouble = new ConstantDoubleInfoStruct()
        {
            Tag = (byte) ConstantPoolTag.Double,
        };
        newDouble.SetValue(value);
        
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Double)
            {
                var cpi = (ConstantDoubleInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.LowBytes == newDouble.LowBytes && cpi.HighBytes == newDouble.HighBytes)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newConstant = ConstantPoolInfo.FromStruct(newDouble.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewClass(string value)
    {
        var classNameIndex = NewUtf8(value);
    
        ushort index = CalculateConstantPoolIndexCount();
    
        var newClass = new ConstantClassInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Class,
            NameIndex = classNameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newClass.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewClass(ushort nameIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Class)
            {
                var cpi = (ConstantClassInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.NameIndex == nameIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newClass = new ConstantClassInfoStruct()
        {
            Tag = (byte) ConstantPoolTag.Class,
            NameIndex = nameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newClass.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewString(string value)
    {
        var nameIndex = NewUtf8(value);
    
        ushort index = CalculateConstantPoolIndexCount();
    
        var newString = new ConstantStringInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.String,
            NameIndex = nameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newString.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewString(ushort nameIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.String)
            {
                var cpi = (ConstantStringInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.NameIndex == nameIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newString = new ConstantStringInfoStruct()
        {
            Tag = (byte) ConstantPoolTag.String,
            NameIndex = nameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newString.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewFieldref(string className, string fieldName, string fieldDescriptor)
    {
        ushort classIndex = NewClass(className);
        ushort nameAndTypeIndex = NewNameAndType(fieldName, fieldDescriptor);
    
        ushort index = CalculateConstantPoolIndexCount();
    
        var newFieldref = new ConstantFieldrefInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Fieldref,
            ClassIndex = classIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newFieldref.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewFieldref(ushort classIndex, ushort nameAndTypeIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Fieldref)
            {
                var cpi = (ConstantFieldrefInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.ClassIndex == classIndex && cpi.NameAndTypeIndex == nameAndTypeIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
    
        var newFieldref = new ConstantFieldrefInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Fieldref,
            ClassIndex = classIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newFieldref.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewMethodref(string className, string fieldName, string fieldDescriptor)
    {
        ushort classIndex = NewClass(className);
        ushort nameAndTypeIndex = NewNameAndType(fieldName, fieldDescriptor);
    
        ushort index = CalculateConstantPoolIndexCount();
    
        var newMethodref = new ConstantMethodrefInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Methodref,
            ClassIndex = classIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newMethodref.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewMethodref(ushort classIndex, ushort nameAndTypeIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Methodref)
            {
                var cpi = (ConstantMethodrefInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.ClassIndex == classIndex && cpi.NameAndTypeIndex == nameAndTypeIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
    
        var newMethodref = new ConstantMethodrefInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Methodref,
            ClassIndex = classIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newMethodref.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewInterfaceMethodref(string className, string fieldName, string fieldDescriptor)
    {
        ushort classIndex = NewClass(className);
        ushort nameAndTypeIndex = NewNameAndType(fieldName, fieldDescriptor);
    
        ushort index = CalculateConstantPoolIndexCount();
    
        var newInterfaceMethodref = new ConstantInterfaceMethodrefInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.InterfaceMethodref,
            ClassIndex = classIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newInterfaceMethodref.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewInterfaceMethodref(ushort classIndex, ushort nameAndTypeIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.InterfaceMethodref)
            {
                var cpi = (ConstantInterfaceMethodrefInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.ClassIndex == classIndex && cpi.NameAndTypeIndex == nameAndTypeIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
    
        var newInterfaceMethodref = new ConstantInterfaceMethodrefInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.InterfaceMethodref,
            ClassIndex = classIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newInterfaceMethodref.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    public ushort NewNameAndType(string name, string descriptor)
    {
        ushort nameIndex = NewUtf8(name);
        ushort descriptorIndex = NewUtf8(descriptor);
    
        ushort index = CalculateConstantPoolIndexCount();
    
        var newNameAndType = new ConstantNameAndTypeInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.NameAndType,
            NameIndex = nameIndex,
            DescriptorIndex = descriptorIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newNameAndType.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewNameAndType(ushort nameIndex, ushort descriptorIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.NameAndType)
            {
                var cpi = (ConstantNameAndTypeInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.NameIndex == nameIndex && cpi.DescriptorIndex == descriptorIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
    
        var newNameAndType = new ConstantNameAndTypeInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.NameAndType,
            NameIndex = nameIndex,
            DescriptorIndex = descriptorIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newNameAndType.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }


    // ========= AI-Generated Code - Start =========

    public ushort NewMethodHandle(byte referenceKind, ushort referenceIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.MethodHandle)
            {
                var cpi = (ConstantMethodHandleInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.ReferenceKind == referenceKind && cpi.ReferenceIndex == referenceIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newMethodHandle = new ConstantMethodHandleInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.MethodHandle,
            ReferenceKind = referenceKind,
            ReferenceIndex = referenceIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newMethodHandle.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewMethodType(string descriptor)
    {
        ushort descriptorIndex = NewUtf8(descriptor);
        
        ushort index = CalculateConstantPoolIndexCount();
        
        var newMethodType = new ConstantMethodTypeInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.MethodType,
            DescriptorIndex = descriptorIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newMethodType.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewMethodType(ushort descriptorIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.MethodType)
            {
                var cpi = (ConstantMethodTypeInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.DescriptorIndex == descriptorIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newMethodType = new ConstantMethodTypeInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.MethodType,
            DescriptorIndex = descriptorIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newMethodType.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewDynamic(ushort bootstrapMethodAttrIndex, ushort nameAndTypeIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Dynamic)
            {
                var cpi = (ConstantDynamicInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.BootstrapMethodAttrIndex == bootstrapMethodAttrIndex && 
                    cpi.NameAndTypeIndex == nameAndTypeIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newDynamic = new ConstantDynamicInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Dynamic,
            BootstrapMethodAttrIndex = bootstrapMethodAttrIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newDynamic.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewInvokeDynamic(ushort bootstrapMethodAttrIndex, ushort nameAndTypeIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.InvokeDynamic)
            {
                var cpi = (ConstantInvokeDynamicInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.BootstrapMethodAttrIndex == bootstrapMethodAttrIndex && 
                    cpi.NameAndTypeIndex == nameAndTypeIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newInvokeDynamic = new ConstantInvokeDynamicInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.InvokeDynamic,
            BootstrapMethodAttrIndex = bootstrapMethodAttrIndex,
            NameAndTypeIndex = nameAndTypeIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newInvokeDynamic.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewModule(string name)
    {
        ushort nameIndex = NewUtf8(name);
        
        ushort index = CalculateConstantPoolIndexCount();
        
        var newModule = new ConstantModuleInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Module,
            NameIndex = nameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newModule.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewModule(ushort nameIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Module)
            {
                var cpi = (ConstantModuleInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.NameIndex == nameIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newModule = new ConstantModuleInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Module,
            NameIndex = nameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newModule.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewPackage(string name)
    {
        ushort nameIndex = NewUtf8(name);
        
        ushort index = CalculateConstantPoolIndexCount();
        
        var newPackage = new ConstantPackageInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Package,
            NameIndex = nameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newPackage.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }

    public ushort NewPackage(ushort nameIndex)
    {
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Package)
            {
                var cpi = (ConstantPackageInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.NameIndex == nameIndex)
                {
                    return index;
                }
            }
            index++;
            if (c.Tag == ConstantPoolTag.Long || c.Tag == ConstantPoolTag.Double)
            {
                index++;
            }
        }
        
        var newPackage = new ConstantPackageInfoStruct()
        {
            Tag = (byte)ConstantPoolTag.Package,
            NameIndex = nameIndex,
        };
        var newConstant = ConstantPoolInfo.FromStruct(newPackage.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    // ========= AI-Generated Code - End =========
    

    #endregion


    public ConstantPoolInfoStruct[] ToArray()
    {
        var array = new List<ConstantPoolInfoStruct>();
        foreach (var c in ConstantPool)
        {
            array.Add(c.ToStruct());
        }

        return array.ToArray();
    }

}