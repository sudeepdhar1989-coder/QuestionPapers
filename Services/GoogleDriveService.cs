using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using QuestionPapers.Models;
using System.IO;

namespace QuestionPapers.Services
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string? _rootFolderId;
        private readonly string _rootFolderName;

        public GoogleDriveService(IConfiguration configuration)
        {
            _rootFolderId = configuration["ROOT_FOLDER_ID"];
            _rootFolderName = configuration["RootFolderName"] ?? "CBSE Paper";

            string[] Scopes = { DriveService.Scope.DriveReadonly };

            GoogleCredential credential;

            // Get credentials from environment variable
            var credJson = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS");

            if (!string.IsNullOrEmpty(credJson))
            {
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credJson));
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            else
            {
                string[] possiblePaths = new[]
                {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "credentials.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "credentials.json"),
            "credentials.json"
        };

                string? credPath = possiblePaths.FirstOrDefault(File.Exists);
                if (credPath == null)
                    throw new FileNotFoundException("credentials.json not found!");

                credential = GoogleCredential.FromFile(credPath).CreateScoped(Scopes);
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
        }


        public async Task<FileNode> GetFileTreeAsync()
        {
            try
            {
                string folderId;

                if (!string.IsNullOrEmpty(_rootFolderId))
                {
                    folderId = _rootFolderId;
                }
                else
                {
                    var rootFolder = await FindFolderAsync(_rootFolderName, null);
                    if (rootFolder == null)
                    {
                        return new FileNode { Name = $"Folder '{_rootFolderName}' not found." };
                    }
                    folderId = rootFolder.Id;
                }

                return await GetFolderContentsAsync(folderId, "CBSE Papers");
            }
            catch (Exception ex)
            {
                return new FileNode { Name = $"Error: {ex.Message}" };
            }
        }

        public async Task<byte[]?> DownloadFileAsync(string fileId)
        {
            try
            {
                var request = _driveService.Files.Get(fileId);
                var stream = new MemoryStream();

                // DownloadAsync automatically handles alt=media
                await request.DownloadAsync(stream);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download error: {ex.Message}");
                return null;
            }
        }

        private async Task<Google.Apis.Drive.v3.Data.File?> FindFolderAsync(string name, string? parentId)
        {
            var request = _driveService.Files.List();
            string q = $"name = '{name}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";

            if (parentId != null)
            {
                q += $" and '{parentId}' in parents";
            }

            request.Q = q;
            request.Fields = "files(id, name)";

            var result = await request.ExecuteAsync();
            return result.Files?.FirstOrDefault();
        }

        private async Task<FileNode> GetFolderContentsAsync(string folderId, string folderName)
        {
            var node = new FileNode
            {
                Name = folderName,
                IsDirectory = true,
                GoogleDriveId = folderId
            };

            var request = _driveService.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false";
            request.Fields = "files(id, name, mimeType, webContentLink, webViewLink)";
            request.OrderBy = "folder, name";

            var result = await request.ExecuteAsync();

            if (result.Files != null && result.Files.Count > 0)
            {
                foreach (var file in result.Files)
                {
                    if (file.MimeType == "application/vnd.google-apps.folder")
                    {
                        var childNode = await GetFolderContentsAsync(file.Id, file.Name);
                        node.Children.Add(childNode);
                    }
                    else
                    {
                        node.Children.Add(new FileNode
                        {
                            Name = file.Name,
                            IsDirectory = false,
                            GoogleDriveId = file.Id,
                            WebContentLink = file.WebContentLink,
                            WebViewLink = file.WebViewLink
                        });
                    }
                }
            }

            return node;
        }
    }
}