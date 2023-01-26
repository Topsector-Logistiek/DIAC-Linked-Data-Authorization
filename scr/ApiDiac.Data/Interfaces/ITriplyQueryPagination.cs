namespace ApiDiac.Data.Interfaces
{
    public interface ITriplyQueryPagination
    {
        Task<string> GetAllPages(string query, string queryPath, int pageSize, string acceptHeaderValue);
    }
}
