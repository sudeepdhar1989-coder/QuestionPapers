using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestionPapers.Models;
using QuestionPapers.Services;

namespace QuestionPapers.Pages;

public class IndexModel : PageModel
{
    private readonly GoogleDriveService _driveService;

    public FileNode? FileTree { get; set; }

    public IndexModel(GoogleDriveService driveService)
    {
        _driveService = driveService;
    }

    public async Task OnGetAsync()
    {
        FileTree = await _driveService.GetFileTreeAsync();
    }
}