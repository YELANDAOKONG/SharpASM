namespace SharpASM.Models.Struct.Union;

public class TypePathStruct
{
    /*
     * type_path {
           u1 path_length;
           {   u1 type_path_kind;
               u1 type_argument_index;
           } path[path_length];
       }
     */
    
    public class TypePathEntryStruct
    {
        public byte TypePathKind { get; set; }
        public byte TypeArgumentIndex { get; set; }
    }
    
    public byte PathLength { get; set; }
    public TypePathEntryStruct[] Path { get; set; } = [];
}