namespace SharpASM.Models.Struct.Attribute;

public class PermittedSubclassesAttributeStruct
{
    /*
     * PermittedSubclasses_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 number_of_classes;
           u2 classes[number_of_classes];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort NumberOfClasses { get; set; }
    public ushort[] Classes { get; set; } = [];
}