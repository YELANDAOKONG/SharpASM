using SharpASM.Models.Struct.Union;

namespace SharpASM.Analysis.Executor.Models;

public class FrameState
{
    public VerificationTypeInfoStruct[] Locals { get; set; }
    public VerificationTypeInfoStruct[] Stack { get; set; }
        
    public FrameState Clone()
    {
        return new FrameState
        {
            Locals = (VerificationTypeInfoStruct[])Locals.Clone(),
            Stack = (VerificationTypeInfoStruct[])Stack.Clone()
        };
    }
}