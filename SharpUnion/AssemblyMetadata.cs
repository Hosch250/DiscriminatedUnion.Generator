using System.Reflection;

namespace SharpUnion;

internal static class AssemblyMetadata
{
    internal static string AssemblyName => Assembly.GetAssembly(typeof(Generator)).GetName().Name;
    internal static string AssemblyVersion => Assembly.GetAssembly(typeof(Generator)).GetName().Version.ToString(3);
}
