namespace SharpUnion.Shared;

[AttributeUsage(AttributeTargets.Class)]
public class SharpUnionAttribute : Attribute
{
    public bool Serializable { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class SharpUnionIgnoreAttribute : Attribute { }