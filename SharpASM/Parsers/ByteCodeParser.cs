using System;
using System.Collections.Generic;
using System.IO;
using SharpASM.Models.Code;
using SharpASM.Utilities;

namespace SharpASM.Parsers
{
    public class ByteCodeParser
    {
        private static readonly HashSet<OperationCode> WideAllowedOpCodes = new HashSet<OperationCode>
        {
            OperationCode.ILOAD,
            OperationCode.LLOAD,
            OperationCode.FLOAD,
            OperationCode.DLOAD,
            OperationCode.ALOAD,
            OperationCode.ISTORE,
            OperationCode.LSTORE,
            OperationCode.FSTORE,
            OperationCode.DSTORE,
            OperationCode.ASTORE,
            OperationCode.RET,
            OperationCode.IINC
        };

        public static List<Code> Parse(byte[] data)
        {
            List<Code> result = new List<Code>();
            int offset = 0;

            while (offset < data.Length)
            {
                OperationCode? prefix = null;
                OperationCode opCode;

                byte currentByte = data[offset++];
                
                // 检查是否是 WIDE 前缀
                if (currentByte == (byte)OperationCode.WIDE)
                {
                    prefix = OperationCode.WIDE;
                    
                    if (offset >= data.Length)
                        throw new InvalidOperationException("Unexpected end of data after WIDE prefix");
                        
                    currentByte = data[offset++];
                }
                
                opCode = (OperationCode)currentByte;

                // 特殊处理 TABLESWITCH 和 LOOKUPSWITCH
                if (opCode == OperationCode.TABLESWITCH || opCode == OperationCode.LOOKUPSWITCH)
                {
                    Code switchCode = ParseSwitchInstruction(data, ref offset, opCode, prefix);
                    result.Add(switchCode);
                    continue;
                }

                // 获取操作数信息
                if (!OperationCodeMapping.TryGetOperandInfo(opCode, out int operandCount, out int[] operandSizes))
                {
                    throw new InvalidOperationException($"Unknown opcode: {opCode}");
                }

                // 如果是WIDE前缀，调整操作数大小
                if (prefix.HasValue && prefix.Value == OperationCode.WIDE)
                {
                    if (!WideAllowedOpCodes.Contains(opCode))
                    {
                        throw new InvalidOperationException($"Opcode {opCode} cannot be used with WIDE prefix");
                    }

                    if (opCode == OperationCode.IINC)
                    {
                        operandSizes = new[] { 2, 2 }; // IINC宽版本：两个2字节
                    }
                    else
                    {
                        // 对于其他可被WIDE修饰的指令，每个操作数变为2字节
                        for (int i = 0; i < operandSizes.Length; i++)
                        {
                            if (operandSizes[i] == 1)
                            {
                                operandSizes[i] = 2;
                            }
                        }
                    }
                }

                List<Operand> operands = new List<Operand>();
                for (int i = 0; i < operandCount; i++)
                {
                    int size = operandSizes[i];
                    byte[] operandData = ByteUtils.ReadBytes(data, ref offset, size);
                    operands.Add(new Operand { Data = operandData });
                }

                Code code;
                if (prefix.HasValue)
                    code = new Code(prefix.Value, opCode, operands);
                else
                    code = new Code(opCode, operands);

                result.Add(code);
            }

            return result;
        }

        public static byte[] Serialize(List<Code> codes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (Code code in codes)
                {
                    if (code.Prefix.HasValue)
                    {
                        stream.WriteByte((byte)code.Prefix.Value);
                    }
                    
                    stream.WriteByte((byte)code.OpCode);

                    // 处理操作数
                    foreach (var operand in code.Operands)
                    {
                        stream.Write(operand.Data, 0, operand.Data.Length);
                    }
                }
                return stream.ToArray();
            }
        }

        private static Code ParseSwitchInstruction(byte[] data, ref int offset, OperationCode opCode, OperationCode? prefix)
        {
            // 保存起始偏移量，用于计算填充字节
            int startOffset = offset - (prefix.HasValue ? 2 : 1);
            
            // 计算对齐填充
            int padding = (4 - (offset % 4)) % 4;
            if (padding > 0)
            {
                offset += padding;
            }

            // 读取 default 字节
            int defaultOffset = (int)ByteUtils.ReadUInt32(data, ref offset);
            
            if (opCode == OperationCode.TABLESWITCH)
            {
                // 读取 Low 和 High
                int low = (int)ByteUtils.ReadUInt32(data, ref offset);
                int high = (int)ByteUtils.ReadUInt32(data, ref offset);
                
                // 计算跳转偏移量数量
                int jumpOffsetCount = high - low + 1;
                
                // 读取所有跳转偏移量
                List<Operand> operands = new List<Operand>
                {
                    Operand.Int(defaultOffset),
                    Operand.Int(low),
                    Operand.Int(high)
                };
                
                for (int i = 0; i < jumpOffsetCount; i++)
                {
                    int jumpOffset = (int)ByteUtils.ReadUInt32(data, ref offset);
                    operands.Add(Operand.Int(jumpOffset));
                }
                
                if (prefix.HasValue)
                    return new Code(prefix.Value, opCode, operands);
                else
                    return new Code(opCode, operands);
            }
            else // LOOKUPSWITCH
            {
                // 读取 NPairs
                int npairs = (int)ByteUtils.ReadUInt32(data, ref offset);
                
                List<Operand> operands = new List<Operand>
                {
                    Operand.Int(defaultOffset),
                    Operand.Int(npairs)
                };
                
                // 读取所有 匹配值 - 偏移量 对
                for (int i = 0; i < npairs; i++)
                {
                    int match = (int)ByteUtils.ReadUInt32(data, ref offset);
                    int jumpOffset = (int)ByteUtils.ReadUInt32(data, ref offset);
                    
                    operands.Add(Operand.Int(match));
                    operands.Add(Operand.Int(jumpOffset));
                }
                
                if (prefix.HasValue)
                    return new Code(prefix.Value, opCode, operands);
                else
                    return new Code(opCode, operands);
            }
        }
    }
}