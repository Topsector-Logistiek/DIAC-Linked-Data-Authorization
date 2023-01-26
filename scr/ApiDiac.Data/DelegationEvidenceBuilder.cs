namespace ApiDiac.Data
{
    using ApiDiac.Domain;
    using Poort8.Ishare.Core.Models;

    public class DelegationEvidenceBuilder
    {
        public static DelegationRequestModel GenerateBasicDelegationRequest(
            string policyIssuer,
            string accessSubject,
            string type,
            List<string> identifiers,
            List<string> actions,
            List<string> serviceProvider,
            List<string>? attributes = null)
        {
            var requestmodel = new DelegationRequestModel();
            requestmodel.DelegationRequest = new DelegationRequestModel.RequestDelegationEvidence();
            requestmodel.DelegationRequest.PolicyIssuer = policyIssuer;

            requestmodel.DelegationRequest.Target = new DelegationEvidence.TargetObject();
            requestmodel.DelegationRequest.Target.AccessSubject = accessSubject;

            requestmodel.DelegationRequest.PolicySets = new List<DelegationRequestModel.RequestPolicySet>();
            requestmodel.DelegationRequest.PolicySets.Add(new DelegationRequestModel.RequestPolicySet());

            requestmodel.DelegationRequest.PolicySets[0].Policies = new List<DelegationEvidence.Policy>();
            requestmodel.DelegationRequest.PolicySets[0].Policies.Add(new DelegationEvidence.Policy());

            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target = new DelegationEvidence.TargetObject();
            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target.Resource =
                new DelegationEvidence.Resource();
            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target.Resource.Type = type;
            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target.Resource.Identifiers = identifiers;
            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target.Resource.Attributes = attributes;

            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target.Actions = actions;
            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target.Environment =
                new DelegationEvidence.Environment();
            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Target.Environment.ServiceProviders =
                serviceProvider;

            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Rules = new List<DelegationEvidence.Rule>();
            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Rules.Add(new DelegationEvidence.Rule());

            requestmodel.DelegationRequest.PolicySets[0].Policies[0].Rules[0].Effect = "Permit";

            return requestmodel;
        }
    }
}