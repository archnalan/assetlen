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
using Hangfire.PostgreSql;
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
// Hangfire builds its own schema when the storage is constructed, which happens
// during service registration — before EF has had a chance to create the
// database. On a first boot against an empty server that raced and lost: the
// job tables were never created, and every endpoint that enqueues a job then
// returned 500.
//
// So: don't prepare the schema here. EnsureHangfireSchema() below runs it once
// the database provably exists.
builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings();
    config.UsePostgreSqlStorage(
        o => o.UseNpgsqlConnection(HangfireConnectionString(builder.Configuration)),
        new PostgreSqlStorageOptions { PrepareSchemaIfNecessary = false });
});
builder.Services.AddHangfireServer();

// OCR runs on its own queue with two workers: each job may start an OCR
// process, and a 700-photo import must not start 20 of them at once.
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"{Environment.MachineName}:ocr";
    options.Queues = new[] { "ocr" };
    options.WorkerCount = 2;
});
builder.Services.AddTransient<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<ITenantServiceDAL, TenantServiceDAL>();
builder.Services.AddScoped<IConfigDAL, ConfigDAL>();
builder.Services.AddScoped<ILogsDAL, LogsDAL>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SmtpSenderService>();
builder.Services.AddScoped<IPandoraSmsService, PandoraSmsService>();
builder.Services.AddScoped<IAuthorizationDAL, AuthorizationDAL>();
builder.Services.AddScoped<IUsersDAL, UsersDAL>();
builder.Services.AddScoped<ISubscriptionRequestDAL, SubscriptionRequestDAL>();

// ── Remote Site services ──
// The single authority on per-project access. Every ASSETLEN DAL depends on it.
builder.Services.AddScoped<IProjectAccessService, ProjectAccessService>();
builder.Services.AddScoped<IActiveStageService, ActiveStageService>();
builder.Services.AddScoped<IProjectDAL, ProjectDAL>();
builder.Services.AddScoped<IProjectMemberDAL, ProjectMemberDAL>();
builder.Services.AddScoped<IStageDAL, StageDAL>();
builder.Services.AddScoped<IFundingDAL, FundingDAL>();
builder.Services.AddScoped<IProgressDAL, ProgressDAL>();
builder.Services.AddScoped<IPMDashboardDAL, PMDashboardDAL>();
builder.Services.AddScoped<IFlagDAL, FlagDAL>();
builder.Services.AddScoped<ICommitmentDAL, CommitmentDAL>();
builder.Services.AddScoped<IAnnotationDAL, AnnotationDAL>();
builder.Services.AddScoped<ILedgerDAL, LedgerDAL>();
builder.Services.AddScoped<IBudgetDAL, BudgetDAL>();
builder.Services.AddScoped<IProjectHealthService, ProjectHealthService>();
builder.Services.AddScoped<IProjectSizingService, ProjectSizingService>();

