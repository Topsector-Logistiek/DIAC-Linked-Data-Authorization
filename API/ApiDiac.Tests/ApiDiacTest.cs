namespace ApiDiac.Tests
{
    using Microsoft.Extensions.Configuration;
    using ApiDiac.Controllers;
    using ApiDiac.Domain;
    using ApiDiac.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text.Json;
    using ApiDiac.Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Poort8.Ishare.Core;
    using Newtonsoft.Json.Linq;

    public class ApiDiacTest
    {
        [Fact]
        public void TestGetLinkedDataForConceptAndIdApi()
        {
            var concept = new Uri("http://test_concept_url");
            var id = new Uri("http://test_id_url");
            var attribute = "sample_attribute";
            var inputData = new InputDataConceptAndId { Concept = concept, Id = id };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(
                tokenHandler.CreateJwtSecurityToken(
                    issuer: "issuer",
                    audience: "clientid",
                    subject: null,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddSeconds(30),
                    issuedAt: DateTime.UtcNow
                )
            );
            token = "Bearer " + token;

            var queryServiceMock = new Mock<IQueryService>();
            queryServiceMock.Setup(o => o.GetJsonLdForIdAndAttribute(id, attribute, false)).Returns("{}").Verifiable();
            queryServiceMock.Setup(o => o.IsAttributeValid(attribute)).Returns(true).Verifiable();

            var delegationEvidence = DelegationEvidenceBuilder.GenerateBasicDelegationRequest(
                "",
                "",
                concept.ToString(),
                new List<string> { id.ToString() },
                new List<string>(),
                new List<string>(),
                new List<string>{ attribute }
            );

            var tokenResponse = new JwtSecurityToken();
            tokenResponse.Payload["delegationEvidence"] = JsonSerializer.Serialize(delegationEvidence.DelegationRequest);

            var ishareAuthServiceMock = new Mock<IIshareAuthService>();
            ishareAuthServiceMock.Setup(o => o.DelegationIsValid(It.IsAny<DelegationRequestModel>()))
                .Returns((true, tokenResponse)).Verifiable();

            var inMemorySettings = new Dictionary<string, string>
            {
                { "AuthorizationRegistryIdentifier", "auth_ident" },
                { "ClientId", "ClientId" },
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var poort8AuthenticationServericeMock = new Mock<IAuthenticationService>();
            poort8AuthenticationServericeMock
                .Setup(o => o.ValidateAuthorizationHeader("ClientId", new StringValues(token))).Verifiable();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", token);

            var controller = new DiacController(queryServiceMock.Object, configuration, ishareAuthServiceMock.Object,
                poort8AuthenticationServericeMock.Object, new Mock<ILogger<DiacController>>().Object)
            { ControllerContext = new ControllerContext { HttpContext = httpContext } };

            var result = controller.GetLinkedDataForConceptAndIdAndAttribute(inputData, attribute);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            var resultObject = result as OkObjectResult;

            poort8AuthenticationServericeMock.Verify();
            ishareAuthServiceMock.Verify();
            queryServiceMock.Verify();

            Assert.NotNull(resultObject);
            Assert.IsType<JObject>(resultObject.Value);
            Assert.Equal(new JObject(), resultObject.Value);
        }

        [Fact]
        public void TestGetLinkedDataForQueryAndProfileApi()
        {
            var profile = new Uri("http://test_profile_url");
            var query = "";
            var acceptHeaderValue = "application/ld+json";
            var inputData = new InputDataProfile { Profile = profile };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(
                tokenHandler.CreateJwtSecurityToken(
                    issuer: "issuer",
                    audience: "clientid",
                    subject: null,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddSeconds(30),
                    issuedAt: DateTime.UtcNow
                )
            );
            token = "Bearer " + token;

            var queryServiceMock = new Mock<IQueryService>();
            queryServiceMock.Setup(o => o.GetLdForProfileAndQuery(profile, query, acceptHeaderValue)).Returns("LD in any format").Verifiable();

            var delegationEvidence = DelegationEvidenceBuilder.GenerateBasicDelegationRequest(
                "",
                "",
                "http://test_type_url",
                new List<string> { profile.ToString() },
                new List<string>(),
                new List<string>(),
                new List<string> { "*" }
            );

            var tokenResponse = new JwtSecurityToken();
            tokenResponse.Payload["delegationEvidence"] = JsonSerializer.Serialize(delegationEvidence.DelegationRequest);

            var ishareAuthServiceMock = new Mock<IIshareAuthService>();
            ishareAuthServiceMock.Setup(o => o.DelegationIsValid(It.IsAny<DelegationRequestModel>()))
                .Returns((true, tokenResponse)).Verifiable();

            var inMemorySettings = new Dictionary<string, string>
            {
                { "AuthorizationRegistryIdentifier", "auth_ident" },
                { "ClientId", "ClientId" },
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var poort8AuthenticationServericeMock = new Mock<IAuthenticationService>();
            poort8AuthenticationServericeMock
                .Setup(o => o.ValidateAuthorizationHeader("ClientId", new StringValues(token))).Verifiable();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", token);

            var controller = new DiacController(queryServiceMock.Object, configuration, ishareAuthServiceMock.Object,
                poort8AuthenticationServericeMock.Object, new Mock<ILogger<DiacController>>().Object)
            { ControllerContext = new ControllerContext { HttpContext = httpContext } };

            var result = controller.GetLinkedDataForProfileAndQuery(inputData, query, acceptHeaderValue);

            Assert.NotNull(result);
            Assert.IsType<ContentResult>(result);

            var resultObject = result as ContentResult;

            Assert.NotNull(resultObject);
            Assert.IsType<string>(resultObject.ContentType);
            Assert.Equal("LD in any format", resultObject.Content);
        }
    }
}