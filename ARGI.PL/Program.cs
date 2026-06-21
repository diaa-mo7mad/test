using ARGI.BLL.Service;
using ARGI.DAL.Data;
using ARGI.DAL.Models;
using ARGI.DAL.Repository;
using ARGI.DAL.Utils;
using ARGI.PL.BackgroundServices;
using ARGI.PL.Hubs;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Globalization;
using System.Text;

namespace ARGI.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {

                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "add the token without bearer"
                    };


                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes.Add("Bearer", securityScheme);


                    document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
                    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

                    return Task.CompletedTask;
                });
            });
            builder.Services.AddScoped<ISeedData, RoleSeedData>();
            builder.Services.AddScoped<IDomeRepository, DomeRepository>();
            builder.Services.AddScoped<IDomeService, DomeService>();
            builder.Services.AddScoped<ISensorRepository, SensorRepository>();
            builder.Services.AddScoped<ISensorService, SensorService>();
            builder.Services.AddScoped<IIrrigationRepository, IrrigationRepository>();
            builder.Services.AddScoped<IIrrigationService, IrrigationService>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAiInsightService, AiInsightService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
            builder.Services.AddHostedService<IrrigationWorker>();
          


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(Options =>
            {

                Options.Password.RequireDigit = true;
                Options.Password.RequireLowercase = true;
                Options.Password.RequireNonAlphanumeric = true;
                Options.Password.RequireUppercase = true;
                Options.Password.RequiredLength = 6;
                Options.User.RequireUniqueEmail = true;
                Options.Lockout.MaxFailedAccessAttempts = 5;
                Options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                Options.SignIn.RequireConfirmedAccount = true;

            })
    .AddEntityFrameworkStores<ApplicationDbContext>()
                     .AddDefaultTokenProviders();


       
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
                };
            });

            var app = builder.Build();
            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.WithTitle("Argidome API v1")
                           .WithTheme(ScalarTheme.Moon)
                           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                           .AddPreferredSecuritySchemes("Bearer");
                });

            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {

                var Services = scope.ServiceProvider;
                var seeders = Services.GetServices<ISeedData>();
                foreach (var seeder in seeders)
                {
                    await seeder.DataSeed();

                }


                app.MapControllers();
                app.MapHub<DomeHub>("/hubs/dome");

                app.Run();
            }
        }
    }
}