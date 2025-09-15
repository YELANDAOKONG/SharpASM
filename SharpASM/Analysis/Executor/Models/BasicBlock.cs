using SharpASM.Models.Code;

namespace SharpASM.Analysis.Executor.Models;

public class BasicBlock
{
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
    public List<Code> Instructions { get; set; } = new List<Code>();
    public List<BasicBlock> Successors { get; set; } = new List<BasicBlock>();
    public List<BasicBlock> ExceptionHandlers { get; set; } = new List<BasicBlock>();
}