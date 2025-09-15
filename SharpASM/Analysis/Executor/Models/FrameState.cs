using SharpASM.Models.Struct.Union;

namespace SharpASM.Analysis.Executor.Models;

public class FrameState
{
    public VerificationTypeInfoStruct[] Locals { get; set; } = Array.Empty<VerificationTypeInfoStruct>();
    public VerificationTypeInfoStruct[] Stack { get; set; } = Array.Empty<VerificationTypeInfoStruct>();
        
    public FrameState Clone()
    {
        return new FrameState
        {
            Locals = (VerificationTypeInfoStruct[])Locals.Clone(),
            Stack = (VerificationTypeInfoStruct[])Stack.Clone()
        };
    }
}