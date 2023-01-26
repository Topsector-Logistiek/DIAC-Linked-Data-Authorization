namespace AdminTools
{
    using System.Net.Http.Json;
    using AdminTools.interfaces;
    using AdminTools.models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class AdminClient : IAdminClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<AdminClient> logger;
        private readonly string dataset;
        private readonly string graphPath;
        private readonly string org;
        private readonly string servicePath;

        public AdminClient(HttpClient httpClient, ILogger<AdminClient> logger, string org, string dataset)
        {
            this.org = org;
            this.dataset = dataset;
            this.httpClient = httpClient;
            this.logger = logger;
            graphPath = "/datasets/" + org + "/" + dataset + "/graphs/";
            servicePath = "/datasets/" + org + "/" + dataset + "/services/";
        }

        public List<Graph> GetGraphs()
        {
            return BasicRequest<List<Graph>>(graphPath);
        }

        public Graph RenameGraph(Graph graph,
            string newGraphName)
        {
            var body = JsonContent.Create(new
            {
                graphName = newGraphName
            });

            var data = httpClient.PatchAsync(graphPath + graph.Id, body)
                .Result;
            if (data.IsSuccessStatusCode)
            {
                var content = data.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Graph>(content)!;
            }

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }

        public Graph RenameGraph(string oldGraphName, string newGraphName)
        {
            var graphs = GetGraphs();
            var graph = graphs.Find(g => g.GraphName == oldGraphName);
            if (graph == null)
                throw new ArgumentException("Graph not found");
            return RenameGraph(graph, newGraphName);
        }

        public void DeleteGraph(Graph graph)
        {
            var data = httpClient.DeleteAsync(graphPath + graph.Id)
                .Result;
            if (data.IsSuccessStatusCode) return;

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }

        public void DeleteGraph(string graphName)
        {
            var graphs = GetGraphs();
            var graph = graphs.Find(g => g.GraphName == graphName);
            if (graph == null)
                throw new ArgumentException("Graph not found");
            DeleteGraph(graph);
        }

        public List<Dataset> GetDatasets()
        {
            return BasicRequest<List<Dataset>>("/datasets/" + org);
        }

        public Dataset CreateDatasetIfNotExists(
            DatasetAccessLevel accessLevel = DatasetAccessLevel.@private)
        {
            var datasets = GetDatasets();
            if (datasets.Any(d => d.Name == dataset))
                return datasets.Find(d => d.Name == dataset)!;
            return CreateDataset(accessLevel);
        }

        public List<SparqlService> GetServices()
        {
            return BasicRequest<List<SparqlService>>(servicePath);
        }

        public SparqlService GetService(string serviceName)
        {
            return BasicRequest<SparqlService>(servicePath +
                                               serviceName);
        }

        public SparqlService CreateService(string serviceName,
            SparqlServiceType serviceType = SparqlServiceType.virtuoso)
        {
            var body = JsonContent.Create(new
            {
                name = serviceName,
                type = serviceType.ToString()
            });

            var data = httpClient.PostAsync(servicePath, body).Result;
            if (data.IsSuccessStatusCode)
            {
                var content = data.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<SparqlService>(content);
            }

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }

        public SparqlService CreateServiceAndWaitForUpdate(
            string serviceName,
            SparqlServiceType serviceType = SparqlServiceType.virtuoso)
        {
            CreateService(serviceName, serviceType);
            return WaitForUpdate(serviceName);
        }

        public void DeleteService(string serviceName)
        {
            var data = httpClient.DeleteAsync(servicePath + serviceName).Result;
            if (!data.IsSuccessStatusCode)
                throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                               "\n result:\n" +
                                               data.Content.ReadAsStringAsync().Result);
        }

        public SparqlService UpdateService(string serviceName)
        {
            var body = JsonContent.Create(new
            {
                sync = true
            });

            var data = httpClient.PostAsync(servicePath + serviceName, body)
                .Result;
            if (data.IsSuccessStatusCode)
            {
                var content = data.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<SparqlService>(content);
            }

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }

        public SparqlService UpdateServiceAndWaitForUpdate(
            string serviceName)
        {
            UpdateService(serviceName);
            return WaitForUpdate(serviceName);
        }

        private Dataset CreateDataset(
            DatasetAccessLevel accessLevel = DatasetAccessLevel.@private)
        {
            var body = JsonContent.Create(new
            {
                name = dataset,
                accessLevel = accessLevel.ToString(),
                displayName = dataset
            });

            var data = httpClient.PostAsync("/datasets/" + org, body).Result;
            if (data.IsSuccessStatusCode)
            {
                var content = data.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Dataset>(content);
            }

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }

        private SparqlService WaitForUpdate(string serviceName)
        {
            var maxWaitCount = 12;
            var service = GetService(serviceName);

            while (service.status != "running")
            {
                Thread.Sleep(5000);
                logger.LogInformation("Wating for service {ServiceName} to be running", serviceName);
                if (maxWaitCount-- == 0) throw new HttpRequestException("Service did not update within timeout");
                service = GetService(serviceName);
            }

            return service;
        }


        private T BasicRequest<T>(string endpoint)
        {
            var data = httpClient.GetAsync(endpoint).Result;
            if (data.IsSuccessStatusCode)
            {
                var content = data.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(content);
            }

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }
    }
}