using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Configuration
{
    public partial class Scene
    {
        [JsonProperty("time")]
        public DateTimeOffset[] Time { get; set; }

        [JsonProperty("weather")]
        public string[] Weather { get; set; }

        [JsonProperty("brands")]
        public string[] Brands { get; set; }

        [JsonProperty("colors")]
        public string[] Colors { get; set; }

        [JsonProperty("position")]
        public Position[] Position { get; set; }

        [JsonProperty("camera")]
        public Camera Camera { get; set; }
    }

    public partial class Camera
    {
        [JsonProperty("pitch")]
        public long[] Pitch { get; set; }

        [JsonProperty("rotation")]
        public long[] Rotation { get; set; }

        [JsonProperty("distance")]
        public long[] Distance { get; set; }
    }

    public partial class Position
    {
        [JsonProperty("enabled")]
        [JsonConverter(typeof(ParseStringConverter))]
        public bool Enabled { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public Ation Location { get; set; }

        [JsonProperty("rotation")]
        public Ation Rotation { get; set; }
    }

    public partial class Ation
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }

    public partial class Scene
    {
        public static Scene FromJson(string json) => JsonConvert.DeserializeObject<Scene>(json, Configuration.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Scene self) => JsonConvert.SerializeObject(self, Configuration.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(bool) || t == typeof(bool?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            bool b;
            if (Boolean.TryParse(value, out b))
            {
                return b;
            }
            throw new Exception("Cannot unmarshal type bool");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (bool)untypedValue;
            var boolString = value ? "true" : "false";
            serializer.Serialize(writer, boolString);
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

}
