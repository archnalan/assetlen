using BeaconLib;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Service.Domain.Interfaces;
using assetlen.Service.DbServices.SmtpClient;
using assetlen.API;
using assetlen.API.Domain;
using assetlen.API.Domain.Interfaces;
using assetlen.API.Middlewares;
using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.OpenTelemetry;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using static assetlen.Service.DbServices.AuthorizationDAL;
using assetlen.Service.FileProcessingServices;
using assetlen.Service.Extensions;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
    ApplicationName = typeof(Program).Assembly.FullName
});
var appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "assetlen");


builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile(Path.Combine(appDataPath, "appsettings-api.json"), optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
string currentPath = AppDomain.CurrentDomain.BaseDirectory;
// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Optional: case-insensitive property matching
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(setup =>
{
    // Include 'SecurityScheme' to use JWT Authentication
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token on the textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    //// Add TenantId as a SecurityScheme
    //var tenantIdSecurityScheme = new OpenApiSecurityScheme
    //{
    //    Name = "TenantId",
    //    In = ParameterLocation.Header,
    //    Type = SecuritySchemeType.ApiKey,
    //    Description = "Tenant identifier required for each request",
    //    Reference = new OpenApiReference
    //    {
    //        Id = "TenantId",
    //        Type = ReferenceType.SecurityScheme
    //    }
    //};

    //setup.AddSecurityDefinition(tenantIdSecurityScheme.Reference.Id, tenantIdSecurityScheme);

    // Apply JWT and TenantId globally
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() },
        //{ tenantIdSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddHangfire(config => config.UseSimpleAssemblyNameTypeSerializer()
.UseRecommendedSerializerSettings()
.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnectionHangfire")));
builder.Services.AddHangfireServer();
builder.Services.AddTransient<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ITenantServiceDAL, TenantServiceDAL>();
builder.Services.AddScoped<ICustomerDAL, CustomerDAL>();
builder.Services.AddScoped<ICategoryDAL, CategoryDAL>();
builder.Services.AddScoped<IConfigDAL, ConfigDAL>();
builder.Services.AddScoped<ICashItemsDAL, CashItemsDAL>();
builder.Services.AddScoped<ICustomerPricingDAL, CustomerPricingDAL>();
builder.Services.AddScoped<IExpenseDAL, ExpenseDAL>();
//builder.Services.AddSingleton<IGenerateBarcodeDAL, GenerateBarcodeDAL>();
builder.Services.AddScoped<IGenerateBarcodeDAL, GenerateBarcodeDAL>();
builder.Services.AddScoped<ILogsDAL, LogsDAL>();
builder.Services.AddScoped<IOrderStatusDAL, OrderStatusDAL>();
builder.Services.AddScoped<IPaymentsDAL, PaymentsDAL>();
builder.Services.AddScoped<IProductsDAL, ProductsDAL>();
builder.Services.AddScoped<IUserFavoritesDAL, UserFavoritesDAL>();
builder.Services.AddScoped<IUserDocumentsDAL, UserDocumentsDAL>();
builder.Services.AddScoped<IProductReceivingDAL, ProductReceivingDAL>();
builder.Services.AddScoped<IProductRelationshipsDAL, ProductRelationshipsDAL>();
builder.Services.AddScoped<IReceiptsDAL, ReceiptsDAL>();
builder.Services.AddScoped<ITransactionDAL, TransactionDAL>();
builder.Services.AddScoped<ITransactionDetailDAL, TransactionDetailDAL>();
builder.Services.AddScoped<IReportsDAL, ReportsDAL>();
builder.Services.AddScoped<ISegmentsDAL, SegmentsDAL>();
builder.Services.AddScoped<IShiftsDAL, ShiftsDAL>();
builder.Services.AddScoped<ISlipsDAL, SlipsDAL>();
builder.Services.AddScoped<ISupplierDAL, SupplierDAL>();
builder.Services.AddScoped<ISupplierPaymentDAL, SupplierPaymentDAL>();
builder.Services.AddScoped<ItaxDAL, taxDAL>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SmtpSenderService>();
builder.Services.AddScoped<IPandoraSmsService, PandoraSmsService>();
builder.Services.AddScoped<IAuthorizationDAL, AuthorizationDAL>();
builder.Services.AddScoped<IDiscountsDAL, DiscountsDAL>();
builder.Services.AddScoped<PricingCalculations>();
builder.Services.AddScoped<IPricingCalculations, PricingCalculations>();
builder.Services.AddScoped<IUsersDAL, UsersDAL>();
builder.Services.AddScoped<IExcelDomainService, ExcelDomainService>();
builder.Services.AddScoped<IExpenseTypeDAL, ExpenseTypeDAL>();
builder.Services.AddScoped<IFileProcessing, FileProcessing>();
builder.Services.AddScoped<IPrinterPreferencesDAL, PrinterPreferencesDAL>();
builder.Services.AddScoped<IProductDetailDAL, ProductDetailDAL>();
builder.Services.AddScoped<IProductDetailFeedbackDAL, ProductDetailFeedbackDAL>();
builder.Services.AddScoped<ISubscriptionRequestDAL, SubscriptionRequestDAL>();
builder.Services.AddScoped<IFeedbackNotificationService, FeedbackNotificationService>();

// ── Remote Site services ──
builder.Services.AddScoped<IProjectDAL, ProjectDAL>();
builder.Services.AddScoped<IProjectMemberDAL, ProjectMemberDAL>();
builder.Services.AddScoped<IStageDAL, StageDAL>();
builder.Services.AddScoped<IFundingDAL, FundingDAL>();
builder.Services.AddScoped<IProgressDAL, ProgressDAL>();
builder.Services.AddScoped<IPMDashboardDAL, PMDashboardDAL>();
builder.Services.AddScoped<IFlagDAL, FlagDAL>();
builder.Services.AddScoped<IProjectHealthService, ProjectHealthService>();

builder.Services.AddSignalR();
builder.Services.AddIdentity<AppUser, IdentityRole>
            (options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AssetlenDbContext>()
            .AddDefaultTokenProviders();


builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
    .AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Legacy refresh-token query path.
            if (context.Request.Query.TryGetValue("access_key_value_temp_refresh", out var legacy))
            {
                context.Token = legacy;
                return Task.CompletedTask;
            }
            // SignalR transports (WebSockets/SSE) cannot set Authorization
            // headers from the browser, so the client passes the token via
            // ?access_token=… on /hubs/* requests.
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validation failed : {headers}", context.Request.Headers);

            logger.LogError(context.Exception, "JWT validation failed");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            //log token
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            //logger.LogInformation("Token validated: {token}", context.SecurityToken);

            var TenantId = context.Request.Headers["TenantId"].FirstOrDefault();
            //if (TenantId == null)
            //{

            //    context.Fail("TenantId header missing");
            //    context.Response.StatusCode = 401;
            //    context.Response.ContentType = "application/json";
            //    var result = System.Text.Json.JsonSerializer.Serialize(new { message = $"Missing TenantId header" });
            //    return context.Response.WriteAsync(result);

            //}
            var token = context.SecurityToken as JsonWebToken;
            var tenantIdFromToken = token.GetPayloadValue<string>("TenantId");


            if (!string.IsNullOrEmpty(tenantIdFromToken))
            {
                //Logger.LogInformation("Here is the client id from the request {clientId}", clientId);
                //if (tenantIdFromToken != TenantId)
                //{
                //    context.Fail("Invalid TenantId");
                //    context.Response.StatusCode = 401;
                //    context.Response.ContentType = "application/json";
                //    var result = System.Text.Json.JsonSerializer.Serialize(new { message = $"Invalid TenantId header or Token" });
                //    return context.Response.WriteAsync(result);
                //}
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddRazorComponents();


//builder.Services.AddScoped<Radzen.DialogService>();
//builder.Services.AddScoped<Radzen.TooltipService>();
//builder.Services.AddScoped<Radzen.ContextMenuService>();
builder.Services.AddScoped<FileUploadManager>();
builder.Services.AddWindowsService();
//builder.Services.AddScoped<FormDataPrep>();
builder.Services.AddScoped<ISyncDAL, SyncDAL>();
builder.Services.AddSingleton<IOnlineIdentityVerifier, OnlineIdentityVerifier>();
builder.Services.AddHostedService<NetworkMonitorService>();
builder.Services.AddScoped<IRecoveryActionService, RecoveryActionService>();
builder.Services.AddScoped<IPasswordHasher<TenantInstance>, PasswordHasher<TenantInstance>>();
builder.Services.AddHttpClient();

//TypeAdapterConfig<tbl_Transaction, TransactionDto>
//    .NewConfig()
//    .Ignore(dest => dest.Customer!.Transactions);  // Ignore circular ref
//.Map(dest => dest.Customer, src => src.Customer != null ? src.Customer.Adapt<CustomerDto>() : null);  // Explicit null handling

var provider = builder.Configuration["AppMode"];

//builder.Services.AddDbContext<AssetlenDbContext>((serviceProvider, options) =>
//    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"],
//        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
//            maxRetryCount: 5,
//            maxRetryDelay: TimeSpan.FromSeconds(30),
//            errorNumbersToAdd: null)

//        ), ServiceLifetime.Scoped);

//if (provider == "1")
//{
//    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "assetlen");
//    Directory.CreateDirectory(dbPath);
//    builder.Configuration["ConnectionStrings:SqliteConnection"] = $"Data Source={Path.Combine(dbPath, "app.db")}";
//}

builder.Services.AddDbContext<AssetlenDbContext>((serviceProvider, options)
    => _ = provider switch
    {
        //"1" => options.UseSqlite(
        //   builder.Configuration["ConnectionStrings:SqliteConnection"],
        //    x => x.MigrationsAssembly("assetlen.Sqlite")),

        "1" => options.UseSqlServer(
           builder.Configuration["ConnectionStrings:DefaultConnection"], //dont edit this line. edit appsettings.json instead
           sqlServerOptions =>
           {
               // Enable retry-on-failure for SQL Server
               sqlServerOptions.EnableRetryOnFailure(
                   maxRetryCount: 5,
                   maxRetryDelay: TimeSpan.FromSeconds(30),
                   errorNumbersToAdd: null
               );
               sqlServerOptions.MigrationsAssembly("assetlen.SqlServer");
           }
       ),
        "2" => options.UseSqlServer(
                  builder.Configuration["ConnectionStrings:DefaultConnection"], //dont edit this line. edit appsettings.json instead
                  sqlServerOptions =>
                  {
                      // Enable retry-on-failure for SQL Server
                      sqlServerOptions.EnableRetryOnFailure(
                          maxRetryCount: 5,
                          maxRetryDelay: TimeSpan.FromSeconds(30),
                          errorNumbersToAdd: null
                      );
                      sqlServerOptions.MigrationsAssembly("assetlen.SqlServer");
                      sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                  }
              ),
        "3" => options.UseSqlServer(
            builder.Configuration["ConnectionStrings:DefaultConnection"], //dont edit this line. edit appsettings.json instead
            sqlServerOptions =>
            {
                // Enable retry-on-failure for SQL Server
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                );
                sqlServerOptions.MigrationsAssembly("assetlen.SqlServer");
            }
        ),
        _ => throw new Exception($"Unsupported provider: {provider}. use 1 or 2")
    });

//builder.Services.AddDbContext<AssetlenDbContext>((serviceProvider, options) =>
//{
//    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
//    options.AddInterceptors(serviceProvider.GetRequiredService<TenantCommandInterceptor>());
//});

//builder.Services.AddDbContextFactory<AssetlenDbContext>(options =>
//    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"],
//        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
//            maxRetryCount: 5,
//            maxRetryDelay: TimeSpan.FromSeconds(30),
//            errorNumbersToAdd: null)),lifetime:ServiceLifetime.Scoped);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .WriteTo.File($"{currentPath.Replace("assetlen.API.dll", "").Replace("assetlen.API.exe", "")}\\Logs\\Logs.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.OpenTelemetry(options =>
    {
        options.IncludedData = IncludedData.MessageTemplateTextAttribute;
        options.IncludedData = IncludedData.SourceContextAttribute;
        options.ResourceAttributes = new Dictionary<string, object>
    {
        { "service.environment", builder.Environment.EnvironmentName },
        { "service.instance.id", Environment.MachineName },
        {"service.name", "assetlen.API" }
    };


        options.Endpoint = builder.Configuration["LogIngestUrl"];
        options.Protocol = OtlpProtocol.HttpProtobuf;
        options.Headers = new Dictionary<string, string>
        {
            ["X-Seq-ApiKey"] = "UhSOvMLPU7RZFIPUcIGF"
        };

    })
    .MinimumLevel.Information()
    .CreateLogger();

builder.Services.AddSerilog();

if (builder.Configuration["AppMode"] == "1")
{
    //for desktop hosting
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    // builder.WebHost.UseUrls("http://[::1]:0");   // IPv6 if needed
}
else if (builder.Configuration["AppMode"] == "3")
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(0); // 0 means dynamically assign a free port
    });

}
//builder.Logging.ClearProviders();
//builder.Logging.AddOpenTelemetry(options =>
//{
//    options.IncludeFormattedMessage = true;
//    options.IncludeScopes = true;
//    options.SetResourceBuilder(ResourceBuilder.CreateEmpty()
//        .AddService("assetlen.API")
//        .AddAttributes(new Dictionary<string, object>
//        {            
//            { "service.environment", builder.Environment.EnvironmentName },
//            { "service.instance.id", Environment.MachineName }
//        }));

