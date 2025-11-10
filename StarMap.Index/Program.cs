
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StarMapIndex.Endpoints;
using StarMapIndex.Helpers;

namespace StarMapIndex
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=starmap.db"));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOAuth("GitHub", OAuthEventHandles.GitHubConfigureOptions);

            builder.Services.AddAuthorization();
            builder.Services.AddGrpc();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapAuthEndpoints();
            app.MapRootEndpoints();
            app.MapModEndpoints();
            app.MapGrpcService<GrpcModRepository>();

            app.Run();
        }
    }
}
