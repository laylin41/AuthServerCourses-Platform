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
                    RedirectUris = { "https://localhost:5001/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
                    AllowedScopes = { "openid", "profile", "api1", "roles" },
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = true
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
