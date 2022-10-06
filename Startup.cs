using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using AspNet.Security.OAuth.LinkedIn;
using AspNet.Security.OAuth.Twitter;
using AspNet.Security.OAuth.Amazon;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;


namespace DoctorCeo
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        private IHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddRazorPages();
            services.AddControllersWithViews();

            if (string.IsNullOrEmpty(Configuration["Facebook:ClientId"]))
            {
                // User-Secrets: https://docs.asp.net/en/latest/security/app-secrets.html
                // See below for registration instructions for each provider.
                throw new InvalidOperationException("User secrets must be configured for each authentication provider.");
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = new PathString("/signin");
                    o.LogoutPath = new PathString("/signout");
                })
                .AddLinkedIn(o =>
                {
                    o.ClientId = Configuration["LinkedIn:ClientId"];
                    o.ClientSecret = Configuration["LinkedIn:ClientSecret"];
                    // your scopes may vary, see https://developer.linkedin.com/docs/oauth2#scopes
                    o.Scope.Add("r_liteprofile");
                    o.Scope.Add("r_emailaddress");
                    // o.Scope.Add("r_organization_social");
                    // o.Scope.Add("w_organization_social");
                    // o.Scope.Add("rw_organization_admin");
                    o.SaveTokens = true; //<-- this is the important line if you wat to use the tokens
                })
                .AddFacebook(o =>
                {
                    o.AppId = Configuration["Facebook:ClientId"];
                    o.AppSecret = Configuration["Facebook:ClientSecret"];
                    o.Scope.Add("email");
                    o.Fields.Add("name");
                    o.Fields.Add("email");
                    o.SaveTokens = true;
                    o.Events = new OAuthEvents()
                    {
                        OnRemoteFailure = HandleOnRemoteFailure
                    };
                })
                .AddGoogle(o =>
                {
                    o.ClientId = Configuration["google:ClientId"];
                    o.ClientSecret = Configuration["google:ClientSecret"];
                    o.AuthorizationEndpoint += "?prompt=consent"; // Hack so we always get a refresh token, it only comes on the first authorization response
                    o.AccessType = "offline";
                    o.SaveTokens = true;
                    o.Events = new OAuthEvents()
                    {
                        OnRemoteFailure = HandleOnRemoteFailure
                    };
                    o.ClaimActions.MapJsonSubKey("urn:google:image", "image", "url");
                    o.ClaimActions.Remove(ClaimTypes.GivenName);
                })
                .AddTwitter(o =>
                {
                    o.ClientId = Configuration["Twitter:ClientId"];
                    o.ClientSecret = Configuration["Twitter:ClientSecret"];
                    // Optionally request additional fields, if needed
                    o.Expansions.Add("pinned_tweet_id");
                    o.TweetFields.Add("text");
                    o.UserFields.Add("created_at");
                    o.UserFields.Add("pinned_tweet_id");
                })
                .AddAmazon(o =>
                {
                    o.ClientId = Configuration["Amazon:ClientId"];
                    o.ClientSecret = Configuration["Amazon:ClientSecret"];
                    // Optionally request the user's postal code, if needed
                    o.Scope.Add("postal_code");
                    o.Fields.Add("postal_code");
                });
        }
        /* Azure AD app model v2 has restrictions that prevent the use of plain HTTP for redirect URLs.
           Therefore, to authenticate through microsoft accounts, try out the sample using the following URL:
           https://localhost:44318/
        */
        // You must first create an app with Microsoft Account and add its ID and Secret to your user-secrets.
        // https://azure.microsoft.com/en-us/documentation/articles/active-directory-v2-app-registration/
        // https://apps.dev.microsoft.com/

        // MICROSOFT
        //         .AddMicrosoftAccount(o =>
        //         {
        //     o.ClientId = Configuration["microsoftaccount:Clientid"];
        //     o.ClientSecret = Configuration["microsoftaccount:Clientsecret"];
        //     o.SaveTokens = true;
        //     o.Scope.Add("offline_access");
        //     o.Events = new OAuthEvents()
        //     {
        //         OnRemoteFailure = HandleOnRemoteFailure
        //     };
        // })
        // You must first create an app with GitHub and add its ID and Secret to your user-secrets.
        // https://github.com/settings/applications/
        // https://docs.github.com/en/developers/apps/authorizing-oauth-apps
        //         .AddOAuth("GitHub", "Github", o =>
        //         {
        //     o.ClientId = Configuration["GitHub:ClientId"];
        //     o.ClientSecret = Configuration["GitHub:ClientSecret"];
        //     o.CallbackPath = new PathString("/signin-github");
        //     o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        //     o.TokenEndpoint = "https://github.com/login/oauth/access_token";
        //     o.UserInformationEndpoint = "https://api.github.com/user";
        //     o.ClaimsIssuer = "OAuth2-Github";
        //     o.SaveTokens = true;
        //     // Retrieving user information is unique to each provider.
        //     o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        //     o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
        //     o.ClaimActions.MapJsonKey("urn:github:name", "name");
        //     o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
        //     o.ClaimActions.MapJsonKey("urn:github:url", "url");
        //     o.Events = new OAuthEvents
        //     {
        //         OnRemoteFailure = HandleOnRemoteFailure,
        //         OnCreatingTicket = async context =>
        //         {
        //             // Get the GitHub user
        //             var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        //             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        //             request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //             var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
        //             response.EnsureSuccessStatusCode();

        //             using (var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
        //             {
        //                 context.RunClaimActions(user.RootElement);
        //             }
        //         }
        //     };
        // })
        //         // You must first create an app with GitHub and add its ID and Secret to your user-secrets.
        //         // https://github.com/settings/applications/
        //         // https://docs.github.com/en/developers/apps/authorizing-oauth-apps
        //         .AddOAuth("GitHub-AccessToken", "GitHub AccessToken only", o =>
        //         {
        //     o.ClientId = Configuration["github-token:Clientid"];
        //     o.ClientSecret = Configuration["github-token:Clientsecret"];
        //     o.CallbackPath = new PathString("/signin-github-token");
        //     o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        //     o.TokenEndpoint = "https://github.com/login/oauth/access_token";
        //     o.SaveTokens = true;
        //     o.Events = new OAuthEvents()
        //     {
        //         OnRemoteFailure = HandleOnRemoteFailure
        //     };
        // })
        // Identity --- https://demo.identityserver.io/
        // https://github.com/IdentityServer/IdentityServer4.Demo/blob/master/src/IdentityServer4Demo/Config.cs
        // .AddOAuth("IdentityServer", "Identity Server", o =>
        // {
        //     o.ClientId = "interactive.public";
        //     o.ClientSecret = "secret";
        //     o.CallbackPath = new PathString("/signin-identityserver");
        //     o.AuthorizationEndpoint = "https://demo.identityserver.io/connect/authorize";
        //     o.TokenEndpoint = "https://demo.identityserver.io/connect/token";
        //     o.UserInformationEndpoint = "https://demo.identityserver.io/connect/userinfo";
        //     o.ClaimsIssuer = "IdentityServer";
        //     o.SaveTokens = true;
        //     o.UsePkce = true;
        //     // Retrieving user information is unique to each provider.
        //     o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        //     o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        //     o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
        //     o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
        //     o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
        //     o.ClaimActions.MapJsonKey("email_verified", "email_verified");
        //     o.ClaimActions.MapJsonKey(ClaimTypes.Uri, "website");
        //     o.Scope.Add("openid");
        //     o.Scope.Add("profile");
        //     o.Scope.Add("email");
        //     o.Scope.Add("offline_access");
        //     o.Events = new OAuthEvents
        //     {
        //         OnRemoteFailure = HandleOnRemoteFailure,
        //         OnCreatingTicket = async context =>
        //         {
        //             // Get the user
        //             var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        //             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        //             request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //             var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
        //             response.EnsureSuccessStatusCode();

        //             using (var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
        //             {
        //                 context.RunClaimActions(user.RootElement);
        //             }
        //         }
        //     };
        // });


        private async Task HandleOnRemoteFailure(RemoteFailureContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body>");
            await context.Response.WriteAsync("A remote failure has occurred: <br>" +
                context.Failure.Message.Split(Environment.NewLine).Select(s => HtmlEncoder.Default.Encode(s) + "<br>").Aggregate((s1, s2) => s1 + s2));

            if (context.Properties != null)
            {
                await context.Response.WriteAsync("Properties:<br>");
                foreach (var pair in context.Properties.Items)
                {
                    await context.Response.WriteAsync($"-{HtmlEncoder.Default.Encode(pair.Key)}={HtmlEncoder.Default.Encode(pair.Value)}<br>");
                }
            }

            await context.Response.WriteAsync("<a href=\"/\">Home</a>");
            await context.Response.WriteAsync("</body></html>");

            // context.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(context.Failure.Message));

            context.HandleResponse();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.IsDevelopment())
            {
                // https obrigatório
                // use no PowerShell - C:\Users\rpbrasil> dotnet dev-certs https --trust
                app.UseDeveloperExceptionPage();
            }

            // Required to serve files with no extension in the .well-known folder
            var options = new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
            };

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            // Choose an authentication type
            app.Map("/signin", signinApp =>
            {
                signinApp.Run(async context =>
                {
                    var authType = context.Request.Query["authscheme"];
                    if (!string.IsNullOrEmpty(authType))
                    {
                        // By default the Client will be redirect back to the URL that issued the challenge (/login?authtype=foo),
                        // send them to the home page instead (/).                        
                        await context.ChallengeAsync(authType, new AuthenticationProperties() { RedirectUri = "https://doctorceo.online#portfolio" });
                        return;
                    }

                    var response = context.Response;
                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("Choose your best access provider: <br>");
                    var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                    foreach (var provider in await schemeProvider.GetAllSchemesAsync())
                    {
                        await response.WriteAsync("<a href=\"?authscheme=" + provider.Name + "\">" + (provider.DisplayName ?? "(suppressed)") + "</a><br>");
                    }
                    await response.WriteAsync("</body></html>");
                });
            });
            // Refresh the access token
            app.Map("/refresh_token", signinApp =>
            {
                signinApp.Run(async context =>
                {
                    var response = context.Response;

                    // Setting DefaultAuthenticateScheme causes User to be set
                    // var user = context.User;

                    // This is what [Authorize] calls
                    var userResult = await context.AuthenticateAsync();
                    var user = userResult.Principal;
                    var authProperties = userResult.Properties;

                    // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                    // var user = await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                    // Deny anonymous request beyond this point.
                    if (!userResult.Succeeded || user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                    {
                        // This is what [Authorize] calls
                        // The cookie middleware will handle this and redirect to /login
                        await context.ChallengeAsync();

                        // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                        // await context.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                        return;
                    }

                    var currentAuthType = user.Identities.First().AuthenticationType;
                    if (string.Equals(GoogleDefaults.AuthenticationScheme, currentAuthType)
                        || string.Equals(MicrosoftAccountDefaults.AuthenticationScheme, currentAuthType)
                        || string.Equals("IdentityServer", currentAuthType))
                    {
                        var refreshToken = authProperties.GetTokenValue("refresh_token");

                        if (string.IsNullOrEmpty(refreshToken))
                        {
                            response.ContentType = "text/html";
                            await response.WriteAsync("<html><body>");
                            await response.WriteAsync("No refresh_token is available.<br>");
                            await response.WriteAsync("<a href=\"/\">Home</a>");
                            await response.WriteAsync("</body></html>");
                            return;
                        }

                        var options = await GetOAuthOptionsAsync(context, currentAuthType);

                        var pairs = new Dictionary<string, string>()
                        {
                            { "Client_id", options.ClientId },
                            { "Client_secret", options.ClientSecret },
                            { "grant_type", "refresh_token" },
                            { "refresh_token", refreshToken }
                        };
                        var content = new FormUrlEncodedContent(pairs);
                        var refreshResponse = await options.Backchannel.PostAsync(options.TokenEndpoint, content, context.RequestAborted);
                        refreshResponse.EnsureSuccessStatusCode();

                        using (var payload = JsonDocument.Parse(await refreshResponse.Content.ReadAsStringAsync()))
                        {

                            // Persist the new acess token
                            authProperties.UpdateTokenValue("access_token", payload.RootElement.GetString("access_token"));
                            refreshToken = payload.RootElement.GetString("refresh_token");
                            if (!string.IsNullOrEmpty(refreshToken))
                            {
                                authProperties.UpdateTokenValue("refresh_token", refreshToken);
                            }
                            if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                            {
                                var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                                authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                            }
                            await context.SignInAsync(user, authProperties);

                            await PrintRefreshedTokensAsync(response, payload, authProperties);
                        }
                        return;
                    }
                    // https://developers.facebook.com/docs/facebook-login/access-tokens/expiration-and-extension
                    else if (string.Equals(FacebookDefaults.AuthenticationScheme, currentAuthType))
                    {
                        var options = await GetOAuthOptionsAsync(context, currentAuthType);

                        var accessToken = authProperties.GetTokenValue("access_token");

                        var query = new QueryBuilder()
                        {
                            { "grant_type", "fb_exchange_token" },
                            { "Client_id", options.ClientId },
                            { "Client_secret", options.ClientSecret },
                            { "fb_exchange_token", accessToken },
                        }.ToQueryString();

                        var refreshResponse = await options.Backchannel.GetStringAsync(options.TokenEndpoint + query);
                        using (var payload = JsonDocument.Parse(refreshResponse))
                        {
                            authProperties.UpdateTokenValue("access_token", payload.RootElement.GetString("access_token"));
                            if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                            {
                                var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                                authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                            }
                            await context.SignInAsync(user, authProperties);

                            await PrintRefreshedTokensAsync(response, payload, authProperties);
                        }
                        return;
                    }
                    else if (string.Equals(TwitterAuthenticationDefaults.AuthenticationScheme, currentAuthType))
                    {
                        var options = await GetOAuthOptionsAsync(context, currentAuthType);

                        var accessToken = authProperties.GetTokenValue("access_token");

                        var query = new QueryBuilder()
                        {
                            { "grant_type", "fb_exchange_token" },
                            { "Client_id", options.ClientId },
                            { "Client_secret", options.ClientSecret },
                            { "fb_exchange_token", accessToken },
                        }.ToQueryString();

                        var refreshResponse = await options.Backchannel.GetStringAsync(options.TokenEndpoint + query);
                        using (var payload = JsonDocument.Parse(refreshResponse))
                        {
                            authProperties.UpdateTokenValue("access_token", payload.RootElement.GetString("access_token"));
                            if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                            {
                                var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                                authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                            }
                            await context.SignInAsync(user, authProperties);

                            await PrintRefreshedTokensAsync(response, payload, authProperties);
                        }
                        return;
                    }

                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("Refresh has not been implemented for this provider.<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });
            // Sign-out to remove the user cookie.
            app.Map("/signout", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = "text/html";
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("You have been logged out. Goodbye " + context.User.Identity.Name + "<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });
            // Display the remote error
            app.Map("/error", errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("An remote failure has occurred: " + context.Request.Query["FailureMessage"] + "<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });

            app.Run(async context =>
            {
                // Setting DefaultAuthenticateScheme causes User to be set
                var user = context.User;

                // This is what [Authorize] calls
                // var user = await context.AuthenticateAsync();

                // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                // var user = await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                // Deny anonymous request beyond this point.
                if (user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                {
                    // This is what [Authorize] calls
                    // The cookie middleware will handle this and redirect to /login
                    await context.ChallengeAsync();

                    // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                    // await context.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                    return;
                }

                // Display user information
                var response = context.Response;
                response.ContentType = "text/html";
                await response.WriteAsync("<html><body>");
                await response.WriteAsync("Hello " + (context.User.Identity.Name ?? "anonymous") + "<br>");
                foreach (var claim in context.User.Claims)
                {
                    await response.WriteAsync(claim.Type + ": " + claim.Value + "<br>");
                }

                await response.WriteAsync("Tokens:<br>");

                await response.WriteAsync("Access Token: " + await context.GetTokenAsync("access_token") + "<br>");
                await response.WriteAsync("Refresh Token: " + await context.GetTokenAsync("refresh_token") + "<br>");
                await response.WriteAsync("Token Type: " + await context.GetTokenAsync("token_type") + "<br>");
                await response.WriteAsync("expires_at: " + await context.GetTokenAsync("expires_at") + "<br>");
                await response.WriteAsync("<a href=\"/signout\">Logout</a><br>");
                await response.WriteAsync("<a href=\"/refresh_token\">Refresh Token</a><br>");
                await response.WriteAsync("</body></html>");
            });
        }

        private Task<OAuthOptions> GetOAuthOptionsAsync(HttpContext context, string currentAuthType)
        {
            if (string.Equals(GoogleDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<GoogleOptions>>().Get(currentAuthType));
            }
            else if (string.Equals(MicrosoftAccountDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<MicrosoftAccountOptions>>().Get(currentAuthType));
            }
            else if (string.Equals(FacebookDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<FacebookOptions>>().Get(currentAuthType));
            }
            else if (string.Equals(LinkedInAuthenticationDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<LinkedInAuthenticationOptions>>().Get(currentAuthType));
            }
            else if (string.Equals(TwitterAuthenticationDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<TwitterAuthenticationOptions>>().Get(currentAuthType));
            }
            else if (string.Equals(AmazonAuthenticationDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<AmazonAuthenticationOptions>>().Get(currentAuthType));
            }
            else if (string.Equals("IdentityServer", currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<OAuthOptions>>().Get(currentAuthType));
            }

            throw new NotImplementedException(currentAuthType);
        }
        private async Task PrintRefreshedTokensAsync(HttpResponse response, JsonDocument payload, AuthenticationProperties authProperties)
        {
            response.ContentType = "text/html";
            await response.WriteAsync("<html><body>");
            await response.WriteAsync("Refreshed.<br>");
            await response.WriteAsync(HtmlEncoder.Default.Encode(payload.RootElement.ToString()).Replace(",", ",<br>") + "<br>");

            await response.WriteAsync("<br>Tokens:<br>");

            await response.WriteAsync("Access Token: " + authProperties.GetTokenValue("access_token") + "<br>");
            await response.WriteAsync("Refresh Token: " + authProperties.GetTokenValue("refresh_token") + "<br>");
            await response.WriteAsync("Token Type: " + authProperties.GetTokenValue("token_type") + "<br>");
            await response.WriteAsync("expires_at: " + authProperties.GetTokenValue("expires_at") + "<br>");

            await response.WriteAsync("<a href=\"/\">Home</a><br>");
            await response.WriteAsync("<a href=\"/refresh_token\">Refresh Token</a><br>");
            await response.WriteAsync("</body></html>");
        }
    }
}
