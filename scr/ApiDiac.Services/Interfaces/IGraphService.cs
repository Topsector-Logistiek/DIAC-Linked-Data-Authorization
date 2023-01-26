namespace ApiDiac.Services.Interfaces
{
    using ApiDiac.Domain;

    public interface IGraphService
    {
        void AddOrUpdateGraph(InputGraph inputGraph, List<string> parsedGraphNames);

        List<string> GetGraphNamesFromContent(string content);
    }
}
