using Microsoft.EntityFrameworkCore;
using OWASPDotNetLab.Data;
using OWASPDotNetLab.Models;
using System.Collections.Generic;
using System.Linq;

namespace OWASPDotNetLab.Services
{
    /// <summary>
    /// ProductService - exposes product lookups that are vulnerable to SQL injection
    /// because they use raw concatenated SQL through FromSqlRaw.
    /// </summary>
    public class ProductService
    {
        private readonly AppDbContext _db;

        public ProductService(AppDbContext db)
        {
            _db = db;
        }

        public List<Product> GetAll()
        {
            return _db.Products.ToList();
        }

        // VULNERABILITY:
        // CWE-89: SQL Injection
        // OWASP: A03:2021 - Injection
        // Description: User-supplied query string is concatenated directly into a raw SQL
        //              statement executed via FromSqlRaw. The result is materialized
        //              back into Product entities.
        // Example Attack: /api/search?q=' OR 1=1 -- returns every product.
        public List<Product> Search(string q)
        {
            var sql = "SELECT * FROM Products WHERE Name LIKE '%" + q + "%'";
            return _db.Products.FromSqlRaw(sql).ToList();
        }
    }
}