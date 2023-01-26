namespace QueryTools
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AdminTools.interfaces;
    using AdminTools.models;
    using Microsoft.Extensions.Logging;
    using QueryTools.Interfaces;
    using QueryTools.Models;

    public class QueryClient : IQueryClient
    {
        private readonly IAdminClient adminClient;
        private readonly HttpClient httpClient;
        private readonly ILogger<QueryClient> logger;

        public QueryClient(HttpClient httpClient, ILogger<QueryClient> logger,
            IAdminClient adminClient)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.adminClient = adminClient;
        }

        public async Task<List<string>> ExecuteQuery(string basicQuery,
            QueryOptions? options = null)
        {
            SparqlService service;
            if (options != null && options.Service != null)
            {
                service = options.Service;
            }
            else
            {
                var services = adminClient.GetServices();
                if (services.Count < 1) throw new Exception("No services found for dataset");
                service = services[0];
            }

            if (options != null && options.AcceptHeader != null)
            {
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(options.AcceptHeader));
            }
            else
            {
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/trig"));
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/csv"));
            }

            var emptyResponse = new List<string> {
                "",
                null,
                "[]",
                "sub,pred,obj\r\n",
                "?sub\t?pred\t?obj\n",
                "{\n  \"head\": {\n    \"link\": [],\n    \"vars\": [\n      \"sub\",\n      \"pred\",\n      \"obj\"\n    ]\n  },\n  \"results\": {\n    \"bindings\": []\n  }\n}",
                "<?xml version=\"1.0\"?>\n<sparql xmlns=\"http://www.w3.org/2005/sparql-results#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2007/SPARQL/result.xsd\">\n  <head>\n    <variable name=\"sub\"/>\n    <variable name=\"pred\"/>\n    <variable name=\"obj\"/>\n  </head>\n  <results/>\n</sparql>\n"
            };

            var currentPage = 0;
            var pageSize = 10000;
            var resultList = new List<string>();

            while (true)
            {
                var query = QueryPagination.GetNextPage(basicQuery, currentPage, pageSize);

                var body = JsonContent.Create(new
                {
                    query
                });

                var data = await httpClient.PostAsync(service.endpoint, body).ConfigureAwait(true);
                var result = data.Content.ReadAsStringAsync().Result;

                if (!data.IsSuccessStatusCode)
                    throw new HttpRequestException("Error executing request:" + data.RequestMessage +
                                               "\n\tresult:\n\t" +
                                               result + "\n\t status code: " +
                                               data.StatusCode);

                if (emptyResponse.Contains(result))
                    break;

                resultList.Add(result);
                currentPage++;
            }

            return resultList;
        }
    }
}