using AuthNet.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services.AddAuthNet(options =>
{
    options.EnablePublicRegistration = false;
    options.PostgresConnectionString = builder.Configuration.GetConnectionString("AuthNet")
        ?? "Host=localhost;Port=5432;Database=authnet_package_consumer;Username=postgres;Password=postgres";
    options.UseDevelopmentEmailSender = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "AuthNet package consumer sample");
app.MapGet("/protected", () => "Authenticated package consumer endpoint")
    .RequireAuthorization();
app.MapRazorPages();
app.MapAuthNet();

app.Run();
