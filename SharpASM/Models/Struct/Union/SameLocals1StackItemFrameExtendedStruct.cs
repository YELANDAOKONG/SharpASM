using SharpASM.Utilities;

namespace SharpASM.Models.Struct.Union;

public class SameLocals1StackItemFrameExtendedStruct
{
    /*
     * same_locals_1_stack_item_frame_extended {
           u1 frame_type = SAME_LOCALS_1_STACK_ITEM_EXTENDED; /* 247 * /
           u2 offset_delta;
           verification_type_info stack[1];
       }
     */
    
    public byte FrameType { get; set; }
    public ushort OffsetDelta { get; set; }
    
    public VerificationTypeInfoStruct[] Stack { get; set; } = [];
    
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(FrameType);
            ByteUtils.WriteUInt16(OffsetDelta, stream);
            if (Stack.Length != 1)
            {
                throw new InvalidOperationException("Stack must have exactly one item");
            }
            var stackBytes = Stack[0].ToBytes();
            stream.Write(stackBytes, 0, stackBytes.Length);
            return stream.ToArray();
        }
    }
}