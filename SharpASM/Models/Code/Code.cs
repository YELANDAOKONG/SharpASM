namespace SharpASM.Models.Code;

public class Code
{
    public OperationCode? Prefix { get; set; } = null;
    public OperationCode OpCode { get; set; }
    public List<Operand> Operands { get; set; } = new List<Operand>();
    
    public Code(OperationCode opCode, IEnumerable<Operand>? operands = null)
    {
        OpCode = opCode;
        if (operands != null) Operands.AddRange(operands);
    }
    
    public Code(OperationCode prefix, OperationCode opCode, IEnumerable<Operand>? operands = null)
        : this(opCode, operands)
    {
        Prefix = prefix;
    }

    #region Functions (Code)

    public static Code ILoad(ushort index)
        {
            if (index <= byte.MaxValue)
                return new Code(OperationCode.ILOAD, new[] { Operand.Index((byte)index) });
            
            return new Code(OperationCode.WIDE, OperationCode.ILOAD, new[] { Operand.WideIndex(index) });
        }
        
        public static Code IConst(int value)
        {
            return value switch
            {
                -1 => new Code(OperationCode.ICONST_M1),
                0 => new Code(OperationCode.ICONST_0),
                1 => new Code(OperationCode.ICONST_1),
                2 => new Code(OperationCode.ICONST_2),
                3 => new Code(OperationCode.ICONST_3),
                4 => new Code(OperationCode.ICONST_4),
                5 => new Code(OperationCode.ICONST_5),
                _ when value >= sbyte.MinValue && value <= sbyte.MaxValue 
                    => new Code(OperationCode.BIPUSH, new[] { Operand.Byte((byte)value) }),
                _ when value >= short.MinValue && value <= short.MaxValue 
                    => new Code(OperationCode.SIPUSH, new[] { Operand.Short((short)value) }),
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };
        }
        
        public static Code InvokeVirtual(ushort methodRefIndex)
            => new Code(OperationCode.INVOKEVIRTUAL, new[] { Operand.MethodRef(methodRefIndex) });
        
        public static Code GetStatic(ushort fieldRefIndex)
            => new Code(OperationCode.GETSTATIC, new[] { Operand.FieldRef(fieldRefIndex) });
        
        public static Code IfEq(short branchOffset)
            => new Code(OperationCode.IFEQ, new[] { Operand.BranchOffset(branchOffset) });
        
        public static Code New(ushort classRefIndex)
            => new Code(OperationCode.NEW, new[] { Operand.WideIndex(classRefIndex) });
        
        public static Code IInc(ushort index, short increment)
        {
            if (index <= byte.MaxValue && increment >= sbyte.MinValue && increment <= sbyte.MaxValue)
            {
                return new Code(OperationCode.IINC, new[] 
                {
                    Operand.Index((byte)index),
                    Operand.Byte((byte)increment)
                });
            }
            
            return new Code(OperationCode.WIDE, OperationCode.IINC, new[]
            {
                Operand.WideIndex(index),
                Operand.Short(increment)
            });
        }

    #endregion
    
    
}
