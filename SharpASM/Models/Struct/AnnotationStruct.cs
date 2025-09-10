namespace SharpASM.Models.Struct;

public class AnnotationStruct
{
    /*
     * annotation {
           u2 type_index;
           u2 num_element_value_pairs;
           {   u2            element_name_index;
               element_value value;
           } element_value_pairs[num_element_value_pairs];
       }       
     */
    
    public ushort TypeIndex { get; set; }
    public ushort NumElementValuePairs { get; set; }
    public ElementValuePairStruct[] ElementValuePairs { get; set; } = [];
}