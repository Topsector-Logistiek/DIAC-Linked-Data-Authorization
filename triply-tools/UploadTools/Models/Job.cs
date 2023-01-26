namespace UploadTools.Models
{
    public class Job
    {
        public string JobId { get; set; }
        public string Status { get; set; }
        public string JobUrl { get; set; }
        public string[] GraphNames { get; set; }
        public string Error { get; set; }

        protected bool Equals(Job other)
        {
            return JobId == other.JobId && Status == other.Status && JobUrl == other.JobUrl &&
                   Error == other.Error && GraphNames.SequenceEqual(other.GraphNames);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Job)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(JobId, Status, JobUrl, GraphNames, Error);
        }

        public static bool operator ==(Job? left, Job? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Job? left, Job? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return
                $"{nameof(JobId)}: {JobId}, {nameof(Status)}: {Status}, {nameof(JobUrl)}: {JobUrl}, {nameof(GraphNames)}: {GraphNames}, {nameof(Error)}: {Error}";
        }
    }
}