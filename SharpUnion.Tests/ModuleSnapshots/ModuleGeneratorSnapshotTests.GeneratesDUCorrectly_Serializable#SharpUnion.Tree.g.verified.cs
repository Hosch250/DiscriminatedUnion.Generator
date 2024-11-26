//HintName: SharpUnion.Tree.g.cs

namespace Program1
{
    [System.Text.Json.Serialization.JsonConverter(typeof(TreeConverter))]
    [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
    internal abstract partial record Tree
    {
        private Tree() { }

        internal sealed partial record Node(params Tree?[] trees) : Tree;
        internal bool IsNode => this is Node;

        internal sealed partial record Leaf(string Name) : Tree;
        internal bool IsLeaf => this is Leaf;

                [SharpUnion.Shared.SharpUnionIgnore]
                [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
                private sealed class TreeConverter : System.Text.Json.Serialization.JsonConverter<Tree>
                {
                    public override Tree? Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
                    {
                        var node = System.Text.Json.Nodes.JsonNode.Parse(ref reader);
                        foreach (var prop in typeof(Tree).GetProperties())
                        {
                            var value = node?[prop.Name];
                            if (value?.GetValueKind() == System.Text.Json.JsonValueKind.True)
                            {
                                var type = typeof(Tree).GetNestedType(prop.Name[2..]);
                                if (type is not null)
                                {
                                    return System.Text.Json.JsonSerializer.Deserialize(node, type) as Tree;
                                }
                            }
                        }

                        return null;
                    }

                    public override void Write(System.Text.Json.Utf8JsonWriter writer, Tree value, System.Text.Json.JsonSerializerOptions options)
                    {
                        writer.WriteStartObject();

                        foreach (var prop in value.GetType().GetProperties())
                        {
                            string propertyName = prop.Name;

                            var val = value.GetType().GetProperty(propertyName)?.GetValue(value);
                            if (val is not null)
                            {
                                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
                                System.Text.Json.JsonSerializer.Serialize(writer, val, options);
                            }
                            else
                            {
                                writer.WriteNull(propertyName);
                            }
                        }

                        writer.WriteEndObject();
                    }
                }
    }
}