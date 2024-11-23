//HintName: DiscriminatedUnion.Shape.g.cs

namespace Project1
{
    [System.Text.Json.JsonConverter(typeof(ShapeConverter))]
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
        private sealed class ShapeConverter : JsonConverter<Shape>
        {
            public override Shape? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var node = JsonNode.Parse(ref reader);
                foreach (var prop in typeof(Shape).GetProperties())
                {
                    var value = node?[prop.Name];
                    if (value?.GetValueKind() == JsonValueKind.True)
                    {
                        var type = typeof(Shape).GetNestedType(prop.Name[2..]);
                        return JsonSerializer.Deserialize(node, type!) as Shape;
                    }
                }

                return null;
            }

            public override void Write(Utf8JsonWriter writer, Shape value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}