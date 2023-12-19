namespace MessengerServer.Services.Abstractions;

public interface IFileNameGenerator
{
    public string GenerateFileName(string? baseFileNameWithoutExtension = null,
        string? fileNameExtension = null);
}