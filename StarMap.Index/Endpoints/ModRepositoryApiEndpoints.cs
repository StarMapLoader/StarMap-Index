/*using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace StarMapIndex.Endpoints
{
    public static class ModRepositoryApiEndpoints
    {
        public static void MapModRepositoryApiEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/publish", async (HttpContext http, AppDbContext db) =>
            {
                var apiKey = http.Request.Headers["X-Api-Key"].FirstOrDefault();
                var mod = await ValidateApiKeyAsync(apiKey ?? "", db);
                if (mod == null)
                {
                    http.Response.StatusCode = 401;
                    await http.Response.WriteAsync("Invalid API key");
                    return;
                }

                // read JSON body { name, version, downloadUrl }
                var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(body))
                {
                    http.Response.StatusCode = 400;
                    await http.Response.WriteAsync("Empty body");
                    return;
                }
                var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var version = root.GetProperty("version").GetString() ?? "";
                var downloadUrl = root.GetProperty("downloadUrl").GetString() ?? "";
                var name = root.TryGetProperty("name", out var n) ? n.GetString() : mod.Name;

                if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(downloadUrl))
                {
                    http.Response.StatusCode = 400;
                    await http.Response.WriteAsync("Missing fields");
                    return;
                }

                var v = new ModVersion
                {
                    ModId = mod.Id,
                    Version = version,
                    DownloadUrl = downloadUrl,
                    CreatedAt = DateTime.UtcNow
                };
                db.Versions.Add(v);
                await db.SaveChangesAsync();

                http.Response.ContentType = "application/json";
                await http.Response.WriteAsync(JsonSerializer.Serialize(new { success = true, mod = mod.Name, version = version }));
            });

            app.MapGet("/api/mods", async (AppDbContext db) =>
            {
                var list = await db.Mods.Select(m => new { m.Id, m.Name, m.Description }).ToListAsync();
                return Results.Json(list);
            });

            app.MapGet("/api/mods/{id:int}/versions", async (int id, AppDbContext db) =>
            {
                var versions = await db.Versions.Where(v => v.ModId == id).OrderByDescending(v => v.CreatedAt)
                    .Select(v => new { v.Id, v.Version, v.DownloadUrl, v.CreatedAt }).ToListAsync();
                return Results.Json(versions);
            });
        }

        static async Task<Mod?> ValidateApiKeyAsync(string apiKey, AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) return null;
            var hash = CryptoHelpers.HashString(apiKey);
            // find mod by hash. In production consider allowing multiple active keys per mod (key records).
            return await db.Mods.FirstOrDefaultAsync(m => m.ApiKeyHash == hash);
        }
    }
}
*/