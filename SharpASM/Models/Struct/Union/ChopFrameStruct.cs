using SharpASM.Utilities;

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