//    options.AddOtlpExporter(otlpOptions =>
//    {
//        otlpOptions.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
//        otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
//        otlpOptions.Headers = "X-Seq-ApiKey=UhSOvMLPU7RZFIPUcIGF";
//    });
//});


var app = builder.Build();

app.UseStaticFiles();
// Configure the HTTP request pipeline.
app.UseSwagger();
// Ensure the middleware runs early in the pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;  // Set Swagger UI at the root (localhost:5000)
    });
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;  // Set Swagger UI at the root (localhost:5000)
    });
}
app.UseCors("AllowAllOrigins");

//syncing logic
var syncType = app.Services.GetRequiredService<IOnlineIdentityVerifier>();
if (syncType.IsOnlineApi())
{
    app.UseMiddleware<OnlineSyncCaptureMiddleware>();
}
else
{
    app.UseMiddleware<OnlineSyncMiddleware>();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<SyncDAL>(
                "pull-changes",
                s => s.PullChangesFromOnlineAsync(),
                Cron.MinuteInterval(int.Parse(builder.Configuration["OnlineApi:Interval"])));
        }
        catch (Exception)
        {
        }
    }
}

app.UseHttpsRedirection();




app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FeedbackHub>("/hubs/feedback");
app.MapHub<assetlen.Service.Hubs.AssetlenHub>("/hubs/assetlen");
// Seed roles using a service scope
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    DatabaseSeeder.InitializeDb(app, app.Logger);
    await DatabaseSeeder.SeedRolesAndAdminAsync(services, builder.Configuration, app.Environment);
}




