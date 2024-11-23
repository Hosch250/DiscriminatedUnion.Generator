namespace DiscriminatedUnion.Generator.Shared;

[AttributeUsage(AttributeTargets.Class)]
public class DiscriminatedUnionAttribute : Attribute
{
    public bool Serializable { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class DiscriminatedUnionIgnoreAttribute : Attribute { }