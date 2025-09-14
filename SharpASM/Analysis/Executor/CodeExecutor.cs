using SharpASM.Models;
using SharpASM.Models.Code;
using SharpASM.Models.Struct.Attribute;
using SharpASM.Models.Struct.Union;
using System.Collections.Generic;
using System.Linq;
using SharpASM.Analysis.Executor.Models;

namespace SharpASM.Analysis.Executor;

public class CodeExecutor
{
    public Class Clazz { get; private set; }
    public CodeAttributeStruct Code { get; private set; }
    public List<Code> Codes { get; private set; }
    private Dictionary<int, FrameState> _frameStates;
    private List<int> _instructionOffsets;

    public CodeExecutor(Class clazz, CodeAttributeStruct code, List<Code> byteCodes)
    {
        Clazz = clazz;
        Code = code;
        Codes = byteCodes;
        _frameStates = new Dictionary<int, FrameState>();
        _instructionOffsets = CalculateInstructionOffsets();
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

    private List<int> CalculateInstructionOffsets()
    {
        var offsets = new List<int>();
        int currentOffset = 0;
        
        foreach (var code in Codes)
        {
            offsets.Add(currentOffset);
            currentOffset += GetInstructionLength(code);
        }
        
        return offsets;
    }

    private int GetInstructionLength(Code code)
    {
        int length = 0;
        if (code.Prefix.HasValue) length += 1;
        length += 1; // opcode
        foreach (var operand in code.Operands)
        {
            length += operand.Data.Length;
        }
        return length;
    }

    private void AnalyzeControlFlow()
    {
        // Identify basic block leaders
        var leaders = new HashSet<int> { 0 }; // First instruction is always a leader
        
        for (int i = 0; i < Codes.Count; i++)
        {
            var code = Codes[i];
            int offset = _instructionOffsets[i];
            
            // Instructions after branches are leaders
            if (IsBranch(code.OpCode) && i + 1 < Codes.Count)
            {
                leaders.Add(_instructionOffsets[i + 1]);
            }
            
            // Branch targets are leaders
            if (IsBranch(code.OpCode))
            {
                int targetOffset = offset + GetBranchOffset(code);
                leaders.Add(targetOffset);
            }
            
            // Exception handlers are leaders
            foreach (var handler in Code.ExceptionTable)
            {
                leaders.Add(handler.HandlerPc);
            }
        }
        
        // TODO: Store basic block information for later use
    }

    private bool IsBranch(OperationCode opCode)
    {
        return opCode switch
        {
            OperationCode.IFEQ or OperationCode.IFNE or OperationCode.IFLT or 
            OperationCode.IFGE or OperationCode.IFGT or OperationCode.IFLE or
            OperationCode.IF_ICMPEQ or OperationCode.IF_ICMPNE or OperationCode.IF_ICMPLT or
            OperationCode.IF_ICMPGE or OperationCode.IF_ICMPGT or OperationCode.IF_ICMPLE or
            OperationCode.IF_ACMPEQ or OperationCode.IF_ACMPNE or OperationCode.IFNULL or
            OperationCode.IFNONNULL or OperationCode.GOTO or OperationCode.JSR or
            OperationCode.GOTO_W or OperationCode.JSR_W => true,
            _ => false
        };
    }

    private int GetBranchOffset(Code code)
    {
        // Extract branch offset from operands
        // This is a simplified implementation
        if (code.Operands.Count > 0)
        {
            byte[] data = code.Operands[0].Data;
            if (data.Length == 2)
            {
                return (short)((data[0] << 8) | data[1]);
            }
        }
        return 0;
    }

    private void InitializeInitialFrameState()
    {
        // TODO: Implement based on method descriptor and access flags
        // For now, create an empty frame state
        _frameStates[0] = new FrameState
        {
            Locals = new VerificationTypeInfoStruct[Code.MaxLocals],
            Stack = new VerificationTypeInfoStruct[0]
        };
    }

    private void SimulateExecution()
    {
        // TODO: Implement abstract interpretation
        // For now, this is a placeholder
        foreach (var offset in _instructionOffsets)
        {
            if (!_frameStates.ContainsKey(offset))
            {
                continue;
            }
            
            var currentState = _frameStates[offset];
            // Simulate instruction effect on frame state
            // Update MaxStack and MaxLocals if needed
        }
    }
    
    private List<StackMapFrameStruct> BuildFrames()
    {
        var frames = new List<StackMapFrameStruct>();
        
        // Create frames for each basic block start
        foreach (var offset in _frameStates.Keys.OrderBy(o => o))
        {
            var frameState = _frameStates[offset];
            var frame = CreateStackMapFrame(offset, frameState);
            frames.Add(frame);
        }
        
        return frames;
    }

    private StackMapFrameStruct CreateStackMapFrame(int offset, FrameState frameState)
    {
        // TODO: Implement proper frame type selection based on JVM spec
        // For now, always use full frames
        return new StackMapFrameStruct
        {
            FullFrame = new FullFrameStruct
            {
                FrameType = 255, // FULL_FRAME
                OffsetDelta = (ushort)offset,
                NumberOfLocals = (ushort)frameState.Locals.Length,
                Locals = frameState.Locals,
                NumberOfStackItems = (ushort)frameState.Stack.Length,
                Stack = frameState.Stack
            }
        };
    }

    public void UpdateMaxStackAndLocals()
    {
        ushort maxStack = 0;
        ushort maxLocals = 0;
        
        foreach (var frameState in _frameStates.Values)
        {
            if (frameState.Stack.Length > maxStack)
                maxStack = (ushort)frameState.Stack.Length;
            
            if (frameState.Locals.Length > maxLocals)
                maxLocals = (ushort)frameState.Locals.Length;
        }
        
        Code.MaxStack = maxStack;
        Code.MaxLocals = maxLocals;
    }
}
