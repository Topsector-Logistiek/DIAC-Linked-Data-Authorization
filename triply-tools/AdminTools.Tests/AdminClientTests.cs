namespace AdminTools.Tests
{
    using System.Net;
    using System.Reflection;
    using AdminTools.models;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using RichardSzalay.MockHttp;

    [TestClass]
    public class AdminClientTests
    {
        private static readonly string
            graphuri = "http://api.testtriply.cc/datasets/testOrg/testDataset/graphs/";

        [TestMethod]
        [Description(
            "GetGraphs :: GIVEN and organization or username and a dataset THEN Return a list of graph objects for that dataset")]
        public void GetGraphs()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                graphuri).Respond("application/json",
                getResponseString("graphResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");


            var expected = new List<Graph>
            {
                new()
                {
                    GraphName = "https://testtriply.cc//Top10nlv1/graphs/default",
                    Id = "627a79ec5d9940bd34f9b333",
                    UploadedAt = "2022-05-10T14:42:52.926Z",
                    NumberOfStatements = 130019
                }
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.GetGraphs();
            CollectionAssert.AreEqual(expected, result);
        }


        [TestMethod]
        [Description(
            "GetServices :: GIVEN and organization or username and a dataset THEN Return a list of sparqlservice objects for that dataset")]
        public void GetServices()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                "http://api.testtriply.cc/datasets/testOrg/testDataset/services/").Respond("application/json",
                getResponseString("servicesResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");


            var expected = new List<SparqlService>
            {
                new()
                {
                    type = SparqlServiceType.virtuoso,
                    version = "22.08.0",
                    name = "Alerts-test",
                    id = "62debef07afb9169d5889ddd",
                    numberOfLoadedStatements = 24256,
                    numberOfGraphs = 17,
                    numberOfLoadedGraphs = 17,
                    numberOfGraphErrors = 0,
                    outOfSync = true,
                    endpoint =
                        "https://api.testtriply.cc/datasets/test/Alerts-test/services/Alerts-test/sparql",
                    status = "running"
                }
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.GetServices();
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        [Description(
            "GetService :: GIVEN and organization or username and a dataset and a service name THEN Return the sparqlservice object for that dataset")]
        public void GetService()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.Expect(HttpMethod.Get,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/testservice")
                .Respond("application/json", getResponseString("serviceResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var expected = new SparqlService
            {
                type = SparqlServiceType.virtuoso,
                version = "22.08.0",
                name = "Alerts-test",
                id = "62debef07afb9169d5889ddd",
                numberOfLoadedStatements = 24256,
                numberOfGraphs = 17,
                numberOfLoadedGraphs = 17,
                numberOfGraphErrors = 0,
                outOfSync = true,
                endpoint = "https://api.testtriply.cc/datasets/test/Alerts-test/services/Alerts-test/sparql",
                status = "running"
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.GetService("testservice");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Description(
            "CreateService :: GIVEN and organization or username and a dataset and a service name THEN create sparqlservice and return a sparqle service object")]
        public void CreateService()
        {
            var body = JsonConvert.SerializeObject(
                new
                {
                    name = "testservice",
                    type = "virtuoso"
                });
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/").WithContent(body)
                .Respond("application/json", getResponseString("serviceResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var expected = new SparqlService
            {
                type = SparqlServiceType.virtuoso,
                version = "22.08.0",
                name = "Alerts-test",
                id = "62debef07afb9169d5889ddd",
                numberOfLoadedStatements = 24256,
                numberOfGraphs = 17,
                numberOfLoadedGraphs = 17,
                numberOfGraphErrors = 0,
                outOfSync = true,
                endpoint = "https://api.testtriply.cc/datasets/test/Alerts-test/services/Alerts-test/sparql",
                status = "running"
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.CreateService("testservice");

            mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Description(
            "CreateServiceAndWaitForUpdate ::GIVEN and organization or username and a dataset and a service name THEN create the sparqlservice and wait for it to be running, then return a sparqle service object")]
        public void CreateServiceAndWaitForUpdate()
        {
            var body = JsonConvert.SerializeObject(
                new
                {
                    name = "testservice",
                    type = "virtuoso"
                });
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/").WithContent(body)
                .Respond("application/json", getResponseString("serviceResponseStarting.json"));
            mockHttp.Expect(HttpMethod.Get,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/testservice")
                .Respond("application/json", getResponseString("serviceResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var expected = new SparqlService
            {
                type = SparqlServiceType.virtuoso,
                version = "22.08.0",
                name = "Alerts-test",
                id = "62debef07afb9169d5889ddd",
                numberOfLoadedStatements = 24256,
                numberOfGraphs = 17,
                numberOfLoadedGraphs = 17,
                numberOfGraphErrors = 0,
                outOfSync = true,
                endpoint = "https://api.testtriply.cc/datasets/test/Alerts-test/services/Alerts-test/sparql",
                status = "running"
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.CreateServiceAndWaitForUpdate("testservice");

            mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Description(
            "UpdateService :: GIVEN and organization or username and a dataset and a service name THEN update the sparql service and return a sparqle service object")]
        public void UpdateService()
        {
            var body = JsonConvert.SerializeObject(
                new
                {
                    sync = true
                });
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/testservice")
                .WithContent(body)
                .Respond("application/json", getResponseString("serviceResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var expected = new SparqlService
            {
                type = SparqlServiceType.virtuoso,
                version = "22.08.0",
                name = "Alerts-test",
                id = "62debef07afb9169d5889ddd",
                numberOfLoadedStatements = 24256,
                numberOfGraphs = 17,
                numberOfLoadedGraphs = 17,
                numberOfGraphErrors = 0,
                outOfSync = true,
                endpoint = "https://api.testtriply.cc/datasets/test/Alerts-test/services/Alerts-test/sparql",
                status = "running"
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.UpdateService("testservice");

            mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Description(
            "UpdateServiceAndWaitForUpdate :: GIVEN and organization or username and a dataset and a service name THEN update the sparql service wait for it to be updated, then return a sparqle service object")]
        public void UpdateServiceAndWaitForUpdate()
        {
            var body = JsonConvert.SerializeObject(
                new
                {
                    sync = true
                });
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/testservice")
                .WithContent(body)
                .Respond("application/json", getResponseString("serviceResponse.json"));
            mockHttp.Expect(HttpMethod.Get,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/testservice")
                .Respond("application/json", getResponseString("serviceResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var expected = new SparqlService
            {
                type = SparqlServiceType.virtuoso,
                version = "22.08.0",
                name = "Alerts-test",
                id = "62debef07afb9169d5889ddd",
                numberOfLoadedStatements = 24256,
                numberOfGraphs = 17,
                numberOfLoadedGraphs = 17,
                numberOfGraphErrors = 0,
                outOfSync = true,
                endpoint = "https://api.testtriply.cc/datasets/test/Alerts-test/services/Alerts-test/sparql",
                status = "running"
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.UpdateServiceAndWaitForUpdate("testservice");

            mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        [Description(
            "DeleteService :: GIVEN and organization or username and a dataset and a service name THEN delete the sparqlservice")]
        public void DeleteService()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Delete,
                    "http://api.testtriply.cc/datasets/testOrg/testDataset/services/testservice")
                .Respond(HttpStatusCode.OK);
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            client.DeleteService("testservice");

            mockHttp.VerifyNoOutstandingExpectation();
        }


        [TestMethod]
        [Description(
            "RenameGraph :: GIVEN an organization or username, a dataset,an old graph object and a new name THEN rename the old graph to the new name")]
        public void RenameGraph()
        {
            var inputgraph = new Graph
            {
                Id = "1234",
                GraphName = "oldGraphName",
                NumberOfStatements = 1234,
                UploadedAt = "2020-01-01T00:00:00.000Z"
            };

            var expected = new Graph
            {
                Id = "1234",
                GraphName = "newGraphName",
                NumberOfStatements = 1234,
                UploadedAt = "2020-01-01T00:00:00.000Z"
            };

            var body = JsonConvert.SerializeObject(
                new
                {
                    graphName = "newGraphName"
                });

            var responeContent = JsonConvert.SerializeObject(expected);


            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Patch,
                graphuri + "1234").WithContent(body).Respond("application/json",
                responeContent);

            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var newGraph = client.RenameGraph(inputgraph, "newGraphName");


            Assert.AreEqual(expected, newGraph);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        [Description(
            "RenameGraph :: GIVEN an organization or username, a dataset,an old graph name and a new name THEN rename the old graph to the new name")]
        public void RenameGraphWithGraphName()
        {
            var resultGraphs = new List<Graph>
            {
                new()
                {
                    Id = "1234",
                    GraphName = "oldGraphName",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                },
                new()
                {
                    Id = "1235",
                    GraphName = "oldGraphName2",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                }
            };

            var inputGraphName = "oldGraphName";

            var expected = new Graph
            {
                Id = "1234",
                GraphName = "newGraphName",
                NumberOfStatements = 1234,
                UploadedAt = "2020-01-01T00:00:00.000Z"
            };

            var body = JsonConvert.SerializeObject(
                new
                {
                    graphName = "newGraphName"
                });

            var responsecententGetGraphs = JsonConvert.SerializeObject(resultGraphs);
            var responeContent = JsonConvert.SerializeObject(expected);


            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                graphuri).Respond("application/json",
                responsecententGetGraphs);
            mockHttp.Expect(HttpMethod.Patch,
                graphuri + "1234").WithContent(body).Respond("application/json",
                responeContent);

            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var newGraph = client.RenameGraph(inputGraphName, "newGraphName");


            Assert.AreEqual(expected, newGraph);
            mockHttp.VerifyNoOutstandingExpectation();
        }


        [TestMethod]
        [Description(
            "RenameGraph :: GIVEN an organization or username, a dataset,an old graph name that does not exist and a new name THEN rename the old graph to the new name")]
        [ExpectedException(typeof(ArgumentException))]
        public void RenameGraphWithGraphNameThatDoesNotExist()
        {
            var resultGraphs = new List<Graph>
            {
                new()
                {
                    Id = "1234",
                    GraphName = "oldGraphName",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                },
                new()
                {
                    Id = "1235",
                    GraphName = "oldGraphName2",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                }
            };

            var inputGraphName = "oldGraphName3";

            var responsecententGetGraphs = JsonConvert.SerializeObject(resultGraphs);


            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                graphuri).Respond("application/json",
                responsecententGetGraphs);


            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            client.RenameGraph(inputGraphName, "newGraphName");
        }


        [TestMethod]
        [Description(
            "DeleteGraph :: GIVEN an organization or username, a dataset and a graph object THEN delete the graph")]
        public void DeleteGraph()
        {
            var inputgraph = new Graph
            {
                Id = "1234",
                GraphName = "oldGraphName",
                NumberOfStatements = 1234,
                UploadedAt = "2020-01-01T00:00:00.000Z"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Delete,
                graphuri + "1234").Respond(HttpStatusCode.NoContent);

            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            client.DeleteGraph(inputgraph);

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        [Description(
            "DeleteGraph :: GIVEN an organization or username, a dataset and a graph name THEN delete the graph")]
        public void DeleteGraphWithGraphName()
        {
            var resultGraphs = new List<Graph>
            {
                new()
                {
                    Id = "1234",
                    GraphName = "oldGraphName",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                },
                new()
                {
                    Id = "1235",
                    GraphName = "oldGraphName2",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                }
            };

            var inputGraphName = "oldGraphName";

            var responsecententGetGraphs = JsonConvert.SerializeObject(resultGraphs);


            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                graphuri).Respond("application/json",
                responsecententGetGraphs);
            mockHttp.Expect(HttpMethod.Delete,
                graphuri + "1234").Respond(HttpStatusCode.NoContent);

            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            client.DeleteGraph(inputGraphName);

            mockHttp.VerifyNoOutstandingExpectation();
        }


        [TestMethod]
        [Description(
            "DeleteGraph :: GIVEN an organization or username, a dataset and a graph name that does not exist THEN delete the graph")]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteGraphWithGraphNameThatDoesNotExist()
        {
            var resultGraphs = new List<Graph>
            {
                new()
                {
                    Id = "1234",
                    GraphName = "oldGraphName",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                },
                new()
                {
                    Id = "1235",
                    GraphName = "oldGraphName2",
                    NumberOfStatements = 1234,
                    UploadedAt = "2020-01-01T00:00:00.000Z"
                }
            };

            var inputGraphName = "oldGraphName3";

            var responsecententGetGraphs = JsonConvert.SerializeObject(resultGraphs);


            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                graphuri).Respond("application/json",
                responsecententGetGraphs);


            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            client.RenameGraph(inputGraphName, "newGraphName");
        }

        [TestMethod]
        [Description(
            "GetDatasets :: GIVEN and organization  THEN Return a list of datasets")]
        public void GetDatasets()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                "http://api.testtriply.cc/datasets/testOrg").Respond("application/json",
                getResponseString("datasetResponse.json"));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");


            var expected = new List<Dataset>
            {
                new()
                {
                    Id = "1234",
                    Name = "testDataset",
                    Statements = 123,
                    AccessLevel = DatasetAccessLevel.@public,
                    AssetCount = 1233,
                    CreatedAt = "2020-01-01T00:00:00.000Z",
                    DisplayName = "testDisplayName",
                    GraphCount = 4,
                    ServiceCount = 1,
                    UpdatedAt = "2020-01-01T00:00:00.000Z",
                    LastGraphsUpdateTime = "2020-01-01T00:00:00.000Z"
                }
            };

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "testDataset");
            var result = client.GetDatasets();

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        [Description(
            "CreateDatasetIfNotExists :: GIVEN an organization or username, a dataset name that does not exist and optionally an access level THEN then create the dataset")]
        public void CreateDatasetIfNotExists()
        {
            var body = JsonConvert.SerializeObject(
                new
                {
                    name = "NewtestDataset",
                    accessLevel = DatasetAccessLevel.@public.ToString(),
                    displayName = "NewtestDataset"
                });

            var expected =
                new Dataset
                {
                    Id = "1234",
                    Name = "NewtestDataset",
                    Statements = 123,
                    AccessLevel = DatasetAccessLevel.@public,
                    AssetCount = 1233,
                    CreatedAt = "2020-01-01T00:00:00.000Z",
                    DisplayName = "testDisplayName",
                    GraphCount = 4,
                    ServiceCount = 1,
                    UpdatedAt = "2020-01-01T00:00:00.000Z",
                    LastGraphsUpdateTime = "2020-01-01T00:00:00.000Z"
                };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get,
                "http://api.testtriply.cc/datasets/testOrg").Respond("application/json",
                getResponseString("datasetResponse.json"));
            mockHttp.Expect(HttpMethod.Post,
                "http://api.testtriply.cc/datasets/testOrg").WithContent(body).Respond("application/json",
                JsonConvert.SerializeObject(expected));

            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var client = new AdminClient(httpclient, Mock.Of<ILogger<AdminClient>>(), "testOrg", "NewtestDataset");
            var actual = client.CreateDatasetIfNotExists(
                DatasetAccessLevel.@public);

            Assert.AreEqual(expected, actual);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        private string getResponseString(string filename)
        {
            var path = Path.Combine(
                new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                "Resources", filename);
            return File.ReadAllText(path);
        }
    }
}