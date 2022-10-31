namespace ApiDiac.Domain
{
    using System.Text.Json.Serialization;

    public class DelegationResponse
    {
        [JsonPropertyName("delegation_token")] public string DelegationToken { get; set; }
    }
}