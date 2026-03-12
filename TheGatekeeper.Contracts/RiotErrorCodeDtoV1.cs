using System.Text.Json.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class RiotErrorCodeDtoV1
{
    [JsonPropertyName("status")]
    public RiotInnerErrorCode Status { get; set; }
}

[ExportTsInterface]
public class RiotInnerErrorCode
{
    public double status_code { get; set; }
    public string message { get; set; }
}
