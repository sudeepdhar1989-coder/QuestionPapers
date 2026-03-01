namespace QuestionPapers.Models
{
    public class FileNode
    {
        public string Name { get; set; }
        public string? GoogleDriveId { get; set; }
        public bool IsDirectory { get; set; }
        public List<FileNode> Children { get; set; } = new List<FileNode>();

        // Links
        public string? WebContentLink { get; set; }
        public string? WebViewLink { get; set; }
    }
}