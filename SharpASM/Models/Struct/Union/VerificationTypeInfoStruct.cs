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
}