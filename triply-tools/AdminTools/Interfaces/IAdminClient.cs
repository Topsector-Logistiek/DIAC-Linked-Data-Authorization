namespace AdminTools.interfaces
{
    using AdminTools.models;

    public interface IAdminClient
    {
        public List<Graph> GetGraphs();

        public Graph RenameGraph(Graph graph,
            string newName);

        public Graph RenameGraph(string oldGraphName,
            string newGraphName);

        public void DeleteGraph(Graph graph);
        public void DeleteGraph(string graphName);

        public List<Dataset> GetDatasets();

        public Dataset CreateDatasetIfNotExists(
            DatasetAccessLevel accessLevel);

        public List<SparqlService> GetServices();

        public SparqlService GetService(string serviceName);

        public SparqlService CreateService(string serviceName,
            SparqlServiceType serviceType = SparqlServiceType.virtuoso);

        public SparqlService CreateServiceAndWaitForUpdate(
            string serviceName,
            SparqlServiceType serviceType = SparqlServiceType.virtuoso);

        public void DeleteService(string serviceName);

        public SparqlService UpdateService(string serviceName);

        public SparqlService UpdateServiceAndWaitForUpdate(
            string serviceName);
    }
}