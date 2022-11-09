namespace ApiDiac.Services.Interfaces
{
    public interface IQueryService
    {
        Task<string> GetJsonLdForIdAndAttribute(Uri id, string attribute, bool framed);

        Task<string> GetLdForProfileAndQuery(Uri profile, string query, string acceptHeaderValue);

        bool IsAttributeValid(string attribute);
    }
}
