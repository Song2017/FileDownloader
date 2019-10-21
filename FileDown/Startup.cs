using Common;
using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Services;
using Services.Business;
using Services.Infrastructure;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace VASD
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Environment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; set; }

        private const string PublicPolicy = "CorsPolicy";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(Configuration);

            var key = Encoding.ASCII.GetBytes(Configuration.GetAppSetting("Secret"));
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            //Cors policy is added to controllers via [EnableCors("CorsPolicy")]
            //or .UseCors("CorsPolicy") globally
            services.AddCors(options =>
            {
                options.AddPolicy(PublicPolicy,
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddScoped<IDBDataService, DBDataService>();
            services.AddScoped<IDBManageService, DBManageService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IValveService, ValveService>();
            services.AddScoped<IReportService, ReportService>();

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetAppSetting("RedisServerConnection");
            });

            // enable custome cache
            services.AddSingleton<ICustomeCache, CustomeCache>();
            services.AddSingleton<IEncryptionService, EncryptionService>();

            services.AddSwaggerDocumentation();
            services.AddLocalization(l => l.ResourcesPath = "Resources");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile("logs\\log-{Date}.log",
                    retainedFileCountLimit: 7, shared: true)
                .CreateLogger();
            var logger = loggerFactory.CreateLogger<Startup>();

            if (env.IsDevelopment())
            {
                logger.LogInformation("Application is in Development.");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                    //Application level exception handler here - this is just a place holder
                    errorApp.Run(async (context) =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync("<html><body>\r\n");
                        await context.Response.WriteAsync(
                            "We're sorry, we encountered an un-expected issue with your application.<br>\r\n");
                        //Capture the exception
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            logger.LogError($"Error: {HtmlEncoder.Default.Encode(error.Error.Message)}");
                            await context.Response.WriteAsync(
                                $"<br>Error: {HtmlEncoder.Default.Encode(error.Error.Message)}<br>\r\n");
                        }
                        await context.Response.WriteAsync("<br><a href=\"/\">Home</a><br>\r\n</body></html>\r\n"
                                                          + new string(' ', 512)); // Padding for IE
                    })
                );
            }

            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-US", "en-US");
                options.SupportedCultures = AppConstants.SupportLanguages;
                options.SupportedUICultures = AppConstants.SupportLanguages;
            });
            app.Use(async (context, next) =>
            {
                // get culture name in cookies
                var cultureName = context.Request.Cookies["CulName"];

                if (!cultureName.IsNullOrEmptyOrSpace() && 
                    !context.Request.Headers[""].IsNullOrEmptyOrSpace())
                {
                    var culture = new CultureInfo(cultureName);
                    if (AppConstants.SupportLanguages.Contains(culture))
                    {
                        CultureInfo.CurrentCulture = culture;
                        CultureInfo.CurrentUICulture = culture;
                    }
                }

                await next();
            });

            app.UseAuthentication();

            // throttle function
            app.UseThrottleMiddleware();
            // swagger function
            app.UseSwaggerDocumentation();
            // add request log
            app.UseSerilogRequestLogging();

            // Apply CORS.
            app.UseCors(PublicPolicy);
            // login.html
            app.UseDefaultFiles();
            // For the wwwroot folder
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), AppConstants.DownloadFolderName)),
                RequestPath = AppConstants.DownloadFolderUrl
            });

            app.UseHttpsRedirection();

            // put last so header configs like CORS or Cookies etc can fire
            app.UseMvcWithDefaultRoute();
        }
    }
}
