using DotnetBase.Authentication;
using DotnetBase.Data;
using DotnetBase.Shared.Extension;
using DotnetBase.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddValidation();
builder.Services.AddControllers();

builder.Services.AddDotnetBaseValidation();
builder.Services.AddDotnetBaseAuthentication();
builder.Services.AddDotnetBaseData();

var app = builder.Build();

app.MapControllers();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
