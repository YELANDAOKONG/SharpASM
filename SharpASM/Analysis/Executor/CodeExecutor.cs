using SharpASM.Models;
using SharpASM.Models.Code;
using SharpASM.Models.Struct.Attribute;
using SharpASM.Models.Struct.Union;

namespace SharpASM.Analysis.Executor;

public class CodeExecutor
{
    public Class Clazz { get; private set; }
    public CodeAttributeStruct Code { get; private set; }
    public List<Code> Codes { get; private set; }

    public CodeExecutor(Class clazz, CodeAttributeStruct code, List<Code> byteCodes)
    {
        Clazz = clazz;
        Code = code;
        Codes = byteCodes;
    }
    
    public StackMapTableAttributeStruct RebuildStackMapTable()
    {
        AnalyzeControlFlow();
        InitializeInitialFrameState();
        SimulateExecution();
        var frames = BuildFrames();
            
        return new StackMapTableAttributeStruct
        {
            Entries = frames.ToArray()
        };
    }

    private void AnalyzeControlFlow()
    {
        throw new NotImplementedException();
    }

    private void InitializeInitialFrameState()
    {
        throw new NotImplementedException();
    }

    private void SimulateExecution()
    {
        throw new NotImplementedException();
    }
    
    private List<StackMapFrameStruct> BuildFrames()
    {
        throw new NotImplementedException();
    }

    // TODO...
}