using AntDesign;
using AntDesign.Locales;
using Blazored.Modal;
using Eccc.Sali;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.IO;
using System.IO.Compression;
using TABS.Audit;
using TABS.Data;
using TABS.Data.Auth;
using WebMarkupMin.AspNetCore5;

namespace TABS
{
    public class Startup
    {
        public readonly string DB_ENV = "";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
            services.AddAuthentication(IISServerDefaults.AuthenticationScheme);

            // Add Quartz services
            services.AddHostedService<QuartzHostedService>();
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            // Add module alert job
            services.AddSingleton<ModuleAlertJob>();
            services.AddSingleton<EmailDigestJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(ModuleAlertJob),
                cronExpression: "0 5 5 * * ?", // Every day at 5:05 AM
                //cronExpression: "0/10 * * * * ?", // Every 10 seconds - CAREFUL WHEN UNCOMMENTING THIS, ONLY USE FOR DEBUGGING !!!! 
                jobData: new()
                {
                    { "sendMail", ((bool)Configuration.GetValue(typeof(bool), "IsProd")) ? "true" : "false" },
                    { "env", (string)Configuration.GetValue(typeof(string), "ENV_URL") },
                }
            ));

            if ((bool)Configuration.GetValue(typeof(bool), "IsProd"))
            {
                // Only add the email digest job if this is a prod environment
                services.AddSingleton(new JobSchedule(
                   jobType: typeof(EmailDigestJob),
                   cronExpression: "0 5 15 * * ?", // Every day at 3:05 PM
                   //cronExpression: "0/10 * * * * ?", // Every 10 seconds - CAREFUL WHEN UNCOMMENTING THIS, ONLY USE FOR DEBUGGING!!!! 
                   jobData: new()
                   {
                       { "env", (string)Configuration.GetValue(typeof(string), "ENV_URL") }
                   }));
            }

            // Add DB entity services
            DBContextFactory.SetConnectionString(DB_ENV);
            services.AddScoped<AuditLogService>();
            services.AddScoped<AuthService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<DBContextFactory>();
            services.AddScoped<CrudService>();
            services.AddTransient<ICrudService, CrudService>();
            services.AddScoped<ApplicationService>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddScoped<UserService>();
            services.AddTransient<IUserService, UserService>();
            services.AddScoped<EmailService>();
            services.AddScoped<UserPreferenceService>();
            services.AddTransient<IUserPreferenceService, UserPreferenceService>();
            services.AddScoped<NotificationsService>();
            services.AddTransient<INotificationsService, NotificationsService>();
            services.AddScoped<SearchService>();
            services.AddTransient<ISearchService, SearchService>();
            services.AddSingleton<Global>();
            services.AddScoped<CookieProvider>();

            // Adding the db context since migrations will still need it
            services.AddDbContext<TABSDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString(DB_ENV))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddBlazoredModal();

            services.AddAntDesign();

            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddControllers();

            services.AddScoped<TABS.Shared.LayoutState>();
            services.AddScoped<TABS.Pages.Dashboard.WidgetState>();
            services.AddControllersWithViews();

            services.AddHttpContextAccessor();

            services.AddWebMarkupMin(
                options =>
                {
                    options.AllowMinificationInDevelopmentEnvironment = true;
                    options.AllowCompressionInDevelopmentEnvironment = true;
                })
                .AddHtmlMinification(
                    options =>
                    {
                        options.MinificationSettings.RemoveRedundantAttributes = true;
                        options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
                        options.MinificationSettings.RemoveHttpsProtocolFromAttributes = true;
                    })
                .AddHttpCompression();

            services.AddResponseCompression(o =>
            {
                o.Providers.Add<BrotliCompressionProvider>();
                o.EnableForHttps = true;
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            app.UseDeveloperExceptionPage();
            app.UseHsts();
            Sali.Initialize(Configuration.GetConnectionString(DB_ENV));

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseResponseCompression();
            app.UseWebMarkupMin();
            //app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
            var frCA = LocaleProvider.GetLocale("fr-FR");
            frCA = JsonConvert.DeserializeObject<Locale>(File.ReadAllText(@"./Locales/fr-CA.json"));
            frCA.LocaleName = "fr-CA";
            LocaleProvider.SetLocale("fr-CA", frCA);
            var supportedCultures = new[] { "en-US", "fr-CA" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<SignalRHub>(SignalRHub.HubUrl);
            });
        }
    }
}
