using SharpASM.Models;
using SharpASM.Models.Code;
using SharpASM.Models.Struct.Attribute;
using SharpASM.Models.Struct.Union;
using SharpASM.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpASM.Analysis
{
    public class StackMapTableRebuilder
    {
        private readonly List<Code> _codes;
        private readonly int[] _instructionOffsets;
        private readonly Dictionary<int, FrameState> _frameStates = new Dictionary<int, FrameState>();
        private readonly ConstantPoolHelper _constantPoolHelper;
        private readonly string _methodDescriptor;
        private readonly bool _isStatic;
        
        public StackMapTableRebuilder(Class classObj, Method method, List<Code> codes)
        {
            _codes = codes;
            _instructionOffsets = CalculateInstructionOffsets(codes);
            _constantPoolHelper = classObj.GetConstantPoolHelper();
            _methodDescriptor = method.Descriptor;
            _isStatic = (method.AccessFlags & MethodAccessFlags.Static) != 0;
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
                // 处理异常处理程序开始位置
                else if (code.OpCode == OperationCode.ATHROW)
                {
                    // ATHROW 指令可能需要异常处理帧
                    // 这里简化处理，实际需要分析异常处理表
                    if (i < _codes.Count - 1 && !framePositions.Contains(i + 1))
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
            var initialState = InitializeFrameState();
            
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
        /// 初始化帧状态（根据方法描述符）
        /// </summary>
        private FrameState InitializeFrameState()
        {
            var state = new FrameState();
            
            // 解析方法描述符
            var descriptorParser = new MethodDescriptorParser(_methodDescriptor);
            var parameters = descriptorParser.GetParameterTypes();
            
            // 初始化局部变量表
            var locals = new List<VerificationTypeInfoStruct>();
            int localIndex = 0;
            
            // 非静态方法，第一个局部变量是 this
            if (!_isStatic)
            {
                locals.Add(new VerificationTypeInfoStruct
                {
                    ObjectVariableInfo = new ObjectVariableInfoStruct
                    {
                        Tag = 7, // ITEM_Object
                        CPoolIndex = _constantPoolHelper.NewClass("java/lang/Object") // 假设 this 是 Object 类型
                    }
                });
                localIndex++;
            }
            
            // 添加参数到局部变量表
            foreach (var paramType in parameters)
            {
                var verificationType = GetVerificationTypeFromDescriptor(paramType);
                locals.Add(verificationType);
                
                // 如果是 long 或 double，占用两个 slot
                if (paramType == "J" || paramType == "D")
                {
                    locals.Add(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                }
                
                localIndex++;
            }
            
            state.Locals = locals.ToArray();
            state.Stack = Array.Empty<VerificationTypeInfoStruct>();
            
            return state;
        }
        
        /// <summary>
        /// 从类型描述符获取验证类型信息
        /// </summary>
        private VerificationTypeInfoStruct GetVerificationTypeFromDescriptor(string descriptor)
        {
            switch (descriptor)
            {
                case "B":
                case "C":
                case "I":
                case "S":
                case "Z":
                    return new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    };
                case "F":
                    return new VerificationTypeInfoStruct
                    {
                        FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
                    };
                case "J":
                    return new VerificationTypeInfoStruct
                    {
                        LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
                    };
                case "D":
                    return new VerificationTypeInfoStruct
                    {
                        DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
                    };
                case "Ljava/lang/String;":
                    return new VerificationTypeInfoStruct
                    {
                        ObjectVariableInfo = new ObjectVariableInfoStruct
                        {
                            Tag = 7, // ITEM_Object
                            CPoolIndex = _constantPoolHelper.NewClass("java/lang/String")
                        }
                    };
                default:
                    if (descriptor.StartsWith("L") && descriptor.EndsWith(";"))
                    {
                        string className = descriptor.Substring(1, descriptor.Length - 2);
                        return new VerificationTypeInfoStruct
                        {
                            ObjectVariableInfo = new ObjectVariableInfoStruct
                            {
                                Tag = 7, // ITEM_Object
                                CPoolIndex = _constantPoolHelper.NewClass(className)
                            }
                        };
                    }
                    else if (descriptor.StartsWith("["))
                    {
                        // 数组类型
                        return new VerificationTypeInfoStruct
                        {
                            ObjectVariableInfo = new ObjectVariableInfoStruct
                            {
                                Tag = 7, // ITEM_Object
                                CPoolIndex = _constantPoolHelper.NewClass("java/lang/Object") // 简化处理
                            }
                        };
                    }
                    else
                    {
                        // 未知类型，默认为 Object
                        return new VerificationTypeInfoStruct
                        {
                            ObjectVariableInfo = new ObjectVariableInfoStruct
                            {
                                Tag = 7, // ITEM_Object
                                CPoolIndex = _constantPoolHelper.NewClass("java/lang/Object")
                            }
                        };
                    }
            }
        }
        
        /// <summary>
        /// 执行单条指令并更新状态
        /// </summary>
        private FrameState ExecuteInstruction(Code code, FrameState state, int offset)
        {
            var newState = state.Clone();
            
            // 根据指令类型更新状态
            switch (code.OpCode)
            {
                case OperationCode.NOP:
                    // 无操作，状态不变
                    break;
                    
                case OperationCode.ACONST_NULL:
                    HandleAConstNull(newState);
                    break;
                    
                case OperationCode.ICONST_M1:
                case OperationCode.ICONST_0:
                case OperationCode.ICONST_1:
                case OperationCode.ICONST_2:
                case OperationCode.ICONST_3:
                case OperationCode.ICONST_4:
                case OperationCode.ICONST_5:
                    HandleIConst(newState);
                    break;
                    
                case OperationCode.LCONST_0:
                case OperationCode.LCONST_1:
                    HandleLConst(newState);
                    break;
                    
                case OperationCode.FCONST_0:
                case OperationCode.FCONST_1:
                case OperationCode.FCONST_2:
                    HandleFConst(newState);
                    break;
                    
                case OperationCode.DCONST_0:
                case OperationCode.DCONST_1:
                    HandleDConst(newState);
                    break;
                    
                case OperationCode.BIPUSH:
                case OperationCode.SIPUSH:
                    HandlePush(code, newState);
                    break;
                    
                case OperationCode.LDC:
                case OperationCode.LDC_W:
                case OperationCode.LDC2_W:
                    HandleLdc(code, newState);
                    break;
                    
                case OperationCode.ILOAD:
                case OperationCode.LLOAD:
                case OperationCode.FLOAD:
                case OperationCode.DLOAD:
                case OperationCode.ALOAD:
                    HandleLoad(code, newState);
                    break;
                    
                case OperationCode.ILOAD_0:
                case OperationCode.ILOAD_1:
                case OperationCode.ILOAD_2:
                case OperationCode.ILOAD_3:
                    HandleLoadN(code, newState, "I");
                    break;
                    
                case OperationCode.LLOAD_0:
                case OperationCode.LLOAD_1:
                case OperationCode.LLOAD_2:
                case OperationCode.LLOAD_3:
                    HandleLoadN(code, newState, "J");
                    break;
                    
                case OperationCode.FLOAD_0:
                case OperationCode.FLOAD_1:
                case OperationCode.FLOAD_2:
                case OperationCode.FLOAD_3:
                    HandleLoadN(code, newState, "F");
                    break;
                    
                case OperationCode.DLOAD_0:
                case OperationCode.DLOAD_1:
                case OperationCode.DLOAD_2:
                case OperationCode.DLOAD_3:
                    HandleLoadN(code, newState, "D");
                    break;
                    
                case OperationCode.ALOAD_0:
                case OperationCode.ALOAD_1:
                case OperationCode.ALOAD_2:
                case OperationCode.ALOAD_3:
                    HandleLoadN(code, newState, "Ljava/lang/Object;");
                    break;
                    
                case OperationCode.ISTORE:
                case OperationCode.LSTORE:
                case OperationCode.FSTORE:
                case OperationCode.DSTORE:
                case OperationCode.ASTORE:
                    HandleStore(code, newState);
                    break;
                    
                case OperationCode.ISTORE_0:
                case OperationCode.ISTORE_1:
                case OperationCode.ISTORE_2:
                case OperationCode.ISTORE_3:
                    HandleStoreN(code, newState, "I");
                    break;
                    
                case OperationCode.LSTORE_0:
                case OperationCode.LSTORE_1:
                case OperationCode.LSTORE_2:
                case OperationCode.LSTORE_3:
                    HandleStoreN(code, newState, "J");
                    break;
                    
                case OperationCode.FSTORE_0:
                case OperationCode.FSTORE_1:
                case OperationCode.FSTORE_2:
                case OperationCode.FSTORE_3:
                    HandleStoreN(code, newState, "F");
                    break;
                    
                case OperationCode.DSTORE_0:
                case OperationCode.DSTORE_1:
                case OperationCode.DSTORE_2:
                case OperationCode.DSTORE_3:
                    HandleStoreN(code, newState, "D");
                    break;
                    
                case OperationCode.ASTORE_0:
                case OperationCode.ASTORE_1:
                case OperationCode.ASTORE_2:
                case OperationCode.ASTORE_3:
                    HandleStoreN(code, newState, "Ljava/lang/Object;");
                    break;
                    
                case OperationCode.IADD:
                case OperationCode.LADD:
                case OperationCode.FADD:
                case OperationCode.DADD:
                case OperationCode.ISUB:
                case OperationCode.LSUB:
                case OperationCode.FSUB:
                case OperationCode.DSUB:
                case OperationCode.IMUL:
                case OperationCode.LMUL:
                case OperationCode.FMUL:
                case OperationCode.DMUL:
                case OperationCode.IDIV:
                case OperationCode.LDIV:
                case OperationCode.FDIV:
                case OperationCode.DDIV:
                case OperationCode.IREM:
                case OperationCode.LREM:
                case OperationCode.FREM:
                case OperationCode.DREM:
                case OperationCode.INEG:
                case OperationCode.LNEG:
                case OperationCode.FNEG:
                case OperationCode.DNEG:
                case OperationCode.ISHL:
                case OperationCode.LSHL:
                case OperationCode.ISHR:
                case OperationCode.LSHR:
                case OperationCode.IUSHR:
                case OperationCode.LUSHR:
                case OperationCode.IAND:
                case OperationCode.LAND:
                case OperationCode.IOR:
                case OperationCode.LOR:
                case OperationCode.IXOR:
                case OperationCode.LXOR:
                    HandleArithmeticInstruction(code, newState);
                    break;
                    
                case OperationCode.I2L:
                case OperationCode.I2F:
                case OperationCode.I2D:
                case OperationCode.L2I:
                case OperationCode.L2F:
                case OperationCode.L2D:
                case OperationCode.F2I:
                case OperationCode.F2L:
                case OperationCode.F2D:
                case OperationCode.D2I:
                case OperationCode.D2L:
                case OperationCode.D2F:
                case OperationCode.I2B:
                case OperationCode.I2C:
                case OperationCode.I2S:
                    HandleConversionInstruction(code, newState);
                    break;
                    
                case OperationCode.LCMP:
                case OperationCode.FCMPL:
                case OperationCode.FCMPG:
                case OperationCode.DCMPL:
                case OperationCode.DCMPG:
                    HandleComparisonInstruction(code, newState);
                    break;
                    
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
                    HandleConditionalBranch(code, newState);
                    break;
                    
                case OperationCode.GOTO:
                case OperationCode.GOTO_W:
                    // 无条件跳转：不改变状态
                    break;
                    
                case OperationCode.JSR:
                case OperationCode.JSR_W:
                    HandleJsr(code, newState);
                    break;
                    
                case OperationCode.RET:
                    HandleRet(code, newState);
                    break;
                    
                case OperationCode.TABLESWITCH:
                case OperationCode.LOOKUPSWITCH:
                    HandleSwitch(code, newState);
                    break;
                    
                case OperationCode.IRETURN:
                case OperationCode.LRETURN:
                case OperationCode.FRETURN:
                case OperationCode.DRETURN:
                case OperationCode.ARETURN:
                case OperationCode.RETURN:
                    HandleReturn(code, newState);
                    break;
                    
                case OperationCode.GETSTATIC:
                case OperationCode.PUTSTATIC:
                case OperationCode.GETFIELD:
                case OperationCode.PUTFIELD:
                    HandleFieldAccess(code, newState);
                    break;
                    
                case OperationCode.INVOKEVIRTUAL:
                case OperationCode.INVOKESPECIAL:
                case OperationCode.INVOKESTATIC:
                case OperationCode.INVOKEINTERFACE:
                case OperationCode.INVOKEDYNAMIC:
                    HandleMethodInvocation(code, newState);
                    break;
                    
                case OperationCode.NEW:
                    HandleNewInstruction(code, newState, offset);
                    break;
                    
                case OperationCode.NEWARRAY:
                case OperationCode.ANEWARRAY:
                case OperationCode.MULTIANEWARRAY:
                    HandleNewArray(code, newState);
                    break;
                    
                case OperationCode.ARRAYLENGTH:
                    HandleArrayLength(newState);
                    break;
                    
                case OperationCode.ATHROW:
                    HandleAthrow(newState);
                    break;
                    
                case OperationCode.CHECKCAST:
                case OperationCode.INSTANCEOF:
                    HandleTypeCheck(code, newState);
                    break;
                    
                case OperationCode.MONITORENTER:
                case OperationCode.MONITOREXIT:
                    HandleMonitor(code, newState);
                    break;
                    
                case OperationCode.WIDE:
                    HandleWide(code, newState);
                    break;
                    
                case OperationCode.DUP:
                case OperationCode.DUP_X1:
                case OperationCode.DUP_X2:
                case OperationCode.DUP2:
                case OperationCode.DUP2_X1:
                case OperationCode.DUP2_X2:
                    HandleDupInstruction(code, newState);
                    break;
                    
                case OperationCode.SWAP:
                    HandleSwap(newState);
                    break;
                    
                case OperationCode.POP:
                case OperationCode.POP2:
                    HandlePopInstruction(code, newState);
                    break;
                    
                case OperationCode.IALOAD:
                case OperationCode.LALOAD:
                case OperationCode.FALOAD:
                case OperationCode.DALOAD:
                case OperationCode.AALOAD:
                case OperationCode.BALOAD:
                case OperationCode.CALOAD:
                case OperationCode.SALOAD:
                    HandleArrayLoad(code, newState);
                    break;
                    
                case OperationCode.IASTORE:
                case OperationCode.LASTORE:
                case OperationCode.FASTORE:
                case OperationCode.DASTORE:
                case OperationCode.AASTORE:
                case OperationCode.BASTORE:
                case OperationCode.CASTORE:
                case OperationCode.SASTORE:
                    HandleArrayStore(code, newState);
                    break;
                    
                default:
                    // 未知指令，保持状态不变
                    break;
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
                var frame = CreateFrame(offsetDelta, state, i > 0 ? _frameStates[framePositions[i - 1]] : null);
                
                frames.Add(frame);
                previousOffset = offset;
            }
            
            return frames;
        }
        
        /// <summary>
        /// 根据状态创建适当的帧
        /// </summary>
        private StackMapFrameStruct CreateFrame(int offsetDelta, FrameState state, FrameState previousState = null)
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
            if (previousState != null && offsetDelta >= 248 && offsetDelta <= 250)
            {
                int choppedLocals = 251 - offsetDelta;
                if (state.Locals.Length == previousState.Locals.Length - choppedLocals)
                {
                    return new StackMapFrameStruct
                    {
                        ChopFrame = new ChopFrameStruct
                        {
                            FrameType = (byte)offsetDelta,
                            OffsetDelta = (ushort)offsetDelta
                        }
                    };
                }
            }
            
            // 4. 检查是否可以使用 append_frame
            if (previousState != null && offsetDelta >= 252 && offsetDelta <= 254)
            {
                int appendedLocals = offsetDelta - 251;
                if (state.Locals.Length == previousState.Locals.Length + appendedLocals)
                {
                    var appendedLocalsArray = new VerificationTypeInfoStruct[appendedLocals];
                    Array.Copy(state.Locals, previousState.Locals.Length, appendedLocalsArray, 0, appendedLocals);
                    
                    return new StackMapFrameStruct
                    {
                        AppendFrame = new AppendFrameStruct
                        {
                            FrameType = (byte)offsetDelta,
                            OffsetDelta = (ushort)offsetDelta,
                            Locals = appendedLocalsArray
                        }
                    };
                }
            }
            
            // 5. 检查是否可以使用 same_frame_extended
            if (offsetDelta > 63 && state.Stack.Length == 0)
            {
                return new StackMapFrameStruct
                {
                    SameFrameExtended = new SameFrameExtendedStruct
                    {
                        FrameType = 251,
                        OffsetDelta = (ushort)offsetDelta
                    }
                };
            }
            
            // 6. 检查是否可以使用 same_locals_1_stack_item_frame_extended
            if (offsetDelta > 63 && state.Stack.Length == 1)
            {
                return new StackMapFrameStruct
                {
                    SameLocals1StackItemFrameExtended = new SameLocals1StackItemFrameExtendedStruct
                    {
                        FrameType = 247,
                        OffsetDelta = (ushort)offsetDelta,
                        Stack = new[] { state.Stack[0] }
                    }
                };
            }
            
            // 7. 使用 full_frame (最通用的帧类型)
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
        
        #region 指令处理方法的实现
        
        private void HandleAConstNull(FrameState state)
        {
            // 推送 null 到操作数栈
            state.Push(new VerificationTypeInfoStruct
            {
                NullVariableInfo = new NullVariableInfoStruct { Tag = 5 } // ITEM_Null
            });
        }
        
        private void HandleIConst(FrameState state)
        {
            // 推送 int 常量到操作数栈
            state.Push(new VerificationTypeInfoStruct
            {
                IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
            });
        }
        
        private void HandleLConst(FrameState state)
        {
            // 推送 long 常量到操作数栈
            state.Push(new VerificationTypeInfoStruct
            {
                LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
            });
        }
        
        private void HandleFConst(FrameState state)
        {
            // 推送 float 常量到操作数栈
            state.Push(new VerificationTypeInfoStruct
            {
                FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
            });
        }
        
        private void HandleDConst(FrameState state)
        {
            // 推送 double 常量到操作数栈
            state.Push(new VerificationTypeInfoStruct
            {
                DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
            });
        }
        
        private void HandlePush(Code code, FrameState state)
        {
            // 推送常量到操作数栈
            state.Push(new VerificationTypeInfoStruct
            {
                IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
            });
        }
        
        private void HandleLdc(Code code, FrameState state)
        {
            // 从常量池加载常量到操作数栈
            if (code.Operands.Count > 0)
            {
                ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                var constant = _constantPoolHelper.ByIndex(index);
                
                if (constant != null)
                {
                    var constantStruct = constant.ToStruct().ToConstantStruct();
                    
                    switch (constantStruct)
                    {
                        case ConstantIntegerInfoStruct intConst:
                            state.Push(new VerificationTypeInfoStruct
                            {
                                IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                            });
                            break;
                        case ConstantFloatInfoStruct floatConst:
                            state.Push(new VerificationTypeInfoStruct
                            {
                                FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
                            });
                            break;
                        case ConstantLongInfoStruct longConst:
                            state.Push(new VerificationTypeInfoStruct
                            {
                                LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
                            });
                            break;
                        case ConstantDoubleInfoStruct doubleConst:
                            state.Push(new VerificationTypeInfoStruct
                            {
                                DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
                            });
                            break;
                        case ConstantStringInfoStruct stringConst:
                            state.Push(new VerificationTypeInfoStruct
                            {
                                ObjectVariableInfo = new ObjectVariableInfoStruct
                                {
                                    Tag = 7, // ITEM_Object
                                    CPoolIndex = _constantPoolHelper.NewClass("java/lang/String")
                                }
                            });
                            break;
                        case ConstantClassInfoStruct classConst:
                            state.Push(new VerificationTypeInfoStruct
                            {
                                ObjectVariableInfo = new ObjectVariableInfoStruct
                                {
                                    Tag = 7, // ITEM_Object
                                    CPoolIndex = _constantPoolHelper.NewClass("java/lang/Class")
                                }
                            });
                            break;
                        default:
                            // 未知类型，默认为 Object
                            state.Push(new VerificationTypeInfoStruct
                            {
                                ObjectVariableInfo = new ObjectVariableInfoStruct
                                {
                                    Tag = 7, // ITEM_Object
                                    CPoolIndex = _constantPoolHelper.NewClass("java/lang/Object")
                                }
                            });
                            break;
                    }
                }
            }
        }
        
        private void HandleLoad(Code code, FrameState state)
        {
            // 从局部变量加载值到操作数栈
            if (code.Operands.Count > 0)
            {
                ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                
                if (index < state.Locals.Length)
                {
                    var local = state.Locals[index];
                    state.Push(local);
                    
                    // 如果是 long 或 double，占用两个 slot
                    if (local.LongVariableInfo != null || local.DoubleVariableInfo != null)
                    {
                        // 下一个 slot 应该是 TOP
                        if (index + 1 < state.Locals.Length)
                        {
                            var nextLocal = state.Locals[index + 1];
                            if (nextLocal.TopVariableInfo == null)
                            {
                                // 如果不是 TOP，可能有问题，但我们还是推送
                                state.Push(new VerificationTypeInfoStruct
                                {
                                    TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                                });
                            }
                        }
                    }
                }
            }
        }
        
        private void HandleLoadN(Code code, FrameState state, string type)
        {
            // 从固定位置的局部变量加载值到操作数栈
            int index = code.OpCode switch
            {
                OperationCode.ILOAD_0 => 0,
                OperationCode.ILOAD_1 => 1,
                OperationCode.ILOAD_2 => 2,
                OperationCode.ILOAD_3 => 3,
                OperationCode.LLOAD_0 => 0,
                OperationCode.LLOAD_1 => 1,
                OperationCode.LLOAD_2 => 2,
                OperationCode.LLOAD_3 => 3,
                OperationCode.FLOAD_0 => 0,
                OperationCode.FLOAD_1 => 1,
                OperationCode.FLOAD_2 => 2,
                OperationCode.FLOAD_3 => 3,
                OperationCode.DLOAD_0 => 0,
                OperationCode.DLOAD_1 => 1,
                OperationCode.DLOAD_2 => 2,
                OperationCode.DLOAD_3 => 3,
                OperationCode.ALOAD_0 => 0,
                OperationCode.ALOAD_1 => 1,
                OperationCode.ALOAD_2 => 2,
                OperationCode.ALOAD_3 => 3,
                _ => 0
            };
            
            if (index < state.Locals.Length)
            {
                var local = state.Locals[index];
                state.Push(local);
                
                // 如果是 long 或 double，占用两个 slot
                if (local.LongVariableInfo != null || local.DoubleVariableInfo != null)
                {
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                }
            }
        }
        
        private void HandleStore(Code code, FrameState state)
        {
            // 从操作数栈存储值到局部变量
            if (code.Operands.Count > 0)
            {
                ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                
                if (index < state.Locals.Length)
                {
                    var value = state.Pop();
                    state.Locals[index] = value;
                    
                    // 如果是 long 或 double，占用两个 slot
                    if (value.LongVariableInfo != null || value.DoubleVariableInfo != null)
                    {
                        if (index + 1 < state.Locals.Length)
                        {
                            state.Locals[index + 1] = new VerificationTypeInfoStruct
                            {
                                TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                            };
                        }
                    }
                }
            }
        }
        
        private void HandleStoreN(Code code, FrameState state, string type)
        {
            // 从操作数栈存储值到固定位置的局部变量
            int index = code.OpCode switch
            {
                OperationCode.ISTORE_0 => 0,
                OperationCode.ISTORE_1 => 1,
                OperationCode.ISTORE_2 => 2,
                OperationCode.ISTORE_3 => 3,
                OperationCode.LSTORE_0 => 0,
                OperationCode.LSTORE_1 => 1,
                OperationCode.LSTORE_2 => 2,
                OperationCode.LSTORE_3 => 3,
                OperationCode.FSTORE_0 => 0,
                OperationCode.FSTORE_1 => 1,
                OperationCode.FSTORE_2 => 2,
                OperationCode.FSTORE_3 => 3,
                OperationCode.DSTORE_0 => 0,
                OperationCode.DSTORE_1 => 1,
                OperationCode.DSTORE_2 => 2,
                OperationCode.DSTORE_3 => 3,
                OperationCode.ASTORE_0 => 0,
                OperationCode.ASTORE_1 => 1,
                OperationCode.ASTORE_2 => 2,
                OperationCode.ASTORE_3 => 3,
                _ => 0
            };
            
            if (index < state.Locals.Length)
            {
                var value = state.Pop();
                state.Locals[index] = value;
                
                // 如果是 long 或 double，占用两个 slot
                if (value.LongVariableInfo != null || value.DoubleVariableInfo != null)
                {
                    if (index + 1 < state.Locals.Length)
                    {
                        state.Locals[index + 1] = new VerificationTypeInfoStruct
                        {
                            TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                        };
                    }
                }
            }
        }
        
        private void HandleArithmeticInstruction(Code code, FrameState state)
        {
            // 算术指令：消耗操作数栈顶部的值并推送结果
            switch (code.OpCode)
            {
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
                    state.Pop(); // 弹出操作数
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
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
                    state.Pop(); // 弹出操作数 (long 占用两个 slot)
                    state.Pop(); // 弹出第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.FADD:
                case OperationCode.FSUB:
                case OperationCode.FMUL:
                case OperationCode.FDIV:
                case OperationCode.FREM:
                case OperationCode.FNEG:
                    state.Pop(); // 弹出操作数
                    state.Push(new VerificationTypeInfoStruct
                    {
                        FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
                    });
                    break;
                    
                case OperationCode.DADD:
                case OperationCode.DSUB:
                case OperationCode.DMUL:
                case OperationCode.DDIV:
                case OperationCode.DREM:
                case OperationCode.DNEG:
                    state.Pop(); // 弹出操作数 (double 占用两个 slot)
                    state.Pop(); // 弹出第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
            }
        }
        
        private void HandleConversionInstruction(Code code, FrameState state)
        {
            // 类型转换指令：消耗操作数栈顶部的值并推送转换后的结果
            var value = state.Pop();
            
            switch (code.OpCode)
            {
                case OperationCode.I2L:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.I2F:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
                    });
                    break;
                    
                case OperationCode.I2D:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.L2I:
                    state.Pop(); // 弹出 long 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
                    
                case OperationCode.L2F:
                    state.Pop(); // 弹出 long 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
                    });
                    break;
                    
                case OperationCode.L2D:
                    state.Pop(); // 弹出 long 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.F2I:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
                    
                case OperationCode.F2L:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.F2D:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.D2I:
                    state.Pop(); // 弹出 double 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
                    
                case OperationCode.D2L:
                    state.Pop(); // 弹出 double 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.D2F:
                    state.Pop(); // 弹出 double 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
                    });
                    break;
                    
                case OperationCode.I2B:
                case OperationCode.I2C:
                case OperationCode.I2S:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
            }
        }
        
        private void HandleComparisonInstruction(Code code, FrameState state)
        {
            // 比较指令：消耗操作数栈顶部的值并推送比较结果
            switch (code.OpCode)
            {
                case OperationCode.LCMP:
                    state.Pop(); // 弹出第二个 long
                    state.Pop(); // 弹出第一个 long
                    state.Pop(); // 弹出第二个 long 的第二个 slot
                    state.Pop(); // 弹出第一个 long 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
                    
                case OperationCode.FCMPL:
                case OperationCode.FCMPG:
                    state.Pop(); // 弹出第二个 float
                    state.Pop(); // 弹出第一个 float
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
                    
                case OperationCode.DCMPL:
                case OperationCode.DCMPG:
                    state.Pop(); // 弹出第二个 double
                    state.Pop(); // 弹出第一个 double
                    state.Pop(); // 弹出第二个 double 的第二个 slot
                    state.Pop(); // 弹出第一个 double 的第二个 slot
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
            }
        }
        
        private void HandleConditionalBranch(Code code, FrameState state)
        {
            // 条件跳转指令：消耗操作数栈顶部的值
            switch (code.OpCode)
            {
                case OperationCode.IFEQ:
                case OperationCode.IFNE:
                case OperationCode.IFLT:
                case OperationCode.IFGE:
                case OperationCode.IFGT:
                case OperationCode.IFLE:
                case OperationCode.IFNULL:
                case OperationCode.IFNONNULL:
                    state.Pop(); // 弹出条件值
                    break;
                    
                case OperationCode.IF_ICMPEQ:
                case OperationCode.IF_ICMPNE:
                case OperationCode.IF_ICMPLT:
                case OperationCode.IF_ICMPGE:
                case OperationCode.IF_ICMPGT:
                case OperationCode.IF_ICMPLE:
                    state.Pop(); // 弹出第二个 int
                    state.Pop(); // 弹出第一个 int
                    break;
                    
                case OperationCode.IF_ACMPEQ:
                case OperationCode.IF_ACMPNE:
                    state.Pop(); // 弹出第二个引用
                    state.Pop(); // 弹出第一个引用
                    break;
            }
        }
        
        private void HandleJsr(Code code, FrameState state)
        {
            // JSR 指令：推送返回地址到操作数栈
            state.Push(new VerificationTypeInfoStruct
            {
                ObjectVariableInfo = new ObjectVariableInfoStruct
                {
                    Tag = 7, // ITEM_Object
                    CPoolIndex = _constantPoolHelper.NewClass("java/lang/ReturnAddress")
                }
            });
        }
        
        private void HandleRet(Code code, FrameState state)
        {
            // RET 指令：从局部变量获取返回地址并跳转
            // 不改变操作数栈状态
        }
        
        private void HandleSwitch(Code code, FrameState state)
        {
            // switch 指令：消耗操作数栈顶部的值
            state.Pop(); // 弹出 switch 值
        }
        
        private void HandleReturn(Code code, FrameState state)
        {
            // 返回指令：清空操作数栈
            switch (code.OpCode)
            {
                case OperationCode.IRETURN:
                    state.Pop(); // 弹出返回值
                    break;
                    
                case OperationCode.LRETURN:
                    state.Pop(); // 弹出返回值
                    state.Pop(); // 弹出 long 的第二个 slot
                    break;
                    
                case OperationCode.FRETURN:
                    state.Pop(); // 弹出返回值
                    break;
                    
                case OperationCode.DRETURN:
                    state.Pop(); // 弹出返回值
                    state.Pop(); // 弹出 double 的第二个 slot
                    break;
                    
                case OperationCode.ARETURN:
                    state.Pop(); // 弹出返回值
                    break;
                    
                case OperationCode.RETURN:
                    // 无返回值，不弹出
                    break;
            }
            
            // 清空操作数栈
            state.Stack = Array.Empty<VerificationTypeInfoStruct>();
        }
        
        private void HandleFieldAccess(Code code, FrameState state)
        {
            // 字段访问指令
            if (code.Operands.Count > 0)
            {
                ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                var constant = _constantPoolHelper.ByIndex(index);
                
                if (constant != null)
                {
                    var constantStruct = constant.ToStruct().ToConstantStruct();
                    
                    if (constantStruct is ConstantFieldrefInfoStruct fieldRef)
                    {
                        var nameAndType = _constantPoolHelper.ByIndex(fieldRef.NameAndTypeIndex);
                        
                        if (nameAndType != null)
                        {
                            var nameAndTypeStruct = nameAndType.ToStruct().ToConstantStruct() as ConstantNameAndTypeInfoStruct;
                            
                            if (nameAndTypeStruct != null)
                            {
                                var descriptor = GetUtf8String(nameAndTypeStruct.DescriptorIndex);
                                
                                switch (code.OpCode)
                                {
                                    case OperationCode.GETSTATIC:
                                        // 获取静态字段：推送字段值到操作数栈
                                        var fieldType = GetVerificationTypeFromDescriptor(descriptor);
                                        state.Push(fieldType);
                                        
                                        // 如果是 long 或 double，占用两个 slot
                                        if (fieldType.LongVariableInfo != null || fieldType.DoubleVariableInfo != null)
                                        {
                                            state.Push(new VerificationTypeInfoStruct
                                            {
                                                TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                                            });
                                        }
                                        break;
                                        
                                    case OperationCode.PUTSTATIC:
                                        // 设置静态字段：弹出字段值
                                        state.Pop(); // 弹出字段值
                                        
                                        // 如果是 long 或 double，弹出第二个 slot
                                        if (descriptor == "J" || descriptor == "D")
                                        {
                                            state.Pop(); // 弹出第二个 slot
                                        }
                                        break;
                                        
                                    case OperationCode.GETFIELD:
                                        // 获取实例字段：弹出对象引用，推送字段值
                                        state.Pop(); // 弹出对象引用
                                        var fieldType2 = GetVerificationTypeFromDescriptor(descriptor);
                                        state.Push(fieldType2);
                                        
                                        // 如果是 long 或 double，占用两个 slot
                                        if (fieldType2.LongVariableInfo != null || fieldType2.DoubleVariableInfo != null)
                                        {
                                            state.Push(new VerificationTypeInfoStruct
                                            {
                                                TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                                            });
                                        }
                                        break;
                                        
                                    case OperationCode.PUTFIELD:
                                        // 设置实例字段：弹出对象引用和字段值
                                        state.Pop(); // 弹出字段值
                                        state.Pop(); // 弹出对象引用
                                        
                                        // 如果是 long 或 double，弹出第二个 slot
                                        if (descriptor == "J" || descriptor == "D")
                                        {
                                            state.Pop(); // 弹出第二个 slot
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void HandleMethodInvocation(Code code, FrameState state)
        {
            // 方法调用指令
            if (code.Operands.Count > 0)
            {
                ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                var constant = _constantPoolHelper.ByIndex(index);
                
                if (constant != null)
                {
                    var constantStruct = constant.ToStruct().ToConstantStruct();
                    
                    if (constantStruct is ConstantMethodrefInfoStruct methodRef ||
                        constantStruct is ConstantInterfaceMethodrefInfoStruct interfaceMethodRef)
                    {
                        ushort nameAndTypeIndex = methodRef != null ? methodRef.NameAndTypeIndex : interfaceMethodRef.NameAndTypeIndex;
                        var nameAndType = _constantPoolHelper.ByIndex(nameAndTypeIndex);
                        
                        if (nameAndType != null)
                        {
                            var nameAndTypeStruct = nameAndType.ToStruct().ToConstantStruct() as ConstantNameAndTypeInfoStruct;
                            
                            if (nameAndTypeStruct != null)
                            {
                                var descriptor = GetUtf8String(nameAndTypeStruct.DescriptorIndex);
                                var descriptorParser = new MethodDescriptorParser(descriptor);
                                var parameters = descriptorParser.GetParameterTypes();
                                
                                // 弹出参数
                                foreach (var paramType in parameters.Reverse())
                                {
                                    state.Pop(); // 弹出参数
                                    
                                    // 如果是 long 或 double，弹出第二个 slot
                                    if (paramType == "J" || paramType == "D")
                                    {
                                        state.Pop(); // 弹出第二个 slot
                                    }
                                }
                                
                                // 如果是实例方法，弹出对象引用
                                if (code.OpCode != OperationCode.INVOKESTATIC && code.OpCode != OperationCode.INVOKEDYNAMIC)
                                {
                                    state.Pop(); // 弹出对象引用
                                }
                                
                                // 如果有返回值，推送返回值到操作数栈
                                var returnType = descriptorParser.GetReturnType();
                                if (returnType != "V")
                                {
                                    var returnVerificationType = GetVerificationTypeFromDescriptor(returnType);
                                    state.Push(returnVerificationType);
                                    
                                    // 如果是 long 或 double，占用两个 slot
                                    if (returnVerificationType.LongVariableInfo != null || returnVerificationType.DoubleVariableInfo != null)
                                    {
                                        state.Push(new VerificationTypeInfoStruct
                                        {
                                            TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void HandleNewInstruction(Code code, FrameState state, int offset)
        {
            // NEW 指令：创建新对象并推送引用到操作数栈
            if (code.Operands.Count > 0)
            {
                ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                var constant = _constantPoolHelper.ByIndex(index);
                
                if (constant != null)
                {
                    var constantStruct = constant.ToStruct().ToConstantStruct();
                    
                    if (constantStruct is ConstantClassInfoStruct classInfo)
                    {
                        var className = GetUtf8String(classInfo.NameIndex);
                        
                        state.Push(new VerificationTypeInfoStruct
                        {
                            UninitializedVariableInfo = new UninitializedVariableInfoStruct
                            {
                                Tag = 8, // ITEM_Uninitialized
                                Offset = (ushort)offset
                            }
                        });
                    }
                }
            }
        }
        
        private void HandleNewArray(Code code, FrameState state)
        {
            // 创建数组指令
            switch (code.OpCode)
            {
                case OperationCode.NEWARRAY:
                    state.Pop(); // 弹出数组长度
                    state.Push(new VerificationTypeInfoStruct
                    {
                        ObjectVariableInfo = new ObjectVariableInfoStruct
                        {
                            Tag = 7, // ITEM_Object
                            CPoolIndex = _constantPoolHelper.NewClass("java/lang/Object") // 简化处理
                        }
                    });
                    break;
                    
                case OperationCode.ANEWARRAY:
                    state.Pop(); // 弹出数组长度
                    if (code.Operands.Count > 0)
                    {
                        ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                        var constant = _constantPoolHelper.ByIndex(index);
                        
                        if (constant != null)
                        {
                            var constantStruct = constant.ToStruct().ToConstantStruct();
                            
                            if (constantStruct is ConstantClassInfoStruct classInfo)
                            {
                                var className = GetUtf8String(classInfo.NameIndex);
                                
                                state.Push(new VerificationTypeInfoStruct
                                {
                                    ObjectVariableInfo = new ObjectVariableInfoStruct
                                    {
                                        Tag = 7, // ITEM_Object
                                        CPoolIndex = _constantPoolHelper.NewClass(className + "[]") // 数组类型
                                    }
                                });
                            }
                        }
                    }
                    break;
                    
                case OperationCode.MULTIANEWARRAY:
                    if (code.Operands.Count > 1)
                    {
                        byte dimensions = code.Operands[1].Data[0];
                        
                        // 弹出维度参数
                        for (int i = 0; i < dimensions; i++)
                        {
                            state.Pop(); // 弹出维度长度
                        }
                        
                        ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                        var constant = _constantPoolHelper.ByIndex(index);
                        
                        if (constant != null)
                        {
                            var constantStruct = constant.ToStruct().ToConstantStruct();
                            
                            if (constantStruct is ConstantClassInfoStruct classInfo)
                            {
                                var className = GetUtf8String(classInfo.NameIndex);
                                
                                state.Push(new VerificationTypeInfoStruct
                                {
                                    ObjectVariableInfo = new ObjectVariableInfoStruct
                                    {
                                        Tag = 7, // ITEM_Object
                                        CPoolIndex = _constantPoolHelper.NewClass(className)
                                    }
                                });
                            }
                        }
                    }
                    break;
            }
        }
        
        private void HandleArrayLength(FrameState state)
        {
            // 数组长度指令：弹出数组引用，推送长度
            state.Pop(); // 弹出数组引用
            state.Push(new VerificationTypeInfoStruct
            {
                IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
            });
        }
        
        private void HandleAthrow(FrameState state)
        {
            // 抛出异常指令：弹出异常对象引用
            state.Pop(); // 弹出异常对象引用
        }
        
        private void HandleTypeCheck(Code code, FrameState state)
        {
            // 类型检查指令：弹出对象引用，推送检查结果
            state.Pop(); // 弹出对象引用
            
            if (code.OpCode == OperationCode.CHECKCAST)
            {
                // CHECKCAST：推送相同的对象引用（但类型改变）
                if (code.Operands.Count > 0)
                {
                    ushort index = BitConverter.ToUInt16(code.Operands[0].Data);
                    var constant = _constantPoolHelper.ByIndex(index);
                    
                    if (constant != null)
                    {
                        var constantStruct = constant.ToStruct().ToConstantStruct();
                        
                        if (constantStruct is ConstantClassInfoStruct classInfo)
                        {
                            var className = GetUtf8String(classInfo.NameIndex);
                            
                            state.Push(new VerificationTypeInfoStruct
                            {
                                ObjectVariableInfo = new ObjectVariableInfoStruct
                                {
                                    Tag = 7, // ITEM_Object
                                    CPoolIndex = _constantPoolHelper.NewClass(className)
                                }
                            });
                        }
                    }
                }
            }
            else if (code.OpCode == OperationCode.INSTANCEOF)
            {
                // INSTANCEOF：推送 boolean 结果
                state.Push(new VerificationTypeInfoStruct
                {
                    IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                });
            }
        }
        
        private void HandleMonitor(Code code, FrameState state)
        {
            // 监视器指令：弹出对象引用
            state.Pop(); // 弹出对象引用
        }
        
        private void HandleWide(Code code, FrameState state)
        {
            // WIDE 指令：修改下一条指令的行为
            // 这里简化处理，实际需要处理下一条指令
        }
        
        private void HandleDupInstruction(Code code, FrameState state)
        {
            // 复制指令：复制操作数栈顶部的值
            switch (code.OpCode)
            {
                case OperationCode.DUP:
                    var value = state.Peek();
                    state.Push(value.Clone());
                    break;
                    
                case OperationCode.DUP_X1:
                    var value1 = state.Pop();
                    var value2 = state.Pop();
                    state.Push(value1.Clone());
                    state.Push(value2);
                    state.Push(value1);
                    break;
                    
                case OperationCode.DUP_X2:
                    var val1 = state.Pop();
                    var val2 = state.Pop();
                    var val3 = state.Pop();
                    state.Push(val1.Clone());
                    state.Push(val3);
                    state.Push(val2);
                    state.Push(val1);
                    break;
                    
                case OperationCode.DUP2:
                    var v1 = state.Pop();
                    var v2 = state.Pop();
                    state.Push(v2.Clone());
                    state.Push(v1.Clone());
                    state.Push(v2);
                    state.Push(v1);
                    break;
                    
                case OperationCode.DUP2_X1:
                    var va1 = state.Pop();
                    var va2 = state.Pop();
                    var va3 = state.Pop();
                    state.Push(va2.Clone());
                    state.Push(va1.Clone());
                    state.Push(va3);
                    state.Push(va2);
                    state.Push(va1);
                    break;
                    
                case OperationCode.DUP2_X2:
                    var vv1 = state.Pop();
                    var vv2 = state.Pop();
                    var vv3 = state.Pop();
                    var vv4 = state.Pop();
                    state.Push(vv2.Clone());
                    state.Push(vv1.Clone());
                    state.Push(vv4);
                    state.Push(vv3);
                    state.Push(vv2);
                    state.Push(vv1);
                    break;
            }
        }
        
        private void HandleSwap(FrameState state)
        {
            // SWAP 指令：交换操作数栈顶部的两个值
            var value1 = state.Pop();
            var value2 = state.Pop();
            state.Push(value1);
            state.Push(value2);
        }
        
        private void HandlePopInstruction(Code code, FrameState state)
        {
            // 弹出指令：移除操作数栈顶部的值
            switch (code.OpCode)
            {
                case OperationCode.POP:
                    state.Pop();
                    break;
                    
                case OperationCode.POP2:
                    state.Pop();
                    state.Pop();
                    break;
            }
        }
        
        private void HandleArrayLoad(Code code, FrameState state)
        {
            // 数组加载指令：弹出数组引用和索引，推送数组元素
            state.Pop(); // 弹出索引
            state.Pop(); // 弹出数组引用
            
            switch (code.OpCode)
            {
                case OperationCode.IALOAD:
                case OperationCode.BALOAD:
                case OperationCode.CALOAD:
                case OperationCode.SALOAD:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        IntegerVariableInfo = new IntegerVariableInfoStruct { Tag = 1 } // ITEM_Integer
                    });
                    break;
                    
                case OperationCode.LALOAD:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        LongVariableInfo = new LongVariableInfoStruct { Tag = 4 } // ITEM_Long
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.FALOAD:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        FloatVariableInfo = new FloatVariableInfoStruct { Tag = 2 } // ITEM_Float
                    });
                    break;
                    
                case OperationCode.DALOAD:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        DoubleVariableInfo = new DoubleVariableInfoStruct { Tag = 3 } // ITEM_Double
                    });
                    state.Push(new VerificationTypeInfoStruct
                    {
                        TopVariableInfo = new TopVariableInfoStruct { Tag = 0 } // ITEM_Top
                    });
                    break;
                    
                case OperationCode.AALOAD:
                    state.Push(new VerificationTypeInfoStruct
                    {
                        ObjectVariableInfo = new ObjectVariableInfoStruct
                        {
                            Tag = 7, // ITEM_Object
                            CPoolIndex = _constantPoolHelper.NewClass("java/lang/Object") // 简化处理
                        }
                    });
                    break;
            }
        }
        
        private void HandleArrayStore(Code code, FrameState state)
        {
            // 数组存储指令：弹出数组引用、索引和值
            switch (code.OpCode)
            {
                case OperationCode.IASTORE:
                case OperationCode.BASTORE:
                case OperationCode.CASTORE:
                case OperationCode.SASTORE:
                    state.Pop(); // 弹出值
                    state.Pop(); // 弹出索引
                    state.Pop(); // 弹出数组引用
                    break;
                    
                case OperationCode.LASTORE:
                    state.Pop(); // 弹出值
                    state.Pop(); // 弹出 long 的第二个 slot
                    state.Pop(); // 弹出索引
                    state.Pop(); // 弹出数组引用
                    break;
                    
                case OperationCode.FASTORE:
                    state.Pop(); // 弹出值
                    state.Pop(); // 弹出索引
                    state.Pop(); // 弹出数组引用
                    break;
                    
                case OperationCode.DASTORE:
                    state.Pop(); // 弹出值
                    state.Pop(); // 弹出 double 的第二个 slot
                    state.Pop(); // 弹出索引
                    state.Pop(); // 弹出数组引用
                    break;
                    
                case OperationCode.AASTORE:
                    state.Pop(); // 弹出值
                    state.Pop(); // 弹出索引
                    state.Pop(); // 弹出数组引用
                    break;
            }
        }
        
        private string GetUtf8String(ushort index)
        {
            var constant = _constantPoolHelper.ByIndex(index);
            if (constant != null && constant.Tag == ConstantPoolTag.Utf8)
            {
                var utf8Struct = constant.ToStruct().ToConstantStruct() as ConstantUtf8InfoStruct;
                if (utf8Struct != null)
                {
                    return Encoding.UTF8.GetString(utf8Struct.Bytes);
                }
            }
            return string.Empty;
        }
        
        #endregion
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
                Locals = Locals?.Select(l => l.Clone()).ToArray(),
                Stack = Stack?.Select(s => s.Clone()).ToArray()
            };
        }
        
        public void Push(VerificationTypeInfoStruct value)
        {
            var newStack = new VerificationTypeInfoStruct[Stack.Length + 1];
            Array.Copy(Stack, 0, newStack, 0, Stack.Length);
            newStack[Stack.Length] = value;
            Stack = newStack;
        }
        
        public VerificationTypeInfoStruct Pop()
        {
            if (Stack.Length == 0)
                return null;
                
            var value = Stack[Stack.Length - 1];
            var newStack = new VerificationTypeInfoStruct[Stack.Length - 1];
            Array.Copy(Stack, 0, newStack, 0, Stack.Length - 1);
            Stack = newStack;
            return value;
        }
        
        public VerificationTypeInfoStruct Peek()
        {
            if (Stack.Length == 0)
                return null;
                
            return Stack[Stack.Length - 1];
        }
    }
    
    /// <summary>
    /// 方法描述符解析器
    /// </summary>
    public class MethodDescriptorParser
    {
        private readonly string _descriptor;
        
        public MethodDescriptorParser(string descriptor)
        {
            _descriptor = descriptor;
        }
        
        public List<string> GetParameterTypes()
        {
            var parameters = new List<string>();
            int index = 1; // 跳过开头的 '('
            
            while (_descriptor[index] != ')')
            {
                string type = ReadType(ref index);
                parameters.Add(type);
            }
            
            return parameters;
        }
        
        public string GetReturnType()
        {
            int index = _descriptor.IndexOf(')') + 1;
            return ReadType(ref index);
        }
        
        private string ReadType(ref int index)
        {
            char c = _descriptor[index++];
            
            switch (c)
            {
                case 'B': return "B";
                case 'C': return "C";
                case 'D': return "D";
                case 'F': return "F";
                case 'I': return "I";
                case 'J': return "J";
                case 'S': return "S";
                case 'Z': return "Z";
                case 'V': return "V";
                case 'L':
                    int start = index;
                    while (_descriptor[index] != ';') index++;
                    string className = _descriptor.Substring(start, index - start);
                    index++; // 跳过 ';'
                    return "L" + className + ";";
                case '[':
                    string elementType = ReadType(ref index);
                    return "[" + elementType;
                default:
                    throw new ArgumentException("Invalid descriptor: " + _descriptor);
            }
        }
    }
}

/// <summary>
/// VerificationTypeInfoStruct 的扩展方法
/// </summary>
public static class VerificationTypeInfoStructExtensions
{
    public static VerificationTypeInfoStruct Clone(this VerificationTypeInfoStruct info)
    {
        if (info == null) return null;
        
        return new VerificationTypeInfoStruct
        {
            TopVariableInfo = info.TopVariableInfo != null ? new TopVariableInfoStruct { Tag = info.TopVariableInfo.Tag } : null,
            IntegerVariableInfo = info.IntegerVariableInfo != null ? new IntegerVariableInfoStruct { Tag = info.IntegerVariableInfo.Tag } : null,
            FloatVariableInfo = info.FloatVariableInfo != null ? new FloatVariableInfoStruct { Tag = info.FloatVariableInfo.Tag } : null,
            LongVariableInfo = info.LongVariableInfo != null ? new LongVariableInfoStruct { Tag = info.LongVariableInfo.Tag } : null,
            DoubleVariableInfo = info.DoubleVariableInfo != null ? new DoubleVariableInfoStruct { Tag = info.DoubleVariableInfo.Tag } : null,
            NullVariableInfo = info.NullVariableInfo != null ? new NullVariableInfoStruct { Tag = info.NullVariableInfo.Tag } : null,
            UninitializedThisVariableInfo = info.UninitializedThisVariableInfo != null ? new UninitializedThisVariableInfoStruct { Tag = info.UninitializedThisVariableInfo.Tag } : null,
            ObjectVariableInfo = info.ObjectVariableInfo != null ? new ObjectVariableInfoStruct
            {
                Tag = info.ObjectVariableInfo.Tag,
                CPoolIndex = info.ObjectVariableInfo.CPoolIndex
            } : null,
            UninitializedVariableInfo = info.UninitializedVariableInfo != null ? new UninitializedVariableInfoStruct
            {
                Tag = info.UninitializedVariableInfo.Tag,
                Offset = info.UninitializedVariableInfo.Offset
            } : null
        };
    }
}
