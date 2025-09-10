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
}