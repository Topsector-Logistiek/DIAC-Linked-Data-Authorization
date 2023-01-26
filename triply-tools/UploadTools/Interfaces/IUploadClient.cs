namespace UploadTools.Interfaces
{
    using UploadTools.Models;

    public interface IUploadClient
    {
        public Job CreateJob(string org, string dataset);

        public void DeleteJob(Job job);

        public void UploadFile(Job job, Stream file, string fileName);

        public void UploadString(Job job, string triples, string fileName);

        public Job StartJob(Job job);

        public Job GetJobStatus(Job job);

        public Job StartJobAndWaitForCompletion(Job job, int backofInSeconds = 10,
            int maxTries = 6);
    }
}