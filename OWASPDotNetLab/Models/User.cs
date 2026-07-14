namespace OWASPDotNetLab.Models
{
    /// <summary>
    /// Represents a registered user in the OWASP lab.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
    }
}