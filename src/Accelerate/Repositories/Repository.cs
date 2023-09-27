namespace Accelerate.Repositories;

public abstract class Repository
{
    public Repository(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);
        Url = url;
    }

    [JsonIgnore]
    public virtual string WorkingDirectoryPath => Url.AbsolutePath.TrimStart('/');

    [JsonPropertyName("url")]
    public virtual Uri Url { get; }
}