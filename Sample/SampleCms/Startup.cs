using EPiServer.Cms.Shell;
using EPiServer.Cms.Shell.UI;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.ContentApi.Core.DependencyInjection;
using EPiServer.DependencyInjection;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

using Geta.Optimizely.Sitemaps;

using Stott.Optimizely.RobotsHandler.Configuration;
using Stott.Security.Optimizely.Features.Configuration;

using OptiExternalBlocks.Features.Configuration;
using OptiExternalBlocks.Common;

namespace SampleCms;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data"));

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services.AddCmsAspNetIdentity<ApplicationUser>()
                .AddCms()
                .AddAdminUserRegistration(x => x.Behavior = RegisterAdminUserBehaviors.Enabled | RegisterAdminUserBehaviors.LocalRequestsOnly)
                .AddEmbeddedLocalization<Startup>();

        services.AddServerSideBlazor();

        services.AddSitemaps(x =>
        {
            x.EnableRealtimeSitemap = false;
            x.EnableRealtimeCaching = true;
            x.RealtimeCacheExpirationInMinutes = 60;
        });

        services.ConfigureContentApiOptions(o =>
        {
            o.IncludeInternalContentRoots = true;
            o.IncludeSiteHosts = true;
            // o.EnablePreviewFeatures = true; // optional
        });

        services.AddContentDeliveryApi();

        services.AddContentGraph();

        services.AddStottSecurity();
        services.AddRobotsHandler();

        // Configure External Content Blocks
        services.AddOptiExternalBlocks(options =>
        {
            options.ConnectionStringName = "EPiServerDB";
            options.AutoMigrate = true;
        }, authOptions =>
        {
            authOptions.AddPolicy(OptiExternalBlocksConstants.AuthorizationPolicy, policy =>
            {
                policy.RequireRole("WebAdmins", "CmsAdmins", "Administrators");
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStottSecurity();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapContent();
            endpoints.MapBlazorHub();
        });
    }
}
