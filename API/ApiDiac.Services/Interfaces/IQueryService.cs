namespace ApiDiac.Services.Interfaces
{
    public interface IQueryService
    {
        string GetJsonLdForIdAndAttribute(Uri id, string attribute, bool framed);

        string GetLdForProfileAndQuery(Uri profile, string query, string acceptHeaderValue);

        bool IsAttributeValid(string attribute);
    }
}
