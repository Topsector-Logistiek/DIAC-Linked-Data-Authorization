namespace ApiDiac.Data
{
    using ApiDiac.Data.Interfaces;
    using System.Text;

    public class TriplyQueryPagination : ITriplyQueryPagination
    {
        private readonly IDataHandler dataHandler;

        public TriplyQueryPagination(IDataHandler dataHandler)
        {
            this.dataHandler = dataHandler;
        }

        public async Task<string> GetAllPages(string query, string queryPath, int pageSize, string acceptHeaderValue)
        {
            var currentPage = 0;
            var fullResult = new StringBuilder();

            var emptyResponse = new List<string> { 
                "",
                null,
                "[]",
                "sub,pred,obj\r\n",
                "?sub\t?pred\t?obj\n",
                "{\n  \"head\": {\n    \"link\": [],\n    \"vars\": [\n      \"sub\",\n      \"pred\",\n      \"obj\"\n    ]\n  },\n  \"results\": {\n    \"bindings\": []\n  }\n}",
                "<?xml version=\"1.0\"?>\n<sparql xmlns=\"http://www.w3.org/2005/sparql-results#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2007/SPARQL/result.xsd\">\n  <head>\n    <variable name=\"sub\"/>\n    <variable name=\"pred\"/>\n    <variable name=\"obj\"/>\n  </head>\n  <results/>\n</sparql>\n"
            };

            while (true)
            {
                var nextPageQuery = GetNextPage(query, currentPage, pageSize);
                var result = await dataHandler.GetObject(nextPageQuery, queryPath);

                if (emptyResponse.Contains(result))
                {
                    break;
                }

                if (acceptHeaderValue == "application/ld+json" || acceptHeaderValue == "application/json")
                {
                    result = result.Substring(2, result.Length - 4);
                    result = result.Insert(result.Length, ",");
                }

                fullResult.AppendLine(result);
                currentPage++;
            }

            if (acceptHeaderValue == "application/ld+json" || acceptHeaderValue == "application/json")
            {
                fullResult.Insert(0, "[\n").Remove(fullResult.Length - 3, 1).Insert(fullResult.Length, "]");
            }

            return fullResult.ToString();
        }

        private string GetNextPage(string query, int currentPage, int pageSize)
        {
            return query + "OFFSET " + (pageSize * currentPage) + " LIMIT " + pageSize;
        }
    }
}
