using SharpASM.Models.Struct;

namespace SharpASM.Models;

public class Attribute
{
    public string Name { get; set; } = string.Empty;
    public byte[] Info { get; set; } = Array.Empty<byte>();
    
    
    /// <summary>
    /// To AttributeInfoStruct without AttributeNameIndex and AttributeLength
    /// </summary>
    /// <returns>Struct</returns>
    public AttributeInfoStruct ToStructInfo()
    {
        AttributeInfoStruct infoStruct = new AttributeInfoStruct()
        {
            AttributeNameIndex = 0,
            AttributeLength = 0,
            Info = this.Info,
        };
        return infoStruct;
    }
    
    /// <summary>
    /// To AttributeInfoStruct with AttributeNameIndex and AttributeLength
    /// </summary>
    /// <returns>Struct</returns>
    public AttributeInfoStruct ToStructInfo(ushort nameIndex, uint length)
    {
        AttributeInfoStruct infoStruct = new AttributeInfoStruct()
        {
            AttributeNameIndex = nameIndex,
            AttributeLength = length,
            Info = this.Info,
        };
        return infoStruct;
    }
}