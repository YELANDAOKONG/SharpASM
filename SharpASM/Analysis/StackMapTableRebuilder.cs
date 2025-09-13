using SharpASM.Models.Code;
using SharpASM.Models.Struct.Attribute;
using SharpASM.Models.Struct.Union;
using SharpASM.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpASM.Analysis;

public class StackMapTableRebuilder
{
    private readonly List<Code> _codes;
    private readonly int[] _instructionOffsets;
    private readonly Dictionary<int, FrameState> _frameStates = new Dictionary<int, FrameState>();
        
    public StackMapTableRebuilder(List<Code> codes)
    {
        _codes = codes;
        _instructionOffsets = CalculateInstructionOffsets(codes);
    }
        
    /// <summary>
    /// 重新构建 StackMapTable 属性
    /// </summary>
    public StackMapTableAttributeStruct Rebuild()
    {
        // 1. 分析控制流并确定需要帧的位置
        var framePositions = AnalyzeControlFlow();
            
        // 2. 模拟执行并计算每个帧位置的状态
        SimulateExecution(framePositions);
            
        // 3. 构建 StackMapTable 帧
        var frames = BuildFrames(framePositions);
            
        // 4. 创建 StackMapTable 属性
        return new StackMapTableAttributeStruct
        {
            Entries = frames.ToArray()
        };
    }
        
    /// <summary>
    /// 分析控制流并确定需要 StackMap 帧的位置
    /// </summary>
    private List<int> AnalyzeControlFlow()
    {
        var framePositions = new List<int> { 0 }; // 方法开始总是需要帧
            
        // 遍历所有指令，找到跳转目标和异常处理开始位置
        for (int i = 0; i < _codes.Count; i++)
        {
            var code = _codes[i];
                
            // 如果是跳转指令，记录跳转目标
            if (IsBranchInstruction(code.OpCode) && code.Operands.Count > 0)
            {
                short offset = BitConverter.ToInt16(code.Operands[0].Data);
                int targetOffset = _instructionOffsets[i] + offset;
                int targetIndex = FindInstructionIndexByOffset(targetOffset);
                    
                if (targetIndex >= 0 && !framePositions.Contains(targetIndex))
                {
                    framePositions.Add(targetIndex);
                }
            }
            // 处理宽跳转指令
            else if ((code.OpCode == OperationCode.GOTO_W || code.OpCode == OperationCode.JSR_W) && 
                     code.Operands.Count > 0)
            {
                int offset = BitConverter.ToInt32(code.Operands[0].Data);
                int targetOffset = _instructionOffsets[i] + offset;
                int targetIndex = FindInstructionIndexByOffset(targetOffset);
                    
                if (targetIndex >= 0 && !framePositions.Contains(targetIndex))
                {
                    framePositions.Add(targetIndex);
                }
            }
            // 处理返回指令后的位置（如果有后续指令）
            else if (IsReturnInstruction(code.OpCode) && i < _codes.Count - 1)
            {
                if (!framePositions.Contains(i + 1))
                {
                    framePositions.Add(i + 1);
                }
            }
        }
            
        framePositions.Sort();
        return framePositions;
    }
        
    /// <summary>
    /// 模拟执行并计算每个帧位置的状态
    /// </summary>
    private void SimulateExecution(List<int> framePositions)
    {
        // 初始化第一个帧的状态
        var initialState = new FrameState
        {
            Locals = new VerificationTypeInfoStruct[0], // 根据方法描述符初始化
            Stack = new VerificationTypeInfoStruct[0]
        };
            
        _frameStates[0] = initialState;
            
        // 按顺序处理每个基本块
        for (int i = 0; i < framePositions.Count; i++)
        {
            int start = framePositions[i];
            int end = (i < framePositions.Count - 1) ? framePositions[i + 1] : _codes.Count;
                
            // 模拟执行从 start 到 end-1 的指令
            var currentState = _frameStates[start];
                
            for (int j = start; j < end; j++)
            {
                currentState = ExecuteInstruction(_codes[j], currentState, _instructionOffsets[j]);
            }
                
            // 记录结束状态（如果有下一个基本块）
            if (i < framePositions.Count - 1)
            {
                _frameStates[framePositions[i + 1]] = currentState;
            }
        }
    }
        
