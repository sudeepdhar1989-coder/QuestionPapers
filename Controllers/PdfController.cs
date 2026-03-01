using Microsoft.AspNetCore.Mvc;
using QuestionPapers.Services;

namespace QuestionPapers.Controllers
{
    [Route("Pdf")]
    [Controller]
    public class PdfController : Controller
    {
        private readonly GoogleDriveService _driveService;

        public PdfController(GoogleDriveService driveService)
        {
            _driveService = driveService;
        }

        [Route("View/{id}")]
        public async Task<IActionResult> View(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid ID");
            }

            try
            {
                // Decode the ID
                string base64 = id.Replace("-", "+").Replace("_", "/");
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                byte[] data = Convert.FromBase64String(base64);
                string fileId = System.Text.Encoding.UTF8.GetString(data);

                // Download file from Google Drive
                var fileContent = await _driveService.DownloadFileAsync(fileId);

                if (fileContent == null || fileContent.Length == 0)
                {
                    return BadRequest("Could not download file. Check permissions.");
                }

                // Return the PDF directly
                return File(fileContent, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}