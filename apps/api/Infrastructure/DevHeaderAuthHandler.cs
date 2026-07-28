using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Hms.Api.Infrastructure;

/// <summary>
/// DEV-ONLY fallback authentication: turns the <c>X-Tenant-Id</c> header into an
/// authenticated principal carrying a <c>tenant_id</c> claim, so local tooling
/// (curl, Swagger, smoke scripts) can call protected endpoints without minting a
/// JWT. Returns <see cref="AuthenticateResult.NoResult"/> outside Development, so
/// production is strictly JWT-only — the header grants nothing there.
/// </summary>
public class DevHeaderAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevHeader";
    private readonly IWebHostEnvironment _env;

    public DevHeaderAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IWebHostEnvironment env)
        : base(options, logger, encoder) => _env = env;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_env.IsDevelopment())
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!Request.Headers.TryGetValue("X-Tenant-Id", out var raw) || !Guid.TryParse(raw, out var tid))
            return Task.FromResult(AuthenticateResult.NoResult());

        // Dev header acts as Owner (full access) for local curl/Swagger.
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("tenant_id", tid.ToString()),
            new Claim(ClaimTypes.Name, "dev-header"),
            new Claim("role", "Owner"),
        }, SchemeName, ClaimTypes.Name, roleType: "role");

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
