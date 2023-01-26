namespace QueryTools.Interfaces
{
    using QueryTools.Models;

    public interface IQueryClient
    {
        public Task<List<string>> ExecuteQuery(string basicQuery,
            QueryOptions? options = null);
    }
}