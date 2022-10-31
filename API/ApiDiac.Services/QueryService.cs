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

        public QueryService(IConfiguration configuration, IDataHandler dataHandler)
        {
            this.configuration = configuration;
            this.dataHandler = dataHandler;

            assemblyPath = new FileInfo(Assembly.GetExecutingAssembly().Location);

            sparqlBaseUri = new Uri(configuration.GetConnectionString("SparqlBaseUri"));
            authHeaderValue = configuration.GetConnectionString("AuthHeaderValue");
        }

        public string GetJsonLdForIdAndAttribute(Uri id, string attribute, bool framed)
        {
            dataHandler.Configure(sparqlBaseUri, authHeaderValue, "application/ld+json");

            (var query, var queryConfig) = ParseSparqlQuery(id, attribute);
            var queryPath = configuration.GetConnectionString("http://your_default_profile");
            var result = dataHandler.GetObject(query, queryPath);

            if (result == "{ }\n" || result == "[]")
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

        public string GetLdForProfileAndQuery(Uri profile, string query, string acceptHeaderValue)
        {
            dataHandler.Configure(sparqlBaseUri, authHeaderValue, acceptHeaderValue);

            var queryPath = configuration.GetConnectionString(profile.ToString());

            var result = dataHandler.GetObject(query, queryPath);

            if (result == "{ }\n" || result == "[]")
            {
                return null;
            }

            return result;
        }

        public (string, JObject) ParseSparqlQuery(Uri id, string attribute)
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

        public string ConvertJsonLdFromStandardToFramed(string jsonLd, JToken frame)
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
