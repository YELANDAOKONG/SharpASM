namespace SharpASM.Models.Struct.Union;

public class ChopFrameStruct
{
    /*
     * chop_frame {
           u1 frame_type = CHOP; /* 248-250 * /
           u2 offset_delta;
       }
     */
    
    public byte FrameType { get; set; }
    public ushort OffsetDelta { get; set; }
}