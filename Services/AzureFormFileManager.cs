using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MessengerServer.Services.Abstractions;
using Environment = MessengerServer.Configuration.Environment;

namespace MessengerServer.Services;

public class AzureFormFileManager : IFormFileManager
{
    private readonly BlobContainerClient container;
    private readonly IFileNameGenerator fileNameGenerator;

    public AzureFormFileManager(BlobServiceClient blobServiceClient,
        string containerName,
        IFileNameGenerator fileNameGenerator)
    {
        this.fileNameGenerator = fileNameGenerator;
        container = blobServiceClient.GetBlobContainerClient(containerName);
        container.CreateIfNotExists(PublicAccessType.BlobContainer);
    }

    /// <inheritdoc />
    public async Task<Uri> SaveFileAsync(IFormFile file, string? customFileName = null)
    {
        var fileName = customFileName ??
                       fileNameGenerator.GenerateFileName(
                           fileNameExtension: Path.GetExtension(file.FileName));
        var blobClient = container.GetBlobClient(fileName);

        await using var fileStream = file.OpenReadStream();
        await blobClient.UploadAsync(fileStream);
        fileStream.Close();
        
        var newUri = new Uri(Environment.AzuriteHostAddress + blobClient.Uri.AbsolutePath);
        return newUri;
    }

    public async Task DeleteFileAsync(string fileName)
    {
        await container.DeleteBlobAsync(fileName);
    }
}