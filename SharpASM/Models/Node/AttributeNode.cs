namespace SharpASM.Models.Node;

public class AttributeNode : ClassNode
{
    public Attribute Attribute { get; set; }
    public object? Target { get; set; } // Field, Method, Class...
    
    public override void Apply()
    {
        if (Parent != null && Target is Field field && !field.Attributes.Contains(Attribute))
        {
            field.Attributes.Add(Attribute);
        }
        else if (Parent != null && Target is Method method && !method.Attributes.Contains(Attribute))
        {
            method.Attributes.Add(Attribute);
        }
        else if (Parent != null && Target is Class cls && !cls.Attributes.Contains(Attribute))
        {
            cls.Attributes.Add(Attribute);
        }
    }
    
    public override void Revert()
    {
        if (Target is Field field)
        {
            field.Attributes.Remove(Attribute);
        }
        else if (Target is Method method)
        {
            method.Attributes.Remove(Attribute);
        }
        else if (Target is Class cls)
        {
            cls.Attributes.Remove(Attribute);
        }
    }
}