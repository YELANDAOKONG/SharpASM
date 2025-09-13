namespace SharpASM.Models.Struct.Union;

public class SameLocals1StackItemFrameStruct
{
    /*
     * same_locals_1_stack_item_frame {
           u1 frame_type = SAME_LOCALS_1_STACK_ITEM; /* 64-127 * /
           verification_type_info stack[1];
       }
     */
    
    public byte FrameType { get; set; }
    public VerificationTypeInfoStruct[] Stack { get; set; } = [];
    
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(FrameType);
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