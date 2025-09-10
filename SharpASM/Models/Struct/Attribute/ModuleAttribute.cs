namespace SharpASM.Models.Struct.Attribute;

public class ModuleAttribute
{
    /*
     * Module_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
       
           u2 module_name_index;
           u2 module_flags;
           u2 module_version_index;
       
           u2 requires_count;
           {   u2 requires_index;
               u2 requires_flags;
               u2 requires_version_index;
           } requires[requires_count];
       
           u2 exports_count;
           {   u2 exports_index;
               u2 exports_flags;
               u2 exports_to_count;
               u2 exports_to_index[exports_to_count];
           } exports[exports_count];
       
           u2 opens_count;
           {   u2 opens_index;
               u2 opens_flags;
               u2 opens_to_count;
               u2 opens_to_index[opens_to_count];
           } opens[opens_count];
       
           u2 uses_count;
           u2 uses_index[uses_count];
       
           u2 provides_count;
           {   u2 provides_index;
               u2 provides_with_count;
               u2 provides_with_index[provides_with_count];
           } provides[provides_count];
       }
     */
    
    public class RequireStruct {   
        public ushort RequiresIndex { get; set; }
        public ushort RequiresFlags { get; set; }
        public ushort RequiresVersionIndex { get; set; }
    } 
    
    public class ExportStruct { 
        public ushort ExportsIndex { get; set; }
        public ushort ExportsFlags { get; set; }
        public ushort ExportsToCount { get; set; }
        public ushort[] ExportsToIndex { get; set; } = [];
    }
    
    public class OpenStruct {   
        public ushort OpensIndex { get; set; }
        public ushort OpensFlags { get; set; }
        public ushort OpensToCount { get; set; }
        public ushort[] OpensToIndex{ get; set; } = [];
    } 
    
    public class ProvideStruct {
        public ushort ProvidesIndex { get; set; }
        public ushort ProvidesWithCount { get; set; }
        public ushort[] ProvidesWithIndex { get; set; } = [];
    } 
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
       
    public ushort ModuleNameIndex { get; set; }
    public ushort ModuleFlags { get; set; }
    public ushort ModuleVersionIndex { get; set; }
       
    public ushort RequiresCount { get; set; }
    public RequireStruct[] Requires { get; set; } = [];
       
    public ushort ExportsCount { get; set; }
    public ExportStruct[] Exports { get; set; } = []; 
       
    public ushort OpensCount { get; set; }
    public OpenStruct[] Opens { get; set; } = [];
       
    public ushort UsesCount { get; set; }
    public ushort[] UsesIndex { get; set; } = [];
       
    public ushort ProvidesCount { get; set; }
    public ProvideStruct[] Provides { get; set; }  = [];
}