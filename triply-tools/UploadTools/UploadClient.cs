namespace UploadTools
{
    using System.Net.Http.Json;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using TusDotNetClient;
    using UploadTools.Interfaces;
    using UploadTools.Models;

    public class UploadClient : IUploadClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<UploadClient> logger;
        private readonly TusClient tusClient;

        public UploadClient(HttpClient httpClient, ILogger<UploadClient> logger,
            TusClient tusClient)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.tusClient = tusClient;
        }

        public Job CreateJob(string org, string dataset)
        {
            var body = JsonContent.Create(new
            {
                type = "upload"
            });
            var data = httpClient.PostAsync("/datasets/" + org + "/" + dataset + "/jobs", body)
                .Result;
            if (data.IsSuccessStatusCode)
            {
                var content = data.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Job>(content);
            }

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }

        public void DeleteJob(Job job)
        {
            var data = httpClient.DeleteAsync(job.JobUrl)
                .Result;
            if (!data.IsSuccessStatusCode)
                throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                               "\n result:\n" +
                                               data.Content.ReadAsStringAsync().Result);
        }

        public void UploadFile(Job job, Stream file, string fileName)
        {
            var fileUrl = tusClient.CreateAsync(job.JobUrl + "/add", file.Length, ("filename", fileName)).Result;
            try
            {
                tusClient.UploadAsync(fileUrl, file).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                // Triply does not follow TUS spec and returns 200 instead of 204 (no content) on the last patch request. This causes the client to throw an exception.
                // If the error contains {\"type\":\"upload\",\"status\":\"created\" that means that triply successfully uploaded the file and we can ignore this error.
                if (e.Message.Contains("WriteFileInServer failed. {\"type\":\"upload\",\"status\":\"created\""))
                    logger.LogInformation("Catching WriteFileInServer, this is not an error: {}",
                        e.Message);
                else
                    throw;
            }
        }

        public void UploadString(Job job, string triples, string fileName)
        {
            UploadFile(job, new MemoryStream(Encoding.UTF8.GetBytes(triples)), fileName);
        }

        public void UploadListOfStrings(Job job, List<string> triples, string fileName)
        {
            foreach (string triple in triples)
            {
                UploadFile(job, new MemoryStream(Encoding.UTF8.GetBytes(triple)), fileName);
            }
        }

        public Job StartJob(Job job)
        {
            var data = httpClient.PostAsync(job.JobUrl + "/start", null)
                .Result;
            if (data.IsSuccessStatusCode)
            {
                var content = data.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Job>(content);
            }

            throw new HttpRequestException("Error executing request:" + data.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Content.ReadAsStringAsync().Result);
        }

        public Job GetJobStatus(Job job)
        {
            var data = httpClient.GetAsync(job.JobUrl);
            if (data.Result.IsSuccessStatusCode)
            {
                var content = data.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Job>(content);
            }

            throw new HttpRequestException("Error executing request:" + data.Result.RequestMessage.RequestUri +
                                           "\n result:\n" +
                                           data.Result.Content.ReadAsStringAsync().Result);
        }

        public Job StartJobAndWaitForCompletion(Job job, int backofInSeconds = 10,
            int maxTries = 6)
        {
            var triesRemaining = maxTries;
            var newJob = StartJob(job);
            while (newJob.Status != "finished")
            {
                if (triesRemaining == 0)
                    throw new Exception("Job did not finish in time");
                Thread.Sleep(backofInSeconds * 1000);
                newJob = GetJobStatus(newJob);
                triesRemaining--;
            }

            return newJob;
        }
    }
}