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

    public TopVariableInfoStruct TopVariableInfo { get; set; } = new();
    public IntegerVariableInfoStruct IntegerVariableInfo { get; set; } = new();
    public FloatVariableInfoStruct FloatVariableInfo { get; set; } = new();
    public LongVariableInfoStruct LongVariableInfo { get; set; } = new();
    public DoubleVariableInfoStruct DoubleVariableInfo { get; set; } = new();
    public NullVariableInfoStruct NullVariableInfo { get; set; } = new();
    public UninitializedThisVariableInfoStruct UninitializedThisVariableInfo { get; set; } = new();
    public ObjectVariableInfoStruct ObjectVariableInfo { get; set; } = new();
    public UninitializedVariableInfoStruct UninitializedVariableInfo { get; set; } = new();
}