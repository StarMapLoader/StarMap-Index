using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;

namespace StarMapIndex.Helpers
{
    public static class OAuthEventHandles
    {
        public static void GitHubConfigureOptions(OAuthOptions options)
        {
            options.ClientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID") ?? "";
            options.ClientSecret = Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET") ?? "";
            options.CallbackPath = "/signin-github";

            options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            options.TokenEndpoint = "https://github.com/login/oauth/access_token";
            options.UserInformationEndpoint = "https://api.github.com/user";

            options.SaveTokens = true;

            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
            options.ClaimActions.MapJsonKey("avatar_url", "avatar_url");

            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Accept.ParseAdd("application/json");
                    request.Headers.UserAgent.ParseAdd("StarMapApp/1.0");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

                    var resp = await context.Backchannel.SendAsync(request);
                    resp.EnsureSuccessStatusCode();
                    var payload = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;

                    context.RunClaimActions(payload);
                },
                OnTicketReceived = context => Task.CompletedTask
            };
        }
    }
}
