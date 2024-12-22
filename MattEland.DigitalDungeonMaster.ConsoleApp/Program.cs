﻿using MattEland.DigitalDungeonMaster.ClientShared;
using MattEland.DigitalDungeonMaster.ConsoleApp;
using MattEland.DigitalDungeonMaster.ConsoleApp.Menus;
using MattEland.DigitalDungeonMaster.ConsoleApp.Settings;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ServiceDiscovery;

// Create the host
HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Services.AddHostedService<DigitalDungeonMasterWorker>();
builder.AddServiceDefaults();

// Web communications
builder.Services.ConfigureHttpClientDefaults(http => http.AddServiceDiscovery());
builder.Services.Configure<ServiceDiscoveryOptions>(o => o.AllowAllSchemes = true);
builder.Services.AddScoped<ApiClient>();
builder.Services.Configure<ApiClientOptions>(o =>
{
    // If we're in Aspire, use the Aspire base URL. Service Discovery will substitute the endpoint of the local service here.
    if (args.Contains("--aspire", StringComparer.OrdinalIgnoreCase))
    {
        o.BaseUrl = "https+http://WebAPI";
    }
});

// Front-end menus
builder.Services.AddScoped<LoadGameMenu>();
builder.Services.AddScoped<NewGameMenu>();
builder.Services.AddScoped<LoginMenu>();
builder.Services.AddScoped<MainMenu>();
builder.Services.AddScoped<AdventureRunner>();

// Configuration options
builder.Services.Configure<UserSavedInfo>(c => builder.Configuration.Bind("UserInfo", c));
builder.Services.Configure<ServerSettings>(c => builder.Configuration.Bind("Server", c));

// Run the application
IHost app = builder.Build();
await app.RunAsync();