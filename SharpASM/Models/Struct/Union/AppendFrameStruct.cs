namespace SharpASM.Models.Struct.Union;

public class AppendFrameStruct
{
    /*
     * append_frame {
           u1 frame_type = APPEND; /* 252-254 * /
           u2 offset_delta;
           verification_type_info locals[frame_type - 251];
       }
     */
    
    public byte FrameType { get; set; }
    public ushort OffsetDelta { get; set; }
    public VerificationTypeInfoStruct[] Locals { get; set; } = [];
}