    /// <summary>
    /// 执行单条指令并更新状态
    /// </summary>
    private FrameState ExecuteInstruction(Code code, FrameState state, int offset)
    {
        // TODO...
        var newState = state.Clone();
            
        // 根据指令类型更新状态
        switch (code.OpCode)
        {
            case OperationCode.ILOAD:
            case OperationCode.LLOAD:
            case OperationCode.FLOAD:
            case OperationCode.DLOAD:
            case OperationCode.ALOAD:
                // 加载指令：从局部变量加载值到操作数栈
                HandleLoadInstruction(code, newState);
                break;
                    
            case OperationCode.ISTORE:
            case OperationCode.LSTORE:
            case OperationCode.FSTORE:
            case OperationCode.DSTORE:
            case OperationCode.ASTORE:
                // 存储指令：从操作数栈存储值到局部变量
                HandleStoreInstruction(code, newState);
                break;
                    
            case OperationCode.IADD:
            case OperationCode.LADD:
            case OperationCode.FADD:
            case OperationCode.DADD:
            case OperationCode.ISUB:
            case OperationCode.LSUB:
                // ... 其他算术指令
                // 算术指令：消耗操作数栈顶部的值并推送结果
                HandleArithmeticInstruction(code, newState);
                break;
                    
            case OperationCode.IFEQ:
            case OperationCode.IFNE:
            case OperationCode.IFLT:
                // ... 其他条件跳转指令
                // 条件跳转：消耗操作数栈顶部的值
                HandleConditionalBranch(code, newState);
                break;
                    
            case OperationCode.GOTO:
            case OperationCode.GOTO_W:
                // 无条件跳转：不改变状态
                break;
                    
            case OperationCode.INVOKEVIRTUAL:
            case OperationCode.INVOKESPECIAL:
            case OperationCode.INVOKESTATIC:
            case OperationCode.INVOKEINTERFACE:
                // 方法调用：根据方法描述符更新状态
                HandleMethodInvocation(code, newState);
                break;
                    
            case OperationCode.NEW:
                // 创建新对象：推送未初始化对象引用
                HandleNewInstruction(code, newState, offset);
                break;
                    
            case OperationCode.DUP:
            case OperationCode.DUP_X1:
            case OperationCode.DUP_X2:
            case OperationCode.DUP2:
            case OperationCode.DUP2_X1:
            case OperationCode.DUP2_X2:
                // 复制指令：复制操作数栈顶部的值
                HandleDupInstruction(code, newState);
                break;
                    
            case OperationCode.POP:
            case OperationCode.POP2:
                // 弹出指令：移除操作数栈顶部的值
                HandlePopInstruction(code, newState);
                break;
                    
            // ... 其他指令处理
        }
            
        return newState;
    }
        
    /// <summary>
    /// 构建 StackMapTable 帧
    /// </summary>
    private List<StackMapFrameStruct> BuildFrames(List<int> framePositions)
    {
        var frames = new List<StackMapFrameStruct>();
        int previousOffset = -1;
            
        for (int i = 0; i < framePositions.Count; i++)
        {
            int position = framePositions[i];
            int offset = _instructionOffsets[position];
            int offsetDelta = (i == 0) ? offset : offset - previousOffset - 1;
                
            var state = _frameStates[position];
            var frame = CreateFrame(offsetDelta, state);
                
            frames.Add(frame);
            previousOffset = offset;
        }
            
        return frames;
    }
        
    /// <summary>
    /// 根据状态创建适当的帧
    /// </summary>
    private StackMapFrameStruct CreateFrame(int offsetDelta, FrameState state)
    {
        // 尝试使用最紧凑的帧类型
            
        // 1. 检查是否可以使用 same_frame
        if (offsetDelta >= 0 && offsetDelta <= 63 && state.Stack.Length == 0)
        {
            return new StackMapFrameStruct
            {
                SameFrame = new SameFrameStruct { FrameType = (byte)offsetDelta }
            };
        }
            
        // 2. 检查是否可以使用 same_locals_1_stack_item_frame
        if (offsetDelta >= 0 && offsetDelta <= 63 && state.Stack.Length == 1)
        {
            return new StackMapFrameStruct
            {
                SameLocals1StackItemFrame = new SameLocals1StackItemFrameStruct
                {
                    FrameType = (byte)(64 + offsetDelta),
                    Stack = new[] { state.Stack[0] }
                }
            };
        }
            
        // 3. 检查是否可以使用 chop_frame
        // (这里需要比较与前一帧的局部变量数量差异)
            
        // 4. 检查是否可以使用 append_frame
        // (这里需要比较与前一帧的局部变量数量差异)
            
        // 5. 使用 full_frame (最通用的帧类型)
        return new StackMapFrameStruct
        {
            FullFrame = new FullFrameStruct
            {
                FrameType = 255,
                OffsetDelta = (ushort)offsetDelta,
                NumberOfLocals = (ushort)state.Locals.Length,
                Locals = state.Locals,
                NumberOfStackItems = (ushort)state.Stack.Length,
                Stack = state.Stack
            }
        };
    }
        
