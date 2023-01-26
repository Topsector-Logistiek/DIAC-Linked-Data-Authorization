namespace ApiDiac.Services.Interfaces
{
    public interface IQueryService
    {
        Task<string> GetJsonLdForIdAndAttribute(Uri id, string attribute, bool framed, bool pagination);

        Task<string> GetLdForProfileAndQuery(Uri profile, string query, string acceptHeaderValue, bool pagination);

        bool IsAttributeValid(string attribute);
    }
}
