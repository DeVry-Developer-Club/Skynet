using Skynet.Core.Interfaces;
namespace Skynet.Core.Services;
public class StorageService : IStorageService
{
    public string ProjectStorageRoot { get; }

    public StorageService(Options.StorageOptions options)
    {
        ProjectStorageRoot = Path.Join(options.ProjectStorageRoot ?? AppDomain.CurrentDomain.BaseDirectory, "Data");
    }

    public string EnsureExists(string relative)
    {
        string path = GetPathFromRoot(relative);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }

    public string GetPathFromRoot(params string[] folders) => Path.Combine(ProjectStorageRoot, Path.Combine(folders));
}
