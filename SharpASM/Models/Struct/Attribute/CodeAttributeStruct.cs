using SharpASM.Models.Struct.Interfaces;
using SharpASM.Parsers;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct.Attribute;

public class CodeAttributeStruct : IAttributeStruct
{
    /*
     * Code_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 max_stack;
           u2 max_locals;
           u4 code_length;
           u1 code[code_length];
           u2 exception_table_length;
           {   u2 start_pc;
               u2 end_pc;
               u2 handler_pc;
               u2 catch_type;
           } exception_table[exception_table_length];
           u2 attributes_count;
           attribute_info attributes[attributes_count];
       }
     */
    
    public class ExceptionTableStruct
    {
        public ushort StartPc { get; set; }
        public ushort EndPc { get; set; }
        public ushort HandlerPc { get; set; }
        public ushort CatchType { get; set; }
    }
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort MaxStack { get; set; }
    public ushort MaxLocals { get; set; }
    public uint CodeLength { get; set; }
    public byte[] Code { get; set; } = [];
    public ushort ExceptionTableLength { get; set; }
    public ExceptionTableStruct[] ExceptionTable { get; set; } = [];
    public ushort AttributesCount { get; set; }
    public AttributeInfoStruct[] Attributes { get; set; } = [];

    public void SetCode(List<Code.Code> codes)
    {
        Code = ByteCodeParser.Serialize(codes);
        CodeLength = (uint) Code.Length;
    }

    public List<Code.Code> GetCode()
    {
        return ByteCodeParser.Parse(Code);
    }
    
    public byte[] ToBytes()
    {
        return ToStructInfo().ToBytes();
    }

    public byte[] ToBytesWithoutIndexAndLength()
    {
        CodeLength = (uint)Code.Length;
        ExceptionTableLength = (ushort)ExceptionTable.Length;
        AttributesCount = (ushort)Attributes.Length;
        using (var stream = new MemoryStream())
        {
            ByteUtils.WriteUInt16(MaxStack, stream);
            ByteUtils.WriteUInt16(MaxLocals, stream);
            ByteUtils.WriteUInt32(CodeLength, stream);
            stream.Write(Code, 0, Code.Length);
            
            ByteUtils.WriteUInt16(ExceptionTableLength, stream);
            foreach (var exception in ExceptionTable)
            {
                ByteUtils.WriteUInt16(exception.StartPc, stream);
                ByteUtils.WriteUInt16(exception.EndPc, stream);
                ByteUtils.WriteUInt16(exception.HandlerPc, stream);
                ByteUtils.WriteUInt16(exception.CatchType, stream);
            }
            
            ByteUtils.WriteUInt16(AttributesCount, stream);
            foreach (var attribute in Attributes)
            {
                var attrBytes = attribute.ToBytes();
                stream.Write(attrBytes, 0, attrBytes.Length);
            }
            return stream.ToArray();
        }
    }

    public AttributeInfoStruct ToStructInfo()
    {
        var infoBytes = ToBytesWithoutIndexAndLength();
        return new AttributeInfoStruct
        {
            AttributeNameIndex = AttributeNameIndex,
            AttributeLength = (uint)infoBytes.Length,
            Info = infoBytes
        };
    }

    public static CodeAttributeStruct FromStructInfo(AttributeInfoStruct info)
    {
        var codeAttr = new CodeAttributeStruct
        {
            AttributeNameIndex = info.AttributeNameIndex,
            AttributeLength = info.AttributeLength
        };
    
        int offset = 0;
        byte[] bytes = info.Info;

        // Read max_stack (2 bytes)
        codeAttr.MaxStack = ByteUtils.ReadUInt16(bytes, ref offset);
    
        // Read max_locals (2 bytes)
        codeAttr.MaxLocals = ByteUtils.ReadUInt16(bytes, ref offset);
    
        // Read code_length (4 bytes)
        codeAttr.CodeLength = ByteUtils.ReadUInt32(bytes, ref offset);
    
        // Read code[code_length]
        codeAttr.Code = ByteUtils.ReadBytes(bytes, ref offset, (int)codeAttr.CodeLength);
    
        // Read exception_table_length (2 bytes)
        codeAttr.ExceptionTableLength = ByteUtils.ReadUInt16(bytes, ref offset);
    
        // Read exception_table[exception_table_length]
        codeAttr.ExceptionTable = new ExceptionTableStruct[codeAttr.ExceptionTableLength];
        for (int i = 0; i < codeAttr.ExceptionTableLength; i++)
        {
            codeAttr.ExceptionTable[i] = new ExceptionTableStruct
            {
                StartPc = ByteUtils.ReadUInt16(bytes, ref offset),
                EndPc = ByteUtils.ReadUInt16(bytes, ref offset),
                HandlerPc = ByteUtils.ReadUInt16(bytes, ref offset),
                CatchType = ByteUtils.ReadUInt16(bytes, ref offset)
            };
        }
    
        // Read attributes_count (2 bytes)
        codeAttr.AttributesCount = ByteUtils.ReadUInt16(bytes, ref offset);
    
        // Read attributes[attributes_count]
        codeAttr.Attributes = new AttributeInfoStruct[codeAttr.AttributesCount];
        for (int i = 0; i < codeAttr.AttributesCount; i++)
        {
            codeAttr.Attributes[i] = AttributeInfoStruct.FromBytes(bytes, ref offset);
        }

        return codeAttr;
    }
    
    
}