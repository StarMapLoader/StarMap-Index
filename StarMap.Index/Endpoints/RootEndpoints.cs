namespace StarMapIndex.Endpoints
{
    public static class RootEndpoints
    {
        public static void MapRootEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", async (HttpContext http) =>
            {
                var user = http.User?.Identity?.IsAuthenticated == true ? http.User.Identity.Name : null;
                var loginUrl = "/signin";
                var logoutUrl = "/signout";

                var html = $@"
                    <!doctype html>
                    <html>
                    <head><meta charset='utf-8'><title>StarMap</title></head>
                    <body style='font-family:Segoe UI,Arial;margin:0;padding:0;'>
                      <header style='display:flex;align-items:center;justify-content:space-between;padding:12px 20px;border-bottom:1px solid #ddd;'>
                        <div style='font-weight:700;font-size:1.2rem;'>StarMap</div>
                        <div>{(user == null ? $"<a href=\"{loginUrl}\">Login with GitHub</a>" : $"Signed in as <strong>{user}</strong> — <a href=\"{logoutUrl}\">Sign out</a> | <a href=\"/mods\">Your Mods</a>")}</div>
                      </header>
                      <main style='padding:20px;'>
                        <h2>Welcome to StarMap</h2>
                        <p>Nothing here yet. Use the login to manage mods.</p>
                      </main>
                    </body>
                    </html>";
                http.Response.ContentType = "text/html; charset=utf-8";
                await http.Response.WriteAsync(html);
            });
        }
    }
}
