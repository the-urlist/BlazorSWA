using System.Text.Json.Serialization;

public class Claim
{
  [JsonPropertyName("typ")]
  public string Type { get; set; }
  [JsonPropertyName("val")]
  public string Value { get; set; }
}