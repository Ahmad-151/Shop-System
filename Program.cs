using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ShopAPI.Authorization;
using ShopAPI.Authorization.Handlers;
using ShopAPI.Authorization.Requirements;
using ShopAPI.FluentValidation;
using ShopAPI.MiddelWares;
using ShopAPI.Repositories;
using ShopAPI.Services;
using ShopAPI.Settings;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterPersonDtoValidation>();

// Dependency Injection - Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Dependency Injection - Services
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Authorization :
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JwtSettings section is missing from configuration. Add it to appsettings.json or User Secrets.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter The Token By Formula : Bearer {token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// Define Cors
builder.Services.AddCors(options =>
{
   options.AddPolicy("ShopApiCorsPolicy", policy =>
   {
       policy
           .WithOrigins(
               "https://localhost:7226",
               "http://localhost:5235"
           )
           .AllowAnyHeader()
           .AllowAnyMethod();
   });
});


var app = builder.Build();

app.UseHttpsRedirection();

// MiddleWares
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Apply Cors - It Must be before MapControllers
app.UseCors("ShopApiCorsPolicy");

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
