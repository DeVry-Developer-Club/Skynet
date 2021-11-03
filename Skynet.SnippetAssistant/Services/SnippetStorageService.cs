using Skynet.SnippetAssistant.Interfaces;

namespace Skynet.SnippetAssistant.Services;
internal class SnippetStorageService : ISnippetStorageService
{
    private readonly Core.Interfaces.IStorageService _storageService;

    public SnippetStorageService(Core.Interfaces.IStorageService storageService)
    {
        _storageService = storageService;
    }

    public string ToolsProfilePath => _storageService.GetPathFromRoot("Data", "Profiles");
    public string GeneratedReportsPath => _storageService.GetPathFromRoot("Data", "Reports");
}
