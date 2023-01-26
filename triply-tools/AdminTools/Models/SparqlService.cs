namespace AdminTools.models
{
    public class SparqlService
    {
        public SparqlServiceType type { get; set; }
        public string version { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public int numberOfLoadedStatements { get; set; }
        public int numberOfGraphs { get; set; }
        public int numberOfLoadedGraphs { get; set; }
        public int numberOfGraphErrors { get; set; }
        public bool outOfSync { get; set; }
        public string endpoint { get; set; }

        public string status { get; set; }

        protected bool Equals(SparqlService other)
        {
            return type == other.type && version == other.version && name == other.name && id == other.id &&
                   numberOfLoadedStatements == other.numberOfLoadedStatements &&
                   numberOfGraphs == other.numberOfGraphs && numberOfLoadedGraphs == other.numberOfLoadedGraphs &&
                   numberOfGraphErrors == other.numberOfGraphErrors && outOfSync == other.outOfSync &&
                   endpoint == other.endpoint && status == other.status;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SparqlService)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(type);
            hashCode.Add(version);
            hashCode.Add(name);
            hashCode.Add(id);
            hashCode.Add(numberOfLoadedStatements);
            hashCode.Add(numberOfGraphs);
            hashCode.Add(numberOfLoadedGraphs);
            hashCode.Add(numberOfGraphErrors);
            hashCode.Add(outOfSync);
            hashCode.Add(endpoint);
            hashCode.Add(status);
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            return $"{name} ({type} {version} {endpoint} {status})";
        }
    }
}