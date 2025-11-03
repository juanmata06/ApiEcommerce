using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));

//* Cache config:
builder.Services.AddResponseCaching(options =>
{
  options.MaximumBodySize = 1024 * 1024; //* => 1mb
  options.UseCaseSensitivePaths = true;
});

//* My repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddRouting(options => options.LowercaseUrls = true); //* Displays routes as lower case

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddControllers(option =>
{
  option.CacheProfiles.Add(CacheProfiles.Default10, new CacheProfile { Duration = 10 });
  option.CacheProfiles.Add(CacheProfiles.Default20, new CacheProfile { Duration = 20 });
});

//* Endpoints auth with Bearer Tokens config
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
var Issuer = builder.Configuration.GetValue<string>("ApiSettings:Issuer");
var Audience = builder.Configuration.GetValue<string>("ApiSettings:Audience");
if (string.IsNullOrEmpty(secretKey))
{
  throw new InvalidOperationException("SecretKey has not been configured");
}
builder.Services.AddAuthentication(opts =>
{
  opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
  opts.RequireHttpsMetadata = false;
  opts.SaveToken = true;
  opts.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    ValidIssuer = Issuer,
    ValidAudience = Audience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    ValidateIssuer = false,
    ValidateAudience = true
  };
});

//* Api versioning config
var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
  option.AssumeDefaultVersionWhenUnspecified = true;
  option.DefaultApiVersion = new ApiVersion(1, 0);
  option.ReportApiVersions = true;
  // option.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));
});

apiVersioningBuilder.AddApiExplorer(option =>
{
  option.GroupNameFormat = "'v'VVV"; //* Ex: v1, v2, v3...
  option.SubstituteApiVersionInUrl = true; //* Ex: api/v{version}/categories
});

//* CORS config
builder.Services.AddCors(opts =>
    {
      opts.AddPolicy(PolicyNames.AllowSpecificOrigin,
          builder =>
          {
            builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
          }

      );
    }
);

//* Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
  options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                    "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                    "Ejemplo: \"12345abcdef\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });
    options.SwaggerDoc("v1", new OpenApiInfo
    {
      Version = "v1",
      Title = "API Ecomerce",
      Description = "API for managing products and users",
      TermsOfService = new Uri("https://guzzo.dev/"),
      Contact = new OpenApiContact
      {
        Name = "Juan Mata",
        Url = new Uri("https://guzzo.dev/"),
      },
      License = new OpenApiLicense
      {
        Name = "Use license",
        Url = new Uri("https://guzzo.dev/"),
      }
    });
  }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  // app.UseSwaggerUI(options =>
  // {
  //   options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
  // });
}

app.UseHttpsRedirection();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
