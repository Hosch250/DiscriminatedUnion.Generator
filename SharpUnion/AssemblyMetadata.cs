using System.Reflection;

namespace SharpUnion;

internal static class AssemblyMetadata
{
    internal static string AssemblyName => Assembly.GetAssembly(typeof(RecordSyntaxGenerator)).GetName().Name;
    internal static string AssemblyVersion => Assembly.GetAssembly(typeof(RecordSyntaxGenerator)).GetName().Version.ToString(3);
}
