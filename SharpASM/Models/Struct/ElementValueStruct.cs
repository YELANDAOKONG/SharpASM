namespace SharpASM.Models.Struct;

public class ElementValueStruct
{
    /*
     * element_value {
           u1 tag;
           union {
               u2 const_value_index;
       
               {   u2 type_name_index;
                   u2 const_name_index;
               } enum_const_value;
       
               u2 class_info_index;
       
               annotation annotation_value;
       
               {   u2            num_values;
                   element_value values[num_values];
               } array_value;
           } value;
       }
     */

    public class UnionStruct
    {
        public class EnumConstValueStruct
        {
            public ushort TypeNameIndex { get; set; }
            public ushort ConstNameIndex { get; set; }
        }

        public class ArrayValueStruct
        {
            public ushort NumValues { get; set; }
            public ElementValueStruct Values { get; set; } = new();
        }

        public ushort ConstValueIndex { get; set; }
        public EnumConstValueStruct EnumConstValue { get; set; } = new();
        public ushort ClassInfoIndex { get; set; }
        public AnnotationStruct AnnotationValue { get; set; } = new();
        public ArrayValueStruct ArrayValue { get; set; } = new();

    }

    public byte Tag { get; set; }
    public UnionStruct Value { get; set; } = new();
}