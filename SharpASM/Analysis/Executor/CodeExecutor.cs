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
            
            var block = new BasicBlock
            {
                StartOffset = startOffset,
                EndOffset = endOffset,
                Instructions = GetInstructionsInRange(startOffset, endOffset)
            };
            
            _basicBlocks.Add(block);
        }
        
        // Connect basic blocks
        foreach (var block in _basicBlocks)
        {
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
        else if (typeInfo.Descriptor == "Ljava/lang/Object;")
        {
            return new VerificationTypeInfoStruct
            {
                ObjectVariableInfo = new ObjectVariableInfoStruct
                {
                    Tag = 7,
                    CPoolIndex = FindClassConstantIndex("java/lang/Object")
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
        else if (typeInfo.Descriptor.StartsWith("["))
        {
            // For arrays, we use Object type for now (simplified)
            return new VerificationTypeInfoStruct
            {
                ObjectVariableInfo = new ObjectVariableInfoStruct
                {
                    Tag = 7,
                    CPoolIndex = FindClassConstantIndex("java/lang/Object")
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
            _frameStates[block.EndOffset] = currentState;
            
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
                
                _frameStates[handler.StartOffset] = handlerState;
            }
        }
    }

    private FrameState GetMergedState(BasicBlock block)
    {
        var predecessors = _basicBlocks.Where(b => b.Successors.Contains(block)).ToList();
        
        if (predecessors.Count == 0)
        {
            // Initial block or exception handler
            if (_frameStates.TryGetValue(block.StartOffset, out var state))
            {
                return state.Clone();
            }
            else
            {
                // Should not happen for well-structured code
                return new FrameState
                {
                    Locals = new VerificationTypeInfoStruct[Code.MaxLocals],
                    Stack = new VerificationTypeInfoStruct[0]
                };
            }
        }
        
        // Start with first predecessor's state
        var firstState = _frameStates[predecessors[0].EndOffset].Clone();
        
        // Merge with other predecessors
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
            if (i < state1.Locals.Length && i < state2.Locals.Length)
            {
                mergedLocals[i] = MergeTypes(state1.Locals[i], state2.Locals[i]);
            }
            else if (i < state1.Locals.Length)
            {
                mergedLocals[i] = state1.Locals[i];
            }
            else if (i < state2.Locals.Length)
            {
                mergedLocals[i] = state2.Locals[i];
            }
            else
            {
                mergedLocals[i] = new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                };
            }
        }
    
        if (state1.Stack.Length != state2.Stack.Length)
        {
            throw new InvalidProgramException("Inconsistent stack height at merge point");
        }
    
        var mergedStack = new VerificationTypeInfoStruct[state1.Stack.Length];
        for (int i = 0; i < state1.Stack.Length; i++)
        {
            mergedStack[i] = MergeTypes(state1.Stack[i], state2.Stack[i]);
        }
    
        return new FrameState
        {
            Locals = mergedLocals,
            Stack = mergedStack
        };
    }
    
    private VerificationTypeInfoStruct MergeTypes(VerificationTypeInfoStruct type1, VerificationTypeInfoStruct type2)
    {
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


    private bool AreTypesEqual(VerificationTypeInfoStruct type1, VerificationTypeInfoStruct type2)
    {
        // Simplified type equality check
        if (type1.TopVariableInfo != null && type2.TopVariableInfo != null) return true;
        if (type1.IntegerVariableInfo != null && type2.IntegerVariableInfo != null) return true;
        if (type1.FloatVariableInfo != null && type2.FloatVariableInfo != null) return true;
        if (type1.LongVariableInfo != null && type2.LongVariableInfo != null) return true;
        if (type1.DoubleVariableInfo != null && type2.DoubleVariableInfo != null) return true;
        if (type1.NullVariableInfo != null && type2.NullVariableInfo != null) return true;
        if (type1.UninitializedThisVariableInfo != null && type2.UninitializedThisVariableInfo != null) return true;
        
        if (type1.ObjectVariableInfo != null && type2.ObjectVariableInfo != null)
        {
            return type1.ObjectVariableInfo.CPoolIndex == type2.ObjectVariableInfo.CPoolIndex;
        }
        
        if (type1.UninitializedVariableInfo != null && type2.UninitializedVariableInfo != null)
        {
            return type1.UninitializedVariableInfo.Offset == type2.UninitializedVariableInfo.Offset;
        }
        
        return false;
    }

    private FrameState SimulateInstruction(FrameState state, Code instruction)
    {
        var newState = state.Clone();
        
        switch (instruction.OpCode)
        {
            // Constants
            case OperationCode.NOP:
                // No operation
                break;
                
            case OperationCode.LCONST_0:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    LongVariableInfo = new LongVariableInfoStruct { Tag = 4 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
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
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.FCONST_1:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.FCONST_2:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
                });
                break;
                
            case OperationCode.DCONST_0:
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 }
                });
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 }
                });
                break;
                
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
                
            case OperationCode.LDC:
            case OperationCode.LDC_W:
            case OperationCode.LDC2_W:
                SimulateLdc(ref newState, instruction);
                break;

            // Loads
            case OperationCode.ILOAD_0:
            case OperationCode.ILOAD_1:
            case OperationCode.ILOAD_2:
            case OperationCode.ILOAD_3:
            case OperationCode.LLOAD_0:
            case OperationCode.LLOAD_1:
            case OperationCode.LLOAD_2:
            case OperationCode.LLOAD_3:
            case OperationCode.FLOAD_0:
            case OperationCode.FLOAD_1:
            case OperationCode.FLOAD_2:
            case OperationCode.FLOAD_3:
            case OperationCode.DLOAD_0:
            case OperationCode.DLOAD_1:
            case OperationCode.DLOAD_2:
            case OperationCode.DLOAD_3:
            case OperationCode.ALOAD_0:
            case OperationCode.ALOAD_1:
            case OperationCode.ALOAD_2:
            case OperationCode.ALOAD_3:
                SimulateLoad(ref newState, instruction);
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
            case OperationCode.ISTORE_0:
            case OperationCode.ISTORE_1:
            case OperationCode.ISTORE_2:
            case OperationCode.ISTORE_3:
            case OperationCode.LSTORE_0:
            case OperationCode.LSTORE_1:
            case OperationCode.LSTORE_2:
            case OperationCode.LSTORE_3:
            case OperationCode.FSTORE_0:
            case OperationCode.FSTORE_1:
            case OperationCode.FSTORE_2:
            case OperationCode.FSTORE_3:
            case OperationCode.DSTORE_0:
            case OperationCode.DSTORE_1:
            case OperationCode.DSTORE_2:
            case OperationCode.DSTORE_3:
            case OperationCode.ASTORE_0:
            case OperationCode.ASTORE_1:
            case OperationCode.ASTORE_2:
            case OperationCode.ASTORE_3:
                SimulateStore(ref newState, instruction);
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

            // Conversions
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
                
            case OperationCode.D2F:
                newState.Stack = Pop(newState.Stack, 2);
                newState.Stack = Push(newState.Stack, new VerificationTypeInfoStruct
                {
                    FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 }
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

            // References
            case OperationCode.PUTSTATIC:
                newState.Stack = Pop(newState.Stack, 1); // Pop value
                break;
                
            case OperationCode.INVOKEDYNAMIC:
                SimulateInvoke(ref newState, instruction);
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
                
            case OperationCode.MONITORENTER:
            case OperationCode.MONITOREXIT:
                newState.Stack = Pop(newState.Stack, 1); // Pop object reference
                break;

            // Extended
            case OperationCode.WIDE:
                // WIDE prefix - handled separately in instruction decoding
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

    
    private void SimulateLoad(ref FrameState state, Code instruction)
    {
        int index = GetLoadStoreIndex(instruction);
        if (index < state.Locals.Length)
        {
            var type = state.Locals[index];
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

    private void SimulateStore(ref FrameState state, Code instruction)
    {
        int index = GetLoadStoreIndex(instruction);
        if (index < state.Locals.Length)
        {
            VerificationTypeInfoStruct value;
            
            // Long and double take two stack slots
            if (instruction.OpCode == OperationCode.LSTORE || instruction.OpCode == OperationCode.DSTORE)
            {
                value = Pop(state.Stack, 2)[0];
            }
            else
            {
                value = Pop(state.Stack, 1)[0];
            }
            
            state.Locals[index] = value;
        }
    }

    private int GetLoadStoreIndex(Code instruction)
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
        // Get method descriptor from constant pool
        ushort methodRefIndex = GetMethodRefIndex(instruction);
        var methodDescriptor = GetMethodDescriptor(methodRefIndex);
        var descriptorInfo = DescriptorParser.ParseMethodDescriptor(methodDescriptor);
        
        // Pop arguments
        int popCount = descriptorInfo.Parameters.Count;
        if ((instruction.OpCode != OperationCode.INVOKESTATIC) && 
            (instruction.OpCode != OperationCode.INVOKEDYNAMIC))
        {
            popCount++; // Add one for 'this' reference
        }
        
        state.Stack = Pop(state.Stack, popCount);
        
        // Push return value if not void
        if (descriptorInfo.ReturnType != null && descriptorInfo.ReturnType.Descriptor != "V")
        {
            var returnType = ConvertDescriptorToVerificationType(descriptorInfo.ReturnType);
            state.Stack = Push(state.Stack, returnType);
            
            // Long and double take two stack slots
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
    
        var methodRefStruct = methodRefInfo.ToStruct().ToConstantStruct() as ConstantMethodrefInfoStruct;
        if (methodRefStruct == null)
        {
            throw new ArgumentException($"Constant at index {methodRefIndex} is not a method reference");
        }
    
        ushort nameAndTypeIndex = methodRefStruct.NameAndTypeIndex;
    
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
            throw new InvalidProgramException("Stack underflow");
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
        // Choose the most compact frame type
        
        // Check for same frame
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
