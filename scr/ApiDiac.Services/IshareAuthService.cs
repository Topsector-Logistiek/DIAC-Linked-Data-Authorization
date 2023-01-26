namespace ApiDiac.Services
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using ApiDiac.Data;
    using ApiDiac.Domain;
    using ApiDiac.Services.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Poort8.Ishare.Core;

    public class IshareAuthService : IIshareAuthService
    {
        private readonly IPolicyEnforcementPoint policyEnforcementPoint;
        private readonly IConfiguration configuration;
        private readonly ILogger<IshareAuthService> logger;
        private readonly IAuthenticationService authenticationService;
        private readonly HttpClient httpClient;

        public IshareAuthService(
            IPolicyEnforcementPoint policyEnforcementPoint,
            IConfiguration configuration,
            ILogger<IshareAuthService> logger,
            IAuthenticationService authenticationService,
            HttpClient httpClient)
        {
            this.policyEnforcementPoint = policyEnforcementPoint;
            this.configuration = configuration;
            this.logger = logger;
            this.authenticationService = authenticationService;
            this.httpClient = httpClient;
        }

        public (bool, JwtSecurityToken) DelegationIsValid(DelegationRequestModel delegationRequestModel)
        {
            var token = authenticationService.GetAccessTokenAtPartyAsync(
                configuration["AuthorizationRegistryIdentifier"],
                configuration["AuthorizationRegistryTokenUrl"]).Result;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response =
                httpClient.PostAsync(
                        new Uri(ConfigTemplate.Expand("AuthorizationRegistryDelegationUrl", configuration)),
                        new StringContent(JsonSerializer.Serialize(delegationRequestModel), Encoding.UTF8,
                            "application/json"))
                    .Result;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Error retrieving delegation evidence" + response.Content.ReadAsStringAsync().Result);
            }

            response.EnsureSuccessStatusCode();


            var delegationResponse = response.Content.ReadFromJsonAsync<DelegationResponse>().Result;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(delegationResponse.DelegationToken);


            logger.LogDebug("Delegation result: " +
                            jwtToken.Payload.GetValueOrDefault("delegationEvidence",
                                "No delegation evidence in reponse"));


            var isPermitted = policyEnforcementPoint.VerifyDelegationTokenPermit(
                configuration["AuthorizationRegistryIdentifier"],
                delegationResponse.DelegationToken);

            return (isPermitted, jwtToken);
        }
    }
}