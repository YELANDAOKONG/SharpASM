namespace SharpASM.Models.Struct;

public class CodeAttributeExceptionTableStruct
{
    public ushort StartPc { get; set; }
    public ushort EndPc { get; set; }
    public ushort HandlerPc { get; set; }
    public ushort CatchType { get; set; }
}