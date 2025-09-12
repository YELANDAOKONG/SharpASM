namespace SharpASM.Models.Code;

public static class OperationCodeMapping
{
    private static readonly Dictionary<OperationCode, (int OperandCount, int[] OperandSizes)> Mapping = 
        new Dictionary<OperationCode, (int, int[])>
        {
            // Constants
            [OperationCode.NOP] = (0, Array.Empty<int>()),
            [OperationCode.ACONST_NULL] = (0, Array.Empty<int>()),
            [OperationCode.ICONST_M1] = (0, Array.Empty<int>()),
            [OperationCode.ICONST_0] = (0, Array.Empty<int>()),
            [OperationCode.ICONST_1] = (0, Array.Empty<int>()),
            [OperationCode.ICONST_2] = (0, Array.Empty<int>()),
            [OperationCode.ICONST_3] = (0, Array.Empty<int>()),
            [OperationCode.ICONST_4] = (0, Array.Empty<int>()),
            [OperationCode.ICONST_5] = (0, Array.Empty<int>()),
            [OperationCode.LCONST_0] = (0, Array.Empty<int>()),
            [OperationCode.LCONST_1] = (0, Array.Empty<int>()),
            [OperationCode.FCONST_0] = (0, Array.Empty<int>()),
            [OperationCode.FCONST_1] = (0, Array.Empty<int>()),
            [OperationCode.FCONST_2] = (0, Array.Empty<int>()),
            [OperationCode.DCONST_0] = (0, Array.Empty<int>()),
            [OperationCode.DCONST_1] = (0, Array.Empty<int>()),
            [OperationCode.BIPUSH] = (1, new[] { 1 }),        // byte
            [OperationCode.SIPUSH] = (1, new[] { 2 }),        // short
            [OperationCode.LDC] = (1, new[] { 1 }),           // index (1 byte)
            [OperationCode.LDC_W] = (1, new[] { 2 }),         // index (2 bytes)
            [OperationCode.LDC2_W] = (1, new[] { 2 }),        // index (2 bytes)

            // Loads
            [OperationCode.ILOAD] = (1, new[] { 1 }),         // index
            [OperationCode.LLOAD] = (1, new[] { 1 }),         // index
            [OperationCode.FLOAD] = (1, new[] { 1 }),         // index
            [OperationCode.DLOAD] = (1, new[] { 1 }),         // index
            [OperationCode.ALOAD] = (1, new[] { 1 }),         // index
            [OperationCode.ILOAD_0] = (0, Array.Empty<int>()),
            [OperationCode.ILOAD_1] = (0, Array.Empty<int>()),
            [OperationCode.ILOAD_2] = (0, Array.Empty<int>()),
            [OperationCode.ILOAD_3] = (0, Array.Empty<int>()),
            [OperationCode.LLOAD_0] = (0, Array.Empty<int>()),
            [OperationCode.LLOAD_1] = (0, Array.Empty<int>()),
            [OperationCode.LLOAD_2] = (0, Array.Empty<int>()),
            [OperationCode.LLOAD_3] = (0, Array.Empty<int>()),
            [OperationCode.FLOAD_0] = (0, Array.Empty<int>()),
            [OperationCode.FLOAD_1] = (0, Array.Empty<int>()),
            [OperationCode.FLOAD_2] = (0, Array.Empty<int>()),
            [OperationCode.FLOAD_3] = (0, Array.Empty<int>()),
            [OperationCode.DLOAD_0] = (0, Array.Empty<int>()),
            [OperationCode.DLOAD_1] = (0, Array.Empty<int>()),
            [OperationCode.DLOAD_2] = (0, Array.Empty<int>()),
            [OperationCode.DLOAD_3] = (0, Array.Empty<int>()),
            [OperationCode.ALOAD_0] = (0, Array.Empty<int>()),
            [OperationCode.ALOAD_1] = (0, Array.Empty<int>()),
            [OperationCode.ALOAD_2] = (0, Array.Empty<int>()),
            [OperationCode.ALOAD_3] = (0, Array.Empty<int>()),
            [OperationCode.IALOAD] = (0, Array.Empty<int>()),
            [OperationCode.LALOAD] = (0, Array.Empty<int>()),
            [OperationCode.FALOAD] = (0, Array.Empty<int>()),
            [OperationCode.DALOAD] = (0, Array.Empty<int>()),
            [OperationCode.AALOAD] = (0, Array.Empty<int>()),
            [OperationCode.BALOAD] = (0, Array.Empty<int>()),
            [OperationCode.CALOAD] = (0, Array.Empty<int>()),
            [OperationCode.SALOAD] = (0, Array.Empty<int>()),

            // Stores
            [OperationCode.ISTORE] = (1, new[] { 1 }),        // index
            [OperationCode.LSTORE] = (1, new[] { 1 }),        // index
            [OperationCode.FSTORE] = (1, new[] { 1 }),        // index
            [OperationCode.DSTORE] = (1, new[] { 1 }),        // index
            [OperationCode.ASTORE] = (1, new[] { 1 }),        // index
            [OperationCode.ISTORE_0] = (0, Array.Empty<int>()),
            [OperationCode.ISTORE_1] = (0, Array.Empty<int>()),
            [OperationCode.ISTORE_2] = (0, Array.Empty<int>()),
            [OperationCode.ISTORE_3] = (0, Array.Empty<int>()),
            [OperationCode.LSTORE_0] = (0, Array.Empty<int>()),
            [OperationCode.LSTORE_1] = (0, Array.Empty<int>()),
            [OperationCode.LSTORE_2] = (0, Array.Empty<int>()),
            [OperationCode.LSTORE_3] = (0, Array.Empty<int>()),
            [OperationCode.FSTORE_0] = (0, Array.Empty<int>()),
            [OperationCode.FSTORE_1] = (0, Array.Empty<int>()),
            [OperationCode.FSTORE_2] = (0, Array.Empty<int>()),
            [OperationCode.FSTORE_3] = (0, Array.Empty<int>()),
            [OperationCode.DSTORE_0] = (0, Array.Empty<int>()),
            [OperationCode.DSTORE_1] = (0, Array.Empty<int>()),
            [OperationCode.DSTORE_2] = (0, Array.Empty<int>()),
            [OperationCode.DSTORE_3] = (0, Array.Empty<int>()),
            [OperationCode.ASTORE_0] = (0, Array.Empty<int>()),
            [OperationCode.ASTORE_1] = (0, Array.Empty<int>()),
            [OperationCode.ASTORE_2] = (0, Array.Empty<int>()),
            [OperationCode.ASTORE_3] = (0, Array.Empty<int>()),
            [OperationCode.IASTORE] = (0, Array.Empty<int>()),
            [OperationCode.LASTORE] = (0, Array.Empty<int>()),
            [OperationCode.FASTORE] = (0, Array.Empty<int>()),
            [OperationCode.DASTORE] = (0, Array.Empty<int>()),
            [OperationCode.AASTORE] = (0, Array.Empty<int>()),
            [OperationCode.BASTORE] = (0, Array.Empty<int>()),
            [OperationCode.CASTORE] = (0, Array.Empty<int>()),
            [OperationCode.SASTORE] = (0, Array.Empty<int>()),

            // Stack
            [OperationCode.POP] = (0, Array.Empty<int>()),
            [OperationCode.POP2] = (0, Array.Empty<int>()),
            [OperationCode.DUP] = (0, Array.Empty<int>()),
            [OperationCode.DUP_X1] = (0, Array.Empty<int>()),
            [OperationCode.DUP_X2] = (0, Array.Empty<int>()),
            [OperationCode.DUP2] = (0, Array.Empty<int>()),
            [OperationCode.DUP2_X1] = (0, Array.Empty<int>()),
            [OperationCode.DUP2_X2] = (0, Array.Empty<int>()),
            [OperationCode.SWAP] = (0, Array.Empty<int>()),

            // Math
            [OperationCode.IADD] = (0, Array.Empty<int>()),
            [OperationCode.LADD] = (0, Array.Empty<int>()),
            [OperationCode.FADD] = (0, Array.Empty<int>()),
            [OperationCode.DADD] = (0, Array.Empty<int>()),
            [OperationCode.ISUB] = (0, Array.Empty<int>()),
            [OperationCode.LSUB] = (0, Array.Empty<int>()),
            [OperationCode.FSUB] = (0, Array.Empty<int>()),
            [OperationCode.DSUB] = (0, Array.Empty<int>()),
            [OperationCode.IMUL] = (0, Array.Empty<int>()),
            [OperationCode.LMUL] = (0, Array.Empty<int>()),
            [OperationCode.FMUL] = (0, Array.Empty<int>()),
            [OperationCode.DMUL] = (0, Array.Empty<int>()),
            [OperationCode.IDIV] = (0, Array.Empty<int>()),
            [OperationCode.LDIV] = (0, Array.Empty<int>()),
            [OperationCode.FDIV] = (0, Array.Empty<int>()),
            [OperationCode.DDIV] = (0, Array.Empty<int>()),
            [OperationCode.IREM] = (0, Array.Empty<int>()),
            [OperationCode.LREM] = (0, Array.Empty<int>()),
            [OperationCode.FREM] = (0, Array.Empty<int>()),
            [OperationCode.DREM] = (0, Array.Empty<int>()),
            [OperationCode.INEG] = (0, Array.Empty<int>()),
            [OperationCode.LNEG] = (0, Array.Empty<int>()),
            [OperationCode.FNEG] = (0, Array.Empty<int>()),
            [OperationCode.DNEG] = (0, Array.Empty<int>()),
            [OperationCode.ISHL] = (0, Array.Empty<int>()),
            [OperationCode.LSHL] = (0, Array.Empty<int>()),
            [OperationCode.ISHR] = (0, Array.Empty<int>()),
            [OperationCode.LSHR] = (0, Array.Empty<int>()),
            [OperationCode.IUSHR] = (0, Array.Empty<int>()),
            [OperationCode.LUSHR] = (0, Array.Empty<int>()),
            [OperationCode.IAND] = (0, Array.Empty<int>()),
            [OperationCode.LAND] = (0, Array.Empty<int>()),
            [OperationCode.IOR] = (0, Array.Empty<int>()),
            [OperationCode.LOR] = (0, Array.Empty<int>()),
            [OperationCode.IXOR] = (0, Array.Empty<int>()),
            [OperationCode.LXOR] = (0, Array.Empty<int>()),
            [OperationCode.IINC] = (2, new[] { 1, 1 }),       // index, const

            // Conversions
            [OperationCode.I2L] = (0, Array.Empty<int>()),
            [OperationCode.I2F] = (0, Array.Empty<int>()),
            [OperationCode.I2D] = (0, Array.Empty<int>()),
            [OperationCode.L2I] = (0, Array.Empty<int>()),
            [OperationCode.L2F] = (0, Array.Empty<int>()),
            [OperationCode.L2D] = (0, Array.Empty<int>()),
            [OperationCode.F2I] = (0, Array.Empty<int>()),
            [OperationCode.F2L] = (0, Array.Empty<int>()),
            [OperationCode.F2D] = (0, Array.Empty<int>()),
            [OperationCode.D2I] = (0, Array.Empty<int>()),
            [OperationCode.D2L] = (0, Array.Empty<int>()),
            [OperationCode.D2F] = (0, Array.Empty<int>()),
            [OperationCode.I2B] = (0, Array.Empty<int>()),
            [OperationCode.I2C] = (0, Array.Empty<int>()),
            [OperationCode.I2S] = (0, Array.Empty<int>()),

            // Comparisons
            [OperationCode.LCMP] = (0, Array.Empty<int>()),
            [OperationCode.FCMPL] = (0, Array.Empty<int>()),
            [OperationCode.FCMPG] = (0, Array.Empty<int>()),
            [OperationCode.DCMPL] = (0, Array.Empty<int>()),
            [OperationCode.DCMPG] = (0, Array.Empty<int>()),
            [OperationCode.IFEQ] = (1, new[] { 2 }),          // branchoffset
            [OperationCode.IFNE] = (1, new[] { 2 }),          // branchoffset
            [OperationCode.IFLT] = (1, new[] { 2 }),          // branchoffset
            [OperationCode.IFGE] = (1, new[] { 2 }),          // branchoffset
            [OperationCode.IFGT] = (1, new[] { 2 }),          // branchoffset
            [OperationCode.IFLE] = (1, new[] { 2 }),          // branchoffset
            [OperationCode.IF_ICMPEQ] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IF_ICMPNE] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IF_ICMPLT] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IF_ICMPGE] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IF_ICMPGT] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IF_ICMPLE] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IF_ACMPEQ] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IF_ACMPNE] = (1, new[] { 2 }),     // branchoffset
            [OperationCode.IFNULL] = (1, new[] { 2 }),        // branchoffset
            [OperationCode.IFNONNULL] = (1, new[] { 2 }),     // branchoffset

            // Control
            [OperationCode.GOTO] = (1, new[] { 2 }),          // branchoffset
            [OperationCode.JSR] = (1, new[] { 2 }),           // branchoffset
            [OperationCode.RET] = (1, new[] { 1 }),           // index
            [OperationCode.GOTO_W] = (1, new[] { 4 }),        // branchoffset (wide)
            [OperationCode.JSR_W] = (1, new[] { 4 }),         // branchoffset (wide)

            [OperationCode.IRETURN] = (0, Array.Empty<int>()),
            [OperationCode.LRETURN] = (0, Array.Empty<int>()),
            [OperationCode.FRETURN] = (0, Array.Empty<int>()),
            [OperationCode.DRETURN] = (0, Array.Empty<int>()),
            [OperationCode.ARETURN] = (0, Array.Empty<int>()),
            [OperationCode.RETURN] = (0, Array.Empty<int>()),

            // References
            [OperationCode.GETSTATIC] = (1, new[] { 2 }),     // index
            [OperationCode.PUTSTATIC] = (1, new[] { 2 }),     // index
            [OperationCode.GETFIELD] = (1, new[] { 2 }),      // index
            [OperationCode.PUTFIELD] = (1, new[] { 2 }),      // index
            [OperationCode.INVOKEVIRTUAL] = (1, new[] { 2 }), // index
            [OperationCode.INVOKESPECIAL] = (1, new[] { 2 }), // index
            [OperationCode.INVOKESTATIC] = (1, new[] { 2 }),  // index
            [OperationCode.INVOKEINTERFACE] = (2, new[] { 2, 1 }), // index, count
            [OperationCode.INVOKEDYNAMIC] = (1, new[] { 2 }), // index
            [OperationCode.NEW] = (1, new[] { 2 }),           // index
            [OperationCode.NEWARRAY] = (1, new[] { 1 }),      // atype
            [OperationCode.ANEWARRAY] = (1, new[] { 2 }),     // index
            [OperationCode.ARRAYLENGTH] = (0, Array.Empty<int>()),
            [OperationCode.ATHROW] = (0, Array.Empty<int>()),
            [OperationCode.CHECKCAST] = (1, new[] { 2 }),     // index
            [OperationCode.INSTANCEOF] = (1, new[] { 2 }),    // index
            [OperationCode.MONITORENTER] = (0, Array.Empty<int>()),
            [OperationCode.MONITOREXIT] = (0, Array.Empty<int>()),

            // Extended
            [OperationCode.WIDE] = (0, Array.Empty<int>()),   // Special handling required
            [OperationCode.MULTIANEWARRAY] = (2, new[] { 2, 1 }), // index, dimensions
            [OperationCode.TABLESWITCH] = (-1, Array.Empty<int>()), // Variable length
            [OperationCode.LOOKUPSWITCH] = (-1, Array.Empty<int>()) // Variable length
        };

    /// <summary>
    /// Gets operand information for the specified opcode
    /// </summary>
    /// <param name="opCode">The operation code to look up</param>
    /// <param name="operandCount">Number of operands for the opcode</param>
    /// <param name="operandSizes">Sizes of each operand in bytes</param>
    /// <returns>True if the opcode is found, false otherwise</returns>
    /// <remarks>
    /// Special cases:
    /// - TABLESWITCH and LOOKUPSWITCH have operandCount = -1 (variable length)
    /// - WIDE requires special handling as it modifies the next instruction
    /// </remarks>
    public static bool TryGetOperandInfo(OperationCode opCode, out int operandCount, out int[] operandSizes)
    {
        if (Mapping.TryGetValue(opCode, out var info))
        {
            operandCount = info.OperandCount;
            operandSizes = info.OperandSizes;
            return true;
        }
        
        operandCount = 0;
        operandSizes = Array.Empty<int>();
        return false;
    }
}
