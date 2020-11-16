using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chat.Hubs;
using Microsoft.Extensions.Configuration;
using ChatApp.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StackExchange.Redis;
using System.Net;

namespace ChatApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration _Configuration)
        {
            Configuration = _Configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddSignalR().AddMessagePackProtocol();
            /*.AddStackExchangeRedis(options =>
            {
                options.ConnectionFactory = async writer =>
                {
                    var config = new ConfigurationOptions
                    {
                        EndPoints = { { Configuration.GetValue<string>("Redis:Host"), Configuration.GetValue<int>("Redis:Port") } },
                        AllowAdmin = Configuration.GetValue<bool>("Redis:AllowAdmin"),
                        ConnectTimeout = Configuration.GetValue<int>("Redis:ConnectTimeout"),
                    };

                    var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
                    connection.ConnectionFailed += (_, e) =>
                    {
                        Console.WriteLine($"Connection to Redis failed: {e?.Exception?.Message}");
                    };

                    if (!connection.IsConnected)
                    {
                        Console.WriteLine("Did not connect to Redis.");
                    }

                    return connection;
                };
            });*/

            services.AddControllers();
            services.AddHealthChecks();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer($"{Configuration.GetConnectionString("DefaultConnection")}")
            );

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))))
                        };

                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];

                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) &&
                                    (path.StartsWithSegments("/chat")))
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });


            services.AddScoped<Func<AppDbContext>>(options => () => options.GetRequiredService<AppDbContext>());

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder
                                  .AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader());

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chat");
                endpoints.MapHealthChecks("/");
            });

        }
    }
}
