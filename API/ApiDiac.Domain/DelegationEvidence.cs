namespace ApiDiac.Domain
{
    using System.Text.Json.Serialization;
    using Poort8.Ishare.Core.Models;

    public class DelegationRequestModel
    {
        [JsonPropertyName("delegationRequest")]
        public RequestDelegationEvidence DelegationRequest { get; set; }

        public class RequestDelegationEvidence
        {
            [JsonPropertyName("policyIssuer")] public string PolicyIssuer { get; set; }

            [JsonPropertyName("target")] public DelegationEvidence.TargetObject Target { get; set; }

            [JsonPropertyName("policySets")] public List<RequestPolicySet> PolicySets { get; set; }
        }

        public class RequestPolicySet
        {
            [JsonPropertyName("policies")] public List<DelegationEvidence.Policy>? Policies { get; set; }
        }
    }
}