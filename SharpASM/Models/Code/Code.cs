namespace SharpASM.Models.Code;

public class Code
{
    public OperationCode OpCode { get; set; }
    public List<Operand> Operands { get; set; } = new List<Operand>();
    
    public Code(OperationCode opCode, IEnumerable<Operand>? operands = null)
    {
        OpCode = opCode;
        if (operands != null) Operands.AddRange(operands);
    }
    
    // TODO...
    
}