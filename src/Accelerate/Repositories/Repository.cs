namespace Accelerate.Repositories;

public abstract class Repository
{
    public Repository(Uri url)
    {
        using var sha1 = SHA1.Create();
        Hash = Convert.ToHexString(sha1.ComputeHash(Encoding.Default.GetBytes(url.ToString()))).ToLowerInvariant();
        Url = url;
    }

    [JsonIgnore]
    public string Hash { get; }

    [JsonPropertyName("url")]
    public Uri Url { get; }
}