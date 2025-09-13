using SharpASM.Utilities;

namespace SharpASM.Models.Struct.Union;

public class VerificationTypeInfoStruct
{
    /*
     * union verification_type_info {
           Top_variable_info;
           Integer_variable_info;
           Float_variable_info;
           Long_variable_info;
           Double_variable_info;
           Null_variable_info;
           UninitializedThis_variable_info;
           Object_variable_info;
           Uninitialized_variable_info;
       }
     */

    public TopVariableInfoStruct? TopVariableInfo { get; set; } = null;
    public IntegerVariableInfoStruct? IntegerVariableInfo { get; set; } = null;
    public FloatVariableInfoStruct? FloatVariableInfo { get; set; } = null;
    public LongVariableInfoStruct? LongVariableInfo { get; set; } = null;
    public DoubleVariableInfoStruct? DoubleVariableInfo { get; set; } = null;
    public NullVariableInfoStruct? NullVariableInfo { get; set; } = null;
    public UninitializedThisVariableInfoStruct? UninitializedThisVariableInfo { get; set; } = null;
    public ObjectVariableInfoStruct? ObjectVariableInfo { get; set; } = null;
    public UninitializedVariableInfoStruct? UninitializedVariableInfo { get; set; } = null;
    
    public byte[] ToBytes()
    {
        if (TopVariableInfo != null)
        {
            return new byte[] { TopVariableInfo.Tag };
        }
        if (IntegerVariableInfo != null)
        {
            return new byte[] { IntegerVariableInfo.Tag };
        }
        if (FloatVariableInfo != null)
        {
            return new byte[] { FloatVariableInfo.Tag };
        }
        if (LongVariableInfo != null)
        {
            return new byte[] { LongVariableInfo.Tag };
        }
        if (DoubleVariableInfo != null)
        {
            return new byte[] { DoubleVariableInfo.Tag };
        }
        if (NullVariableInfo != null)
        {
            return new byte[] { NullVariableInfo.Tag };
        }
        if (UninitializedThisVariableInfo != null)
        {
            return new byte[] { UninitializedThisVariableInfo.Tag };
        }
        if (ObjectVariableInfo != null)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(ObjectVariableInfo.Tag);
                ByteUtils.WriteUInt16(ObjectVariableInfo.CPoolIndex, stream);
                return stream.ToArray();
            }
        }
        if (UninitializedVariableInfo != null)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(UninitializedVariableInfo.Tag);
                ByteUtils.WriteUInt16(UninitializedVariableInfo.Offset, stream);
                return stream.ToArray();
            }
        }
        throw new InvalidOperationException("No verification type info set");
    }
}