using SharpASM.Models.Struct;
using SharpASM.Models.Type;

namespace SharpASM.Models;

public class ConstantPoolInfo
{
    public ConstantPoolTag Tag { get; set; }
    public byte[] Info { get; set; } = [];

    public ConstantPoolInfoStruct ToStruct()
    {
        var result = new ConstantPoolInfoStruct
        {
            Tag = (byte)Tag,
            Info = Info
        };
        return result;
    }

    public static ConstantPoolInfo FromStruct(ConstantPoolInfoStruct data)
    {
        var result = new ConstantPoolInfo
        {
            Tag = (ConstantPoolTag)data.Tag,
            Info = data.Info
        };
        return result;
    }
}