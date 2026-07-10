using AuthNet.AspNetCore;
using AuthNet.SampleHost;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(
        builder.Environment.ContentRootPath,
        "App_Data",
        "DataProtectionKeys")));

builder.Services.AddRazorPages();
SampleHostAuthNetPersistence.AddAuthNet(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
await app.UseAuthNet(authNet =>
{
    authNet
        .ApplyMigrations(app.Configuration.GetValue<bool>("AuthNet:ApplyMigrations"))
        .InitialAdministrator(
            username: "admin",
            password: "Password1!",
            email: "admin@admin.com")
        .InitialAdministrator(app.Configuration.GetSection("AuthNet:InitialAdministrator"));
});

app.Run();
