using assetlen.Service.DataAccess;
using assetlen.Service.DbServices;
using assetlen.Service.Extensions;
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
                // Create the bootstrap admin under the name configured in
                // appsettings. A suffix is added only if that name is taken —
                // the credentials in appsettings must be the ones that work.
                var desiredUserName = Configuration.GetSection("UserSettings")["UserName"];
                var desiredEmail = Configuration.GetSection("UserSettings")["UserEmail"];

                var poweruser = new AppUser
                {
                    UserName = await UserNameAllocator.ResolveUserNameAsync(userManager, desiredUserName),
                    Email = await UserNameAllocator.ResolveEmailAsync(userManager, desiredEmail),
                    FirstName = "System",
                    LastName = "Admin",

                };

                var userPassword = Configuration.GetSection("UserSettings")["UserPassword"];
                var _user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == desiredEmail);
                if (_user is null) _user = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.UserName == desiredUserName);


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
                                await SeedTenantSettingsAsync(context, logger, seedData);

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

        /// <summary>
        /// Seeds the platform settings a new tenant needs. ASSETLEN seeds no
        /// domain data — no reference lists, no sample projects. A tenant starts empty.
        /// </summary>
        public static async Task SeedTenantSettingsAsync(AssetlenDbContext context, ILogger logger, InitialSeedDataDto seedData)
        {
            try
            {
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
                logger.LogInformation("Tenant settings seeding complete.");
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
