namespace QueryTools.Models
{
    using AdminTools.models;

    public class QueryOptions
    {
        public string? AcceptHeader { get; set; }
        public SparqlService? Service { get; set; }
    }
}