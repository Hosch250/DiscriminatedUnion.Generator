//HintName: DiscriminatedUnion.Shape.g.cs

namespace Project1
{
    [System.Text.Json.Serialization.JsonConverter(typeof(ShapeConverter))]
    abstract partial record Shape
    {
        private Shape() { }

        internal sealed partial record Circle : Shape;
        internal bool IsCircle => this is Circle;

        internal sealed partial record EquilateralTriangle : Shape;
        internal bool IsEquilateralTriangle => this is EquilateralTriangle;

        internal sealed partial record Square : Shape;
        internal bool IsSquare => this is Square;

        internal sealed partial record Rectangle : Shape;
        internal bool IsRectangle => this is Rectangle;

        [DiscriminatedUnion.Generator.Shared.DiscriminatedUnionIgnore]
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
                System.Text.Json.JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}