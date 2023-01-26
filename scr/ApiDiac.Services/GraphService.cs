namespace ApiDiac.Services
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Headers;
    using AdminTools;
    using AdminTools.models;
    using ApiDiac.Domain;
    using ApiDiac.Services.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using QueryTools;
    using TusDotNetClient;
    using UploadTools;
    using VDS.RDF;
    using VDS.RDF.Parsing;

    public class GraphService : IGraphService
    {
        private readonly Uri sparqlBaseUri;
        private readonly string authHeaderValue;
        private readonly IConfiguration configuration;

        public GraphService(IConfiguration configuration, IIshareAuthService ishareAuthService)
        {
            this.configuration = configuration;

            sparqlBaseUri = new Uri(configuration.GetConnectionString("SparqlBaseUri"));
            authHeaderValue = configuration.GetConnectionString("AuthHeaderValue");
        }

        public void AddOrUpdateGraph(InputGraph inputGraph, List<string> parsedGraphNames)
        {
            var org = sparqlBaseUri.ToString().Split('/').Last();
            var queryPath = configuration.GetConnectionString(inputGraph.DatasetName.ToString());
            if (queryPath == null)
            {
                throw new InvalidDataException("The dataset provided is invalid");
            }

            var dataset = queryPath.Split('/').First();
            var service = queryPath.Split('/')[2];

            var jwtToken = new JwtSecurityToken(authHeaderValue.Split(" ").Last());
            var triplyHost = jwtToken.Issuer!;

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(triplyHost);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authHeaderValue.Split(" ").Last());

            var uploadClient = new UploadClient(httpClient, new Logger<UploadClient>(new NullLoggerFactory()), new TusClient { AdditionalHeaders = { { "Authorization", authHeaderValue } } });
            var adminClient = new AdminClient(httpClient, new Logger<AdminClient>(new NullLoggerFactory()), org, dataset);
            var queryClient = new QueryClient(httpClient, new Logger<QueryClient>(new NullLoggerFactory()), adminClient);

            adminClient.CreateDatasetIfNotExists(DatasetAccessLevel.@internal);

            var existGraphs = adminClient.GetGraphs();
            var existGraphNames = new List<string>();
            foreach (var graph in existGraphs)
            {
                existGraphNames.Add(graph.GraphName);
            }

            var commonGraphs = existGraphNames.Intersect(parsedGraphNames);
            foreach (var graph in commonGraphs)
            {
                adminClient.DeleteGraph(graph);
            }

            var uploadJob = uploadClient.CreateJob(org, dataset);
            uploadClient.UploadString(uploadJob, inputGraph.content, $"{parsedGraphNames.FirstOrDefault()}.json");
            var uploadedJob = uploadClient.StartJobAndWaitForCompletion(uploadJob);

            var uploadedGraphs = uploadedJob.GraphNames;
            var uploadedAndParsedGraphs = uploadedGraphs.Zip(parsedGraphNames, (u, p) => new { uploadedGraphs = u, parsedGraphNames = p });
            foreach (var graph in uploadedAndParsedGraphs)
            {
                adminClient.RenameGraph(graph.uploadedGraphs, graph.parsedGraphNames);
            }

            if (service != null)
            {
                var targetServices = adminClient.GetServices();
                var targetService = targetServices.Find(serv => serv.name == service);
                if (targetService == null)
                {
                    adminClient.CreateServiceAndWaitForUpdate(service);
                }
                else
                {
                    adminClient.UpdateServiceAndWaitForUpdate(service);
                }
            }
        }

        public List<string> GetGraphNamesFromContent(string content)
        {
            var store = new TripleStore();
            var jsonLdParser = new JsonLdParser();
            store.LoadFromString(content, jsonLdParser);
            var graphUris = store.Graphs.GraphUris;
            if (graphUris.FirstOrDefault() == null)
            {
                throw new InvalidDataException("The content provided does not contain any graphs");
            }

            var graphNames = new List<string>();
            foreach (var graphUri in graphUris)
            {
                var graphName = graphUri.ToString();
                if (graphName.EndsWith("/"))
                {
                    graphName = graphName.Remove(graphName.Length - 1);
                }

                graphNames.Add(graphName);
            }

            return graphNames;
        }
    }
}
