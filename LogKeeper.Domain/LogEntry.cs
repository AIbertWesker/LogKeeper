namespace LogKeeper.Domain;

public class LogEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public string Application { get; set; }
    public string CorrelationId { get; set; }
    public string ClientIp { get; set; }
    public string MachineName { get; set; }
    public string Exception { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}
