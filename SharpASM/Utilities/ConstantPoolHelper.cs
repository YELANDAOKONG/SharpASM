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
        var newLong = new ConstantLongInfoStruct()
        {
            Tag = (byte) ConstantPoolTag.Long,
        };
        newLong.SetValue(value);
        
        ushort index = 1;
        foreach (var c in ConstantPool)
        {
            if (c.Tag == ConstantPoolTag.Long)
            {
                var cpi = (ConstantLongInfoStruct)(c.ToStruct().ToConstantStruct());
                if (cpi.LowBytes == newLong.LowBytes && cpi.HighBytes == newLong.HighBytes)
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
        
        var newConstant = ConstantPoolInfo.FromStruct(newLong.ToStructInfo());
        ConstantPool.Add(newConstant);
        return index;
    }
    
    
    // TODO...
    
    #endregion
    
    
}