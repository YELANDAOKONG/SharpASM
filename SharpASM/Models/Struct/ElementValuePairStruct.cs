namespace SharpASM.Models.Struct;

public class ElementValuePairStruct
{
    public ushort ElementNameIndex { get; set; }
    public ElementValueStruct Value { get; set; } = new();
}