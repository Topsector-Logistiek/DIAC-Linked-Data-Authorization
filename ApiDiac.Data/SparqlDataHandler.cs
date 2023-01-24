namespace ApiDiac.Data
{
    using ApiDiac.Data.Interfaces;

    public class SparqlDataHandler : IDataHandler
    {
        private Uri? baseUri;
        private string? authHeaderValue;
        private string? acceptHeaderValue;

        public void Configure(Uri baseUri, string authHeaderValue, string acceptHeaderValue)
        {
            this.baseUri = baseUri;
            this.authHeaderValue = authHeaderValue;
            this.acceptHeaderValue = acceptHeaderValue;
        }

        public async Task<string> GetObject(string query, string queryPath)
        {
            ValidateConfig();

            var response = ExecuteQuery(query, new Uri($"{baseUri.OriginalString}/{queryPath}")).Result;

            using var sr = new StreamReader(response.Content.ReadAsStream());
            var data = sr.ReadToEnd();

            return data;
        }

        private void ValidateConfig()
        {
            if (baseUri == null || authHeaderValue == null || acceptHeaderValue == null)
            {
                throw new Exception("SparqlDataHandler must first be configured before retrieving data.");
            }
        }

        private async Task<HttpResponseMessage> ExecuteQuery(string query, Uri requestPath)
        {
            using var client = HttpClientFactory.Create();
            using var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("query", query) });

            client.DefaultRequestHeaders.Add("Authorization", authHeaderValue);
            client.DefaultRequestHeaders.Add("Accept", acceptHeaderValue);

            return await client.PostAsync(requestPath, content).ConfigureAwait(true);
        }
    }
}
