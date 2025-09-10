namespace SharpASM.Models.Struct.Attribute;

public class ModulePackagesAttributeStruct
{
    /*
     * ModulePackages_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 package_count;
           u2 package_index[package_count];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort PackageCount { get; set; }
    public ushort[] PackageIndex { get; set; } = [];


}