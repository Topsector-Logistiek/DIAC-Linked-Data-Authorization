namespace QueryTools.Models
{
    public class QueryPagination
    {
        public static string GetNextPage(string query, int currentPage, int pageSize)
        {
            return query + "OFFSET " + (pageSize * currentPage) + " LIMIT " + pageSize;
        }
    }
}
