using api.Data;
using api.Interfaces;
using api.Middleware;
using api.Models;
using api.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using api.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPasswordHasher<Student>, PasswordHasher<Student>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

builder.Services.AddTransient<GlobalErrorHandling>();

builder.Services.AddDbContext<ApiDBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// Configure the HTTP request pipeline.

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = 
    options.DefaultChallengeScheme = 
    options.DefaultForbidScheme = 
    options.DefaultScheme =
    options.DefaultSignInScheme = 
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY") ?? throw new InvalidDataException("JWT secret key is missing or empty."))
        )
    };
});


builder.Services.AddCors(options => 
{
    options.AddPolicy("AndroidApp", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:6000");
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowCredentials();
    });

    options.AddPolicy("IosApp", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:6100");
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowCredentials();
    });

    options.AddPolicy("WebApp", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:6200");
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowCredentials();
    });

});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalErrorHandling>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors("AndroidApp");
app.UseCors("IosApp");
app.UseCors("WebApp");

app.Run();
