namespace Accelerate.Tests;

public sealed class RepositoryCollectionTest
{
    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    public void Constructor_WithInvalidJson_ThrowsAccelerateException(string jsonString)
    {
        _ = Assert.Throws<AccelerateException>(() => new RepositoryCollection(jsonString));
    }

    [Theory]
    [InlineData("[{}]")]
    [InlineData("[{\"type\":null}]")]
    [InlineData("[{\"repo\":null}]")]
    public void Constructor_WithMissingProperty_ThrowsAccelerateException(string jsonString)
    {
        _ = Assert.Throws<AccelerateException>(() => new RepositoryCollection(jsonString));
    }

    [Theory]
    [InlineData("[{\"type\":null,\"repo\":{}}]")]
    [InlineData("[{\"type\":\"Accelerate.Repositories.GitLab\",\"repo\":{}}]")]
    [InlineData("[{\"type\":\"Accelerate.Repositories.AzureDevOps\",\"repo\":null}]")]
    [InlineData("[{\"type\":\"Accelerate.Repositories.AzureDevOps\",\"repo\":{}}]")]
    public void Constructor_WithUnsupportedRepository_ThrowsAccelerateException(string jsonString)
    {
        _ = Assert.Throws<AccelerateException>(() => new RepositoryCollection(jsonString));
    }

    [Theory]
    [InlineData("[{\"type\":\"Accelerate.Repositories.AzureDevOps\",\"repo\":{\"url\":\"https://dev.azure.com/\"}}]")]
    [InlineData("[{\"type\":\"Accelerate.Repositories.GitHub\",\"repo\":{\"url\":\"https://github.com/\"}}]")]
    public void Should_Create_RepositoryCollection_From_JsonString(string jsonString)
    {
        _ = Assert.Single(new RepositoryCollection(jsonString));
    }
}