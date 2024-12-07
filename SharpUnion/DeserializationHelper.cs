namespace SharpUnion;

internal static class DeserializationHelper
{
    internal static string GetJsonConverter(string typeName) =>
        $@"

                [SharpUnion.Shared.SharpUnionIgnore]
                [System.CodeDom.Compiler.GeneratedCode(""{AssemblyMetadata.AssemblyName}"", ""{AssemblyMetadata.AssemblyVersion}"")]
                private sealed class {typeName}Converter : System.Text.Json.Serialization.JsonConverter<{typeName}>
                {{
                    public override {typeName}? Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
                    {{
                        var node = System.Text.Json.Nodes.JsonNode.Parse(ref reader);
                        foreach (var prop in typeof({typeName}).GetProperties())
                        {{
                            var value = node?[prop.Name];
                            if (value?.GetValueKind() == System.Text.Json.JsonValueKind.True)
                            {{
                                var type = typeof({typeName}).GetNestedType(prop.Name[2..]);
                                if (type is not null)
                                {{
                                    return System.Text.Json.JsonSerializer.Deserialize(node, type) as {typeName};
                                }}
                            }}
                        }}

                        return null;
                    }}

                    public override void Write(System.Text.Json.Utf8JsonWriter writer, {typeName} value, System.Text.Json.JsonSerializerOptions options)
                    {{
                        writer.WriteStartObject();

                        foreach (var prop in value.GetType().GetProperties())
                        {{
                            string propertyName = prop.Name;

                            var val = value.GetType().GetProperty(propertyName)?.GetValue(value);
                            if (val is not null)
                            {{
                                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
                                System.Text.Json.JsonSerializer.Serialize(writer, val, options);
                            }}
                            else
                            {{
                                writer.WriteNull(propertyName);
                            }}
                        }}

                        writer.WriteEndObject();
                    }}
                }}";
}