using System.Diagnostics;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog.Core;

namespace RoadCaptain.App.Web
{
    public class Program
    {
        internal static Logger? Logger;

        public static void Main(string[] args)
        {
            Logger = LoggerBootstrapper.CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Add services to the container.
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder => InversionOfControl.ConfigureContainer(containerBuilder, Logger, builder.Configuration));

            builder
                .Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "";
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://secure.zwift.com/auth/realms/zwift";
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = "https://secure.zwift.com/auth/realms/zwift",
                        ValidateAudience = false
                    };
                    options.Events = new JwtBearerEvents();
                    options.Events.OnAuthenticationFailed += context =>
                    {
                        Debugger.Break();
                        return Task.CompletedTask;
                    };
                    options.Events.OnTokenValidated += context =>
                    {
                        Debugger.Break();
                        return Task.CompletedTask;
                    };
                });

            builder.Services.AddAuthorization(
                configure =>
                {
                    configure.AddPolicy(
                        "ZwiftUserPolicy",
                        configurePolicy =>
                        {
                            configurePolicy.AuthenticationSchemes = new List<string> { JwtBearerDefaults.AuthenticationScheme} ;
                            configurePolicy.RequireAuthenticatedUser();
                        });
                    
                });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            var monitoringEvents = app.Services.GetService<MonitoringEvents>();
            app.Lifetime.ApplicationStarted.Register(() => monitoringEvents.ApplicationStarted());
            app.Lifetime.ApplicationStopping.Register(() => monitoringEvents.ApplicationStopping());
            app.Lifetime.ApplicationStopped.Register(() => monitoringEvents.ApplicationStopped());

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}