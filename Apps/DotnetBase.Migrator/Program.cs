using DotnetBase.Data;
using DotnetBase.Migrator.Script;
using DotnetBase.Migrator.Service;
using DotnetBase.Migrator.Service.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDotnetBaseData();

builder.Services.AddScoped<IMigrationHistoryService, MigrationHistoryService>();
builder.Services.AddScoped<IMigrationFileService, MigrationFileService>();
builder.Services.AddScoped<IMigrationExecutor, MigrationExecutor>();
builder.Services.AddScoped<IMigrationRunner, MigrationRunner>();

var app = builder.Build();

var runner = app.Services.GetRequiredService<IMigrationRunner>();

await runner.RunMigrationAsync();