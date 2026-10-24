using Lifeline;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton(new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
});
builder.Services.AddDbContext<Database>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("debug", p =>
    {
        p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
    });
    options.AddPolicy("prod", p =>
    {
        p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("debug");
    app.UseFileServer();
}
else
{
    app.UseHttpsRedirection();
    app.UseCors("prod");
}

app.MapControllers();

app.Run();
