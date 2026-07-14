namespace OWASPDotNetLab.Models
{
    /// <summary>
    /// Represents a product available in the lab catalogue.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}