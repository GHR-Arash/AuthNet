using AuthNet.AspNetCore;
using AuthNet.Persistence.Postgres;
using AuthNet.SampleHost;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

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

var useInMemoryDatabase = SampleHostAuthNetPersistence.ShouldUseInMemoryDatabase(app.Environment, app.Configuration);
if (SampleHostAuthNetPersistence.ShouldApplyMigrations(app.Configuration, useInMemoryDatabase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
    db.Database.Migrate();
}

await SampleHostAdminBootstrap.BootstrapAsync(app.Services, app.Configuration);

app.MapStaticAssets();
app.MapAuthNet();

app.Run();
