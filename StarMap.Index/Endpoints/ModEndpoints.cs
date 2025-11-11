using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace StarMapIndex.Endpoints
{
    public static class ModEndpoints
    {
        public static void MapModEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/mods", [Authorize] async (HttpContext http, AppDbContext db) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var mods = await db.Mods.Where(m => m.AuthorId == userId).OrderByDescending(m => m.Id).ToListAsync();

                var rows = new StringBuilder();
                foreach (var m in mods)
                {
                    rows.AppendLine($"<li><strong>{System.Net.WebUtility.HtmlEncode(m.Name)}</strong> — API Key ID: {m.ApiKeyId} — <a href=\"/mods/{m.Id}\">Manage</a></li>");
                }
                if (!mods.Any()) rows.AppendLine("<li>No mods yet.</li>");

                var html = $@"
                    <!doctype html>
                    <html><body style='font-family:Segoe UI,Arial;margin:0;padding:16px;'>
                      <a href='/'>← Home</a>
                      <h2>Your Mods</h2>
                      <ul>{rows}</ul>

                      <h3>Create new Mod</h3>
                      <form method='post' action='/mods/create'>
                        <label>Mod name: <input name='name' required /></label>
                        <br/><label>Description: <input name='description' /></label>
                        <br/><button type='submit'>Create</button>
                      </form>
                    </body></html>";
                http.Response.ContentType = "text/html; charset=utf-8";
                await http.Response.WriteAsync(html);
            });

            // create mod -> returns API key (show once)
            app.MapPost("/mods/create", [Authorize] async (HttpContext http, AppDbContext db) =>
            {
                var form = await http.Request.ReadFormAsync();
                var name = form["name"].ToString().Trim();
                var desc = form["description"].ToString().Trim();
                if (string.IsNullOrEmpty(name))
                {
                    http.Response.Redirect("/mods");
                    return;
                }

                var id = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);
                if (user == null) return;

                // generate an API key (secure random) and store only hash
                var apiKeyPlain = CryptoHelpers.GenerateApiKey();
                var apiKeyHash = CryptoHelpers.HashString(apiKeyPlain);
                var apiKeyId = Guid.NewGuid().ToString("n"); // visible identifier for key (not the secret)
                var mod = new Mod
                {
                    Name = name,
                    Description = desc,
                    Author = user,
                    ApiKeyHash = apiKeyHash,
                    ApiKeyId = apiKeyId
                };
                db.Mods.Add(mod);
                await db.SaveChangesAsync();

                // Show the API key once
                var html = $@"
                    <!doctype html><html><body style='font-family:Segoe UI,Arial;padding:16px;'>
                      <a href='/mods'>← Back</a>
                      <h2>Mod created: {System.Net.WebUtility.HtmlEncode(name)}</h2>
                      <p> Mod id: {System.Net.WebUtility.HtmlEncode(mod.Id.ToString())}</code></p>
                      <p><strong>Save this API key now — you'll only see it once:</strong></p>
                      <pre style='background:#f3f3f3;padding:8px;border-radius:4px'>{System.Net.WebUtility.HtmlEncode(apiKeyPlain)}</pre>
                      <p>API Key ID (useful in logs): <code>{System.Net.WebUtility.HtmlEncode(apiKeyId)}</code></p>
                      <p>To publish: POST to <code>/api/publish</code> with header <code>X-Api-Key: &lt;your key&gt;</code> and JSON body containing <code>name</code>, <code>version</code>, <code>downloadUrl</code>.</p>
                    </body></html>";
                http.Response.ContentType = "text/html; charset=utf-8";
                await http.Response.WriteAsync(html);
            });

            app.MapGet("/mods/{id:minlength(36)}", [Authorize] async (string id, HttpContext http, AppDbContext db) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var mod = await db.Mods.FindAsync(Guid.Parse(id));
                if (mod == null)
                {
                    http.Response.StatusCode = 404;
                    await http.Response.WriteAsync("Not found or not permitted");
                    return;
                }

                var versions = await db.Versions
                    .Where(v => v.ModId == mod.Id)
                    .OrderByDescending(v => v.CreatedAt)
                    .ToListAsync();

                var html = $@"
                    <!doctype html><html><body style='font-family:Segoe UI,Arial;padding:16px;'>
                      <a href='/mods'>← Back</a>
                      <h2>{System.Net.WebUtility.HtmlEncode(mod.Name)}</h2>
                      <p>Description: {System.Net.WebUtility.HtmlEncode(mod.Description)}</p>
                      <p>API Key ID: <code>{System.Net.WebUtility.HtmlEncode(mod.ApiKeyId)}</code></p>
                      <h3>Versions</h3>
                      <ul>{string.Join("", (versions).Select(v => $"<li>{System.Net.WebUtility.HtmlEncode(v.Version)} — <a href=\"{System.Net.WebUtility.HtmlEncode(v.DownloadUrl)}\">download</a> — {v.CreatedAt:O}</li>"))}</ul>
                      <hr/>
                      <h3>Rotate API key</h3>
                      <form method='post' action='/mods/{mod.Id}/rotate'>
                        <button type='submit'>Rotate (create new API key)</button>
                      </form>
                      <hr/>
                    <h3>Danger zone!</h3>
                      <button id=""delete-btn"" data-id=""{mod.Id}"">Delete mod</button>
                    </body>
                    <script>
                    document.getElementById(""delete-btn"").addEventListener(""click"", async (e) => {{
                      const id = e.target.dataset.id;

                      const response = await fetch(`/mods/{id}`, {{
                        method: ""DELETE"",
                        headers: {{
                          ""Content-Type"": ""application/json""
                        }}
                      }});
                        window.location.href = ""/mods""
                    }});
                    </script>
                    </html>";
                http.Response.ContentType = "text/html; charset=utf-8";
                await http.Response.WriteAsync(html);
            });

            app.MapDelete("/mods/{id:minlength(36)}", [Authorize] async (string id, HttpContext http, AppDbContext db) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var mod = await db.Mods.FindAsync(Guid.Parse(id));

                if (mod == null)
                {
                    http.Response.StatusCode = 404;
                    await http.Response.WriteAsync("Not found");
                    return;
                }

                if (mod.AuthorId != userId)
                {
                    http.Response.StatusCode = 403;
                    await http.Response.WriteAsync("Forbidden");
                    return;
                }

                db.Mods.Remove(mod);
                await db.SaveChangesAsync();

                http.Response.StatusCode = 200;
            });

            app.MapPost("/mods/{id:minlength(36)}/rotate", [Authorize] async (string id, HttpContext http, AppDbContext db) =>
            {
                var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var mod = await db.Mods.FindAsync(Guid.Parse(id));
                if (mod == null || mod.AuthorId != userId)
                {
                    http.Response.StatusCode = 404;
                    await http.Response.WriteAsync("Not found or not permitted");
                    return;
                }

                var apiKeyPlain = CryptoHelpers.GenerateApiKey();
                mod.ApiKeyHash = CryptoHelpers.HashString(apiKeyPlain);
                mod.ApiKeyId = Guid.NewGuid().ToString("n");
                await db.SaveChangesAsync();

                var html = $@"
                    <!doctype html><html><body style='font-family:Segoe UI,Arial;padding:16px;'>
                      <a href='/mods/{mod.Id}'>← Back</a>
                      <h2>New API key for {System.Net.WebUtility.HtmlEncode(mod.Name)}</h2>
                      <p>Save this key now — it won't be shown again:</p>
                      <pre style='background:#f3f3f3;padding:8px;border-radius:4px'>{System.Net.WebUtility.HtmlEncode(apiKeyPlain)}</pre>
                    </body></html>";
                http.Response.ContentType = "text/html; charset=utf-8";
                await http.Response.WriteAsync(html);
            });

            app.MapPost("/mods/{id:minlength(36)}/version", async (string id, HttpContext http, AppDbContext db) =>
            {
                var mod = await db.Mods.FindAsync(Guid.Parse(id));

                if (mod == null)
                {
                    http.Response.StatusCode = 404;
                    await http.Response.WriteAsync("Not found");
                    return;
                }

                if (!http.Request.Headers.TryGetValue("X-Api-Key", out var providedApiKey) || providedApiKey.FirstOrDefault() is not string apiKey)
                {
                    http.Response.StatusCode = 401;
                    await http.Response.WriteAsync("Missing or malformed API key");
                    return;
                }
                var providedApiKeyHash = CryptoHelpers.HashString(apiKey);

                if (providedApiKeyHash != mod.ApiKeyHash)
                {
                    http.Response.StatusCode = 403;
                    await http.Response.WriteAsync("Invalid API key");
                    return;
                }

                var newVersion = JsonSerializer.Deserialize<AddVersionRequestBody>(await new StreamReader(http.Request.Body).ReadToEndAsync());

                if (newVersion == null || string.IsNullOrEmpty(newVersion.Version) || string.IsNullOrEmpty(newVersion.DownloadUrl))
                {
                    http.Response.StatusCode = 400;
                    await http.Response.WriteAsync("Invalid request body");
                    return;
                }

                await db.Entry(mod).Reference(m => m.LatestVersion).LoadAsync();

                if (!Version.TryParse(newVersion.Version, out var parsedNewVersion) || (mod.LatestVersion is not null && Version.TryParse(mod.LatestVersion.Version, out var lastVersion) && parsedNewVersion <= lastVersion))
                {
                    http.Response.StatusCode = 400;
                    await http.Response.WriteAsync("New version cannot be a lower or equal version than last version");
                    return;
                }

                var version = new ModVersion
                {
                    Mod = mod,
                    Version = newVersion.Version,
                    DownloadUrl = newVersion.DownloadUrl
                };
                db.Versions.Add(version);
                mod.LatestVersion = version;
                await db.SaveChangesAsync();

                http.Response.StatusCode = 200;
                await http.Response.WriteAsync("Mod version updated successfully");
            });
        }
    }
}
