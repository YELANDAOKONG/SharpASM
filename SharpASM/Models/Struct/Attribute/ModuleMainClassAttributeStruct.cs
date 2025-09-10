namespace SharpASM.Models.Struct.Attribute;

public class ModuleMainClassAttributeStruct
{
    /*
     * ModuleMainClass_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 main_class_index;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public uint MainClassIndex { get; set; }
}