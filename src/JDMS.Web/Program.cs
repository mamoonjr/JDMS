using JDMS.Application.Constants;
using JDMS.Infrastructure;
using JDMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Render وغيره: الاستماع على المنفذ الذي يحدده المنصة (PORT)
var renderPort = Environment.GetEnvironmentVariable("PORT");
var isRender = !string.IsNullOrWhiteSpace(renderPort);
if (isRender)
    builder.WebHost.UseUrls($"http://+:{renderPort.Trim()}");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: true);

try
{
    var mysqlPreview = MySqlConnectionHelper.DescribeSafe(MySqlConnectionHelper.Resolve(builder.Configuration));
    Console.WriteLine($"[JDMS] MySQL: {mysqlPreview}");
}
catch (Exception ex)
{
    Console.WriteLine($"[JDMS] MySQL configuration error: {ex.Message}");
    throw;
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication();
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddJsonOptions(json =>
{
    json.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

var app = builder.Build();

var companyImagesDir = Path.Combine(app.Environment.WebRootPath, "images", "company");
Directory.CreateDirectory(companyImagesDir);

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    if (!isRender)
        app.UseHsts();
}

app.UseForwardedHeaders();

// Render يُنهي HTTPS أمام التطبيق — إعادة التوجيه لـ HTTPS تسبب أخطاء (400/حلقة)
if (!isRender)
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Text("ok")).AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
