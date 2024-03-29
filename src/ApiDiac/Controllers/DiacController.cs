﻿namespace ApiDiac.Controllers
{
    using System;
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
        private readonly IGraphService graphService;
        private readonly IConfiguration configuration;
        private readonly IIshareAuthService ishareAuthService;
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<DiacController> logger;

        public DiacController(
            IQueryService queryService,
            IGraphService graphService,
            IConfiguration configuration,
            IIshareAuthService ishareAuthService,
            IAuthenticationService authenticationService,
            ILogger<DiacController> logger)
        {
            this.queryService = queryService;
            this.graphService = graphService;
            this.configuration = configuration;
            this.ishareAuthService = ishareAuthService;
            this.authenticationService = authenticationService;
            this.logger = logger;
        }

        [SwaggerOperation(Summary = "Provide a concept, id and attribute to receive data in JSON-LD. Optionally, set framed to true to receive data in framed JSON-LD, set pagination to true to receive data from all the pages or provide an access subject to request data on behalf of another user.")]
        [Produces("application/ld+json")]
        [HttpGet("GetData/{Concept}/{Id}")]
        public async Task<IActionResult> GetData([FromRoute] InputDataConceptAndId inputData, [FromQuery][Required][DefaultValue("sample_attribute")] string attribute, [FromQuery] string? accessSubject = null, [FromQuery] bool framed = false, [FromQuery] bool pagination = false)
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

            var delegationEvidenceObject = System.Text.Json.JsonSerializer.Deserialize<DelegationEvidence>(delegationEvidenceClaim?.ToString()!);

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

            var result = await queryService.GetData(new Uri(delegationEvidenceIdentifier), delegationEvidenceAttribute, framed, pagination);

            if (result == null)
            {
                return NotFound();
            }

            var resultObject = JsonConvert.DeserializeObject(result);

            return Ok(resultObject);
        }

        [SwaggerOperation(Summary = "Provide a profile, query and accept header to receive data in the requested format. Optionally, set pagination to true to receive data from all the pages or provide an access subject to request data on behalf of another user.")]
        [HttpGet("RunSparqlQuery/{Profile}")]
        public async Task<IActionResult> RunSparqlQuery([FromRoute] InputDataProfile inputData, [FromQuery][Required] string query, [FromHeader(Name = "accept")][Required] string acceptHeaderValue, [FromQuery] string? accessSubject = null, [FromQuery] bool pagination = false)
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

            var delegationEvidenceObject = System.Text.Json.JsonSerializer.Deserialize<DelegationEvidence>(delegationEvidenceClaim?.ToString()!);

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

            var result = await queryService.RunSparqlQuery(new Uri(delegationEvidenceIdentifier), query, acceptHeaderValue, pagination);

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

        [SwaggerOperation(Summary = "Provide a dataset name and a JSON-LD content to add or update a graph. Optionally, provide an access subject to provide data on behalf of another user.")]
        [HttpPost("AddOrUpdateGraph")]
        public IActionResult AddOrUpdateGraph([FromForm][Required] InputGraph inputGraph, [FromQuery] string? accessSubject = null)
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

            var decodedDataset = HttpUtility.UrlDecode(inputGraph.DatasetName.OriginalString);

            if (!Uri.IsWellFormedUriString(decodedDataset, UriKind.Absolute))
            {
                return BadRequest("The URL of the dataset is not valid.");
            }

            var graphs = graphService.GetGraphNamesFromContent(inputGraph.content);
            var identifiers = new List<string>();
            var attribute = string.Empty;

            foreach (var graph in graphs)
            {
                var delegationRequest = DelegationEvidenceBuilder.GenerateBasicDelegationRequest(
                configuration["AuthorizationRegistryIdentifier"],
                accessSubject,
                "http://your_type",
                new List<string> { graph },
                new List<string> { "YourAction.write" },
                new List<string> { serviceProvider },
                new List<string> { decodedDataset });

                (var isAuthorized, var delegationevidence) = ishareAuthService.DelegationIsValid(delegationRequest);

                if (!isAuthorized)
                {
                    return Unauthorized("Delegation is not valid");
                }

                delegationevidence.Payload.TryGetValue("delegationEvidence", out object? delegationEvidenceClaim);

                var delegationEvidenceObject = System.Text.Json.JsonSerializer.Deserialize<DelegationEvidence>(delegationEvidenceClaim?.ToString()!);

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

                identifiers.Add(delegationEvidenceIdentifier);
                attribute = delegationEvidenceAttribute;
            }

            var inputData = new InputGraph { DatasetName = new Uri(attribute), content = inputGraph.content };
            graphService.AddOrUpdateGraph(inputData, identifiers);

            return Ok();
        }
    }
}