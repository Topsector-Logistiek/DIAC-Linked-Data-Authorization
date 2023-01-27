namespace ApiDiac.Services.Interfaces
{
    public interface IQueryService
    {
        Task<string> GetData(Uri id, string attribute, bool framed, bool pagination);

        Task<string> RunSparqlQuery(Uri profile, string query, string acceptHeaderValue, bool pagination);

        bool IsAttributeValid(string attribute);
    }
}
