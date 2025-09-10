namespace SharpASM.Models.Code;

public class Code
{
    public OperationCode OpCode { get; set; }
    public List<byte> Operands { get; set; } = new List<byte>();
    
    public Code(OperationCode opCode, IEnumerable<byte>? operands = null)
    {
        OpCode = opCode;
        if (operands != null) Operands.AddRange(operands);
    }
    
    // TODO...
    
}