namespace ApiDiac.Data.Interfaces
{
    public interface IDataHandler
    {
        void Configure(Uri baseUri, string authHeaderValue, string acceptHeaderValue);

        string GetObject(string query, string queryPath);
    }
}