if (app.Configuration["AppMode"] == "2")
{
    app.Run(); //disable for desktop hosting
}
else if (app.Configuration["AppMode"] == "1")
{
    await app.StartAsync();
    // Get assigned port
    var port = app.Urls.Select(u => new Uri(u)).First().Port;

    // Write port to AppData
    var appDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "assetlen"
    );


    Directory.CreateDirectory(appDataDir);
    File.WriteAllText(Path.Combine(appDataDir, "port.txt"), port.ToString());
    await app.WaitForShutdownAsync();
}
else if (app.Configuration["AppMode"] == "3")
{
    await app.StartAsync();
    // Get assigned port
    var port = app.Urls.Select(u => new Uri(u)).First().Port;




    if (!string.IsNullOrEmpty(port.ToString()))
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation($"Application is running on port: {port}");
        var beacon = new Beacon("AssetlenServiceDiscovery", (ushort)port);
        beacon.BeaconData = await (new DiscoveryService(port).CreateBroadcastMessage());
        beacon.Start();


        //beacon.Beacondata in a file in the current directory. delete the file if it exists first
        //var beaconFilePath = Path.Combine(currentPath, "beacon.txt");
        //if (File.Exists(beaconFilePath))
        //{
        //    File.Delete(beaconFilePath);
        //}
        //File.WriteAllText(beaconFilePath, beacon.BeaconData);
        logger.LogInformation($"Application is running on port: {beacon.BeaconData}");

        app.Lifetime.ApplicationStopping.Register(() => beacon.Stop());
        await app.WaitForShutdownAsync();
    }
    else
    {
        Console.WriteLine("Application is running, but no port was assigned.");
        await app.WaitForShutdownAsync();
    }
}


