namespace SharpASM.Models.Struct.Union;

public class SameFrameExtendedStruct
{
    /*
     * same_frame_extended {
           u1 frame_type = SAME_FRAME_EXTENDED; /* 251 * /
           u2 offset_delta;
       }
     */
    
    public byte FrameType { get; set; }
    public ushort OffsetDelta { get; set; }
}