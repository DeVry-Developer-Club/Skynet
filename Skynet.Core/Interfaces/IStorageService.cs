namespace Skynet.Core.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Root location for project storage (may not be AppDomain)
    /// </summary>
    string ProjectStorageRoot { get; }

    /// <summary>
    /// Get a path relative to <see cref="ProjectStorageRoot"/>
    /// </summary>
    /// <param name="folders"></param>
    /// <returns></returns>
    string GetPathFromRoot(params string[] folders);

    /// <summary>
    /// Ensures the <paramref name="relative"/> path
    /// exists, if not it creates it. Then returns the full path
    /// </summary>
    /// <param name="relative"></param>
    /// <returns>Full path</returns>
    string EnsureExists(string relative);
}