// ── Artifact store (P2 — assetlen.md Law 2) ──
// Storage is content-addressed on local disk for now. Swapping in Blob or
// Drive is an IArtifactStorage implementation and nothing else, which is why
// tbl_Artifact stores a *relative* path.
builder.Services.AddSingleton<IArtifactStorage>(sp => new LocalArtifactStorage(
    builder.Configuration["Artifacts:StorageRoot"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data"),
    sp.GetRequiredService<ILogger<LocalArtifactStorage>>()));
builder.Services.AddSingleton<IThumbnailGenerator, ImageSharpThumbnailGenerator>();
builder.Services.AddScoped<IArtifactDAL, ArtifactDAL>();

// ── Ingest: the front door (P3 — assetlen.md D3) ──
// Writes through the artifact store above, so it registers after it.
builder.Services.AddScoped<IIngestDAL, IngestDAL>();

// ── Extraction: pile into register (P5 — assetlen.md Law 3) ──
// The rules are always registered and always the fallback; the Claude
// extractor only runs when a key and Extraction:UseClaude are both set.
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Extraction.IMessageExtractor,
    assetlen.Service.FileProcessingServices.Extraction.RuleMessageExtractor>();
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Extraction.IMessageExtractor,
    assetlen.Service.FileProcessingServices.Extraction.ClaudeMessageExtractor>();
builder.Services.AddScoped<IExtractionDAL, ExtractionDAL>();
builder.Services.AddScoped<ISearchDAL, SearchDAL>();

// ── Peter's surfaces (P7): the home across every project and the daily brief ──
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Brief.IVantageIndex,
    assetlen.Service.FileProcessingServices.Brief.VantageIndex>();
builder.Services.AddScoped<IBriefDAL, BriefDAL>();

// ── The works report (works-report.md): assembled, drafted under a validator, issued ──
// The template narrator is always registered and always the fallback; Claude
// drafts only with a key, Report:UseClaude, and the project owner's consent.
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Report.IReportNarrator,
    assetlen.Service.FileProcessingServices.Report.TemplateReportNarrator>();
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Report.IReportNarrator,
    assetlen.Service.FileProcessingServices.Report.ClaudeReportNarrator>();
builder.Services.AddScoped<assetlen.Service.FileProcessingServices.Report.IVideoPosterQueue,
    assetlen.Service.FileProcessingServices.Report.HangfireVideoPosterQueue>();
builder.Services.AddScoped<assetlen.Service.FileProcessingServices.Report.VideoPosterJob>();
builder.Services.AddScoped<assetlen.Service.FileProcessingServices.Report.ScheduledReportJob>();
builder.Services.AddScoped<IWorksReportDAL, WorksReportDAL>();

// OCR: Tesseract when configured or on the PATH, otherwise Windows' own OCR.
// Registration order is the preference order under Ocr:Engine = auto.
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Ocr.IImageOcrEngine,
    assetlen.Service.FileProcessingServices.Ocr.TesseractOcrEngine>();
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Ocr.IImageOcrEngine,
    assetlen.Service.FileProcessingServices.Ocr.WindowsOcrEngine>();
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Ocr.IOcrService,
    assetlen.Service.FileProcessingServices.Ocr.OcrService>();
builder.Services.AddScoped<assetlen.Service.FileProcessingServices.Ocr.IArtifactTextQueue,
    assetlen.Service.FileProcessingServices.Ocr.HangfireArtifactTextQueue>();
builder.Services.AddScoped<assetlen.Service.FileProcessingServices.Ocr.ArtifactTextJob>();

// Voice notes are read into the same table as OCR text, so they are searchable (P9).
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Ocr.IAudioTranscriber,
    assetlen.Service.FileProcessingServices.Ocr.WindowsSpeechTranscriber>();

// ── The contractor tier (P9 — tier 3; nothing above depends on it) ──
builder.Services.AddScoped<IFrameExposure, FrameExposureService>();
builder.Services.AddScoped<ICurationDAL, CurationDAL>();
builder.Services.AddScoped<CutoffPublishJob>();
builder.Services.AddSingleton(sp => new assetlen.Service.FileProcessingServices.Push.VapidKeys(
    builder.Configuration,
    Path.Combine(builder.Configuration["Artifacts:StorageRoot"] ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data"),
        "push", "vapid.json")));
builder.Services.AddSingleton<assetlen.Service.FileProcessingServices.Push.PushQueue>();
builder.Services.AddScoped<assetlen.Service.FileProcessingServices.Push.INotifier,
    assetlen.Service.FileProcessingServices.Push.Notifier>();
builder.Services.AddScoped<IPushDAL, PushDAL>();
builder.Services.AddHttpClient("webpush", c => c.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddHostedService<assetlen.Service.FileProcessingServices.Push.PushDispatcher>();

// ── Development demo world ──
// Registered unconditionally so the container is identical in every
// environment; DevController is what refuses to run it outside Development.
builder.Services.AddScoped<IDevSeedService, DevSeedService>();

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
builder.Services.AddWindowsService();
//builder.Services.AddScoped<FormDataPrep>();
builder.Services.AddScoped<ISyncDAL, SyncDAL>();
builder.Services.AddSingleton<IOnlineIdentityVerifier, OnlineIdentityVerifier>();
builder.Services.AddHostedService<NetworkMonitorService>();

// Empties the bin: archived projects are soft-deleted 30 days after they were
// binned. On a timer rather than on a read path — a dashboard load must not
// turn into a multi-table write because a clock ticked.
builder.Services.AddHostedService<ArchiveSweepService>();
builder.Services.AddScoped<IRecoveryActionService, RecoveryActionService>();
builder.Services.AddScoped<IPasswordHasher<TenantInstance>, PasswordHasher<TenantInstance>>();
builder.Services.AddHttpClient();

//TypeAdapterConfig<tbl_Transaction, TransactionDto>
//    .NewConfig()
//    .Ignore(dest => dest.Customer!.Transactions);  // Ignore circular ref
//.Map(dest => dest.Customer, src => src.Customer != null ? src.Customer.Adapt<CustomerDto>() : null);  // Explicit null handling

var provider = builder.Configuration["AppMode"];

if (provider is not ("1" or "2" or "3"))
    throw new Exception($"Unsupported AppMode: {provider}. use 1, 2 or 3");

// §5.1.1 — PostgreSQL is the only database; migrations live in assetlen.Postgres.
builder.Services.AddDbContext<AssetlenDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"), //dont edit this line. edit appsettings.json instead
        npgsql =>
        {
            npgsql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsql.MigrationsAssembly("assetlen.Postgres");
        }));


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
}

