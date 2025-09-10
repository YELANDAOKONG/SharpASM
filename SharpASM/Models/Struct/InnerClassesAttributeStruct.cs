namespace SharpASM.Models.Struct;

public class InnerClassesAttributeStruct
{
    /*
     * InnerClasses_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 number_of_classes;
           {   u2 inner_class_info_index;
               u2 outer_class_info_index;
               u2 inner_name_index;
               u2 inner_class_access_flags;
           } classes[number_of_classes];
       }
     */
    
    public class ClassesStruct
    {
        public ushort InnerClassInfoIndex { get; set; }
        public ushort OuterClassInfoIndex { get; set; }
        public ushort InnerNameIndex { get; set; }
        public ushort InnerClassAccessFlags { get; set; }
    }
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort NumberOfClasses { get; set; }
    public ClassesStruct[] Classes { get; set; } = [];
}