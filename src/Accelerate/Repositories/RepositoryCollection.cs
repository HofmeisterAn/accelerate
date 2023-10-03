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
            throw new AccelerateException(AccelerateErrorCode.Undefined);
        }
        catch (Exception)
        {
            throw new AccelerateException(AccelerateErrorCode.Undefined);
        }

        if (!JsonValueKind.Array.Equals(rootElement.ValueKind))
        {
            throw new AccelerateException(AccelerateErrorCode.Undefined);
        }

        foreach (var item in rootElement.EnumerateArray())
        {
            Repository? repository;

            if (!item.TryGetProperty("type", out var typeProperty) || !item.TryGetProperty("repo", out var repoProperty))
            {
                throw new AccelerateException(AccelerateErrorCode.Undefined);
            }

            var typeName = typeProperty.GetString();

            if (typeName == null)
            {
                throw new AccelerateException(AccelerateErrorCode.Undefined);
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
                        throw new AccelerateException(AccelerateErrorCode.Undefined);
                }
            }
            catch
            {
                throw new AccelerateException(AccelerateErrorCode.Undefined);
            }

            if (repository == null)
            {
                throw new AccelerateException(AccelerateErrorCode.Undefined);
            }

            Add(repository);
        }
    }
}