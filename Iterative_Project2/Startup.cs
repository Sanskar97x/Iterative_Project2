using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Iterative_Project2.Infrastructures.Services;
using Iterative_Project2.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace Iterative_Project2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Connection string comes from User Secrets (ConnectionStrings:MyConnection)
            var connectionString = Configuration.GetConnectionString("MyConnection");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IUserService, UserService>();
            
            

            // CORS for the Angular dev server
            services.AddCors(options =>
            {
                options.AddPolicy("AngularDev", policy =>
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            // Retrieve JWT settings from User Secrets
            var jwtKey = Configuration["JWT:Key"];
            var jwtIssuer = Configuration["JWT:Issuer"];
            var jwtAudience = Configuration["JWT:Audience"];


            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException(
                    "JWT Key is missing.");

            if (string.IsNullOrWhiteSpace(jwtIssuer))
                throw new InvalidOperationException(
                    "JWT Issuer is missing.");

            if (string.IsNullOrWhiteSpace(jwtAudience))
                throw new InvalidOperationException(
                    "JWT Audience is missing.");

            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
                throw new InvalidOperationException(
                    "JWT Key must be at least 32 bytes.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey!)
                ),

                ClockSkew = TimeSpan.Zero
            };
        });

            services.AddHttpContextAccessor();

            // Authorization Configuration
            services.AddAuthorization();

            services.AddControllers();

            // Swagger with JWT "Authorize" button
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Iterative_Project2", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste your JWT token only (Swagger adds 'Bearer ' automatically)."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Must be first so it catches exceptions from everything below
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Iterative_Project2 v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AngularDev");      // after UseRouting, before auth
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseAuthentication();        // must come before UseAuthorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}