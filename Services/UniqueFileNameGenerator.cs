using System.Text;
using MessengerServer.Services.Abstractions;

namespace MessengerServer.Services;

public class UniqueFileNameGenerator : IFileNameGenerator
{
    public string GenerateFileName(string? baseFileNameWithoutExtension = null,
        string? fileNameExtension = null)
    {
        StringBuilder fileNameBuilder = new();
        if (!string.IsNullOrWhiteSpace(baseFileNameWithoutExtension))
            fileNameBuilder.Append(baseFileNameWithoutExtension)
                .Append('_');
        fileNameBuilder.Append(Guid.NewGuid()
            .ToString("N"));

        var fileName = fileNameBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(fileNameExtension))
            fileName = Path.ChangeExtension(fileName, fileNameExtension);

        return fileName;
    }
}