namespace AdminTools.Factories
{
    using AdminTools.interfaces;

    public class RequestFactory : IRequestFactory
    {
        private readonly HttpClient request;

        public RequestFactory(string baseURl, string? authHeaderValue)
        {
            request = new HttpClient();
            request.BaseAddress = new Uri(baseURl);
            if (authHeaderValue != null)
                request.DefaultRequestHeaders.Add("Authorization", $"Bearer {authHeaderValue}");
        }

        public HttpClient GetHttpClient()
        {
            return request;
        }
    }
}