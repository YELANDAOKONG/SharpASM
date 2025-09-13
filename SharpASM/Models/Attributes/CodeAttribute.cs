using SharpASM.Models.Code;
using SharpASM.Models.Struct.Attribute;
using System.Collections.Generic;
using System.Linq;
using SharpASM.Models.Struct;

namespace SharpASM.Models.Attributes;

public class CodeAttribute : AttributeBase
{
    public ushort MaxStack { get; set; }
    public ushort MaxLocals { get; set; }
    public List<Code.Code> Code { get; set; } = new List<Code.Code>();
    public List<ExceptionTableEntry> ExceptionTable { get; set; } = new List<ExceptionTableEntry>();
    public List<AttributeBase> Attributes { get; set; } = new List<AttributeBase>();

    public override byte[] ToBytes()
    {
        // TODO...
        throw new NotImplementedException();
    }

    public override void FromBytes(byte[] data)
    {
        // TODO...
        throw new NotImplementedException();
    }
}

public class ExceptionTableEntry
{
    public ushort StartPc { get; set; }
    public ushort EndPc { get; set; }
    public ushort HandlerPc { get; set; }
    public ushort CatchType { get; set; }
}
