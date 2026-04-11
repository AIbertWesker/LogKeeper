namespace LogKeeper.Infrastructure.Options;

internal sealed class MongoSettings
{
    public static string SectionName => "MongoSettings";

    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}
