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
    private VerificationTypeInfoStruct[] Locals { get; set; } = [];
    public ushort NumberOfStackItems { get; set; }
    private VerificationTypeInfoStruct[] Stack { get; set; } = [];

}