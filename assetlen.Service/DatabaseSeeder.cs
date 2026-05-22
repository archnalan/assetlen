using assetlen.Service.DataAccess;
using assetlen.Service.DbServices;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace assetlen.API
{
    public static class DatabaseSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration Configuration, IWebHostEnvironment _env)
        {

            await CreateRoles(serviceProvider, Configuration, _env);
        }
        private static async Task CreateRoles(IServiceProvider serviceProvider, IConfiguration Configuration, IWebHostEnvironment _env)
        {
            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
                var context = serviceProvider.GetRequiredService<AssetlenDbContext>();
                var logger = serviceProvider.GetService<ILogger<AssetlenDbContext>>();
                List<string> roleNames = new List<string>(Enum.GetNames(typeof(statics.UserRoles)));

                foreach (var roleName in roleNames)
                {
                    bool roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
                //create default user
                string random = Guid.NewGuid().ToString().Substring(0, 4);
                var poweruser = new AppUser
                {
                    UserName = random + Configuration.GetSection("UserSettings")["UserName"],
                    Email = random + Configuration.GetSection("UserSettings")["UserEmail"],
                    FirstName = "System",
                    LastName = "Admin",

                };

                var userPassword = Configuration.GetSection("UserSettings")["UserPassword"];
                var _user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == poweruser.Email);
                if (_user is null) _user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.UserName == poweruser.UserName);


                if (_user == null && !context.tbl_Tenants.IgnoreQueryFilters().Any())
                {
                    var strategy = context.Database.CreateExecutionStrategy();

                    await strategy.ExecuteAsync(async () =>
                    {
                        using (var scope = await context.Database.BeginTransactionAsync())
                        {
                            //create tenant first
                            var tenantDto = new tbl_Tenant
                            {
                                TenantId = Guid.NewGuid().ToString(),
                                Name = "DefaultTenantLocal"
                            };
                            var newTenant = await context.tbl_Tenants.AddAsync(tenantDto);
                            await context.SaveChangesAsync();



                            //create power user

                            poweruser.TenantId = newTenant.Entity.TenantId;

                            var createPowerUser = await userManager.CreateAsync(poweruser, userPassword);
                            if (createPowerUser.Succeeded)
                            {

                                // Bootstrap user runs the platform AND owns the
                                // first tenant. Later tenants get Contractor only.
                                await userManager.AddToRoleAsync(poweruser, UserRoles.Contractor);
                                await userManager.AddToRoleAsync(poweruser, UserRoles.SystemAdmin);



                                var seedData = new InitialSeedDataDto(poweruser.TenantId);
                                await SeedSegmentsSupplierCategoriesTaxAsync(context, logger, seedData);

                                await scope.CommitAsync();
                            }
                        }
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static async Task SeedSegmentsSupplierCategoriesTaxAsync(AssetlenDbContext context, ILogger logger, InitialSeedDataDto seedData)
        {
            try
            {

                //categories
                var categoryExists = context.tbl_Categories.Any(c => c.TenantId == seedData.tenantId);
                if (!categoryExists)
                {
                    var item = seedData.categories;
                    context.Add(item);
                    logger.LogInformation("Seeding Category: {Category}", item.Category);
                }
                //segments
                var segmentExists = context.tbl_Segments.Any(c => c.TenantId == seedData.tenantId);
                if (!segmentExists)
                {
                    var item = seedData.segments;
                    context.Add(item);
                    logger.LogInformation("Seeding Segment: {Segment}", item.Segment);
                }
                //suppliers
                var supplierExists = context.tbl_Suppliers.Any(c => c.TenantId == seedData.tenantId);
                if (!supplierExists)
                {
                    var item = seedData.suppliers;
                    context.Add(item);
                    logger.LogInformation("Seeding supplier: {supplier}", item.FullName);
                }
                //taxes
                var taxExists = context.tbl_Taxes.Any(c => c.TenantId == seedData.tenantId);
                if (!taxExists)
                {

                    context.AddRange(seedData.taxes);
                    logger.LogInformation("Seeding Taxes");

                }
                //paymentmodes
                // Seed Payment Modes
                var paymentModesExist = context.tbl_PaymentModes.Any();
                if (!paymentModesExist && seedData.paymentModes is not null)
                {


                    context.tbl_PaymentModes.AddRange(seedData.paymentModes);

                    foreach (var mode in seedData.paymentModes)
                    {
                        logger.LogInformation("Seeding Payment Mode: {mode}", mode.Description);
                    }
                }

                // Order Statuses
                var orderStatusExists = context.tbl_OrderStatuses.Any(c => c.TenantId == seedData.tenantId);
                if (!orderStatusExists)
                {
                    context.tbl_OrderStatuses.AddRange(seedData.orderStatuses);
                    foreach (var status in seedData.orderStatuses)
                    {
                        logger.LogInformation("Seeding Order Status: {OrderName}", status.OrderName);
                    }
                }

                // Seed Cash Items. no query filters
                var cashItemsExist = context.tbl_CashItems.Any();
                if (!cashItemsExist)
                {


                    context.tbl_CashItems.AddRange(seedData.cashItems);

                    foreach (var item in seedData.cashItems)
                    {
                        logger.LogInformation("Seeding Cash Item: {amount}", item.Amount);
                    }
                }

                //settings
                var settings = context.tbl_Configurations.Any(c => c.TenantId == seedData.tenantId);
                if (!settings)
                {
                    foreach (var item in seedData.configSeedData)
                    {
                        bool exists = context.tbl_Configurations.Any(c => c.ConfigId == item.ConfigId);
                        if (!exists)
                        {
                            context.tbl_Configurations.Add(item);
                            logger.LogInformation("Seeding configuration SettingID: {SettingID}", item.ConfigId);
                        }
                    }

                }


                await context.SaveChangesAsync();
                logger.LogInformation("Configuration seeding complete.");


            }
            catch (Exception ex)
            {
                logger.LogError("Error seeding initial data: {ex}", ex);
                throw new Exception("Error seeding initial data", ex);
            }
        }
        public static void InitializeDb(this WebApplication app, ILogger _logger)
        {

            //if (!app.Environment.IsDevelopment())
            //{
            //    return;
            //}
            //TODO: Add version checker to determine if seeding is necessary on startup


            try
            {
                using var scope = app.Services.CreateScope();
                {
                    var database = scope.ServiceProvider.GetRequiredService<AssetlenDbContext>().Database;
                    var connection = database.GetConnectionString();
                    _logger.LogInformation("Starting app with Connection string {conn}", connection);
                    database.Migrate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error seeding Application data, {ex}", ex);
                throw;

            }
        }
    }
}
