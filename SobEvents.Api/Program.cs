using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Scalar.AspNetCore;
using SobEvents.Infrastructure.Persistence.Context;
using SobEvents.Infrastructure.Persistence.SeedData;
using SobEvents.Application.Interfaces;
using SobEvents.Api.Middlewares;
using SobEvents.Api.Filters;
using SobEvents.Application.Commands.Events;
using SobEvents.Application.Behaviors;
using MediatR;
using FluentValidation;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SobEvents.Infrastructure.Services;
using SobEvents.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Identity;
using SobEvents.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SobEvents.Application.DTOs;
using SobEvents.Infrastructure.Identity;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

//di container validation (catch captive dependencies at startup)
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// controller service
builder.Services.AddControllers(options =>
{
    //register acgion filter globally for all ocntroller
    options.Filters.Add<AuditLogFilter>();
}
);


//  Named CORS Policy for Angular Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Local Angular Dev Server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Strictly required for SignalR and HttpOnly cookies!
    });
});

//Antiforgery Double-Submit Configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

 //api versioning 
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Rate Limiter Configuration
builder.Services.AddRateLimiter(options =>
{
    // Return honest 429 status code
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // 1. General Limiter for Browsing (Events & Ticket Types)
    options.AddTokenBucketLimiter(policyName: "general-limiter", opt =>
    {
        opt.TokenLimit = 20;               // Maximum capacity in bucket
        opt.QueueLimit = 0;                // Do not buffer requests; reject immediately
        opt.TokensPerPeriod = 5;           // Add 5 tokens...
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // ...every 10 seconds
        opt.AutoReplenishment = true;
    });

    // 2. Strict Limiter for Booking Reservations (Anti-Scalper Defense)
    options.AddTokenBucketLimiter(policyName: "booking-limiter", opt =>
    {
        opt.TokenLimit = 5;                // Maximum 5 rapid clicks
        opt.QueueLimit = 0;
        opt.TokensPerPeriod = 1;           // Add 1 token...
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // ...every 10 seconds
        opt.AutoReplenishment = true;
    });

    // Format the 429 response into standard RFC 7807 ProblemDetails
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
            Detail = "Rate limit exceeded. You are making requests too quickly. Please wait and try again.",
            Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
        };

        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
    };
});


// problemdetails & exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();



// dbcontext
builder.Services.AddDbContext<SobEventsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISobEventsDbContext>(provider => 
    provider.GetRequiredService<SobEventsDbContext>());


//JWT Options Pattern with Fail-on-Start Validation
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();


// Identity Configuration
builder.Services.AddDataProtection();
builder.Services.AddIdentityCore<AppUser>(options =>
{
    // password complexity rule
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;

    // Brute-force Lockout Defense
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole<int>>()
.AddEntityFrameworkStores<SobEventsDbContext>()
.AddDefaultTokenProviders();

//  JWT Bearer Authentication Configuration
var jwtConfig = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration section is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
        ClockSkew = TimeSpan.Zero // Strict expiration without 5-minute grace period
    };


    // Extracts JWT from incoming HttpOnly Cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("accessToken", out var cookieToken))
            {
                context.Token = cookieToken;
            }
            return Task.CompletedTask;
        }
    };
});



builder.Services.AddAuthorization();

// Token Service
builder.Services.AddScoped<ITokenService, TokenService>();


//  Register MediatR, FluentValidation, and Pipeline Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly);

      // 1. Logging Behavior runs first (measures total execution time)
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    
    // Register the Validation Behavior pipeline globally!
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateEventCommand).Assembly);    

// HybridCache with stampede protection
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

// service registrations
//  HttpContextAccessor & Scoped CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
//singletton so it can be shared across all request
builder.Services.AddSingleton<ITicketJobQueue, TicketJobQueue>();
// backgorund service
builder.Services.AddHostedService<TicketProcessingWorker>();
// builder.Services.AddScoped<IEventService, EventService>();
// builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
// builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

//  global exception middleware (must be at the top of the pipeline)
app.UseExceptionHandler();
app.UseStatusCodePages(); // translates 404s/401s into problemdetails automatically

//http request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Generates the JSON blueprint
    app.MapScalarApiReference(); // Draws Scalar UI
}

// run seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SobEventsDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await DbSeeder.SeedAsync(context, userManager, roleManager);
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AngularDevClient");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
