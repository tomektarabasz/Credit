namespace CreditSuice.Domain.Model
{
    public class EventData
    {
        public string id { get; set; }
        public string state { get; set; }
        public string type { get; set; }
        public string host { get; set; }
        public long timestamp { get; set; }
    }
}
