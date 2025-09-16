using SharpASM.Models;
using SharpASM.Models.Code;
using SharpASM.Models.Struct.Attribute;
using SharpASM.Models.Struct.Union;
using System.Collections.Generic;
using System.Linq;
using SharpASM.Analysis.Executor.Models;
using SharpASM.Utilities;
using System;
using SharpASM.Models.Struct;
using SharpASM.Models.Type;

namespace SharpASM.Analysis.Executor;

public class CodeExecutor
{
    public Class Clazz { get; private set; }
    public CodeAttributeStruct Code { get; private set; }
    public List<Code> Codes { get; private set; }
    
    public ConstantPoolHelper Helper { get; private set; }
    
    private Dictionary<int, FrameState> _frameStates;
    private List<int> _instructionOffsets;
    private List<BasicBlock> _basicBlocks;
    private Method _method;

    public CodeExecutor(Class clazz, Method method, CodeAttributeStruct code, List<Code> byteCodes)
    {
        Clazz = clazz;
        Code = code;
        Codes = byteCodes;

        Helper = Clazz.GetConstantPoolHelper();
        
        _method = method;
        _frameStates = new Dictionary<int, FrameState>();
        _instructionOffsets = CalculateInstructionOffsets();
        _basicBlocks = new List<BasicBlock>();
    }
    
    public StackMapTableAttributeStruct RebuildStackMapTable()
    {
        if (Code.CodeLength == 0)
        {
            return new StackMapTableAttributeStruct { Entries = Array.Empty<StackMapFrameStruct>() };
        }
        
        AnalyzeControlFlow();
        InitializeInitialFrameState();
        SimulateExecution();
        var frames = BuildFrames();
    
        Clazz.ConstantPool = Helper.ToList();
        
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
        
        if (OperationCodeMapping.TryGetOperandInfo(code.OpCode, out int operandCount, out int[] operandSizes))
        {
            for (int i = 0; i < Math.Min(operandCount, code.Operands.Count); i++)
            {
                length += operandSizes[i];
            }
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
                leaders.Add(handler.StartPc); // Start of protected block
                if (handler.EndPc < Code.CodeLength)
                {
                    leaders.Add(handler.EndPc); // Instruction after protected block
                }
            }
        }
        
        // Create basic blocks
        var sortedLeaders = leaders.OrderBy(o => o).ToList();
        for (int i = 0; i < sortedLeaders.Count; i++)
        {
            int startOffset = sortedLeaders[i];
            int endOffset = (i < sortedLeaders.Count - 1) ? sortedLeaders[i + 1] - 1 : (int)Code.CodeLength - 1;
            
            // 确保基本块范围有效
            if (startOffset > endOffset)
            {
                continue;
            }
            
            var block = new BasicBlock
            {
                StartOffset = startOffset,
                EndOffset = endOffset,
                Instructions = GetInstructionsInRange(startOffset, endOffset)
            };
            
            // 只添加有指令的基本块
            if (block.Instructions.Count > 0)
            {
                _basicBlocks.Add(block);
            }
        }
        
