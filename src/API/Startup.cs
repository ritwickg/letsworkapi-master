using LetsWork.Domain.Interfaces.Repositories;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using LetsWork.Infrastructure.Data;
using LetsWork.Infrastructure.ServiceManagers;
using LetsWork.Infrastructure.Services;
using LetsWork.API.Extensions;
using LetsWork.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;
using NLog.Web;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

namespace LetsWork.API
{
    public class Startup
    {
        public IConfiguration _configuration { get; }
        public IHostingEnvironment _env { get; }

        public Startup(IHostingEnvironment HostingEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(HostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            this._configuration = builder.Build();
            this._env = HostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //====================================DB Context Registration======================================
            services.AddDbContext<LetsWorkDbContext>(options =>
            {
                options.UseSqlServer(_configuration["ConfigSettings:SQLProvider:ConnectionString"], sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly("Infrastructure");
                });
            });

            //==================================CORS Policy==========================================================
            services.ConfigureCors();

            //============Register our strongly typed application configuration options===============================
            services.Configure<ConfigSettings>(options => _configuration.GetSection("ConfigSettings").Bind(options));

            //===================Registering ASP .NET Core Identity classes===========================================
            services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<LetsWorkDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            });

            //================================JWT Token Validation=========================================
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = false;
                    cfg.Audience = _configuration["ConfigSettings:JWT:Issuer"];
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = _configuration["ConfigSettings:JWT:Issuer"],
                        ValidAudience = _configuration["ConfigSettings:JWT:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["ConfigSettings:JWT:Secret"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                })
                .AddGoogle(options =>
                {
                    options.ClientId = _configuration["ConfigSettings:GoogleAuth:ClientID"];
                    options.ClientSecret = _configuration["ConfigSettings:GoogleAuth:ClientSecret"];
                });


            //==============================Swagger Configuration===================================================
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Visneto API v1", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() }
                });

                // Include comments for swagger xml documentation
                foreach (string name in Directory.GetFiles(_env.ContentRootPath, "*.XML", SearchOption.AllDirectories))
                    c.IncludeXmlComments(name);
            });


            //=============================Resolving services for dependency injection==============================

            services.AddScoped<IAsyncRepository<Booking>,BookingRepository>();
            services.AddScoped<IAsyncRepository<ProfileImage>,ProfileImageRepository>();
            services.AddScoped<IAsyncRepository<ReferralCode>, ReferralCodeRepository>();
            services.AddScoped<IAsyncRepository<ReferralCodeTransaction>, ReferralCodeTransactionRepository>();
            services.AddScoped<IAsyncRepository<VenueDetail>, VenueRepository>();
            services.AddScoped<IAsyncRepository<VenueImage>, VenueImageRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IVenueService, VenueService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ITokenProviderService, TokenProviderService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IReferralCodeService, ReferralCodeService>();
            services.AddScoped<IProfileService, ProfileService>();

            //============================Global model state error handling due to APIController attribute in ASP .NET Core 2.1================
            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    string modelStateErrorMessage = string.Join(Environment.NewLine, (actionContext.ModelState
                                              .Where(e => e.Value.Errors.Count > 0)
                                              .Select(e => e.Value.Errors.First().ErrorMessage)));

                    return new ContentResult
                    {
                        Content = JsonConvert.SerializeObject(new { message = modelStateErrorMessage }),
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                };
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        options.SerializerSettings.Formatting = Formatting.Indented;
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, LetsWorkDbContext LetsWorkDBContext, IConfiguration Configuration, ILoggerFactory LoggerFactory)
        {
            //Create DB when app is spun up
            LetsWorkDBContext.Database.EnsureCreated();

            //Seed database with initial values
            CreateUserRoles(serviceProvider);

            //Refer the middleware order from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.1#order
            //1) CORS middleware
            app.UseCors("CorsPolicy");

            //2) Global Exception middleware
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMiddleware<ExceptionMiddleware>();
            }
            else
            {
                app.UseMiddleware<ExceptionMiddleware>();
                app.UseHsts();
            }

            //3) Logging middleware
            // add NLog to ASP.NET Core
            LoggerFactory.AddNLog();

            //needed for non-NETSTANDARD platforms: configure nlog.config in your project root. NB: you need NLog.Web.AspNetCore package for this. 
            env.ConfigureNLog("nlog.config");

            //4) ASP .NET Core Identity middleware
            app.UseAuthentication();

            app.UseStaticFiles();

            //5) Swagger metadata middleware
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Visneto API v1");
                c.DocumentTitle = "Visneto API v1 Documentation";
                c.DocExpansion(DocExpansion.None);
                c.RoutePrefix = string.Empty;
            });

            //6) HTTPS redirection middleware
            app.UseHttpsRedirection();

            //7) Registering MVC and API Controllers middleware
            app.UseMvc();
        }

        #region Seed Logic
        private void CreateUserRoles(IServiceProvider serviceProvider)
        {
            RoleManager<ApplicationRole> roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IdentityResult roleResult;
            //Adding Admin Role

            bool adminRoleCheck = roleManager.RoleExistsAsync(UserType.Admin.ToString()).GetAwaiter().GetResult();
            bool userRoleCheck = roleManager.RoleExistsAsync(UserType.User.ToString()).GetAwaiter().GetResult();

            //create admin role if its already not present and seed it to the database
            if (!adminRoleCheck)
                roleResult = roleManager.CreateAsync(new ApplicationRole { Name = UserType.Admin.ToString() }).GetAwaiter().GetResult();

            //create user role if its already not present and seed it to the database
            if (!userRoleCheck)
                roleManager.CreateAsync(new ApplicationRole { Name = UserType.User.ToString() }).GetAwaiter().GetResult();

            //Assign Admin role to the main User here we have given our newly registered 
            //login id for Admin management
            ApplicationUser user = userManager.FindByEmailAsync($"letsworkproject2018@gmail.com").GetAwaiter().GetResult();

            if (user == null)
            {
                ApplicationUser rootAdmin = new ApplicationUser
                {
                    UserName = "admin1",
                    Email = $"letsworkproject2018@gmail.com",
                    FirstName = "Admin",
                    LastName = "Admin",
                    PhoneNumber = "1234567890",
                    EmailConfirmed = true
                };

                IdentityResult result = userManager.CreateAsync(rootAdmin, "Admin@1234").GetAwaiter().GetResult();
                userManager.AddToRoleAsync(rootAdmin, UserType.Admin.ToString()).GetAwaiter().GetResult();
            }
        }
        #endregion
    }
}
