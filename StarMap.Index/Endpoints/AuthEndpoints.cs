using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace StarMapIndex.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            // ---- Authentication endpoints
            app.MapGet("/signin", async (HttpContext http) =>
            {
                // Trigger OAuth challenge -> GitHub
                await http.ChallengeAsync("GitHub", new AuthenticationProperties { RedirectUri = "/post-login" });
            });

            app.MapGet("/post-login", async (HttpContext http, AppDbContext db) =>
            {
                // After successful OAuth sign-in, create/find local user and set cookie with user's id as NameIdentifier
                var idClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var login = http.User.Identity?.Name ?? http.User.FindFirst("login")?.Value ?? "github_user";

                if (idClaim == null)
                {
                    await http.SignOutAsync();
                    http.Response.Redirect("/");
                    return;
                }

                var gitId = idClaim;
                var user = await db.Users.FirstOrDefaultAsync(u => u.GithubId == gitId);
                if (user == null)
                {
                    user = new User { GithubId = gitId, DisplayName = login };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }

                // sign-in locally using cookie with our own claims
                var claims = new List<Claim> {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.DisplayName ?? "user")
    };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                http.Response.Redirect("/mods");
            });

            app.MapGet("/signout", async (HttpContext http) =>
            {
                await http.SignOutAsync();
                http.Response.Redirect("/");
            });
        }
    }
}
