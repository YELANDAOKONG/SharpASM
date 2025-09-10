namespace SharpASM.Models.Node;

public class MethodNode : ClassNode
{
    public Method Method { get; set; }
    
    public override void Apply()
    {
        if (Parent != null && !Parent.Methods.Contains(Method))
        {
            Parent.Methods.Add(Method);
        }
    }
    
    public override void Revert()
    {
        if (Parent != null)
        {
            Parent.Methods.Remove(Method);
        }
    }
}