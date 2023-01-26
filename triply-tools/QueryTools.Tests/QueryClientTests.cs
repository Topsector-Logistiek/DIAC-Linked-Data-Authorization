namespace QueryTools.Tests
{
    using AdminTools.interfaces;
    using AdminTools.models;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using QueryTools.Models;
    using RichardSzalay.MockHttp;

    [TestClass]
    public class QueryClientTests
    {
        [TestMethod]
        [Description(
            "ExecuteQuery :: Given a org, dataset and query THEN execute the query and return the response")]
        public async Task ExecuteQuery()
        {
            var mockLogger = new Mock<ILogger<QueryClient>>();

            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(x => x.GetServices()).Returns(
                new List<SparqlService>
                    { new() { name = "testService", endpoint = "http://testservice.com" } }).Verifiable();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://testservice.com").WithHeaders(new List<KeyValuePair<string, string>>
                    { new("Accept", "application/trig"), new("Accept", "text/csv") })
                .Respond("text/csv", "");
            var mockHttpClient = mockHttp.ToHttpClient();
            mockHttpClient.BaseAddress = new Uri("http://test/");

            var queryClient = new QueryClient(mockHttpClient, mockLogger.Object, mockAdminClient.Object);

            var result = await queryClient.ExecuteQuery("testQuery");

            mockHttp.VerifyNoOutstandingExpectation();
            mockAdminClient.Verify();
            Assert.AreEqual(null, result.FirstOrDefault());
        }

        [TestMethod]
        [Description(
            "ExecuteQuery :: Given a org, dataset, query and queryOptions THEN execute the query and return the response")]
        public async Task ExecuteQueryWithOptions()
        {
            var mockLogger = new Mock<ILogger<QueryClient>>();

            var mockAdminClient = new Mock<IAdminClient>();

            var queryOptions = new QueryOptions
            {
                AcceptHeader = "application/test",
                Service = new SparqlService { endpoint = "http://testEndpoint/test/" }
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://testEndpoint/test/")
                .WithHeaders(new List<KeyValuePair<string, string>> { new("Accept", "application/test") }).Respond(
                    "application/test",
                    "");
            var mockHttpClient = mockHttp.ToHttpClient();
            mockHttpClient.BaseAddress = new Uri("http://test/");

            var queryClient = new QueryClient(mockHttpClient, mockLogger.Object, mockAdminClient.Object);

            var result = await queryClient.ExecuteQuery("testQuery", queryOptions);

            mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(null, result.FirstOrDefault());
            mockAdminClient.Verify();
        }


        [TestMethod]
        [Description(
            "ExecuteQuery :: Given a org, dataset and query but no valid dataset can be found THEN throw an exception")]
        public async Task ExecuteQueryNoAvailableService()
        {
            var mockLogger = new Mock<ILogger<QueryClient>>();

            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(x => x.GetServices())
                .Returns(new List<SparqlService>())
                .Verifiable();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://testservice.com").WithHeaders(new List<KeyValuePair<string, string>>
                    { new("Accept", "application/trig"), new("Accept", "text/csv") })
                .Respond("text/csv", "");
            var mockHttpClient = mockHttp.ToHttpClient();
            mockHttpClient.BaseAddress = new Uri("http://test/");

            var queryClient = new QueryClient(mockHttpClient, mockLogger.Object, mockAdminClient.Object);

            var result =
                await Assert.ThrowsExceptionAsync<Exception>(() =>
                    queryClient.ExecuteQuery("testQuery"));

            Assert.AreEqual("No services found for dataset",
                result.Message);
            mockHttp.VerifyNoOutstandingExpectation();
            mockAdminClient.Verify();
        }
    }
}