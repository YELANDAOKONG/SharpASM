namespace SharpASM.Models.Struct;

public class SourceFileAttributeStruct
{
    /*
     * SourceFile_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 sourcefile_index;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort SourceFileIndex { get; set; }
    
}