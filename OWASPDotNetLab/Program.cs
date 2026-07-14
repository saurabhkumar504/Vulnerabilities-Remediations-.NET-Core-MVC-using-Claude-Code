using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OWASPDotNetLab.Data;
using OWASPDotNetLab.Services;

var builder = WebApplication.CreateBuilder(args);

// VULNERABILITY:
// CWE-798: Use of Hard-coded Credentials
// OWASP: A05:2021 - Security Misconfiguration
// Description: API key and JWT secret are hardcoded in source. Anyone with
//              read access to the repo can extract them and forge tokens.
const string ApiKey = "SUPER_SECRET_API_KEY";
const string JwtSecret = "HARDCODED_JWT_SECRET";

// VULNERABILITY:
// CWE-16: Improper Configuration of Operational Settings
// OWASP: A05:2021 - Security Misconfiguration
// Description: Development exception page is enabled in production. Detailed
//              stack traces, query strings, and source code paths are leaked
//              to attackers via DeveloperExceptionPageMiddleware.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("OWASPDotNetLab"));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddHttpClient();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        // VULNERABILITY: CWE-614 / A05 - Secure flag is not set on the auth cookie
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// VULNERABILITY: CWE-209 / A05 - Developer exception page is always on
if (app.Environment.IsDevelopment() || true)
{
    app.UseDeveloperExceptionPage();
}
else
{
    // VULNERABILITY: CWE-209 - UseExceptionHandler with no error page leaks
    // verbose exceptions via stack traces in logs.
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Seed the in-memory database with intentionally weak data.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

// Touch hardcoded secrets so the compiler keeps them; they are still in source
// and discoverable through the /api/secrets endpoint.
_ = ApiKey;
_ = JwtSecret;

app.Run();