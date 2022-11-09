namespace ApiDiac.Services
{
    using System.Reflection;
    using ApiDiac.Data.Interfaces;
    using ApiDiac.Services.Interfaces;
    using JsonLD.Core;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using VDS.RDF.Parsing;

    public sealed class QueryService : IQueryService
    {
        private const string LocalQueryPathTemplate = "Data/{0}.rq";
        private const string LocalQueryPropertiesTemplate = "Data/{0}.json";
        private readonly FileInfo assemblyPath;
        private readonly Uri sparqlBaseUri;
        private readonly string authHeaderValue;
        private readonly IConfiguration configuration;
        private readonly IDataHandler dataHandler;
        private readonly ITriplyQueryPagination triplyQueryPagination;

        public QueryService(IConfiguration configuration, IDataHandler dataHandler, ITriplyQueryPagination triplyQueryPagination)
        {
            this.configuration = configuration;
            this.dataHandler = dataHandler;
            this.triplyQueryPagination = triplyQueryPagination;

            assemblyPath = new FileInfo(Assembly.GetExecutingAssembly().Location);

            sparqlBaseUri = new Uri(configuration.GetConnectionString("SparqlBaseUri"));
            authHeaderValue = configuration.GetConnectionString("AuthHeaderValue");
        }

        public async Task<string> GetJsonLdForIdAndAttribute(Uri id, string attribute, bool framed)
        {
            var pageSize = 10000;
            var acceptHeaderValue = "application/ld+json";

            dataHandler.Configure(sparqlBaseUri, authHeaderValue, acceptHeaderValue);

            (var query, var queryConfig) = ParseSparqlQuery(id, attribute);
            var queryPath = configuration.GetConnectionString("http://your_default_profile");
            var result = await triplyQueryPagination.GetAllPages(query, queryPath, pageSize, acceptHeaderValue);

            if (string.IsNullOrEmpty(result) || result == "[]")
            {
                return null;
            }

            if (!framed)
            {
                return result;
            }
            else
            {
                return ConvertJsonLdFromStandardToFramed(result, queryConfig["frame"]);
            }
        }

        public async Task<string> GetLdForProfileAndQuery(Uri profile, string query, string acceptHeaderValue)
        {
            var pageSize = 10000;

            var emptyResponse = new List<string> {
                "",
                null,
                "[]",
                "sub,pred,obj\r\n",
                "?sub\t?pred\t?obj\n",
                "{\n  \"head\": {\n    \"link\": [],\n    \"vars\": [\n      \"sub\",\n      \"pred\",\n      \"obj\"\n    ]\n  },\n  \"results\": {\n    \"bindings\": []\n  }\n}",
                "<?xml version=\"1.0\"?>\n<sparql xmlns=\"http://www.w3.org/2005/sparql-results#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2007/SPARQL/result.xsd\">\n  <head>\n    <variable name=\"sub\"/>\n    <variable name=\"pred\"/>\n    <variable name=\"obj\"/>\n  </head>\n  <results/>\n</sparql>\n"
            };

            dataHandler.Configure(sparqlBaseUri, authHeaderValue, acceptHeaderValue);
            var queryPath = configuration.GetConnectionString(profile.ToString());
            var result = await triplyQueryPagination.GetAllPages(query, queryPath, pageSize, acceptHeaderValue);

            if (emptyResponse.Contains(result))
            {
                return null;
            }

            return result;
        }

        private (string, JObject) ParseSparqlQuery(Uri id, string attribute)
        {
            try
            {
                var queryPath = Path.Combine(assemblyPath.Directory.FullName, String.Format(LocalQueryPathTemplate, attribute));
                var queryConfigPath = Path.Combine(assemblyPath.Directory.FullName, String.Format(LocalQueryPropertiesTemplate, attribute));

                if (!(File.Exists(queryPath) && File.Exists(queryConfigPath)))
                {
                    throw new Exception($"Query '{attribute}' not found.");
                }

                var parser = new SparqlQueryParser();
                var generalQuery = parser.ParseFromFile(queryPath).ToString();

                var idDecoded = id.ToString();

                var query = generalQuery.Replace("\"identifier\"", $"<{idDecoded}>");

                var queryConfig = JObject.Parse(File.ReadAllText(queryConfigPath));

                return (query, queryConfig);
            }
            catch (RdfParseException parseEx)
            {
                throw new Exception($"Error while parsing SPARQL query: {parseEx}");
            }
        }

        private string ConvertJsonLdFromStandardToFramed(string jsonLd, JToken frame)
        {
            var data = JToken.Parse(jsonLd);
            var opts = new JsonLdOptions();
            var framedJsonLd = JsonLdProcessor.Frame(data, frame, opts).ToString();

            return framedJsonLd;
        }

        public bool IsAttributeValid(string attribute)
        {
            var queryPath = Path.Combine(assemblyPath.Directory.FullName, String.Format(LocalQueryPathTemplate, attribute));
            var queryConfigPath = Path.Combine(assemblyPath.Directory.FullName, String.Format(LocalQueryPropertiesTemplate, attribute));

            return (File.Exists(queryPath) && File.Exists(queryConfigPath));
        }
    }
}
