namespace AdminTools.models
{
    public class Dataset
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public DatasetAccessLevel AccessLevel { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int GraphCount { get; set; }
        public int Statements { get; set; }
        public int ServiceCount { get; set; }

        public int AssetCount { get; set; }

        public string LastGraphsUpdateTime { get; set; }


        protected bool Equals(Dataset other)
        {
            return Id == other.Id && Name == other.Name && DisplayName == other.DisplayName &&
                   AccessLevel == other.AccessLevel &&
                   CreatedAt == other.CreatedAt && UpdatedAt == other.UpdatedAt && GraphCount == other.GraphCount &&
                   Statements == other.Statements && ServiceCount == other.ServiceCount &&
                   AssetCount == other.AssetCount &&
                   LastGraphsUpdateTime == other.LastGraphsUpdateTime;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Dataset)obj);
        }

        public int GetHashCode(Dataset obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.DisplayName);
            hashCode.Add(obj.AccessLevel);
            hashCode.Add(obj.CreatedAt);
            hashCode.Add(obj.UpdatedAt);
            hashCode.Add(obj.GraphCount);
            hashCode.Add(obj.Statements);
            hashCode.Add(obj.ServiceCount);
            hashCode.Add(obj.AssetCount);
            hashCode.Add(obj.LastGraphsUpdateTime);
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(AccessLevel)}: {AccessLevel}, {nameof(CreatedAt)}: {CreatedAt}";
        }
    }
}