app.UseHttpsRedirection();




app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<assetlen.Service.Hubs.AssetlenHub>("/hubs/assetlen");
// Seed roles using a service scope
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    DatabaseSeeder.InitializeDb(app, app.Logger);
    await DatabaseSeeder.SeedRolesAndAdminAsync(services, builder.Configuration, app.Environment);
}

// The database now exists, so Hangfire can safely build its tables.
EnsureHangfireSchema(HangfireConnectionString(builder.Configuration), app.Logger);

// Works reports issue on their own — weekly and on milestones — whether or not
// anybody logs in (works-report.md §7, Law 0).
using (var scope = app.Services.CreateScope())
{
    try
    {
        var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        // Registered only now: on a fresh database the job tables did not exist
        // until EnsureHangfireSchema above.
        if (!syncType.IsOnlineApi())
            recurring.AddOrUpdate<SyncDAL>(
                "pull-changes",
                s => s.PullChangesFromOnlineAsync(),
                Cron.MinuteInterval(int.Parse(builder.Configuration["OnlineApi:Interval"] ?? "1")));

        recurring.AddOrUpdate<assetlen.Service.FileProcessingServices.Report.ScheduledReportJob>(
            "works-report-weekly", j => j.WeeklyAsync(),
            builder.Configuration["Report:WeeklyCron"] ?? Cron.Weekly(DayOfWeek.Sunday, 18),
            TimeZoneInfo.Local);
        recurring.AddOrUpdate<assetlen.Service.FileProcessingServices.Report.ScheduledReportJob>(
            "works-report-milestones", j => j.MilestonesAsync(), Cron.Hourly(),
            TimeZoneInfo.Local);

        // The brief's curated frames cross at the cutoff whether or not the mediator
        // touched them (assetlen.md §5). Every quarter hour; the cutoff hour decides.
        recurring.AddOrUpdate<CutoffPublishJob>(
            "brief-cutoff", j => j.RunAsync(), "*/15 * * * *", TimeZoneInfo.Local);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Scheduled works reports are not registered; background jobs may be unavailable.");
    }
}


static string? HangfireConnectionString(IConfiguration configuration) =>
    configuration.GetConnectionString("PostgresHangfire") ?? configuration.GetConnectionString("Postgres");

static void EnsureHangfireSchema(string? connectionString, Microsoft.Extensions.Logging.ILogger logger)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogWarning("No Hangfire connection string; background jobs are disabled.");
        return;
    }

    for (var attempt = 1; attempt <= 5; attempt++)
    {
        try
        {
            // Its own "hangfire" schema, beside EF's tables in the same database.
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            connection.Open();
            PostgreSqlObjectsInstaller.Install(connection, "hangfire");
            logger.LogInformation("Hangfire schema is present.");
            return;
        }
        catch (Exception ex) when (attempt < 5)
        {
            logger.LogWarning("Hangfire schema attempt {Attempt} failed ({Message}); retrying.",
                attempt, ex.Message);
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }
        catch (Exception ex)
        {
            // Non-fatal: the API still serves reads. Endpoints that enqueue a
            // job will fail loudly, which is the correct signal to fix this.
            logger.LogError(ex, "Could not prepare the Hangfire schema.");
        }
    }
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


