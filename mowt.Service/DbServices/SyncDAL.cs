using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Service.Extensions;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using mowt.API.Domain.Interfaces;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static mowt.Shared.Models.statics.statics;

namespace mowt.Service.DbServices
{
    public class SyncDAL : ISyncDAL
    {
        private readonly mowtDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SyncDAL> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigDAL _userSettings;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private bool settings => statics.allSettings.TryGetValue((int)statics.Configurations.OnlineSyncEnabled, out var onlineSyncEnabled) ? bool.Parse(onlineSyncEnabled) : false;
        private string _apiKey => statics.allSettings.TryGetValue((int)statics.Configurations.OnlineSyncToken, out var onlineSyncToken) ? onlineSyncToken : "";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOnlineIdentityVerifier _onlineIdentityVerifier;
        private readonly IAuthorizationDAL _authorizationDAL;

        private readonly List<string> headUrls = new List<string>
        {
            "https://www.google.com/generate_204",
            "https://httpbin.org/status/200",
            "https://example.com"
        };
        public SyncDAL(mowtDbContext context, ILogger<SyncDAL> logger, IHttpClientFactory httpClientFactory, IConfigDAL userSettings, IBackgroundJobClient backgroundJobClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IOnlineIdentityVerifier onlineIdentityVerifier, IAuthorizationDAL authorizationDAL)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userSettings = userSettings;
            _backgroundJobClient = backgroundJobClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _onlineIdentityVerifier = onlineIdentityVerifier;
            _authorizationDAL = authorizationDAL;
        }

