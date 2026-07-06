using AuthNet.AspNetCore;
using AuthNet.Persistence.Postgres;
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
builder.Services.AddAuthNet(options =>
{
    builder.Configuration.GetSection("AuthNet").Bind(options);
    options.PostgresConnectionString = builder.Configuration.GetConnectionString("AuthNet");
});

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

if (app.Configuration.GetValue<bool>("AuthNet:ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
    db.Database.Migrate();
}

app.MapStaticAssets();
app.UseAuthNet();

app.Run();
