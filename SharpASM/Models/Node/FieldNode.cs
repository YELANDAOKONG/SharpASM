namespace SharpASM.Models.Node;

public class FieldNode : ClassNode
{
    public Field Field { get; set; }
    
    public override void Apply()
    {
        if (Parent != null && !Parent.Fields.Contains(Field))
        {
            Parent.Fields.Add(Field);
        }
    }
    
    public override void Revert()
    {
        if (Parent != null)
        {
            Parent.Fields.Remove(Field);
        }
    }
}