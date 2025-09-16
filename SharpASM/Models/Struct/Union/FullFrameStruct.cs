using SharpASM.Utilities;

namespace SharpASM.Models.Struct.Union;

public class FullFrameStruct
{
    /*
     * full_frame {
           u1 frame_type = FULL_FRAME; /* 255 * /
           u2 offset_delta;
           u2 number_of_locals;
           verification_type_info locals[number_of_locals];
           u2 number_of_stack_items;
           verification_type_info stack[number_of_stack_items];
       }
     */
    
    public byte FrameType { get; set; }
    public ushort OffsetDelta { get; set; }
    public ushort NumberOfLocals { get; set; }
    public VerificationTypeInfoStruct[] Locals { get; set; } = [];
    public ushort NumberOfStackItems { get; set; }
    public VerificationTypeInfoStruct[] Stack { get; set; } = [];
    
    // public byte[] ToBytes()
    // {
    //     using (var stream = new MemoryStream())
    //     {
    //         stream.WriteByte(FrameType);
    //         ByteUtils.WriteUInt16(OffsetDelta, stream);
    //         ByteUtils.WriteUInt16(NumberOfLocals, stream);
    //         for (int i = 0; i < NumberOfLocals; i++)
    //         {
    //             var localBytes = Locals[i].ToBytes();
    //             stream.Write(localBytes, 0, localBytes.Length);
    //         }
    //         ByteUtils.WriteUInt16(NumberOfStackItems, stream);
    //         for (int i = 0; i < NumberOfStackItems; i++)
    //         {
    //             var stackBytes = Stack[i].ToBytes();
    //             stream.Write(stackBytes, 0, stackBytes.Length);
    //         }
    //         return stream.ToArray();
    //     }
    // }
    
    public byte[] ToBytes()
    {
        // Validate array sizes
        if (Locals.Length != NumberOfLocals)
        {
            throw new InvalidOperationException($"Number of locals ({Locals.Length}) does not match NumberOfLocals ({NumberOfLocals})");
        }

        if (Stack.Length != NumberOfStackItems)
        {
            throw new InvalidOperationException($"Number of stack items ({Stack.Length}) does not match NumberOfStackItems ({NumberOfStackItems})");
        }

        using (var stream = new MemoryStream())
        {
            stream.WriteByte(FrameType);
            ByteUtils.WriteUInt16(OffsetDelta, stream);
            ByteUtils.WriteUInt16(NumberOfLocals, stream);
            for (int i = 0; i < NumberOfLocals; i++)
            {
                var localBytes = Locals[i].ToBytes();
                stream.Write(localBytes, 0, localBytes.Length);
            }
            ByteUtils.WriteUInt16(NumberOfStackItems, stream);
            for (int i = 0; i < NumberOfStackItems; i++)
            {
                var stackBytes = Stack[i].ToBytes();
                stream.Write(stackBytes, 0, stackBytes.Length);
            }
            return stream.ToArray();
        }
    }

}