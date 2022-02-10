using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Graph;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MentorApp.Graph;

namespace MentorApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddControllersWithViews();
        //}

        public void ConfigureServices(IServiceCollection services)
        {

            services
                // Use OpenId authentication
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
// Specify this is a web app and needs auth code flow
.AddMicrosoftIdentityWebApp(options =>
{
    Configuration.Bind("AzureAd", options);

    options.Prompt = "select_account";

    options.Events.OnTokenValidated = async context =>
    {
        var tokenAcquisition = context.HttpContext.RequestServices
            .GetRequiredService<ITokenAcquisition>();

        var graphClient = new GraphServiceClient(
            new DelegateAuthenticationProvider(async (request) =>
            {
                var token = await tokenAcquisition
                    .GetAccessTokenForUserAsync(GraphConstants.Scopes, user: context.Principal);
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            })
        );

        // Get user information from Graph
        var user = await graphClient.Me.Request()
            .Select(u => new
            {
                u.DisplayName,
                u.Mail,
                u.UserPrincipalName,
                u.MailboxSettings
            })
            .GetAsync();

        context.Principal.AddUserGraphInfo(user);

        // Get the user's photo
        // If the user doesn't have a photo, this throws
        try
        {
            var photo = await graphClient.Me
                .Photos["48x48"]
                .Content
                .Request()
                .GetAsync();

            context.Principal.AddUserGraphPhoto(photo);
        }
        catch (ServiceException ex)
        {
            if (ex.IsMatch("ErrorItemNotFound") ||
                ex.IsMatch("ConsumerPhotoIsNotSupported"))
            {
                context.Principal.AddUserGraphPhoto(null);
            }
            else
            {
                throw;
            }
        }
    };

    options.Events.OnAuthenticationFailed = context =>
    {
        var error = WebUtility.UrlEncode(context.Exception.Message);
        context.Response
            .Redirect($"/Home/ErrorWithMessage?message=Authentication+error&debug={error}");
        context.HandleResponse();

        return Task.FromResult(0);
    };

    options.Events.OnRemoteFailure = context =>
    {
        if (context.Failure is OpenIdConnectProtocolException)
        {
            var error = WebUtility.UrlEncode(context.Failure.Message);
            context.Response
                .Redirect($"/Home/ErrorWithMessage?message=Sign+in+error&debug={error}");
            context.HandleResponse();
        }

        return Task.FromResult(0);
    };
})
                // Add ability to call web API (Graph)
                // and get access tokens
                .EnableTokenAcquisitionToCallDownstreamApi(options =>
                {
                    Configuration.Bind("AzureAd", options);
                }, GraphConstants.Scopes)
// Add a GraphServiceClient via dependency injection
.AddMicrosoftGraph(options =>
{
    options.Scopes = string.Join(' ', GraphConstants.Scopes);
})


                // Use in-memory token cache
                // See https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization
                .AddInMemoryTokenCaches();

            //Require authentication
            //services.AddControllersWithViews(options =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();
            //    options.Filters.Add(new AuthorizeFilter(policy));
            //})
            services.AddControllersWithViews()
            // Add the Microsoft Identity UI pages for signin/out
            .AddMicrosoftIdentityUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}


