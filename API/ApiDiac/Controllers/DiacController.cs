namespace ApiDiac.Controllers
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Web;
    using ApiDiac.Data;
    using ApiDiac.Domain;
    using ApiDiac.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Poort8.Ishare.Core;
    using Poort8.Ishare.Core.Models;
    using Swashbuckle.AspNetCore.Annotations;

    [ApiController]
    [Route("[controller]")]
    public class DiacController : ControllerBase
    {
        private readonly IQueryService queryService;
        private readonly IConfiguration configuration;
        private readonly IIshareAuthService ishareAuthService;
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<DiacController> logger;

        public DiacController(
            IQueryService queryService,
            IConfiguration configuration,
            IIshareAuthService ishareAuthService,
            IAuthenticationService authenticationService,
            ILogger<DiacController> logger)
        {
            this.queryService = queryService;
            this.configuration = configuration;
            this.ishareAuthService = ishareAuthService;
            this.authenticationService = authenticationService;
            this.logger = logger;
        }

        [SwaggerOperation(Summary = "Provide a concept, id and attribute to receive data in JSON-LD. Optionally, set framed to true to receive data in framed JSON-LD or provide an access subject to request data on behalf of another user.")]
        [Produces("application/ld+json")]
        [HttpGet("GetLinkedDataForConceptAndIdAndAttribute/{Concept}/{Id}")]
        public IActionResult GetLinkedDataForConceptAndIdAndAttribute([FromRoute] InputDataConceptAndId inputData, [FromQuery][Required][DefaultValue("sample_attribute")] string attribute, [FromQuery] string? accessSubject = null, [FromQuery] bool framed = false)
        {
            var authorization = Request.Headers.Authorization;
            var hadAuth = AuthenticationHeaderValue.TryParse(authorization, out var authHeader);
            if (!hadAuth || authHeader.Scheme != "Bearer")
            {
                return Unauthorized("Authorization header is missing or not Bearer");
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(authHeader.Parameter))
            {
                return Unauthorized("Authorization header is not a valid JWT");
            }
            var jwtToken = handler.ReadJwtToken(authHeader.Parameter);
            var jwtTokenAudiances = jwtToken.Payload.Aud;
            if (jwtTokenAudiances.Count != 1)
            {
                return Unauthorized("JWT token has more than one audience");
            }

            string serviceProvider;
            if (accessSubject == null)
            {
                accessSubject = jwtTokenAudiances.First();
                serviceProvider = configuration["ClientId"];
            }
            else
            {
                accessSubject = accessSubject;
                serviceProvider = configuration["AuthorizationRegistryIdentifier"];
            }

            try
            {
                authenticationService.ValidateAuthorizationHeader(configuration["ClientId"], authorization);
            }
            catch (Exception e)
            {
                logger.LogWarning("Returning bad request: invalid authorization header. {msg}", e.Message);
                return Unauthorized("Invalid authorization token.");
            }

            var decodedConcept = HttpUtility.UrlDecode(inputData.Concept.OriginalString);
            var decodedId = HttpUtility.UrlDecode(inputData.Id.OriginalString);

            if (!(Uri.IsWellFormedUriString(decodedConcept, UriKind.Absolute)
                && Uri.IsWellFormedUriString(decodedId, UriKind.Absolute)))
            {
                return BadRequest("The URL of the concept and/or id is not valid.");
            }

            var delegationRequest = DelegationEvidenceBuilder.GenerateBasicDelegationRequest(
                configuration["AuthorizationRegistryIdentifier"],
                accessSubject,
                decodedConcept,
                new List<string> { decodedId },
                new List<string> { "YourAction.read" },
                new List<string> { serviceProvider },
                new List<string> { attribute });

            (var isAuthorized, var delegationevidence) = ishareAuthService.DelegationIsValid(delegationRequest);

            if (!isAuthorized)
            {
                return Unauthorized("Delegation is not valid");
            }

            delegationevidence.Payload.TryGetValue("delegationEvidence", out object? delegationEvidenceClaim);

            var delegationEvidenceObject =
                System.Text.Json.JsonSerializer.Deserialize<DelegationEvidence>(delegationEvidenceClaim?.ToString()!);

            var delegationEvidenceIdentifiers = delegationEvidenceObject.PolicySets.First().Policies.First().Target.Resource.Identifiers;
            var delegationEvidenceAttributes = delegationEvidenceObject.PolicySets.First().Policies.First().Target.Resource.Attributes;
            if (delegationEvidenceIdentifiers.Count != 1)
            {
                logger.LogWarning("More than one ids found in delegation evidence, using only first one");
            }
            if (delegationEvidenceAttributes.Count != 1)
            {
                logger.LogWarning("More than one attributes found in delegation evidence, using only first one");
            }

            var delegationEvidenceIdentifier = delegationEvidenceIdentifiers.First();
            var delegationEvidenceAttribute = delegationEvidenceAttributes.First();

            if (!queryService.IsAttributeValid(attribute))
            {
                return BadRequest("The attribute is not valid.");
            }

            var result = queryService.GetJsonLdForIdAndAttribute(new Uri(delegationEvidenceIdentifier), delegationEvidenceAttribute, framed);

            if (result == null)
            {
                return NotFound();
            }

            var resultObject = JsonConvert.DeserializeObject(result);

            return Ok(resultObject);
        }

        [SwaggerOperation(Summary = "Provide a profile, query and accept header to receive data in the requested format. Optionally, provide an access subject to request data on behalf of another user.")]
        [HttpGet("GetLinkedDataForProfileAndQuery/{Profile}")]
        public IActionResult GetLinkedDataForProfileAndQuery([FromRoute] InputDataProfile inputData, [FromQuery][Required] string query, [FromHeader(Name = "accept")][Required] string acceptHeaderValue, [FromQuery] string? accessSubject = null)
        {
            var authorization = Request.Headers.Authorization;
            var hadAuth = AuthenticationHeaderValue.TryParse(authorization, out var authHeader);
            if (!hadAuth || authHeader.Scheme != "Bearer")
            {
                return Unauthorized("Authorization header is missing or not Bearer");
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(authHeader.Parameter))
            {
                return Unauthorized("Authorization header is not a valid JWT");
            }
            var jwtToken = handler.ReadJwtToken(authHeader.Parameter);
            var jwtTokenAudiances = jwtToken.Payload.Aud;
            if (jwtTokenAudiances.Count != 1)
            {
                return Unauthorized("JWT token has more than one audience");
            }

            string serviceProvider;
            if (accessSubject == null)
            {
                accessSubject = jwtTokenAudiances.First();
                serviceProvider = configuration["ClientId"];
            }
            else
            {
                accessSubject = accessSubject;
                serviceProvider = configuration["AuthorizationRegistryIdentifier"];
            }

            try
            {
                authenticationService.ValidateAuthorizationHeader(serviceProvider, authorization);
            }
            catch (Exception e)
            {
                logger.LogWarning("Returning bad request: invalid authorization header. {msg}", e.Message);
                return Unauthorized("Invalid authorization token.");
            }

            var decodedProfile = HttpUtility.UrlDecode(inputData.Profile.OriginalString);

            if (!Uri.IsWellFormedUriString(decodedProfile, UriKind.Absolute))
            {
                return BadRequest("The URL of the profile is not valid.");
            }

            var delegationRequest = DelegationEvidenceBuilder.GenerateBasicDelegationRequest(
                configuration["AuthorizationRegistryIdentifier"],
                accessSubject,
                "http://your_type",
                new List<string> { decodedProfile },
                new List<string> { "YourAction.read" },
                new List<string> { serviceProvider },
                new List<string> { "*" });

            (var isAuthorized, var delegationevidence) = ishareAuthService.DelegationIsValid(delegationRequest);

            if (!isAuthorized)
            {
                return Unauthorized("Delegation is not valid");
            }

            delegationevidence.Payload.TryGetValue("delegationEvidence", out object? delegationEvidenceClaim);

            var delegationEvidenceObject =
                System.Text.Json.JsonSerializer.Deserialize<DelegationEvidence>(delegationEvidenceClaim?.ToString()!);

            var delegationEvidenceIdentifiers = delegationEvidenceObject.PolicySets.First().Policies.First().Target.Resource.Identifiers;
            var delegationEvidenceAttributes = delegationEvidenceObject.PolicySets.First().Policies.First().Target.Resource.Attributes;
            if (delegationEvidenceIdentifiers.Count != 1)
            {
                logger.LogWarning("More than one ids found in delegation evidence, using only first one");
            }
            if (delegationEvidenceAttributes.Count != 1)
            {
                logger.LogWarning("More than one attributes found in delegation evidence, using only first one");
            }

            var result = queryService.GetLdForProfileAndQuery(new Uri(decodedProfile), query, acceptHeaderValue);

            if (result == null)
            {
                return NotFound();
            }

            var response = new ContentResult()
            {
                Content = result,
                ContentType = acceptHeaderValue,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };

            return response;
        }
    }
}