namespace Accelerate.Repositories;

public abstract class Repository
{
    public Repository(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);
        Url = url;
    }

    [JsonIgnore]
    public string WorkingDirectoryPath => Path.Combine(Url.Segments.Select(segment => segment.Trim('/')).ToArray());

    [JsonPropertyName("url")]
    public Uri Url { get; }
}