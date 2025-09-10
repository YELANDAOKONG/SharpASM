namespace SharpASM.Models.Node;

public abstract class ClassNode
{
    public Class? Parent { get; set; }
    public abstract void Apply();
    public abstract void Revert();
}