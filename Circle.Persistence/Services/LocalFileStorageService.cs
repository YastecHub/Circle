using Circle.Application.Abstractions;
using Circle.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace Circle.Persistence.Services;

public sealed class LocalFileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private readonly string rootPath = ResolveRootPath(options.Value.RootPath);

    public async Task<string> SaveCustomerDocumentAsync(Guid customerId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var safeFileName = MakeSafeFileName(fileName);
        var relativePath = Path.Combine("customer-documents", customerId.ToString("N"), $"{Guid.NewGuid():N}-{safeFileName}");
        var absolutePath = GetAbsolutePath(relativePath);
        var directoryPath = Path.GetDirectoryName(absolutePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using var output = File.Create(absolutePath);
        await content.CopyToAsync(output, cancellationToken);

        return relativePath.Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(storagePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Stored file was not found.", storagePath);
        }

        Stream stream = File.OpenRead(absolutePath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(storagePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string GetAbsolutePath(string storagePath)
    {
        var normalizedPath = storagePath.Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.GetFullPath(Path.Combine(rootPath, normalizedPath));
        var normalizedRoot = Path.GetFullPath(rootPath);

        if (!absolutePath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage path is outside the configured document root.");
        }

        return absolutePath;
    }

    private static string ResolveRootPath(string? configuredRootPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredRootPath))
        {
            return Path.GetFullPath(configuredRootPath);
        }

        var home = Environment.GetEnvironmentVariable("HOME");
        return Path.GetFullPath(string.IsNullOrWhiteSpace(home)
            ? Path.Combine(AppContext.BaseDirectory, "circle-documents")
            : Path.Combine(home, "circle-documents"));
    }

    private static string MakeSafeFileName(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            safeFileName = safeFileName.Replace(invalidCharacter, '-');
        }

        return string.IsNullOrWhiteSpace(safeFileName) ? "document" : safeFileName;
    }
}