    /// <summary>
    /// 计算指令偏移量
    /// </summary>
    private int[] CalculateInstructionOffsets(List<Code> codes)
    {
        int[] offsets = new int[codes.Count];
        int currentOffset = 0;
            
        for (int i = 0; i < codes.Count; i++)
        {
            offsets[i] = currentOffset;
            currentOffset += CalculateCodeLength(new List<Code> { codes[i] });
        }
            
        return offsets;
    }
        
    /// <summary>
    /// 计算代码长度
    /// </summary>
    private int CalculateCodeLength(List<Code> codes)
    {
        int length = 0;
            
        foreach (var code in codes)
        {
            // 前缀长度
            if (code.Prefix.HasValue)
                length += 1;
                    
            // 操作码长度
            length += 1;
                
            // 操作数长度
            if (OperationCodeMapping.TryGetOperandInfo(code.OpCode, out int operandCount, out int[] operandSizes))
            {
                for (int i = 0; i < Math.Min(operandCount, code.Operands.Count); i++)
                {
                    length += operandSizes[i];
                }
            }
        }
            
        return length;
    }
        
    /// <summary>
    /// 根据偏移量查找指令索引
    /// </summary>
    private int FindInstructionIndexByOffset(int targetOffset)
    {
        for (int i = 0; i < _instructionOffsets.Length; i++)
        {
            if (_instructionOffsets[i] == targetOffset)
                return i;
                    
            if (i < _instructionOffsets.Length - 1 && 
                _instructionOffsets[i] < targetOffset && 
                _instructionOffsets[i + 1] > targetOffset)
                return i;
        }
            
        return -1;
    }
        
    /// <summary>
    /// 检查是否为跳转指令
    /// </summary>
    private bool IsBranchInstruction(OperationCode opCode)
    {
        return opCode == OperationCode.IFEQ ||
               opCode == OperationCode.IFNE ||
               opCode == OperationCode.IFLT ||
               opCode == OperationCode.IFGE ||
               opCode == OperationCode.IFGT ||
               opCode == OperationCode.IFLE ||
               opCode == OperationCode.IF_ICMPEQ ||
               opCode == OperationCode.IF_ICMPNE ||
               opCode == OperationCode.IF_ICMPLT ||
               opCode == OperationCode.IF_ICMPGE ||
               opCode == OperationCode.IF_ICMPGT ||
               opCode == OperationCode.IF_ICMPLE ||
               opCode == OperationCode.IF_ACMPEQ ||
               opCode == OperationCode.IF_ACMPNE ||
               opCode == OperationCode.IFNULL ||
               opCode == OperationCode.IFNONNULL ||
               opCode == OperationCode.GOTO ||
               opCode == OperationCode.JSR;
    }
        
    /// <summary>
    /// 检查是否为返回指令
    /// </summary>
    private bool IsReturnInstruction(OperationCode opCode)
    {
        return opCode == OperationCode.RETURN ||
               opCode == OperationCode.IRETURN ||
               opCode == OperationCode.LRETURN ||
               opCode == OperationCode.FRETURN ||
               opCode == OperationCode.DRETURN ||
               opCode == OperationCode.ARETURN;
    }
        
    // 各种指令处理方法的实现...
    private void HandleLoadInstruction(Code code, FrameState state)
    {
        // 实现加载指令的处理逻辑
    }
        
    private void HandleStoreInstruction(Code code, FrameState state)
    {
        // 实现存储指令的处理逻辑
    }
        
    private void HandleArithmeticInstruction(Code code, FrameState state)
    {
        // 实现算术指令的处理逻辑
    }
        
    private void HandleConditionalBranch(Code code, FrameState state)
    {
        // 实现条件跳转指令的处理逻辑
    }
        
    private void HandleMethodInvocation(Code code, FrameState state)
    {
        // 实现方法调用指令的处理逻辑
    }
        
    private void HandleNewInstruction(Code code, FrameState state, int offset)
    {
        // 实现NEW指令的处理逻辑
    }
        
    private void HandleDupInstruction(Code code, FrameState state)
    {
        // 实现复制指令的处理逻辑
    }
        
    private void HandlePopInstruction(Code code, FrameState state)
    {
        // 实现弹出指令的处理逻辑
    }
}
    
/// <summary>
/// 表示帧状态（局部变量和操作数栈的类型信息）
/// </summary>
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