/*
 * Application (client) ID       fed0f06e-b43a-4bbe-ae7a-40f36f8e17f4
 * 
 * 
 *  Client secrets    88iA~_VCCT6I8~6cy_vhWpJtl24aO_lw~3
 *  
 *  
 *  
 *  
 *  
 *  
 *  TTOKEN
 *  
 *  eyJ0eXAiOiJKV1QiLCJub25jZSI6Ikd1XzlHd1V2c2hzajN4Q1B5LVY1NWxJcU9LRHVHQUhKTWJGSHdPeTRkUVEiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyIsImtpZCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9iNzM5NTBmYi0xNjI1LTRiMzgtODIwYy1hZGJlOGIxZWFlMmUvIiwiaWF0IjoxNjI1NjU3OTcxLCJuYmYiOjE2MjU2NTc5NzEsImV4cCI6MTYyNTY2MTg3MSwiYWNjdCI6MCwiYWNyIjoiMSIsImFjcnMiOlsidXJuOnVzZXI6cmVnaXN0ZXJzZWN1cml0eWluZm8iLCJ1cm46bWljcm9zb2Z0OnJlcTEiLCJ1cm46bWljcm9zb2Z0OnJlcTIiLCJ1cm46bWljcm9zb2Z0OnJlcTMiLCJjMSIsImMyIiwiYzMiLCJjNCIsImM1IiwiYzYiLCJjNyIsImM4IiwiYzkiLCJjMTAiLCJjMTEiLCJjMTIiLCJjMTMiLCJjMTQiLCJjMTUiLCJjMTYiLCJjMTciLCJjMTgiLCJjMTkiLCJjMjAiLCJjMjEiLCJjMjIiLCJjMjMiLCJjMjQiLCJjMjUiXSwiYWlvIjoiRTJaZ1lBaWI0L2I4OWI1dXRxdnI2MC9rbldYNzhOaXljeU1Mei8rV3dtemV1UFVSVi9rQSIsImFtciI6WyJwd2QiXSwiYXBwX2Rpc3BsYXluYW1lIjoiTWVudG9yQXBwIiwiYXBwaWQiOiJmZWQwZjA2ZS1iNDNhLTRiYmUtYWU3YS00MGYzNmY4ZTE3ZjQiLCJhcHBpZGFjciI6IjEiLCJmYW1pbHlfbmFtZSI6IkRlbWlyIiwiZ2l2ZW5fbmFtZSI6IlXEn3VyIiwiaWR0eXAiOiJ1c2VyIiwiaXBhZGRyIjoiNDYuMi4xMzQuMTk1IiwibmFtZSI6IlXEn3VyIERlbWlyIChCaWxnZUFkYW0gQm9vc3QpIiwib2lkIjoiY2I3ODRjN2UtOTAyZi00MWY3LWJjYmUtMzAwYzJhZTFkNGU3IiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAxMDgxQ0NDNjUiLCJyaCI6IjAuQVRFQS0xQTV0eVVXT0V1Q0RLMi1peDZ1TG03dzBQNDZ0TDVMcm5wQTgyLU9GX1F4QUlBLiIsInNjcCI6IkNhbGVuZGFycy5SZWFkV3JpdGUgTWFpbGJveFNldHRpbmdzLlJlYWQgb3BlbmlkIHByb2ZpbGUgVXNlci5SZWFkIGVtYWlsIiwic3ViIjoibnYyR0xHVWhQRVhwaTI3NlF2MlpPWlM3RF9ia3VpdDEtRllHaTNvVk90TSIsInRlbmFudF9yZWdpb25fc2NvcGUiOiJFVSIsInRpZCI6ImI3Mzk1MGZiLTE2MjUtNGIzOC04MjBjLWFkYmU4YjFlYWUyZSIsInVuaXF1ZV9uYW1lIjoidWd1ci5kZW1pckBiaWxnZWFkYW1ib29zdC5jb20iLCJ1cG4iOiJ1Z3VyLmRlbWlyQGJpbGdlYWRhbWJvb3N0LmNvbSIsInV0aSI6ImZPSndtMHV2SVUtYVo5YkE1RmZOQUEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbImI3OWZiZjRkLTNlZjktNDY4OS04MTQzLTc2YjE5NGU4NTUwOSJdLCJ4bXNfc3QiOnsic3ViIjoiXzdoMTJxaXVjMFo0QUxoNDJZcnhwWVZ0SHlIWVo0WjdEdDdlWnZaN1dRUSJ9LCJ4bXNfdGNkdCI6MTUyNDY0MzU3NH0.eOgY3lskJgkD2Azx-0ayNBnswc6_-Kf96Utie0utMPFG3j1VM3Zvt45jAwZlKXu41Vk87OJ6WipDMKRFEVPhSSu0hNawH-quV5t8wyZc951SBX4TgW86WWiFs6BLsVjiA07_yz5SaBgHT6z9uCkIZcyqjMUylzeaerQv0fA1K1qFUEE1vKDDcZ2fPyZqGzGI7ZNKRKzlB7cHGbeIYWqNbyajied00xTFZtkXqYjhkMGsQ3cQxm9WvbwGsHYTFnsjkpnxz_VAaPhaSbAIwSe2jggXPSCws8AMzD1selIQlkP-I8ol8VuOJTR1CKnU7YSpGM4VoMlAlfH2oNPMaFVm2g
 *  
 *  
 *  
 */

