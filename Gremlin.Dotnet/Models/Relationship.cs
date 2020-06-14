namespace GremlinExampleApi.Models
{
    public class Relationship
    {
        public string Id { get; set; }
        public string SourcePersonId { get; set; }
        public string TargetPersonId { get; set; }
        public string Type { get; set; }
    }
}
