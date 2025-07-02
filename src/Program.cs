using Microsoft.AspNetCore.Authentication.JwtBearer;
using FavoriteQuoutesWebApi.Models;
using FavoriteQuoutesWebApi.Storage;

var builder = WebApplication.CreateBuilder(args);
// Register in-memory stores for dependency injection
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
builder.Services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
builder.Services.AddSingleton<IBookStore, InMemoryBookStore>();
builder.Services.AddSingleton<IQuoteStore, InMemoryQuoteStore>();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddCors(
    options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {

                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
            else
            {
                policy
                    .WithOrigins("https://books-and-quotes-frontend.onrender.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        });
    }
);

// Build app
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline to use HTTPS redirection.
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngularApp");

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Add endpoints
app.MapControllers();

// Run app
app.Run();
