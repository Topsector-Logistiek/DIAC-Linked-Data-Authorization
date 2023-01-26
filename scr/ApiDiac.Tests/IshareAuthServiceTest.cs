namespace ApiDiac.Tests
{
    using System.Text.Json;
    using ApiDiac.Domain;
    using ApiDiac.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Poort8.Ishare.Core;
    using RichardSzalay.MockHttp;
    using Xunit;

    public class IshareAuthServiceTest
    {
        [Fact]
        public void TestIshareAuthService()
        {
            var testJwtResponse = new DelegationResponse();
            testJwtResponse.DelegationToken = "";
            var testJwtResponseJson = JsonSerializer.Serialize(testJwtResponse);


            var inMemorySettings = new Dictionary<string, string>
            {
                { "AuthorizationRegistryIdentifier", "auth_ident" },
                { "ClientId", "ClientId" },
                { "AuthorizationRegistryTokenUrl", "http://test_token_url" },
                { "AuthorizationRegistryDelegationUrl", "http://test_del_url" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var policyEnforcementPointMock = new Mock<IPolicyEnforcementPoint>();
            policyEnforcementPointMock.Setup(o => o.VerifyDelegationTokenPermit(
                inMemorySettings["AuthorizationRegistryIdentifier"],
                testJwtResponse.DelegationToken,
                null,
                null,
                null)).Returns(true);

            var logger = new Mock<ILogger<IshareAuthService>>();

            var authenticationService = new Mock<IAuthenticationService>();
            authenticationService.Setup(o =>
                    o.GetAccessTokenAtPartyAsync(inMemorySettings["AuthorizationRegistryIdentifier"],
                        inMemorySettings["AuthorizationRegistryTokenUrl"]))
                .ReturnsAsync("access_token");

            var testDelegation = new DelegationRequestModel();
            var testDelegationJson = JsonSerializer.Serialize(testDelegation);

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect("http://test_del_url").WithHeaders(@"Authorization", "Bearer access_token")
                .Respond("application/json", testJwtResponseJson);


            var service = new IshareAuthService(policyEnforcementPointMock.Object, configuration, logger.Object,
                authenticationService.Object, mockHttp.ToHttpClient());

            var result = service.DelegationIsValid(testDelegation);

            Assert.True(result.Item1);
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}