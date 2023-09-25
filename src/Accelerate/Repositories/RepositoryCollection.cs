namespace Accelerate.Repositories;

public sealed class RepositoryCollection : List<Repository>
{
    public RepositoryCollection(string jsonString)
    {
        JsonElement rootElement;

        try
        {
            rootElement = JsonDocument.Parse(jsonString).RootElement;
        }
        catch (JsonException)
        {
            throw new AccelerateException(AccelerateErrorCode.Failure);
        }
        catch (Exception)
        {
            throw new AccelerateException(AccelerateErrorCode.Failure);
        }

        if (!JsonValueKind.Array.Equals(rootElement.ValueKind))
        {
            throw new AccelerateException(AccelerateErrorCode.Failure);
        }

        foreach (var item in rootElement.EnumerateArray())
        {
            Repository? repository;

            if (!item.TryGetProperty("type", out var typeProperty) || !item.TryGetProperty("repo", out var repoProperty))
            {
                throw new AccelerateException(AccelerateErrorCode.Failure);
            }

            var typeName = typeProperty.GetString();

            if (typeName == null)
            {
                throw new AccelerateException(AccelerateErrorCode.Failure);
            }

            try
            {
                switch (typeName)
                {
                    case "Accelerate.Repositories.AzureDevOps":
                        repository = repoProperty.Deserialize<AzureDevOps>();
                        break;
                    case "Accelerate.Repositories.GitHub":
                        repository = repoProperty.Deserialize<GitHub>();
                        break;
                    default:
                        throw new AccelerateException(AccelerateErrorCode.Failure);
                }
            }
            catch
            {
                throw new AccelerateException(AccelerateErrorCode.Failure);
            }

            if (repository == null)
            {
                throw new AccelerateException(AccelerateErrorCode.Failure);
            }

            Add(repository);
        }
    }
}