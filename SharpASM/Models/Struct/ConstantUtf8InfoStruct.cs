using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantUtf8InfoStruct : IConstantStruct
{
    /*
     * CONSTANT_Utf8_info {
           u1 tag;
           u2 length;
           u1 bytes[length];
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Utf8;
    public ushort Length { get; set; }
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    
    public static ConstantUtf8InfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantUtf8InfoStruct();
        info.Tag = tag;
        info.Length = ByteUtils.ReadUInt16(data, ref offset);
        info.Bytes = ByteUtils.ReadBytes(data, ref offset, info.Length);
        return info;
    }

    public static ConstantUtf8InfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantUtf8InfoStruct();
        info.Tag = data[offset++];
        info.Length = ByteUtils.ReadUInt16(data, ref offset);
        info.Bytes = ByteUtils.ReadBytes(data, ref offset, info.Length);
        return info;
    }
    
    public byte GetTag() => Tag;
        
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(Tag);
            ByteUtils.WriteUInt16(Length, stream);
            stream.Write(Bytes, 0, Bytes.Length);
            return stream.ToArray();
        }
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt16(Length, stream);
        stream.Write(Bytes, 0, Bytes.Length);
        return stream.ToArray();
    }
    
    public static ConstantUtf8InfoStruct FromString(string text)
    {
        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return new ConstantUtf8InfoStruct
        {
            Length = (ushort)utf8Bytes.Length,
            Bytes = utf8Bytes
        };
    }
    
    public override string ToString()
    {
        return System.Text.Encoding.UTF8.GetString(Bytes);
    }
    
    public ConstantPoolInfoStruct ToStructInfo()
    {
        ConstantPoolInfoStruct result = new ConstantPoolInfoStruct()
        {
            Tag = Tag,
            Info = ToBytesWithoutTag()
        };
        return result;
    }
}