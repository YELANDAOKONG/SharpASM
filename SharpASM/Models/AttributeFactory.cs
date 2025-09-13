using SharpASM.Models.Attributes;

namespace SharpASM.Models;

public class AttributeFactory
{
    public static AttributeBase CreateAttribute(string name, byte[] data)
    {
        switch (name)
        {
            case "Code":
                var codeAttr = new CodeAttribute { Name = name };
                codeAttr.FromBytes(data);
                return codeAttr;
            default:
                return new UnknownAttribute { Name = name, Data = data };
        }
    }
}