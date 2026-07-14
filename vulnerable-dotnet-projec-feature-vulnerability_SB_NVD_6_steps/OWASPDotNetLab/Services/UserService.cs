using OWASPDotNetLab.Data;
using OWASPDotNetLab.Models;
using System.Collections.Generic;
using System.Linq;

namespace OWASPDotNetLab.Services
{
    /// <summary>
    /// UserService - intentional code smells and vulnerabilities live here so
    /// that static analysis tools can locate them through a clean service layer.
    /// </summary>
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        // VULNERABILITY:
        // CWE-256: Plaintext Storage of a Password
        // OWASP: A02:2021 - Cryptographic Failures
        // Description: Passwords are compared using string equality against the
        //              database-stored plaintext password. No hashing is performed.
        // Example Attack: Any database dump reveals every password verbatim.
        public User Authenticate(string username, string password)
        {
            return _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User GetById(int id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }

        public List<User> GetAll()
        {
            return _db.Users.ToList();
        }

        // VULNERABILITY:
        // CWE-915: Improperly Controlled Modification of Dynamically-Determined Object Attributes
        // OWASP: A08:2021 - Software and Data Integrity Failures (Mass Assignment)
        // Description: Caller-supplied User objects are persisted verbatim, allowing
        //              the caller to set Role, Balance, Id, etc.
        // Example Attack: POST /api/register with { "username":"a","password":"b","role":"ADMIN","balance":999999 }
        public User Create(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
            return user;
        }
    }
}