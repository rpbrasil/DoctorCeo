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
            //var allowOrigins = "AllowSpecificOrigins";
            services.AddRouting();
            services.AddControllersWithViews();
            // services.AddCors(options =>
            // {
            //     options.AddPolicy(name: allowOrigins,
            //        policy =>
            //        {
            //            policy.WithOrigins("https://localhost:7222", "https://doctorceo.azurewebsites.net").WithHeaders("Access-Control-Allow-Origin", "Content-Type").AllowAnyMethod();

            //        });
            // });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.LoginPath = new PathString("/signin");
                o.LogoutPath = new PathString("/signout");
            })
            .AddLinkedIn(o =>
            {
                o.ClientId = Configuration["LinkedIn:ClientId"];
                o.ClientSecret = Configuration["LinkedIn:ClientSecret"];
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
            })
            .AddGoogle(o =>
            {
                o.ClientId = Configuration["google:ClientId"];
                o.ClientSecret = Configuration["google:ClientSecret"];
                o.AuthorizationEndpoint += "?prompt=consent"; // Hack so we always get a refresh token, it only comes on the first authorization response
                o.AccessType = "offline";
                o.SaveTokens = true;
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
                //o.Scope.Add("postal_code");
                //o.Fields.Add("postal_code");
            });
            services.AddMvc().AddRazorPagesOptions(opt =>
            {
                opt.RootDirectory = "/views";
            });
        }
    
        public void Configure(IApplicationBuilder app)
        {
            if (HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var options = new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
            };

            // app.UseHsts();
            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            // after the UseRouting method and before the UseAuthorization method.
            // app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}

