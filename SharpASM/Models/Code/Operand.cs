namespace SharpASM.Models.Code;

public class Operand
{
    public byte[] Data { get; set; } = [];
    public static Operand Empty => new Operand { Data = Array.Empty<byte>() };
    
    public static Operand Byte(byte value) 
        => new Operand { Data = new[] { value } };
    
    public static Operand Short(short value)
        => new Operand { Data = new[] { (byte)(value >> 8), (byte)value } };
    
    public static Operand Int(int value)
        => new Operand { Data = new[] 
        {
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
        }};
    
    public static Operand Index(byte index) => Byte(index);
    public static Operand WideIndex(ushort index) => Short((short)index);
    
    public static Operand StringRef(ushort stringIndex) => Short((short)stringIndex);
    public static Operand MethodRef(ushort methodIndex) => Short((short)methodIndex);
    public static Operand FieldRef(ushort fieldIndex) => Short((short)fieldIndex);
    
    public static Operand BranchOffset(short offset) => Short(offset);
    public static Operand WideBranchOffset(int offset) => Int(offset);
    
    public static Operand DynamicCallSite(ushort bootstrapIndex, ushort nameAndTypeIndex)
        => new Operand { Data = new[] 
        {
            (byte)(bootstrapIndex >> 8), (byte)bootstrapIndex,
            (byte)(nameAndTypeIndex >> 8), (byte)nameAndTypeIndex
        }};
}