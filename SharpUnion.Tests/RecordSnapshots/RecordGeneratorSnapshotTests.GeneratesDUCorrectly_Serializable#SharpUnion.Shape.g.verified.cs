//HintName: SharpUnion.Shape.g.cs

namespace Project1
{
    [System.Text.Json.Serialization.JsonConverter(typeof(ShapeConverter))]
    abstract partial record Shape
    {
        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        private Shape() { }

        internal sealed partial record Circle : Shape;

        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        internal bool IsCircle => this is Circle;

        internal sealed partial record EquilateralTriangle : Shape;

        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        internal bool IsEquilateralTriangle => this is EquilateralTriangle;

        internal sealed partial record Square : Shape;

        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        internal bool IsSquare => this is Square;

        internal sealed partial record Rectangle : Shape;

        [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
        internal bool IsRectangle => this is Rectangle;

                [SharpUnion.Shared.SharpUnionIgnore]
                [System.CodeDom.Compiler.GeneratedCode("SharpUnion", "2.0.0")]
                private sealed class ShapeConverter : System.Text.Json.Serialization.JsonConverter<Shape>
                {
                    public override Shape? Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
                    {
                        var node = System.Text.Json.Nodes.JsonNode.Parse(ref reader);
                        foreach (var prop in typeof(Shape).GetProperties())
                        {
                            var value = node?[prop.Name];
                            if (value?.GetValueKind() == System.Text.Json.JsonValueKind.True)
                            {
                                var type = typeof(Shape).GetNestedType(prop.Name[2..]);
                                if (type is not null)
                                {
                                    return System.Text.Json.JsonSerializer.Deserialize(node, type) as Shape;
                                }
                            }
                        }

                        return null;
                    }

                    public override void Write(System.Text.Json.Utf8JsonWriter writer, Shape value, System.Text.Json.JsonSerializerOptions options)
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