namespace OWASPDotNetLab.Models
{
    /// <summary>
    /// Represents a comment authored by a user.
    /// Body is intentionally rendered with Html.Raw in the view to demonstrate
    /// stored XSS for training.
    /// </summary>
    public class Comment
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Body { get; set; }
    }
}