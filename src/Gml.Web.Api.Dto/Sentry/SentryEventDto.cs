using System;
using Newtonsoft.Json;

namespace Gml.Web.Api.Dto.Sentry;

public class SentryEventDto
{
    [JsonProperty("sdk")]
    public Sdk Sdk { get; set; }

    [JsonProperty("event_id")]
    public string EventId { get; set; }

    [JsonProperty("trace")]
    public Trace Trace { get; set; }

    [JsonProperty("sent_at")]
    public DateTime SentAt { get; set; }
}

public class Sdk
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }
}

public class Trace
{
    [JsonProperty("trace_id")]
    public string TraceId { get; set; }

    [JsonProperty("public_key")]
    public string PublicKey { get; set; }

    [JsonProperty("release")]
    public string Release { get; set; }

    [JsonProperty("environment")]
    public string Environment { get; set; }
}
