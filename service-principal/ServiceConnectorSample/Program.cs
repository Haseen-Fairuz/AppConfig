﻿using Azure.Identity;
using ServiceConnectorSample;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from Azure App ConfigurationF
builder.Configuration.AddAzureAppConfiguration(options =>
{
    // Service Connector configured below environment variables at Azure WebApp's AppSetting already.
    string appConfigurationEndpoint = Environment.GetEnvironmentVariable("AZURE_APPCONFIGURATION_ENDPOINT");
    string clientId = Environment.GetEnvironmentVariable("AZURE_APPCONFIGURATION_CLIENTID");
    string clientSecret = Environment.GetEnvironmentVariable("AZURE_APPCONFIGURATION_CLIENTSECRET");
    string tenantId = Environment.GetEnvironmentVariable("AZURE_APPCONFIGURATION_TENANTID");
    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    if (!string.IsNullOrEmpty(appConfigurationEndpoint))
    {
        options.Connect(new Uri(appConfigurationEndpoint), credential)
               // Load all keys that start with `WebDemo:` and have no label
               .Select("SampleApplication:*")
               // Configure to reload configuration if the registered key 'SampleApplication:Settings:Messages' is modified.
               // Use the default cache expiration of 30 seconds. It can be overriden via AzureAppConfigurationRefreshOptions.SetCacheExpiration.
               .ConfigureRefresh(refreshOptions =>
               {
                   refreshOptions.Register("SampleApplication:Settings:Messages", refreshAll: true);
               });
    }
});

// Add services to the container.
builder.Services.AddRazorPages();

// Add Azure App Configuration and feature management services to the container.
builder.Services.AddAzureAppConfiguration();
//                .AddFeatureManagement();

// Bind configuration to the Settings object
builder.Services.Configure<Settings>(builder.Configuration.GetSection("SampleApplication:Settings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Use Azure App Configuration middleware for dynamic configuration refresh.
app.UseAzureAppConfiguration();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();