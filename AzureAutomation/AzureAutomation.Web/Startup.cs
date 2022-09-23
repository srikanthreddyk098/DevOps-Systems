//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Newtonsoft.Json.Serialization;

namespace AzureAutomation.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication(sharedOptions =>
            //    {
            //        sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            //    })
            //    .AddAzureAd(options => Configuration.Bind("AzureAd", options))
            //    .AddCookie();

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration);

            var builder = services.AddMvc(options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddRazorPagesOptions(options =>
                {
                    //options.Conventions.AllowAnonymousToFolder("/Account");
                    options.Conventions.AddPageRoute("/RestartVms", "");
                })
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
                .AddMicrosoftIdentityUI();

            if (Env.IsDevelopment()) {
                builder.AddRazorRuntimeCompilation();
            }

            services.AddSingleton<AzureVmPriceService>();

            //initialize custom security policies
            //services.AddAuthorization(options =>
            //{
            //    AdminAuthorizationPolicy.Init(Configuration["AdminGroupId"]);
            //    options.AddPolicy(AdminAuthorizationPolicy.Name, AdminAuthorizationPolicy.Build);
            //});

            services.AddAuthorization(policies =>
            {
                policies.AddPolicy("DevOpsAdmin", p => { p.RequireClaim("role", "DevOpsAdmin"); });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment()) {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            loggerFactory.AddLog4Net();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
