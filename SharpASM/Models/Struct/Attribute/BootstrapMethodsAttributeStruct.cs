namespace SharpASM.Models.Struct.Attribute;

public class BootstrapMethodsAttributeStruct
{
    /*
     * BootstrapMethods_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 num_bootstrap_methods;
           {   u2 bootstrap_method_ref;
               u2 num_bootstrap_arguments;
               u2 bootstrap_arguments[num_bootstrap_arguments];
           } bootstrap_methods[num_bootstrap_methods];
       }
     */

    public class BootstrapMethodStruct
    {
        public ushort BootstrapMethodRef { get; set; }
        public ushort NumBootstrapArguments { get; set; }
        public ushort[] BootstrapArguments { get; set; } = [];
    }
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort NumBootstrapMethods { get; set; }
    public BootstrapMethodStruct[] BootstrapMethods { get; set; } = [];
    
}