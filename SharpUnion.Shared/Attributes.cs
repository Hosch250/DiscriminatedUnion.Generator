namespace SharpUnion.Shared;

/// <summary>
/// Marks a record for the discriminated union generator
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SharpUnionAttribute : Attribute
{
    public bool Serializable { get; set; }
}

/// <summary>
/// Create a discriminated union with string syntax
/// </summary>
[AttributeUsage(AttributeTargets.Module)]
public class SharpUnionModuleAttribute(string definition) : Attribute
{
    public string Definition { get; } = definition;
    public bool Serializable { get; set; } = false;
    public Accessibility Accessibility { get; set; } = Accessibility.Public;
}

/// <summary>
/// Marks a child type to be ignored by the discriminated union generator
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SharpUnionIgnoreAttribute : Attribute { }

/// <summary>
/// Enum of valid accessibility modifiers
/// </summary>
public enum Accessibility
{
    /// <summary>
    /// Marks a discriminated union and its member types as public
    /// </summary>
    Public,
    /// <summary>
    /// Marks a discriminated union and its member types as internal
    /// </summary>
    Internal
}