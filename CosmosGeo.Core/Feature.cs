// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using CosmosGeo.Core;
//
//    var feature = Feature.FromJson(jsonString);

namespace CosmosGeo.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Feature
    {
        [JsonProperty("locationName")]
        public string LocationName { get; set; }

        [JsonProperty("geometry")]
        public Microsoft.Azure.Documents.Spatial.Geometry Geometry { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("_rid")]
        public string Rid { get; set; }

        [JsonProperty("_self")]
        public string Self { get; set; }

        [JsonProperty("_etag")]
        public string Etag { get; set; }

        [JsonProperty("_attachments")]
        public string Attachments { get; set; }

        [JsonProperty("_ts")]
        public long Ts { get; set; }
    }

    public partial class Feature
    {
        public static Feature FromJson(string json) => JsonConvert.DeserializeObject<Feature>(json, CosmosGeo.Core.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Feature self) => JsonConvert.SerializeObject(self, CosmosGeo.Core.Converter.Settings);
    }

    internal class Converter
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
}