        // Connect basic blocks
        foreach (var block in _basicBlocks)
        {
            // 跳过没有指令的块
            if (block.Instructions.Count == 0)
            {
                continue;
            }
            
            var lastInstruction = block.Instructions.Last();
            int lastOffset = _instructionOffsets[Codes.IndexOf(lastInstruction)];
            
            if (IsBranch(lastInstruction.OpCode))
            {
                int targetOffset = lastOffset + GetBranchOffset(lastInstruction);
                var targetBlock = _basicBlocks.FirstOrDefault(b => b.StartOffset <= targetOffset && b.EndOffset >= targetOffset);
                if (targetBlock != null)
                {
                    block.Successors.Add(targetBlock);
                }
                
                // Fall-through for conditional branches
                if (IsConditionalBranch(lastInstruction.OpCode) && lastOffset < Code.CodeLength - 1)
                {
                    int fallThroughOffset = lastOffset + GetInstructionLength(lastInstruction);
                    var fallThroughBlock = _basicBlocks.FirstOrDefault(b => b.StartOffset <= fallThroughOffset && b.EndOffset >= fallThroughOffset);
                    if (fallThroughBlock != null)
                    {
                        block.Successors.Add(fallThroughBlock);
                    }
                }
            }
            else if (!IsReturn(lastInstruction.OpCode) && !IsThrow(lastInstruction.OpCode))
            {
                // Fall-through for non-branching instructions
                if (lastOffset < Code.CodeLength - 1)
                {
                    int fallThroughOffset = lastOffset + GetInstructionLength(lastInstruction);
                    var fallThroughBlock = _basicBlocks.FirstOrDefault(b => b.StartOffset <= fallThroughOffset && b.EndOffset >= fallThroughOffset);
                    if (fallThroughBlock != null)
                    {
                        block.Successors.Add(fallThroughBlock);
                    }
                }
            }
            
            // Exception handlers
            foreach (var handler in Code.ExceptionTable)
            {
                if (block.StartOffset >= handler.StartPc && block.EndOffset < handler.EndPc)
                {
                    var handlerBlock = _basicBlocks.FirstOrDefault(b => b.StartOffset == handler.HandlerPc);
                    if (handlerBlock != null)
                    {
                        block.ExceptionHandlers.Add(handlerBlock);
                    }
                }
            }
        }
    }

    private List<Code> GetInstructionsInRange(int startOffset, int endOffset)
    {
        var result = new List<Code>();
        int currentOffset = 0;
        
        for (int i = 0; i < Codes.Count; i++)
        {
            var code = Codes[i];
            int codeLength = GetInstructionLength(code);
            
            if (currentOffset >= startOffset && currentOffset <= endOffset)
            {
                result.Add(code);
            }
            
            currentOffset += codeLength;
            
            if (currentOffset > endOffset)
            {
                break;
            }
        }
        
        return result;
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

    private bool IsConditionalBranch(OperationCode opCode)
    {
        return opCode switch
        {
            OperationCode.IFEQ or OperationCode.IFNE or OperationCode.IFLT or 
            OperationCode.IFGE or OperationCode.IFGT or OperationCode.IFLE or
            OperationCode.IF_ICMPEQ or OperationCode.IF_ICMPNE or OperationCode.IF_ICMPLT or
            OperationCode.IF_ICMPGE or OperationCode.IF_ICMPGT or OperationCode.IF_ICMPLE or
            OperationCode.IF_ACMPEQ or OperationCode.IF_ACMPNE or OperationCode.IFNULL or
            OperationCode.IFNONNULL => true,
            _ => false
        };
    }

    private bool IsReturn(OperationCode opCode)
    {
        return opCode switch
        {
            OperationCode.RETURN or OperationCode.IRETURN or OperationCode.LRETURN or
            OperationCode.FRETURN or OperationCode.DRETURN or OperationCode.ARETURN => true,
            _ => false
        };
    }

    private bool IsThrow(OperationCode opCode)
    {
        return opCode == OperationCode.ATHROW;
    }

    private int GetBranchOffset(Code code)
    {
        if (code.Operands.Count == 0) return 0;
        
        byte[] data = code.Operands[0].Data;
        if (data.Length == 2)
        {
            return (short)((data[0] << 8) | data[1]);
        }
        else if (data.Length == 4 && (code.OpCode == OperationCode.GOTO_W || code.OpCode == OperationCode.JSR_W))
        {
            return (int)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
        }
        
        return 0;
    }

    private void InitializeInitialFrameState()
    {
        // Parse method descriptor to get parameter types
        var descriptorInfo = DescriptorParser.ParseMethodDescriptor(_method.Descriptor);
        var initialLocals = new List<VerificationTypeInfoStruct>();
        
        // Add 'this' for non-static methods
        bool isStatic = (_method.AccessFlags & MethodAccessFlags.Static) != 0;
        if (!isStatic)
        {
            initialLocals.Add(CreateObjectTypeInfo(Clazz.ThisClass));
        }
        
        // Add parameters
        foreach (var param in descriptorInfo.Parameters)
        {
            var typeInfo = ConvertDescriptorToVerificationType(param);
            initialLocals.Add(typeInfo);
            
            // Long and double take two slots
            if (param.Descriptor == "J" || param.Descriptor == "D")
            {
                initialLocals.Add(new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
            }
        }
        
        // Pad to MaxLocals if needed
        while (initialLocals.Count < Code.MaxLocals)
        {
            initialLocals.Add(new VerificationTypeInfoStruct
            {
                TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
            });
        }
        
        _frameStates[0] = new FrameState
        {
            Locals = initialLocals.ToArray(),
            Stack = new VerificationTypeInfoStruct[0]
        };
    }

    private VerificationTypeInfoStruct ConvertDescriptorToVerificationType(DescriptorParser.FieldTypeInfo typeInfo)
    {
        if (typeInfo.IsPrimitive)
        {
            return typeInfo.Descriptor switch
            {
                "B" or "C" or "I" or "S" or "Z" => new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                },
                "F" => new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                },
                "J" => new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                },
                "D" => new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                },
                _ => throw new ArgumentException($"Unknown primitive type: {typeInfo.Descriptor}")
            };
        }
        else if (typeInfo.Descriptor.StartsWith("["))
        {
            // Handle array types properly
            string arrayDescriptor = typeInfo.Descriptor;
            ushort classIndex = FindClassConstantIndex(arrayDescriptor);
        
            return new VerificationTypeInfoStruct
            {
                ObjectVariableInfo = new ObjectVariableInfoStruct
                {
                    Tag = 7,
                    CPoolIndex = classIndex
                }
            };
        }
        else if (typeInfo.Descriptor.StartsWith("L"))
        {
            string className = typeInfo.Descriptor.Substring(1, typeInfo.Descriptor.Length - 2);
            return new VerificationTypeInfoStruct
            {
                ObjectVariableInfo = new ObjectVariableInfoStruct
                {
                    Tag = 7,
                    CPoolIndex = FindClassConstantIndex(className)
                }
            };
        }
    
        throw new ArgumentException($"Unknown type descriptor: {typeInfo.Descriptor}");
    }

    private ushort FindClassConstantIndex(string className)
    {
        var classIndex = Helper.FindClass(className);
        if (classIndex != null)
        {
            return classIndex.Value;
        }
    
        return Helper.NewClass(className);
    }

    private VerificationTypeInfoStruct CreateObjectTypeInfo(string className)
    {
        return new VerificationTypeInfoStruct
        {
            ObjectVariableInfo = new ObjectVariableInfoStruct
            {
                Tag = 7,
                CPoolIndex = FindClassConstantIndex(className)
            }
        };
    }

    private void SimulateExecution()
    {
        // Worklist algorithm for data flow analysis
        var worklist = new Queue<BasicBlock>();
        var visited = new HashSet<BasicBlock>();
        
        // Start with initial block
        var initialBlock = _basicBlocks.First(b => b.StartOffset == 0);
        worklist.Enqueue(initialBlock);
        visited.Add(initialBlock);
        
        // 确保初始状态已设置
        if (!_frameStates.ContainsKey(0))
        {
            _frameStates[0] = GetMergedState(initialBlock);
        }
        
        while (worklist.Count > 0)
        {
            var block = worklist.Dequeue();
            
            // Get initial state for this block (merge from predecessors)
            FrameState currentState = GetMergedState(block);
            
            // Simulate each instruction in the block
            foreach (var instruction in block.Instructions)
            {
                currentState = SimulateInstruction(currentState, instruction);
            }
            
            // Update final state for this block
            if (_frameStates.ContainsKey(block.EndOffset))
            {
                _frameStates[block.EndOffset] = currentState;
            }
            else
            {
                _frameStates.Add(block.EndOffset, currentState);
            }
            
            // Propagate to successors
            foreach (var successor in block.Successors)
            {
                if (!visited.Contains(successor))
                {
                    worklist.Enqueue(successor);
                    visited.Add(successor);
                }
            }
            
            // Propagate to exception handlers
            foreach (var handler in block.ExceptionHandlers)
            {
                if (!visited.Contains(handler))
                {
                    worklist.Enqueue(handler);
                    visited.Add(handler);
                }
                
                // Exception handlers start with exception object on stack
                var handlerState = new FrameState
                {
                    Locals = currentState.Locals,
                    Stack = new[] { CreateObjectTypeInfo("java/lang/Throwable") }
                };
                
                if (_frameStates.ContainsKey(handler.StartOffset))
                {
                    _frameStates[handler.StartOffset] = handlerState;
                }
                else
                {
                    _frameStates.Add(handler.StartOffset, handlerState);
                }
            }
        }
    }

    private FrameState GetMergedState(BasicBlock block)
    {
        var predecessors = _basicBlocks.Where(b => b.Successors.Contains(block)).ToList();
    
        if (predecessors.Count == 0)
        {
            // 初始块或异常处理程序
            if (_frameStates.TryGetValue(block.StartOffset, out var state))
            {
                return state.Clone();
            }
            else
            {
                // 如果没有找到初始状态，创建一个新的空状态
                return new FrameState
                {
                    Locals = new VerificationTypeInfoStruct[Code.MaxLocals],
                    Stack = new VerificationTypeInfoStruct[0]
                };
            }
        }
    
        // 检查所有前驱是否都有状态
        foreach (var pred in predecessors)
        {
            if (!_frameStates.ContainsKey(pred.EndOffset))
            {
                // 如果某个前驱没有状态，创建一个新的空状态
                _frameStates[pred.EndOffset] = new FrameState
                {
                    Locals = new VerificationTypeInfoStruct[Code.MaxLocals],
                    Stack = new VerificationTypeInfoStruct[0]
                };
            }
        }
    
        // 从第一个前驱开始合并状态
        var firstState = _frameStates[predecessors[0].EndOffset].Clone();
    
        // 与其他前驱合并
        for (int i = 1; i < predecessors.Count; i++)
        {
            var otherState = _frameStates[predecessors[i].EndOffset];
            firstState = MergeFrameStates(firstState, otherState);
        }
    
        return firstState;
    }

    private FrameState MergeFrameStates(FrameState state1, FrameState state2)
    {
        var mergedLocals = new VerificationTypeInfoStruct[Code.MaxLocals];
    
        for (int i = 0; i < Code.MaxLocals; i++)
        {
            VerificationTypeInfoStruct? local1 = null;
            VerificationTypeInfoStruct? local2 = null;
    
            if (i < state1.Locals.Length)
            {
                local1 = state1.Locals[i];
            }
    
            if (i < state2.Locals.Length)
            {
                local2 = state2.Locals[i];
            }
    
            mergedLocals[i] = MergeTypes(local1, local2);
        }

        // Handle inconsistent stack heights by finding the maximum height
        int maxStackHeight = Math.Max(state1.Stack.Length, state2.Stack.Length);
        var mergedStack = new VerificationTypeInfoStruct[maxStackHeight];
    
        for (int i = 0; i < maxStackHeight; i++)
        {
            VerificationTypeInfoStruct? stack1 = null;
            VerificationTypeInfoStruct? stack2 = null;
        
            if (i < state1.Stack.Length)
            {
                stack1 = state1.Stack[i];
            }
        
            if (i < state2.Stack.Length)
            {
                stack2 = state2.Stack[i];
            }
        
            mergedStack[i] = MergeTypes(stack1, stack2);
        }

        return new FrameState
        {
            Locals = mergedLocals,
            Stack = mergedStack
        };
    }
    
    private VerificationTypeInfoStruct? MergeTypes(VerificationTypeInfoStruct? type1, VerificationTypeInfoStruct? type2)
    {
        if (type1 == null) return type2;
        if (type2 == null) return type1;
        
        // 如果类型相同，直接返回
        if (AreTypesEqual(type1, type2))
        {
            return type1;
        }
    
        // 如果一个是 NULL，另一个是对象类型，可以合并为对象类型
        if (type1.NullVariableInfo != null && type2.ObjectVariableInfo != null)
        {
            return type2;
        }
    
        if (type2.NullVariableInfo != null && type1.ObjectVariableInfo != null)
        {
            return type1;
        }
    
        // 如果都是数值类型，可以合并为整数类型（简化处理）
        if ((type1.IntegerVariableInfo != null || type1.FloatVariableInfo != null || 
             type1.LongVariableInfo != null || type1.DoubleVariableInfo != null) &&
            (type2.IntegerVariableInfo != null || type2.FloatVariableInfo != null || 
             type2.LongVariableInfo != null || type2.DoubleVariableInfo != null))
        {
            return new VerificationTypeInfoStruct
            {
                IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
            };
        }
    
        // 默认情况下，使用 Top 类型
        return new VerificationTypeInfoStruct
        {
            TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
        };
    }


    private bool AreTypesEqual(VerificationTypeInfoStruct? type1, VerificationTypeInfoStruct? type2)
    {
        // 检查 Null 值
        if (type1 == null && type2 == null) return true;
        if (type1 == null || type2 == null) return false;
        
        // 检查 Top 类型
        if (type1.TopVariableInfo != null && type2.TopVariableInfo != null) return true;
        if (type1.TopVariableInfo != null || type2.TopVariableInfo != null) return false;
        
        // 检查 Integer 类型
        if (type1.IntegerVariableInfo != null && type2.IntegerVariableInfo != null) return true;
        if (type1.IntegerVariableInfo != null || type2.IntegerVariableInfo != null) return false;
        
        // 检查 Float 类型
        if (type1.FloatVariableInfo != null && type2.FloatVariableInfo != null) return true;
        if (type1.FloatVariableInfo != null || type2.FloatVariableInfo != null) return false;
        
        // 检查 Long 类型
        if (type1.LongVariableInfo != null && type2.LongVariableInfo != null) return true;
        if (type1.LongVariableInfo != null || type2.LongVariableInfo != null) return false;
        
        // 检查 Double 类型
        if (type1.DoubleVariableInfo != null && type2.DoubleVariableInfo != null) return true;
        if (type1.DoubleVariableInfo != null || type2.DoubleVariableInfo != null) return false;
        
        // 检查 Null 类型
        if (type1.NullVariableInfo != null && type2.NullVariableInfo != null) return true;
        if (type1.NullVariableInfo != null || type2.NullVariableInfo != null) return false;
        
        // 检查 UninitializedThis 类型
        if (type1.UninitializedThisVariableInfo != null && type2.UninitializedThisVariableInfo != null) return true;
        if (type1.UninitializedThisVariableInfo != null || type2.UninitializedThisVariableInfo != null) return false;
        
        // 检查 Object 类型
        if (type1.ObjectVariableInfo != null && type2.ObjectVariableInfo != null)
        {
            return type1.ObjectVariableInfo.CPoolIndex == type2.ObjectVariableInfo.CPoolIndex;
        }
        if (type1.ObjectVariableInfo != null || type2.ObjectVariableInfo != null) return false;
        
        // 检查 Uninitialized 类型
        if (type1.UninitializedVariableInfo != null && type2.UninitializedVariableInfo != null)
        {
            return type1.UninitializedVariableInfo.Offset == type2.UninitializedVariableInfo.Offset;
        }
        if (type1.UninitializedVariableInfo != null || type2.UninitializedVariableInfo != null) return false;
        
        // 如果没有任何类型信息，返回 False
        return false;
    }

    private FrameState SimulateInstruction(FrameState state, Code instruction) // , bool printDebug = false)
    {
        var newState = state.Clone();
        
        // if (printDebug) Console.WriteLine($"[%] SIMULATE: (L{state.Locals.Length} S{state.Stack.Length}) {instruction.OpCode.ToString()}");
        switch (instruction.OpCode)
        {
            // Constants
            case OperationCode.NOP:
                // No operation
                break;
                
            case OperationCode.ACONST_NULL:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    NullVariableInfo = new NullVariableInfoStruct { Tag = 5 }
                });
                break;
                
            case OperationCode.ICONST_M1:
            case OperationCode.ICONST_0:
            case OperationCode.ICONST_1:
            case OperationCode.ICONST_2:
            case OperationCode.ICONST_3:
            case OperationCode.ICONST_4:
            case OperationCode.ICONST_5:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.LCONST_0:
            case OperationCode.LCONST_1:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.FCONST_0:
            case OperationCode.FCONST_1:
            case OperationCode.FCONST_2:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.DCONST_0:
            case OperationCode.DCONST_1:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.BIPUSH:
            case OperationCode.SIPUSH:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.LDC:
            case OperationCode.LDC_W:
            case OperationCode.LDC2_W:
                SimulateLdc(ref newState, instruction);
                break;

            // Loads
            case OperationCode.ILOAD:
            case OperationCode.ILOAD_0:
            case OperationCode.ILOAD_1:
            case OperationCode.ILOAD_2:
            case OperationCode.ILOAD_3:
                SimulateLoad(ref newState, instruction, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.LLOAD:
            case OperationCode.LLOAD_0:
            case OperationCode.LLOAD_1:
            case OperationCode.LLOAD_2:
            case OperationCode.LLOAD_3:
                SimulateLoad(ref newState, instruction, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                break;
                
            case OperationCode.FLOAD:
            case OperationCode.FLOAD_0:
            case OperationCode.FLOAD_1:
            case OperationCode.FLOAD_2:
            case OperationCode.FLOAD_3:
                SimulateLoad(ref newState, instruction, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.DLOAD:
            case OperationCode.DLOAD_0:
            case OperationCode.DLOAD_1:
            case OperationCode.DLOAD_2:
            case OperationCode.DLOAD_3:
                SimulateLoad(ref newState, instruction, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                break;
                
            case OperationCode.ALOAD:
            case OperationCode.ALOAD_0:
            case OperationCode.ALOAD_1:
            case OperationCode.ALOAD_2:
            case OperationCode.ALOAD_3:
                SimulateLoad(ref newState, instruction, new VerificationTypeInfoStruct
                {
                    ObjectVariableInfo = new ObjectVariableInfoStruct
                    {
                        Tag = 7,
                        CPoolIndex = FindClassConstantIndex("java/lang/Object")
                    }
                });
                break;
                
            case OperationCode.IALOAD:
                newState.Stack = Pop(newState.Stack, 2); // Pop index and array ref
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.LALOAD:
                newState.Stack = Pop(newState.Stack, 2); // Pop index and array ref
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.FALOAD:
                newState.Stack = Pop(newState.Stack, 2); // Pop index and array ref
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.DALOAD:
                newState.Stack = Pop(newState.Stack, 2); // Pop index and array ref
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.AALOAD:
                newState.Stack = Pop(newState.Stack, 2); // Pop index and array ref
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    ObjectVariableInfo = new ObjectVariableInfoStruct
                    {
                        Tag = 7,
                        CPoolIndex = FindClassConstantIndex("java/lang/Object")
                    }
                });
                break;
                
            case OperationCode.BALOAD:
            case OperationCode.CALOAD:
            case OperationCode.SALOAD:
                newState.Stack = Pop(newState.Stack, 2); // Pop index and array ref
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;

            // Stores
            case OperationCode.ISTORE:
            case OperationCode.ISTORE_0:
            case OperationCode.ISTORE_1:
            case OperationCode.ISTORE_2:
            case OperationCode.ISTORE_3:
                SimulateStore(ref newState, instruction, 1);
                break;
                
            case OperationCode.LSTORE:
            case OperationCode.LSTORE_0:
            case OperationCode.LSTORE_1:
            case OperationCode.LSTORE_2:
            case OperationCode.LSTORE_3:
                SimulateStore(ref newState, instruction, 2);
                break;
                
            case OperationCode.FSTORE:
            case OperationCode.FSTORE_0:
            case OperationCode.FSTORE_1:
            case OperationCode.FSTORE_2:
            case OperationCode.FSTORE_3:
                SimulateStore(ref newState, instruction, 1);
                break;
                
            case OperationCode.DSTORE:
            case OperationCode.DSTORE_0:
            case OperationCode.DSTORE_1:
            case OperationCode.DSTORE_2:
            case OperationCode.DSTORE_3:
                SimulateStore(ref newState, instruction, 2);
                break;
                
            case OperationCode.ASTORE:
            case OperationCode.ASTORE_0:
            case OperationCode.ASTORE_1:
            case OperationCode.ASTORE_2:
            case OperationCode.ASTORE_3:
                SimulateStore(ref newState, instruction, 1);
                break;
                
            case OperationCode.IASTORE:
                newState.Stack = Pop(newState.Stack, 3); // Pop value, index, and array ref
                break;
                
            case OperationCode.LASTORE:
                newState.Stack = Pop(newState.Stack, 4); // Pop value (2 slots), index, and array ref
                break;
                
            case OperationCode.FASTORE:
                newState.Stack = Pop(newState.Stack, 3); // Pop value, index, and array ref
                break;
                
            case OperationCode.DASTORE:
                newState.Stack = Pop(newState.Stack, 4); // Pop value (2 slots), index, and array ref
                break;
                
            case OperationCode.AASTORE:
                newState.Stack = Pop(newState.Stack, 3); // Pop value, index, and array ref
                break;
                
            case OperationCode.BASTORE:
            case OperationCode.CASTORE:
            case OperationCode.SASTORE:
                newState.Stack = Pop(newState.Stack, 3); // Pop value, index, and array ref
                break;

            // Stack
            case OperationCode.POP:
                newState.Stack = Pop(newState.Stack, 1);
                break;
                
            case OperationCode.POP2:
                newState.Stack = Pop(newState.Stack, 2);
                break;
                
            case OperationCode.DUP:
                SimulateDup(ref newState);
                break;
                
            case OperationCode.DUP_X1:
                SimulateDupX1(ref newState);
                break;
                
            case OperationCode.DUP_X2:
                SimulateDupX2(ref newState);
                break;
                
            case OperationCode.DUP2:
                SimulateDup2(ref newState);
                break;
                
            case OperationCode.DUP2_X1:
                SimulateDup2X1(ref newState);
                break;
                
            case OperationCode.DUP2_X2:
                SimulateDup2X2(ref newState);
                break;
                
            case OperationCode.SWAP:
                SimulateSwap(ref newState);
                break;

            // Math
            case OperationCode.IADD:
            case OperationCode.ISUB:
            case OperationCode.IMUL:
            case OperationCode.IDIV:
            case OperationCode.IREM:
            case OperationCode.INEG:
            case OperationCode.ISHL:
            case OperationCode.ISHR:
            case OperationCode.IUSHR:
            case OperationCode.IAND:
            case OperationCode.IOR:
            case OperationCode.IXOR:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.LADD:
            case OperationCode.LSUB:
            case OperationCode.LMUL:
            case OperationCode.LDIV:
            case OperationCode.LREM:
            case OperationCode.LNEG:
            case OperationCode.LSHL:
            case OperationCode.LSHR:
            case OperationCode.LUSHR:
            case OperationCode.LAND:
            case OperationCode.LOR:
            case OperationCode.LXOR:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.FADD:
            case OperationCode.FSUB:
            case OperationCode.FMUL:
            case OperationCode.FDIV:
            case OperationCode.FREM:
            case OperationCode.FNEG:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.DADD:
            case OperationCode.DSUB:
            case OperationCode.DMUL:
            case OperationCode.DDIV:
            case OperationCode.DREM:
            case OperationCode.DNEG:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.IINC:
                // IINC doesn't affect the stack
                break;

            // Conversions
            case OperationCode.I2L:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.I2F:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.I2D:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.L2I:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.L2F:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.L2D:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.F2I:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.F2L:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.F2D:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.D2I:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.D2L:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case OperationCode.D2F:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.I2B:
            case OperationCode.I2C:
            case OperationCode.I2S:
                newState.Stack = Pop(newState.Stack, 1);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;

            // Comparisons
            case OperationCode.LCMP:
                newState.Stack = Pop(newState.Stack, 4); // Pop two longs (4 slots)
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.FCMPL:
            case OperationCode.FCMPG:
                newState.Stack = Pop(newState.Stack, 2); // Pop two floats
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.DCMPL:
            case OperationCode.DCMPG:
                newState.Stack = Pop(newState.Stack, 4); // Pop two doubles (4 slots)
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;

            // Control flow
            case OperationCode.IFEQ:
            case OperationCode.IFNE:
            case OperationCode.IFLT:
            case OperationCode.IFGE:
            case OperationCode.IFGT:
            case OperationCode.IFLE:
            case OperationCode.IF_ICMPEQ:
            case OperationCode.IF_ICMPNE:
            case OperationCode.IF_ICMPLT:
            case OperationCode.IF_ICMPGE:
            case OperationCode.IF_ICMPGT:
            case OperationCode.IF_ICMPLE:
            case OperationCode.IF_ACMPEQ:
            case OperationCode.IF_ACMPNE:
            case OperationCode.IFNULL:
            case OperationCode.IFNONNULL:
                newState.Stack = Pop(newState.Stack, IsConditionalBranch(instruction.OpCode) ? 1 : 2);
                break;
                
            case OperationCode.GOTO:
            case OperationCode.JSR:
            case OperationCode.GOTO_W:
            case OperationCode.JSR_W:
                // No stack effect
                break;
                
            case OperationCode.RET:
                // Return from subroutine - handled by control flow
                break;
                
            case OperationCode.TABLESWITCH:
            case OperationCode.LOOKUPSWITCH:
                newState.Stack = Pop(newState.Stack, 1); // Pop key
                break;
                
            case OperationCode.IRETURN:
                newState.Stack = Pop(newState.Stack, 1);
                break;
                
            case OperationCode.LRETURN:
                newState.Stack = Pop(newState.Stack, 2);
                break;
                
            case OperationCode.FRETURN:
                newState.Stack = Pop(newState.Stack, 1);
                break;
                
            case OperationCode.DRETURN:
                newState.Stack = Pop(newState.Stack, 2);
                break;
                
            case OperationCode.ARETURN:
                newState.Stack = Pop(newState.Stack, 1);
                break;
                
            case OperationCode.RETURN:
                // No stack effect
                break;

            // References
            case OperationCode.GETSTATIC:
                SimulateGetStatic(ref newState, instruction);
                break;
                
            case OperationCode.PUTSTATIC:
                SimulatePutStatic(ref newState, instruction);
                break;
                
            case OperationCode.GETFIELD:
                SimulateGetField(ref newState, instruction);
                break;
                
            case OperationCode.PUTFIELD:
                SimulatePutField(ref newState, instruction);
                break;
                
            case OperationCode.INVOKEVIRTUAL:
            case OperationCode.INVOKESPECIAL:
            case OperationCode.INVOKESTATIC:
            case OperationCode.INVOKEINTERFACE:
                SimulateInvoke(ref newState, instruction);
                break;
                
            case OperationCode.INVOKEDYNAMIC:
                SimulateInvokeDynamic(ref newState, instruction);
                break;
                
            case OperationCode.NEW:
                SimulateNew(ref newState, instruction);
                break;
                
            case OperationCode.NEWARRAY:
                newState.Stack = Pop(newState.Stack, 1); // Pop array size
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    ObjectVariableInfo = new ObjectVariableInfoStruct
                    {
                        Tag = 7,
                        CPoolIndex = FindClassConstantIndex("java/lang/Object")
                    }
                });
                break;
                
            case OperationCode.ANEWARRAY:
                newState.Stack = Pop(newState.Stack, 1); // Pop array size
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    ObjectVariableInfo = new ObjectVariableInfoStruct
                    {
                        Tag = 7,
                        CPoolIndex = GetClassIndex(instruction)
                    }
                });
                break;
                
            case OperationCode.ARRAYLENGTH:
                newState.Stack = Pop(newState.Stack, 1); // Pop array reference
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.ATHROW:
                newState.Stack = Pop(newState.Stack, 1); // Pop exception object
                break;
                
            case OperationCode.CHECKCAST:
                // CHECKCAST doesn't change the stack, just verifies the type
                break;
                
            case OperationCode.INSTANCEOF:
                newState.Stack = Pop(newState.Stack, 1); // Pop object reference
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case OperationCode.MONITORENTER:
            case OperationCode.MONITOREXIT:
                newState.Stack = Pop(newState.Stack, 1); // Pop object reference
                break;

            // Extended
            case OperationCode.WIDE:
                // WIDE prefix - handled separately in instruction decoding
                break;
                
            case OperationCode.MULTIANEWARRAY:
                int dimensions = instruction.Operands[1].Data[0];
                newState.Stack = Pop(newState.Stack, dimensions); // Pop dimension counts
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    ObjectVariableInfo = new ObjectVariableInfoStruct
                    {
                        Tag = 7,
                        CPoolIndex = GetClassIndex(instruction)
                    }
                });
                break;
                
            default:
                throw new NotSupportedException($"Unsupported opcode: {instruction.OpCode}");
        }
        
        return newState;
    }
    
    private void SimulateLdc(ref FrameState state, Code instruction)
    {
        ushort index = GetConstantIndex(instruction);
        var constantInfo = Helper.ByIndex(index);
        
        if (constantInfo == null)
            return;
            
        var constantStruct = constantInfo.ToStruct().ToConstantStruct();
        
        switch (constantStruct)
        {
            case ConstantIntegerInfoStruct intInfo:
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
                });
                break;
                
            case ConstantFloatInfoStruct floatInfo:
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case ConstantLongInfoStruct longInfo:
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case ConstantDoubleInfoStruct doubleInfo:
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
            case ConstantStringInfoStruct stringInfo:
            case ConstantClassInfoStruct classInfo:
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    ObjectVariableInfo = new ObjectVariableInfoStruct
                    {
                        Tag = 7,
                        CPoolIndex = FindClassConstantIndex("java/lang/Object")
                    }
                });
                break;
        }
    }

    private ushort GetConstantIndex(Code instruction)
    {
        if (instruction.Operands.Count > 0)
        {
            byte[] data = instruction.Operands[0].Data;
            if (data.Length == 1)
            {
                return data[0];
            }
            else if (data.Length == 2)
            {
                return (ushort)((data[0] << 8) | data[1]);
            }
        }
        return 0;
    }
    
    private void SimulateGetStatic(ref FrameState state, Code instruction)
    {
        // 获取字段引用索引
        ushort fieldRefIndex = GetFieldRefIndex(instruction);
    
        // 从常量池获取字段类型
        var fieldType = GetFieldType(fieldRefIndex);
        var verificationType = ConvertDescriptorToVerificationType(fieldType);
    
        // 将字段值推入栈
        state.Stack = Push(state.Stack, verificationType);
    
        // 对于 Long 和 Double 类型，需要额外的栈槽
        if (fieldType.Descriptor == "J" || fieldType.Descriptor == "D")
        {
            state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
            {
                TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
            });
        }
    }
    
    private void SimulatePutStatic(ref FrameState state, Code instruction)
    {
        // 获取字段引用索引
        ushort fieldRefIndex = GetFieldRefIndex(instruction);
        
        // 从常量池获取字段类型
        var fieldType = GetFieldType(fieldRefIndex);
        
        // 弹出字段值
        if (fieldType.Descriptor == "J" || fieldType.Descriptor == "D")
        {
            state.Stack = Pop(state.Stack, 2);
        }
        else
        {
            state.Stack = Pop(state.Stack, 1);
        }
    }
    
    
    private void SimulateInvokeDynamic(ref FrameState state, Code instruction)
    {
        // 获取调用点索引
        ushort invokeDynamicIndex = GetInvokeDynamicIndex(instruction);
        
        // 从常量池获取方法描述符
        var methodDescriptor = GetInvokeDynamicDescriptor(invokeDynamicIndex);
        var descriptorInfo = DescriptorParser.ParseMethodDescriptor(methodDescriptor);
        
        // 弹出参数
        int popCount = descriptorInfo.Parameters.Count;
        state.Stack = Pop(state.Stack, popCount);
        
        // 如果有返回值，推入栈
        if (descriptorInfo.ReturnType != null && descriptorInfo.ReturnType.Descriptor != "V")
        {
            var returnType = ConvertDescriptorToVerificationType(descriptorInfo.ReturnType);
            state.Stack = Push(state.Stack, returnType);
            
            // 对于 long 和 double 类型，需要额外的栈槽
            if (descriptorInfo.ReturnType.Descriptor == "J" || 
                descriptorInfo.ReturnType.Descriptor == "D")
            {
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
            }
        }
    }
    
    private ushort GetFieldRefIndex(Code instruction)
    {
        if (instruction.Operands.Count > 0)
        {
            byte[] data = instruction.Operands[0].Data;
            if (data.Length == 2)
            {
                return (ushort)((data[0] << 8) | data[1]);
            }
        }
        return 0;
    }

    private DescriptorParser.FieldTypeInfo GetFieldType(ushort fieldRefIndex)
    {
        // 从常量池获取字段描述符
        var fieldRefInfo = Helper.ByIndex(fieldRefIndex);
        if (fieldRefInfo == null)
        {
            throw new ArgumentException($"Field ref not found at index {fieldRefIndex}");
        }
        
        var fieldRefStruct = fieldRefInfo.ToStruct().ToConstantStruct() as ConstantFieldrefInfoStruct;
        if (fieldRefStruct == null)
        {
            throw new ArgumentException($"Constant at index {fieldRefIndex} is not a field reference");
        }
        
        ushort nameAndTypeIndex = fieldRefStruct.NameAndTypeIndex;
        
        var nameAndTypeInfo = Helper.ByIndex(nameAndTypeIndex);
        if (nameAndTypeInfo == null)
        {
            throw new ArgumentException($"NameAndType not found at index {nameAndTypeIndex}");
        }
        
        var nameAndTypeStruct = nameAndTypeInfo.ToStruct().ToConstantStruct() as ConstantNameAndTypeInfoStruct;
        if (nameAndTypeStruct == null)
        {
            throw new ArgumentException($"Constant at index {nameAndTypeIndex} is not a name and type");
        }
        
        ushort descriptorIndex = nameAndTypeStruct.DescriptorIndex;
        
        var descriptorInfo = Helper.ByIndex(descriptorIndex);
        if (descriptorInfo == null)
        {
            throw new ArgumentException($"Descriptor not found at index {descriptorIndex}");
        }
        
        var descriptorStruct = descriptorInfo.ToStruct().ToConstantStruct() as ConstantUtf8InfoStruct;
        if (descriptorStruct == null)
        {
            throw new ArgumentException($"Constant at index {descriptorIndex} is not a UTF8 string");
        }
        
        // 解析字段描述符
        return DescriptorParser.ParseFieldDescriptor(descriptorStruct.ToString());
    }

    private ushort GetInvokeDynamicIndex(Code instruction)
    {
        if (instruction.Operands.Count > 0)
        {
            byte[] data = instruction.Operands[0].Data;
            if (data.Length == 2)
            {
                return (ushort)((data[0] << 8) | data[1]);
            }
        }
        return 0;
    }

    private string GetInvokeDynamicDescriptor(ushort invokeDynamicIndex)
    {
        // 从常量池获取调用动态描述符
        var invokeDynamicInfo = Helper.ByIndex(invokeDynamicIndex);
        if (invokeDynamicInfo == null)
        {
            throw new ArgumentException($"InvokeDynamic not found at index {invokeDynamicIndex}");
        }
        
        var invokeDynamicStruct = invokeDynamicInfo.ToStruct().ToConstantStruct() as ConstantInvokeDynamicInfoStruct;
        if (invokeDynamicStruct == null)
        {
            throw new ArgumentException($"Constant at index {invokeDynamicIndex} is not an InvokeDynamic");
        }
        
        ushort nameAndTypeIndex = invokeDynamicStruct.NameAndTypeIndex;
        
        var nameAndTypeInfo = Helper.ByIndex(nameAndTypeIndex);
        if (nameAndTypeInfo == null)
        {
            throw new ArgumentException($"NameAndType not found at index {nameAndTypeIndex}");
        }
        
        var nameAndTypeStruct = nameAndTypeInfo.ToStruct().ToConstantStruct() as ConstantNameAndTypeInfoStruct;
        if (nameAndTypeStruct == null)
        {
            throw new ArgumentException($"Constant at index {nameAndTypeIndex} is not a name and type");
        }
        
        ushort descriptorIndex = nameAndTypeStruct.DescriptorIndex;
        
        var descriptorInfo = Helper.ByIndex(descriptorIndex);
        if (descriptorInfo == null)
        {
            throw new ArgumentException($"Descriptor not found at index {descriptorIndex}");
        }
        
        var descriptorStruct = descriptorInfo.ToStruct().ToConstantStruct() as ConstantUtf8InfoStruct;
        if (descriptorStruct == null)
        {
            throw new ArgumentException($"Constant at index {descriptorIndex} is not a UTF8 string");
        }
        
        return descriptorStruct.ToString();
    }



    private void SimulateDupX1(ref FrameState state)
    {
        if (state.Stack.Length >= 2)
        {
            var top = state.Stack[state.Stack.Length - 1];
            var second = state.Stack[state.Stack.Length - 2];
            
            state.Stack = Pop(state.Stack, 2);
            state.Stack = Push(state.Stack, top);
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
        }
    }

    private void SimulateDupX2(ref FrameState state)
    {
        if (state.Stack.Length >= 3)
        {
            var top = state.Stack[state.Stack.Length - 1];
            var second = state.Stack[state.Stack.Length - 2];
            var third = state.Stack[state.Stack.Length - 3];
            
            state.Stack = Pop(state.Stack, 3);
            state.Stack = Push(state.Stack, top);
            state.Stack = Push(state.Stack, third);
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
        }
    }

    private void SimulateDup2(ref FrameState state)
    {
        if (state.Stack.Length >= 2)
        {
            var top = state.Stack[state.Stack.Length - 1];
            var second = state.Stack[state.Stack.Length - 2];
            
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
        }
    }

    private void SimulateDup2X1(ref FrameState state)
    {
        if (state.Stack.Length >= 3)
        {
            var top = state.Stack[state.Stack.Length - 1];
            var second = state.Stack[state.Stack.Length - 2];
            var third = state.Stack[state.Stack.Length - 3];
            
            state.Stack = Pop(state.Stack, 3);
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
            state.Stack = Push(state.Stack, third);
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
        }
    }

    private void SimulateDup2X2(ref FrameState state)
    {
        if (state.Stack.Length >= 4)
        {
            var top = state.Stack[state.Stack.Length - 1];
            var second = state.Stack[state.Stack.Length - 2];
            var third = state.Stack[state.Stack.Length - 3];
            var fourth = state.Stack[state.Stack.Length - 4];
            
            state.Stack = Pop(state.Stack, 4);
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
            state.Stack = Push(state.Stack, fourth);
            state.Stack = Push(state.Stack, third);
            state.Stack = Push(state.Stack, second);
            state.Stack = Push(state.Stack, top);
        }
    }

    private void SimulateSwap(ref FrameState state)
    {
        if (state.Stack.Length >= 2)
        {
            var top = state.Stack[state.Stack.Length - 1];
            var second = state.Stack[state.Stack.Length - 2];
            
            state.Stack = Pop(state.Stack, 2);
            state.Stack = Push(state.Stack, top);
            state.Stack = Push(state.Stack, second);
        }
    }

    
    private void SimulateLoad(ref FrameState state, Code instruction, VerificationTypeInfoStruct type)
    {
        int index = GetLoadStoreIndex(instruction);
        if (index < state.Locals.Length)
        {
            state.Stack = Push(state.Stack, type);
        
            // Long and double take two stack slots
            if (instruction.OpCode == OperationCode.LLOAD || instruction.OpCode == OperationCode.DLOAD)
            {
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
            }
        }
    }


    private void SimulateStore(ref FrameState state, Code instruction, int slotCount)
    {
        int index = GetLoadStoreIndex(instruction);
    
        // Add bounds checking
        if (index >= state.Locals.Length)
        {
            // Handle error - resize locals array or skip operation
            var newLocals = new VerificationTypeInfoStruct[index + 1];
            Array.Copy(state.Locals, newLocals, state.Locals.Length);
            for (int i = state.Locals.Length; i < newLocals.Length; i++)
            {
                newLocals[i] = new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                };
            }
            state.Locals = newLocals;
        }

        // Pop the specified number of stack slots
        var values = Pop(state.Stack, slotCount);
    
        // Ensure we have enough values popped
        if (values.Length == 0)
        {
            // Handle empty stack case
            return;
        }

        // Store the first value to the local variable
        state.Locals[index] = values[0];

        // For long and double, also store Top type to the next local variable slot
        if (slotCount == 2 && index + 1 < state.Locals.Length)
        {
            state.Locals[index + 1] = new VerificationTypeInfoStruct
            {
                TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
            };
        }
    }



    private int GetLoadStoreIndex(Code instruction)
    {
        if (instruction.Operands.Count == 0 || instruction.Operands[0].Data == null)
        {
            return 0;
        }

        if (instruction.Operands.Count > 0)
        {
            byte[] data = instruction.Operands[0].Data;
            if (data.Length == 1)
            {
                return data[0];
            }
            else if (data.Length == 2)
            {
                return (data[0] << 8) | data[1];
            }
        }
        
        // Handle fixed index instructions (ILOAD_0, ILOAD_1, etc.)
        return instruction.OpCode switch
        {
            OperationCode.ILOAD_0 or OperationCode.LLOAD_0 or OperationCode.FLOAD_0 or 
            OperationCode.DLOAD_0 or OperationCode.ALOAD_0 => 0,
            OperationCode.ILOAD_1 or OperationCode.LLOAD_1 or OperationCode.FLOAD_1 or 
            OperationCode.DLOAD_1 or OperationCode.ALOAD_1 => 1,
            OperationCode.ILOAD_2 or OperationCode.LLOAD_2 or OperationCode.FLOAD_2 or 
            OperationCode.DLOAD_2 or OperationCode.ALOAD_2 => 2,
            OperationCode.ILOAD_3 or OperationCode.LLOAD_3 or OperationCode.FLOAD_3 or 
            OperationCode.DLOAD_3 or OperationCode.ALOAD_3 => 3,
            _ => 0
        };
    }

    private void SimulateBinaryOp(ref FrameState state)
    {
        // Pop two operands, push result
        state.Stack = Pop(state.Stack, 2);
        
        // Determine result type based on operation
        VerificationTypeInfoStruct resultType = new VerificationTypeInfoStruct
        {
            IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
        };
        
        state.Stack = Push(state.Stack, resultType);
    }

    private void SimulateGetField(ref FrameState state, Code instruction)
    {
        // Pop object reference
        state.Stack = Pop(state.Stack, 1);
        
        // Push field type (simplified - always use integer)
        state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
        {
            IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 }
        });
    }

    private void SimulatePutField(ref FrameState state, Code instruction)
    {
        // Pop value and object reference
        state.Stack = Pop(state.Stack, 2);
    }

    private void SimulateInvoke(ref FrameState state, Code instruction)
    {
        // 获取方法引用索引
        ushort methodRefIndex = GetMethodRefIndex(instruction);
    
        // 检查指令类型并验证常量池条目
        if (instruction.OpCode == OperationCode.INVOKEINTERFACE)
        {
            var methodRefInfo = Helper.ByIndex(methodRefIndex);
            if (methodRefInfo != null && methodRefInfo.Tag != ConstantPoolTag.InterfaceMethodref)
            {
                throw new ArgumentException($"INVOKEINTERFACE requires an interface method reference at index {methodRefIndex}");
            }
        }
        else
        {
            var methodRefInfo = Helper.ByIndex(methodRefIndex);
            if (methodRefInfo != null && methodRefInfo.Tag != ConstantPoolTag.Methodref)
            {
                throw new ArgumentException($"Non-interface invoke requires a method reference at index {methodRefIndex}");
            }
        }

        // 从常量池获取方法描述符
        var methodDescriptor = GetMethodDescriptor(methodRefIndex);
        var descriptorInfo = DescriptorParser.ParseMethodDescriptor(methodDescriptor);

        // 计算需要弹出的参数数量
        int popCount = descriptorInfo.Parameters.Count;

        // 对于非静态方法，需要额外弹出 this 引用
        if (instruction.OpCode != OperationCode.INVOKESTATIC && 
            instruction.OpCode != OperationCode.INVOKEDYNAMIC)
        {
            popCount++;
        }

        // 弹出参数
        state.Stack = Pop(state.Stack, popCount);

        // 如果有返回值，推入栈
        if (descriptorInfo.ReturnType != null && descriptorInfo.ReturnType.Descriptor != "V")
        {
            var returnType = ConvertDescriptorToVerificationType(descriptorInfo.ReturnType);
            state.Stack = Push(state.Stack, returnType);
    
            // 对于 Long 和 Double 类型，需要额外的栈槽
            if (descriptorInfo.ReturnType.Descriptor == "J" || 
                descriptorInfo.ReturnType.Descriptor == "D")
            {
                state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
            }
        }
    }

    private ushort GetMethodRefIndex(Code instruction)
    {
        if (instruction.Operands.Count > 0)
        {
            byte[] data = instruction.Operands[0].Data;
            if (data.Length == 2)
            {
                return (ushort)((data[0] << 8) | data[1]);
            }
        }
        return 0;
    }

    private string GetMethodDescriptor(ushort methodRefIndex)
    {
        var methodRefInfo = Helper.ByIndex(methodRefIndex);
        if (methodRefInfo == null)
        {
            throw new ArgumentException($"Method ref not found at index {methodRefIndex}");
        }

        // Check if it's a method reference or interface method reference
        if (methodRefInfo.Tag != ConstantPoolTag.Methodref && 
            methodRefInfo.Tag != ConstantPoolTag.InterfaceMethodref)
        {
            throw new ArgumentException($"Constant at index {methodRefIndex} is not a method reference (tag: {methodRefInfo.Tag})");
        }

        ushort nameAndTypeIndex;
        
        if (methodRefInfo.Tag == ConstantPoolTag.Methodref)
        {
            var methodRefStruct = methodRefInfo.ToStruct().ToConstantStruct() as ConstantMethodrefInfoStruct;
            if (methodRefStruct == null)
            {
                throw new ArgumentException($"Constant at index {methodRefIndex} is not a method reference");
            }
            nameAndTypeIndex = methodRefStruct.NameAndTypeIndex;
        }
        else // InterfaceMethodref
        {
            var interfaceMethodRefStruct = methodRefInfo.ToStruct().ToConstantStruct() as ConstantInterfaceMethodrefInfoStruct;
            if (interfaceMethodRefStruct == null)
            {
                throw new ArgumentException($"Constant at index {methodRefIndex} is not an interface method reference");
            }
            nameAndTypeIndex = interfaceMethodRefStruct.NameAndTypeIndex;
        }

        var nameAndTypeInfo = Helper.ByIndex(nameAndTypeIndex);
        if (nameAndTypeInfo == null)
        {
            throw new ArgumentException($"NameAndType not found at index {nameAndTypeIndex}");
        }

        var nameAndTypeStruct = nameAndTypeInfo.ToStruct().ToConstantStruct() as ConstantNameAndTypeInfoStruct;
        if (nameAndTypeStruct == null)
        {
            throw new ArgumentException($"Constant at index {nameAndTypeIndex} is not a name and type");
        }

        ushort descriptorIndex = nameAndTypeStruct.DescriptorIndex;

        var descriptorInfo = Helper.ByIndex(descriptorIndex);
        if (descriptorInfo == null)
        {
            throw new ArgumentException($"Descriptor not found at index {descriptorIndex}");
        }

        var descriptorStruct = descriptorInfo.ToStruct().ToConstantStruct() as ConstantUtf8InfoStruct;
        if (descriptorStruct == null)
        {
            throw new ArgumentException($"Constant at index {descriptorIndex} is not a UTF8 string");
        }

        return descriptorStruct.ToString();
    }

    private void SimulateNew(ref FrameState state, Code instruction)
    {
        // Push uninitialized object reference
        ushort classIndex = GetClassIndex(instruction);
        state.Stack = Push(state.Stack, new VerificationTypeInfoStruct
        {
            UninitializedVariableInfo = new UninitializedVariableInfoStruct
            {
                Tag = 8,
                Offset = (ushort)_instructionOffsets[Codes.IndexOf(instruction)]
            }
        });
    }

    private ushort GetClassIndex(Code instruction)
    {
        if (instruction.Operands.Count > 0)
        {
            byte[] data = instruction.Operands[0].Data;
            if (data.Length == 2)
            {
                return (ushort)((data[0] << 8) | data[1]);
            }
        }
        return 0;
    }

    private void SimulateDup(ref FrameState state)
    {
        if (state.Stack.Length > 0)
        {
            var top = state.Stack[state.Stack.Length - 1];
            state.Stack = Push(state.Stack, top);
        }
    }

    private VerificationTypeInfoStruct[] Push(VerificationTypeInfoStruct[] stack, VerificationTypeInfoStruct value)
    {
        var newStack = new VerificationTypeInfoStruct[stack.Length + 1];
        Array.Copy(stack, newStack, stack.Length);
        newStack[stack.Length] = value;
        return newStack;
    }

    private VerificationTypeInfoStruct[] Pop(VerificationTypeInfoStruct[] stack, int count)
    {
        if (stack.Length < count)
        {
            // Instead of throwing, return empty array or handle gracefully
            // This can happen during simulation when stack is empty
            return new VerificationTypeInfoStruct[0];
        }
  
        var newStack = new VerificationTypeInfoStruct[stack.Length - count];
        Array.Copy(stack, newStack, stack.Length - count);
        return newStack;
    }
    
    private List<StackMapFrameStruct> BuildFrames()
    {
        var frames = new List<StackMapFrameStruct>();
        
        // Get all frame positions (basic block starts)
        var framePositions = _basicBlocks.Select(b => b.StartOffset)
            .Concat(_basicBlocks.SelectMany(b => b.ExceptionHandlers).Select(h => h.StartOffset))
            .Distinct()
            .OrderBy(o => o)
            .ToList();
        
        int previousOffset = 0;
        
        foreach (var offset in framePositions)
        {
            if (_frameStates.TryGetValue(offset, out var frameState))
            {
                int offsetDelta = offset - previousOffset;
                previousOffset = offset;
                
                var frame = CreateStackMapFrame(offsetDelta, frameState);
                frames.Add(frame);
            }
        }
        
        return frames;
    }

    private StackMapFrameStruct CreateStackMapFrame(int offsetDelta, FrameState frameState)
    {
        // Ensure locals array has exactly MaxLocals elements
        if (frameState.Locals.Length != Code.MaxLocals)
        {
            var adjustedLocals = new VerificationTypeInfoStruct[Code.MaxLocals];
            Array.Copy(frameState.Locals, adjustedLocals, Math.Min(frameState.Locals.Length, Code.MaxLocals));
            for (int i = frameState.Locals.Length; i < Code.MaxLocals; i++)
            {
                adjustedLocals[i] = new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                };
            }
            frameState.Locals = adjustedLocals;
        }
        
        // Choose the most compact frame type
        if (frameState.Stack.Length == 0)
        {
            return new StackMapFrameStruct
            {
                SameFrame = new SameFrameStruct
                {
                    FrameType = (byte)Math.Min(offsetDelta, 63)
                }
            };
        }
        
        // Check for same locals with one stack item
        if (frameState.Stack.Length == 1 && offsetDelta <= 63)
        {
            return new StackMapFrameStruct
            {
                SameLocals1StackItemFrame = new SameLocals1StackItemFrameStruct
                {
                    FrameType = (byte)(64 + offsetDelta),
                    Stack = new[] { frameState.Stack[0] }
                }
            };
        }
        
        // Use extended frames for larger offsets or more complex states
        if (frameState.Stack.Length == 1)
        {
            return new StackMapFrameStruct
            {
                SameLocals1StackItemFrameExtended = new SameLocals1StackItemFrameExtendedStruct
                {
                    FrameType = 247,
                    OffsetDelta = (ushort)offsetDelta,
                    Stack = new[] { frameState.Stack[0] }
                }
            };
        }
        
        // Use full frame for complex states
        return new StackMapFrameStruct
        {
            FullFrame = new FullFrameStruct
            {
                FrameType = 255,
                OffsetDelta = (ushort)offsetDelta,
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