        public async Task<ServiceResult<PaginationDetails<SyncLogDto>>> GetChangesFromOnlineApi(DateTime lastSync, int offSet = 0, int batchSize = 100)
        {
            try
            {
                var result = await _context.tbl_SyncLogs.AsNoTracking()
                    .Where(log => log.Timestamp > lastSync)
                    .OrderBy(log => log.Timestamp)
                    .Take(batchSize)
                    .ToPaginatedResultAsync(offSet, batchSize, CancellationToken.None, "", false);

                return ServiceResult<PaginationDetails<SyncLogDto>>.Success(result.Adapt<PaginationDetails<SyncLogDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching changes from SyncDAL: {Error}", ex);
                return ServiceResult<PaginationDetails<SyncLogDto>>.Failure(
                    new ServerErrorException("Could not fetch online api changes."));
            }
        }

        public async Task<ServiceResult<bool>> RetryPendingSyncJobs()
        {
            try
            {
                var jobs = GetPendingSyncJobs();
                _logger.LogInformation("Found {JobCount} sync jobs to retry", jobs.Count);

                foreach (var job in jobs)
                {
                    if (await ShouldRetryJob(job))
                    {
                        await RequeueJob(job);
                    }
                }
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while retrying pending sync jobs: {Error}", ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not retry sync jobs."));
            }
        }

        public async Task<bool> IsInternetAvailable()
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            foreach (var url in headUrls)
            {
                try
                {
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode) return true;
                }
                catch
                {
                    // Ignore individual failures
                }
            }
            return false;
        }

        public async Task SyncWithOnlineApi(string onlineUrl, string method, Dictionary<string, string> headers, string requestBody)
        {
            var jobrequest = new JobContext
            {
                OnlineApiUrl = onlineUrl,
                Method = method,
                Headers = headers,
                RequestBody = requestBody,
            };
            var jobId = BackgroundJob.Enqueue<SyncDAL>(x =>
                x.ProcessSyncJobAsync(
                    jobrequest
                ));
            _logger.LogInformation($"Enqueued sync job {jobId}");
        }

        public void SyncWithOnlineApi(HttpRequest originalRequest, object? requestBody)
        {
            var jobId = BackgroundJob.Enqueue<SyncDAL>(x =>
                x.ProcessSyncJobAsync(
                    JobContext.FromRequest(originalRequest, requestBody)
                ));

            _logger.LogInformation($"Enqueued sync job {jobId}");
        }

        [AutomaticRetry(Attempts = 0)] // Disable automatic retries
        public async Task ProcessSyncJobAsync(JobContext context)
        {
            await UpdateConfigSettings();
            if (!(bool.TryParse(_configuration["OnlineApi:IsEnabled"], out var settingval) && settingval))
            {
                _logger.LogWarning("Sync job aborted - sync disabled");
                throw new SyncDisabledException();
            }
            var apiKey = _configuration["OnlineApi:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey)) context.Headers.TryAdd("X-Offline-Api-Key", apiKey);
            else
            {
                _logger.LogWarning("Sync job aborted - Api key not found in config");
                throw new SyncDisabledException();
            }

            if (!await IsInternetAvailable())
            {
                _logger.LogWarning("Sync job aborted - no internet");
                throw new NoInternetException();
            }
            if (context.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var userId = _authorizationDAL.GetUserIdFromToken(authHeader.ToString().Replace("Bearer", "").Trim());
                context.Headers.TryAdd("X-UserId", userId);
            }
            context.Headers.Remove("Authorization"); // Remove any existing Authorization header to avoid conflicts

            await ExecuteSyncRequest(context);
        }

        public async Task UpdateConfigSettings()
        {
            if (string.IsNullOrEmpty(_configuration["OnlineApi:ApiKey"]))
            {
                var configs = await _context.tbl_Configurations.IgnoreQueryFilters().ToListAsync();
                if (configs != null && configs.Count > 0)
                {
                    var settings = configs.ToDictionary(s => s.ConfigId, s => s.StringValue ?? string.Empty);
                    statics.allSettings = settings;
                    _configuration["OnlineApi:ApiKey"] = settings.TryGetValue((int)statics.Configurations.OnlineSyncToken, out var token) ? token : "";
                    _configuration["OnlineApi:IsEnabled"] = settings.TryGetValue((int)statics.Configurations.OnlineSyncEnabled, out var onlineallowed) ? onlineallowed : "false";
                }
            }
            if (string.IsNullOrEmpty(_configuration["OnlineApi:SyncTokenExp"]) || (!string.IsNullOrEmpty(_configuration["OnlineApi:SyncTokenExp"]) && DateTime.Parse(_configuration["OnlineApi:SyncTokenExp"]).ToUniversalTime() < DateTime.UtcNow.AddMinutes(1)))
            {
                AppUser user = null;
                if (string.IsNullOrEmpty(_configuration["OfflineApi:SyncUserId"]))
                {
                    user = await _context.Users.IgnoreQueryFilters().OrderBy(x => x.DateTimeCreated).FirstOrDefaultAsync(x => x.IsDeleted != true);
                    _configuration["OfflineApi:SyncUserId"] = user?.Id ?? string.Empty;
                }
                else
                {
                    user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == _configuration["OfflineApi:SyncUserId"] && x.IsDeleted != true);
                }
                var tokenObj = await _authorizationDAL.GenerateToken(user, user.TenantId);
                _configuration["OnlineApi:SyncTokenExp"] = tokenObj.exp.ToString();
                _configuration["OnlineApi:SyncToken"] = tokenObj.token;
                _configuration["OnlineApi:SyncUserId"] = user.Id;
                _configuration["OnlineApi:SyncTenantId"] = user.TenantId;
            }
        }

        private List<KeyValuePair<string, FailedJobDto>> GetPendingSyncJobs()
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var jobs = new List<KeyValuePair<string, FailedJobDto>>();

            // Get failed jobs
            var failedJobs = monitoringApi.FailedJobs(0, int.MaxValue)
                .Where(j => IsSyncJob(j.Value.Job))
                .ToList();

            // Get scheduled jobs (convert ScheduledJobDto to FailedJobDto-like structure for consistency)
            var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue)
                .Where(j => IsSyncJob(j.Value.Job))
                .Select(j => new KeyValuePair<string, FailedJobDto>(
                    j.Key,
                    new FailedJobDto
                    {
                        Job = j.Value.Job,
                        InFailedState = false, // Scheduled jobs are not failed
                        FailedAt = null,
                        ExceptionType = null,
                        ExceptionMessage = null,
                        ExceptionDetails = null
                    }))
                .ToList();

            jobs.AddRange(failedJobs);
            jobs.AddRange(scheduledJobs);

            return jobs;
        }

        private List<KeyValuePair<string, FailedJobDto>> GetFailedSyncJobs()
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            return monitoringApi.FailedJobs(0, int.MaxValue)
                .Where(j => IsSyncJob(j.Value.Job))
                .ToList();
        }

        private bool IsSyncJob(Job job)
        {
            // Check if the job is for the ProcessSyncJobAsync method
            return job != null && job.Method.Name == nameof(ProcessSyncJobAsync);
        }

        private async Task<bool> ShouldRetryJob(KeyValuePair<string, FailedJobDto> job)
        {
            // If the job is not in a failed state (e.g., scheduled job), it can be retried
            if (!job.Value.InFailedState)
            {
                return true;
            }

            // For failed jobs, check the exception type
            return job.Value.ExceptionType switch
            {
                nameof(SyncDisabledException) => settings,
                nameof(NoInternetException) => await IsInternetAvailable(),
                _ => false
            };
        }

        private async Task RequeueJob(KeyValuePair<string, FailedJobDto> job)
        {
            try
            {
                // Extract the JobContext from the job arguments
                var context = JobHelper.FromJson<JobContext>(job.Value.Job.Args[0].ToString());

                // Enqueue new job with original parameters
                var newJobId = _backgroundJobClient.Enqueue<SyncDAL>(
                    x => x.ProcessSyncJobAsync(context)
                );

                // Delete the old job
                BackgroundJob.Delete(job.Key);

                _logger.LogInformation("Requeued job {OldJobId} as {NewJobId}", job.Key, newJobId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to requeue job {JobId}: {Error}", job.Key, ex);
            }
        }

        private async Task ExecuteSyncRequest(JobContext context)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient("SyncClient");
                client.BaseAddress = new Uri(context.OnlineApiUrl);

                // Recreate original request
                var request = new HttpRequestMessage(new HttpMethod(context.Method), context.RequestPath);

                // Properly set content and content type
                request.Content = new StringContent(context.RequestBody, Encoding.UTF8, "application/json");

                // Apply headers, excluding content-type from headers as it's already set above
                foreach (var header in context.Headers)
                {
                    if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var response = await client.SendAsync(request);
                var res = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Syncing with online API completed. Response received.");

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while syncing with online API: {Error}", ex);
                // Do not throw or expose details to user
            }
        }

        public async Task PullChangesFromOnlineAsync()
        {
            var isOnline = _onlineIdentityVerifier.IsOnlineApi();
            if (isOnline)
            {
                return;
            }
            // 1. Check sync conditions
            if (!settings) return;
            if (!await IsInternetAvailable())
            {
                _logger.LogWarning("Sync job aborted - no internet");
                return;
            }

            // 2. Get last sync timestamp
            var lastSync = (await _context.tbl_SyncLogs.OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync())?.DateTimeCreated ?? DateTime.UtcNow.AddYears(-100);

            var apiKey = _configuration["OnlineApi:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                await UpdateConfigSettings();
                apiKey = _configuration["OnlineApi:ApiKey"];
            }

            // 3. Fetch changes from online API
            var onlineBaseUrl = _configuration["OnlineApi:BaseUrl"];
            using var httpClient = _httpClientFactory.CreateClient();

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Sync job aborted - no token");
                return;
            }
            var tenantId = ExtractTenantId(apiKey);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("TenantId", _configuration["OnlineApi:SyncTenantId"]);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Offline-Api-Key", _configuration["OnlineApi:ApiKey"]);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-UserId", _configuration["OnlineApi:SyncUserId"]);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("IsOnlineSync", "true");

            var offset = 0;
            var batchSize = 100;
            bool isMoreData = false;

            do
            {
                try
                {
                    var response = await httpClient.GetAsync(
                        $"{onlineBaseUrl}/api/sync/getchanges?lastSync={lastSync:o}&Offset={offset}&batchSize={batchSize}");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to fetch changes from online API. StatusCode: {StatusCode}", response.StatusCode);
                        isMoreData = false; // Stop if the request fails
                        return;
                    }

                    var changes = await response.Content.ReadFromJsonAsync<PaginationDetails<SyncLogDto>>();
                    if (changes.IsNext)
                    {
                        isMoreData = true; //more data to process
                        offset = batchSize * (offset + batchSize);
                    }
                    else
                    {
                        isMoreData = false; // No more data to process
                    }
                    // 4. Apply changes locally
                    foreach (var change in changes.Data)
                    {
                        await ReplayChangeLocally(change);
                    }

                    // 5. Update last sync timestamp
                    if (changes.Data.Any())
                    {
                        var newLastSync = changes.Data.Max(c => c.Timestamp);
                        await _context.AddAsync(new tbl_SyncLog
                        {
                            TenantId = tenantId,
                            Timestamp = newLastSync,
                            Method = "Default",
                            Endpoint = "Default",
                            Headers = null,
                            Payload = string.Empty
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occurred while pulling changes from online API: {Error}", ex);
                    return;
                }
            } while (isMoreData);
        }

        public static string? ExtractTenantId(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(jwtToken);

                // Find the "TenantId" claim
                var tenantIdClaim = jwt.Claims.FirstOrDefault(c =>
                    c.Type.Equals("TenantId", StringComparison.OrdinalIgnoreCase));

                return tenantIdClaim?.Value;
            }
            catch (Exception)
            {
                // Log or handle parsing error as needed
                return null;
            }
        }

        private async Task ReplayChangeLocally(SyncLogDto change)
        {
            var offlineBaseUrl = _configuration["OfflineApi:BaseUrl"];

            using var client = _httpClientFactory.CreateClient();

            try
            {
                if (!string.IsNullOrEmpty(change.Headers))
                {
                    var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(change.Headers);
                    if (headers.TryGetValue("Authorization", out _)) headers.Remove("Authorization"); // Remove Authorization header to avoid conflicts
                    headers.Add("Authorization", $"Bearer {_configuration["OnlineApi:SyncToken"]}"); // Add the user JWT
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                var request = new HttpRequestMessage(
                    new HttpMethod(change.Method),
                    $"{offlineBaseUrl}/{change.Endpoint.TrimStart('/')}") // Local API endpoint
                {
                    Content = new StringContent(change.Payload, Encoding.UTF8, "application/json")
                };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", change.UserJwt);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while applying change locally: {Error}", ex);
                // Do not throw or expose details to user
            }
        }
    }
}
