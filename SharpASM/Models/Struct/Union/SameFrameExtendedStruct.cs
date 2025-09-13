using SharpASM.Utilities;

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
    
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(FrameType);
            ByteUtils.WriteUInt16(OffsetDelta, stream);
            return stream.ToArray();
        }
    }
}