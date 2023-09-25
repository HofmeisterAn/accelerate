namespace Accelerate.Verbs;

public sealed class ArgCollection : ReadOnlyCollection<string>
{
    public ArgCollection(params string[] args) : base(new List<string>(args))
    {
    }
}