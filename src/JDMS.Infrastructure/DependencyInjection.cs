using JDMS.Application.Interfaces;
using JDMS.Infrastructure.Data;
using JDMS.Infrastructure.Identity;
using JDMS.Infrastructure.Repositories;
using JDMS.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JDMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = MySqlConnectionHelper.Resolve(configuration);

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mySql =>
            {
                mySql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                mySql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null);
            }));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IQuickEntryService, QuickEntryService>();
        services.AddScoped<IPosService, PosService>();
        services.AddScoped<ICompanySettingsService, CompanySettingsService>();
        services.AddHttpContextAccessor();

        return services;
    }
}
