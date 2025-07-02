var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
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

// Add endpoints
app.MapControllers();

// Run app
app.Run();
