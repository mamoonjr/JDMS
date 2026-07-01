using JDMS.Application.Constants;
using JDMS.Domain.Entities;
using JDMS.Infrastructure.Services;
using JDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JDMS.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var hostEnv = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
        var isDevelopment = hostEnv.EnvironmentName == Microsoft.Extensions.Hosting.Environments.Development;

        await context.Database.MigrateAsync();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureAdminUserAsync(userManager, logger, isDevelopment);

        if (!await context.CompanySettings.AnyAsync())
        {
            await context.CompanySettings.AddAsync(new CompanySettings
            {
                CompanyName = "نظام إدارة التوصيل الأردني - JDMS",
                LogoPath = CompanySettingsService.DefaultLogoRelativePath,
                Address = "عمان، المملكة الأردنية الهاشمية",
                Phone = "0790000000",
                Email = "info@jdms.jo",
                TaxRate = 0.16m
            });
        }
        else
        {
            var company = await context.CompanySettings.FirstAsync();
            if (string.IsNullOrWhiteSpace(company.LogoPath))
            {
                company.LogoPath = CompanySettingsService.DefaultLogoRelativePath;
                await context.SaveChangesAsync();
            }
        }

        await EnsureJordanLocationsAsync(context, logger);

        if (!await context.Products.AnyAsync())
        {
            await context.Products.AddRangeAsync(
                new Product { ProductName = "مياه معدنية 1.5 لتر", SKU = "WTR-001", UnitPrice = 0.50m, Description = "مياه معدنية" },
                new Product { ProductName = "خبز عربي", SKU = "BRD-001", UnitPrice = 0.35m, Description = "خبز طازج" },
                new Product { ProductName = "حليب 1 لتر", SKU = "MLK-001", UnitPrice = 1.20m, Description = "حليب كامل الدسم" }
            );
        }

        if (!await context.Customers.AnyAsync())
        {
            var amman = await context.Governorates.Include(g => g.Areas).FirstAsync(g => g.NameAr == "عمان");
            var area = amman.Areas.FirstOrDefault(a => a.NameAr == "العبدلي")
                ?? amman.Areas.First(a => a.IsActive);
            var customer = new Customer
            {
                CustomerCode = "CUS-000001",
                FullName = "أحمد محمد",
                MobileNumber = "0791234567",
                Email = "ahmad@example.com"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            context.Addresses.Add(new Address
            {
                CustomerId = customer.Id,
                GovernorateId = amman.Id,
                AreaId = area.Id,
                Neighborhood = "العبدلي",
                Street = "شارع الملكة رانيا",
                Building = "15",
                IsDefault = true
            });
        }

        if (!await context.Drivers.AnyAsync())
        {
            var area = await context.Areas.FirstAsync();
            context.Drivers.Add(new Driver
            {
                DriverName = "خالد العمري",
                PhoneNumber = "0799876543",
                VehicleType = Domain.Enums.VehicleType.Motorcycle,
                AssignedAreaId = area.Id
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeded successfully.");
    }

    private static async Task EnsureAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger, bool resetPasswordInDev)
    {
        const string userName = "admin";
        const string password = "Admin@123";

        var admin = await userManager.FindByNameAsync(userName);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = userName,
                Email = "admin@jdms.jo",
                FullName = "مدير النظام",
                EmailConfirmed = true,
                IsActive = true
            };
            var createResult = await userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return;
            }
        }
        else
        {
            admin.IsActive = true;
            admin.EmailConfirmed = true;
            await userManager.UpdateAsync(admin);

            if (resetPasswordInDev)
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
                var resetResult = await userManager.ResetPasswordAsync(admin, resetToken, password);
                if (!resetResult.Succeeded)
                {
                    logger.LogWarning("Admin password reset failed: {Errors}",
                        string.Join("; ", resetResult.Errors.Select(e => e.Description)));
                }
            }
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Administrator))
            await userManager.AddToRoleAsync(admin, Roles.Administrator);

        logger.LogInformation("Admin user ready. Username: {UserName}", userName);
    }

    /// <summary>
    /// مزامنة المحافظات والمناطق مع القائمة المعتمدة: إضافة الناقص، إعادة تفعيل المطابق، إزالة/تعطيل الزائد.
    /// </summary>
    private static async Task EnsureJordanLocationsAsync(ApplicationDbContext context, ILogger logger)
    {
        var addedGovs = 0;
        var addedAreas = 0;
        var removedAreas = 0;
        var deactivatedAreas = 0;

        var seedGovNames = JordanLocationSeedData.All.Select(s => s.NameAr).ToHashSet();

        foreach (var seed in JordanLocationSeedData.All)
        {
            var governorate = await context.Governorates
                .FirstOrDefaultAsync(g => g.NameAr == seed.NameAr);

            if (governorate == null)
            {
                governorate = new Governorate
                {
                    NameAr = seed.NameAr,
                    NameEn = seed.NameEn,
                    IsActive = true
                };
                context.Governorates.Add(governorate);
                await context.SaveChangesAsync();
                addedGovs++;
            }
            else
            {
                governorate.NameEn = seed.NameEn;
                governorate.IsActive = true;
            }

            var expectedNames = seed.Areas.Select(a => a.NameAr).ToHashSet();

            foreach (var areaSeed in seed.Areas)
            {
                var existing = await context.Areas.FirstOrDefaultAsync(a =>
                    a.GovernorateId == governorate.Id && a.NameAr == areaSeed.NameAr);
                if (existing == null)
                {
                    context.Areas.Add(new Area
                    {
                        GovernorateId = governorate.Id,
                        NameAr = areaSeed.NameAr,
                        NameEn = areaSeed.NameAr,
                        DeliveryFee = areaSeed.DeliveryFee,
                        IsActive = true
                    });
                    addedAreas++;
                }
                else
                {
                    existing.NameEn = areaSeed.NameAr;
                    existing.DeliveryFee = areaSeed.DeliveryFee;
                    existing.IsActive = true;
                }
            }

            await context.SaveChangesAsync();

            var dbAreas = await context.Areas
                .Where(a => a.GovernorateId == governorate.Id)
                .ToListAsync();

            foreach (var obsolete in dbAreas.Where(a => !expectedNames.Contains(a.NameAr)))
            {
                var removed = await TryRemoveOrReassignAreaAsync(context, obsolete, governorate.Id, expectedNames);
                if (removed) removedAreas++;
                else
                {
                    obsolete.IsActive = false;
                    deactivatedAreas++;
                }
            }
        }

        var extraGovernorates = await context.Governorates
            .Where(g => !seedGovNames.Contains(g.NameAr))
            .ToListAsync();
        foreach (var gov in extraGovernorates)
            gov.IsActive = false;

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Jordan locations synced: +{AddedGovs} governorates, +{AddedAreas} areas, -{Removed} removed, {Deactivated} deactivated.",
            addedGovs, addedAreas, removedAreas, deactivatedAreas);
    }

    private static async Task<bool> TryRemoveOrReassignAreaAsync(
        ApplicationDbContext context,
        Area obsolete,
        int governorateId,
        HashSet<string> expectedNames)
    {
        var fallback = await context.Areas
            .Where(a => a.GovernorateId == governorateId && expectedNames.Contains(a.NameAr) && a.Id != obsolete.Id)
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync();

        var addresses = await context.Addresses.Where(a => a.AreaId == obsolete.Id).ToListAsync();
        var drivers = await context.Drivers.Where(d => d.AssignedAreaId == obsolete.Id).ToListAsync();

        if (addresses.Count > 0 || drivers.Count > 0)
        {
            if (fallback == null) return false;

            foreach (var addr in addresses)
                addr.AreaId = fallback.Id;
            foreach (var driver in drivers)
                driver.AssignedAreaId = fallback.Id;
        }

        context.Areas.Remove(obsolete);
        await context.SaveChangesAsync();
        return true;
    }
}
