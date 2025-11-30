using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace AuthServer
{
    public static class Config
    {
        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client
                {
                    ClientId = "courses_platform_client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = {
                        "https://localhost:5001/signin-oidc",
                        "http://127.0.0.1:7890/callback",
                        "myapp://auth/callback"
                    },
                    PostLogoutRedirectUris = {
                        "https://localhost:5001/signout-callback-oidc",
                        "http://127.0.0.1:7890/callback",
                        "myapp://auth/callback"
                    },
                    AllowedScopes = { "openid", "profile", "api1", "roles", "offline_access" },
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = true,
                    AllowOfflineAccess = true,
                    AllowedCorsOrigins =
                    {
                        "https://localhost:5001",
                        "http://localhost:5005",
                        "https://localhost:5005",
                        "http://127.0.0.1:7890",
                        "https://127.0.0.1:7890",
                        "https://10.0.2.2:5000",
                        "http://10.0.2.2:5000"
                    }
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("api1", "Main API")
            };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", new[] { "role" })
            };
    }
}
