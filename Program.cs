using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ShopAPI.Authorization;
using ShopAPI.Authorization.Handlers;
using ShopAPI.Authorization.Requirements;
using ShopAPI.Infrastructure;
using ShopAPI.Middlewares;
using ShopAPI.Observability;
using ShopAPI.Repositories;
using ShopAPI.Response;
using ShopAPI.Services;
using ShopAPI.Settings;
using ShopAPI.Validators;
using System.Security.Claims;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var trustedProxyIps = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    foreach (var proxyIp in trustedProxyIps)
    {
        if (IPAddress.TryParse(proxyIp, out var address))
            options.KnownProxies.Add(address);
    }
});

// ============================================================
// Configuration (secrets come from User Secrets / environment variables, never from the repo)
// ============================================================

// JwtSettings is validated when the application STARTS, so a missing or weak
// key fails immediately with a clear message instead of on the first request.
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.Key),
        "JwtSettings:Key is missing. Set it with: dotnet user-secrets set \"JwtSettings:Key\" \"<random 32+ chars>\"")
    .Validate(s => s.Key is null || Encoding.UTF8.GetByteCount(s.Key) >= 32,
        "JwtSettings:Key must be at least 32 bytes long.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer), "JwtSettings:Issuer is missing.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Audience), "JwtSettings:Audience is missing.")
    .Validate(s => s.ExpiryMinutes > 0, "JwtSettings:ExpiryMinutes must be greater than zero.")
    .Validate(s => s.RefreshTokenExpiryDays > 0, "JwtSettings:RefreshTokenExpiryDays must be greater than zero.")
    .ValidateOnStart();

// ============================================================
// Services
// ============================================================

builder.Services.AddControllers(options => options.Filters.Add<AuditActionFilter>());

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        return new BadRequestObjectResult(
            ApiResponse<object>.FailureResponse("Validation failed.", errors.Count > 0 ? errors : new List<string> { "The request body is invalid." }));
    };
});

// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterPersonDtoValidation>();

// Infrastructure
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddHealthChecks()
    .AddCheck<ShopDbHealthCheck>("shopdb", tags: new[] { "ready" });

// Auditing: request threads only enqueue; a background service writes to SQL.
builder.Services.AddSingleton<AuditQueue>();
builder.Services.AddSingleton<IAuditStore, SqlAuditStore>();
builder.Services.AddSingleton<IAuditService, ChannelAuditService>();
builder.Services.AddHostedService<AuditBackgroundWriter>();

// Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Services
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ============================================================
// Authentication & Authorization
// ============================================================

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var subject = context.Principal?.FindFirst("sub")?.Value;
                if (!int.TryParse(subject, out var personId))
                {
                    context.Fail("The token does not contain a valid person id.");
                    return;
                }

                var people = context.HttpContext.RequestServices
                    .GetRequiredService<IPersonRepository>();
                var person = await people.GetByIdAsync(personId);

                if (person is null || !person.IsActive)
                    context.Fail("The account is inactive.");
            },
            OnChallenge = async context =>
            {
                context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Security.Authentication")
                    .LogWarning("JWT authentication challenge for {Path}", context.HttpContext.Request.Path);

                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(ApiResponse<string>.FailureResponse(
                    "Authentication required. Please provide a valid token."));
            },
            OnForbidden = async context =>
            {
                var http = context.HttpContext;
                var userId = http.User.FindFirst("sub")?.Value;

                http.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Security.Authorization")
                    .LogWarning("Authorization denied for {Path} and user {UserId}", http.Request.Path, userId ?? "anonymous");

                await http.RequestServices.GetRequiredService<IAuditService>().RecordAsync(new AuditEntry(
                    "Authorization.Forbidden", "Endpoint", null, "Failure", StatusCodes.Status403Forbidden,
                    int.TryParse(userId, out var forbiddenUserId) ? forbiddenUserId : null,
                    http.User.FindFirst(ClaimTypes.Role)?.Value,
                    http.Request.Method, http.Request.Path,
                    http.Connection.RemoteIpAddress?.ToString(),
                    http.Items[CorrelationIdMiddleware.HeaderName]?.ToString() ?? http.TraceIdentifier,
                    "Authorization denied"));

                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                await context.Response.WriteAsJsonAsync(ApiResponse<string>.FailureResponse(
                    "You do not have permission to access this resource."));
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.OrderOwner, policy =>
        policy.Requirements.Add(new OrderOwnerOrAdminRequirement(routeParamName: "id")));

    options.AddPolicy(PolicyNames.PersonOwner, policy =>
        policy.Requirements.Add(new PersonOwnerOrAdminRequirement("id")));

    options.AddPolicy(PolicyNames.AdminOnly, policy =>
        policy.RequireRole("Admin"));
});
builder.Services.AddScoped<IAuthorizationHandler, OrderOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PersonOwnerHandler>();

// ============================================================
// Swagger (Development only, see pipeline below)
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the access token only. Swagger adds the 'Bearer' prefix."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// ============================================================
// CORS - allowed origins come from configuration ("Cors:AllowedOrigins").
// With no origins configured, cross-origin browser calls are simply not allowed.
// ============================================================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ShopApiCorsPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            throw new InvalidOperationException("CORS AllowedOrigins must be configured.");
        }
    });
});

// ============================================================
// Rate limiting
// ============================================================
builder.Services.AddRateLimiter(options =>
{
    // Per IP only. (The previous "ip + phone from body" key required reading the request body
    // before rate limiting and let one IP try unlimited different phone numbers.)
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 0
            }));

    options.AddPolicy("RegisterPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromHours(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0
            }));

    options.AddPolicy("RefreshTokenPolicy", context =>
    {
        var key = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst("sub")?.Value
            : context.Connection.RemoteIpAddress?.ToString();

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: key ?? "anonymous",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 3,
                QueueLimit = 0
            });
    });

    options.AddPolicy("LogoutPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Caps how many orders are being created at the same moment across the whole API.
    options.AddConcurrencyLimiter("OrderCreationPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.QueueLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Admin operations: intentionally stricter than normal catalog reads.
    options.AddPolicy("AdminOperationsPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.FindFirst("sub")?.Value
                ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("PerUserPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.IsAuthenticated == true
                ? context.User.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous"
                : context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Rejected requests are only logged (cheap). Writing each one to SQL would let an
    // attacker turn the rate limiter itself into a database-write amplifier.
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Security.RateLimiting")
            .LogWarning("Rate limit rejected request {Path} from {IpAddress}",
                context.HttpContext.Request.Path,
                context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.FailureResponse("You have exceeded the allowed number of requests. Please try again later."),
            token);
    };
});

// ============================================================
// Pipeline
// ============================================================
var app = builder.Build();

app.UseForwardedHeaders();

_ = app.Services.GetRequiredService<IDbConnectionFactory>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Correlation and telemetry wrap the exception handler and all downstream components.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTelemetryMiddleware>();

app.UseCors("ShopApiCorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapControllers();

app.Run();

public partial class Program { }
