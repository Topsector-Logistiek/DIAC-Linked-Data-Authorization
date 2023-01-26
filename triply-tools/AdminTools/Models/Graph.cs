namespace AdminTools.models
{
    public class Graph
    {
        public string GraphName { get; set; }
        public string Id { get; set; }
        public string UploadedAt { get; set; }
        public int NumberOfStatements { get; set; }

        protected bool Equals(Graph other)
        {
            return GraphName == other.GraphName && Id == other.Id && UploadedAt == other.UploadedAt &&
                   NumberOfStatements == other.NumberOfStatements;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Graph)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GraphName, Id, UploadedAt, NumberOfStatements);
        }

        public override string ToString()
        {
            return $"{GraphName} - {Id} - {UploadedAt} - {NumberOfStatements}";
        }
    }
}