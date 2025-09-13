using SharpASM.Utilities;

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
    
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(FrameType);
            ByteUtils.WriteUInt16(OffsetDelta, stream);
            foreach (var local in Locals)
            {
                var localBytes = local.ToBytes();
                stream.Write(localBytes, 0, localBytes.Length);
            }
            return stream.ToArray();
        }
    }
}