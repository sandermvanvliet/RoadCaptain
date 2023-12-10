// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Security.Claims;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting.Systemd;
using Serilog.Core;

namespace RoadCaptain.App.Web
{
    public class Program
    {
        internal static Logger? Logger;

        public static void Main(string[] args)
        {
            Logger = LoggerBootstrapper.CreateLogger();

            var monitoringEvents = new MonitoringEventsWithSerilog(Logger);

            var builder = WebApplication.CreateBuilder(args);
            
            builder.Host.UseSystemd();
            
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
                        monitoringEvents.Error(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
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
                    
                    configure.AddPolicy(
                        "AdministratorPolicy",
                        configurePolicy =>
                        {
                            configurePolicy.AuthenticationSchemes = new List<string> { JwtBearerDefaults.AuthenticationScheme} ;
                            configurePolicy.RequireAuthenticatedUser();
                            configurePolicy.RequireClaim("name", "Sander van Vliet [RoadCaptain]");
                        });
                });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            
            app.Lifetime.ApplicationStarted.Register(() => monitoringEvents.ApplicationStarted());
            app.Lifetime.ApplicationStopping.Register(() => monitoringEvents.ApplicationStopping());
            app.Lifetime.ApplicationStopped.Register(() => monitoringEvents.ApplicationStopped());

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}
