namespace SharpASM.Models.Struct.Union;

public class SameFrameStruct
{
    /*
     * same_frame {
           u1 frame_type = SAME; /* 0-63 * /
       }
     */

    public byte FrameType { get; set; }
}