using OWASPDotNetLab.Models;

namespace OWASPDotNetLab.Data
{
    /// <summary>
    /// Seeds the in-memory database with intentionally weak / insecure data
    /// for use in OWASP training scenarios.
    ///
    /// VULNERABILITY:
    /// CWE-256: Plaintext Storage of a Password
    /// OWASP: A02:2021 - Cryptographic Failures
    /// Description: All passwords are stored in plaintext. No hashing, salting,
    /// or key derivation is performed. This is intentional for training.
    /// Example Attack: A database leak exposes every credential in cleartext.
    /// </summary>
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // VULNERABILITY: CWE-256 / A02 - Plaintext passwords
            context.Users.AddRange(
                new User { Id = 1, Username = "alice", Password = "alice123", Email = "alice@owasp.local", Role = "USER",  Balance = 500.00m },
                new User { Id = 2, Username = "bob",   Password = "bob123",   Email = "bob@owasp.local",   Role = "USER",  Balance = 300.00m },
                new User { Id = 3, Username = "admin", Password = "admin123", Email = "admin@owasp.local", Role = "ADMIN", Balance = 99999.99m }
            );

            context.Products.AddRange(
                new Product { Id = 1, Name = "Laptop",  Description = "14-inch developer laptop",        Price = 1499.99m },
                new Product { Id = 2, Name = "Mouse",   Description = "Wireless optical mouse",          Price = 24.99m   },
                new Product { Id = 3, Name = "Keyboard",Description = "Mechanical keyboard with RGB",    Price = 129.99m  }
            );

            // VULNERABILITY: CWE-79 / A03 - Stored XSS seed
            context.Comments.AddRange(
                new Comment { Id = 1, Author = "System", Body = "Welcome to OWASP Lab" }
            );

            context.SaveChanges();
        }
    }
}