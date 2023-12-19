namespace MessengerServer.Services.Abstractions;

public interface IFormFileManager
{
    /// <summary>
    ///     Saves the file and returns path or URL to this file.
    /// </summary>
    /// <returns><see cref="Uri" /> path or URL to saved file.</returns>
    public Task<Uri> SaveFileAsync(IFormFile file, string? customFileName = null);

    public Task DeleteFileAsync(string fileName);
}