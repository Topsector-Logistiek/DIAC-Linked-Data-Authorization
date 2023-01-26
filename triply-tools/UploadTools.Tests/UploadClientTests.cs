namespace UploadTools.Tests
{
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using RichardSzalay.MockHttp;
    using TusDotNetClient;
    using UploadTools.Models;

    [TestClass]
    public class UploadClientTests
    {
        [TestMethod]
        [Description(
            "CreateJob :: GIVEN organization and dataset THEN create a job")]
        public void CreateJob()
        {
            var body = JsonConvert.SerializeObject(new
            {
                type = "upload"
            });

            var expectedJob = new Job
            {
                JobId = "1",
                JobUrl = "testurl",
                Status = "ok",
                GraphNames = Array.Empty<string>()
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post,
                "http://api.testtriply.cc/datasets/testOrg/testDataset/jobs").WithContent(body).Respond(
                "application/json",
                JsonConvert.SerializeObject(expectedJob));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var uploadClient = new UploadClient(httpclient,
                Mock.Of<ILogger<UploadClient>>(),
                Mock.Of<TusClient>());
            var job = uploadClient.CreateJob("testOrg", "testDataset");

            Assert.AreEqual(expectedJob, job);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        [Description(
            "DeleteJob :: GIVEN Job THEN delete the job")]
        public void DeleteJob()
        {
            var expectedJob = new Job
            {
                JobId = "1",
                JobUrl = "http://joburl/test",
                Status = "ok",
                GraphNames = Array.Empty<string>()
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Delete, expectedJob.JobUrl).Respond("application/json", "{}");
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var uploadClient = new UploadClient(httpclient,
                Mock.Of<ILogger<UploadClient>>(),
                Mock.Of<TusClient>());
            uploadClient.DeleteJob(expectedJob);

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        [Description("StartJob :: GIVEN job THEN start the job")]
        public void StartJob()
        {
            var inputJob = new Job
            {
                JobId = "1",
                JobUrl = "http://joburl/test",
                Status = "ok",
                GraphNames = Array.Empty<string>()
            };

            var expectedJob = new Job
            {
                JobId = "2",
                JobUrl = "http://joburl/test/expected",
                Status = "starting",
                GraphNames = Array.Empty<string>()
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post, inputJob.JobUrl + "/start")
                .Respond("application/json", JsonConvert.SerializeObject(expectedJob));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var uploadClient = new UploadClient(httpclient,
                Mock.Of<ILogger<UploadClient>>(),
                Mock.Of<TusClient>());
            var job = uploadClient.StartJob(inputJob);

            Assert.AreEqual(expectedJob, job);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        [Description(
            "StartJobAndWaitForCompletion :: GIVEN job THEN start the job and wait for completion")]
        public void StartJobAndWaitForCompletion()
        {
            var inputJob = new Job
            {
                JobId = "1",
                JobUrl = "http://joburl/test",
                Status = "ok",
                GraphNames = Array.Empty<string>()
            };

            var testjob = new Job
            {
                JobId = "2",
                JobUrl = "http://joburl/test/job1",
                Status = "starting",
                GraphNames = Array.Empty<string>()
            };

            var expectedJob = new Job
            {
                JobId = "2",
                JobUrl = "http://joburl/test/expected",
                Status = "finished",
                GraphNames = Array.Empty<string>()
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Post, inputJob.JobUrl + "/start")
                .Respond("application/json", JsonConvert.SerializeObject(testjob));
            mockHttp.Expect(HttpMethod.Get, testjob.JobUrl).Respond("application/json",
                JsonConvert.SerializeObject(expectedJob));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var uploadClient = new UploadClient(httpclient,
                Mock.Of<ILogger<UploadClient>>(),
                Mock.Of<TusClient>());
            var job = uploadClient.StartJobAndWaitForCompletion(inputJob);

            Assert.AreEqual(expectedJob, job);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        [Description(
            "GetJobStatus :: GIVEN job THEN get the job status")]
        public void GetJobStatus()
        {
            var inputJob = new Job
            {
                JobId = "1",
                JobUrl = "http://joburl/test",
                Status = "ok",
                GraphNames = Array.Empty<string>()
            };

            var expectedJob = new Job
            {
                JobId = "2",
                JobUrl = "http://joburl/test/expected",
                Status = "finished",
                GraphNames = Array.Empty<string>()
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.Expect(HttpMethod.Get, inputJob.JobUrl).Respond("application/json",
                JsonConvert.SerializeObject(expectedJob));
            var httpclient = mockHttp.ToHttpClient();
            httpclient.BaseAddress = new Uri("http://api.testtriply.cc");

            var uploadClient = new UploadClient(httpclient,
                Mock.Of<ILogger<UploadClient>>(),
                Mock.Of<TusClient>());
            var job = uploadClient.GetJobStatus(inputJob);

            Assert.AreEqual(expectedJob, job);
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}