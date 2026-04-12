using System.Text;
using System.Text.Json.Serialization;
using ClaudeProjectBackend.Api.Middleware;
using ClaudeProjectBackend.Application;
using ClaudeProjectBackend.Application.Common.Interfaces;
using ClaudeProjectBackend.Infrastructure;
using ClaudeProjectBackend.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// ── Bootstrap logger — captures output before the host is built ───────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Claude Project Backend");

    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ──────────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services));

    // ── Controllers ───────────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Serialize enums as camelCase strings (e.g. UserRole.Admin → "admin")
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
        });

    // ── Application + Infrastructure ──────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Current user ──────────────────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // ── Authentication ────────────────────────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
        });

    builder.Services.AddAuthorization();

    // ── Exception handling ────────────────────────────────────────────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ── CORS ──────────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
            policy
                .WithOrigins(
                    builder.Configuration["Cors:AllowedOrigins"]!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries))
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
    });

    // ── Rate limiting ─────────────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("auth", config =>
        {
            config.PermitLimit = 10;
            config.Window = TimeSpan.FromMinutes(1);
        });

        options.AddFixedWindowLimiter("general", config =>
        {
            config.PermitLimit = 60;
            config.Window = TimeSpan.FromMinutes(1);
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // ── Swagger ───────────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Claude Project Backend",
            Version = "v1",
            Description = "REST API for Claude Project Web"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization — enter: Bearer {token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // ── Health checks ─────────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    // ─────────────────────────────────────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────────────────────────────────────

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
