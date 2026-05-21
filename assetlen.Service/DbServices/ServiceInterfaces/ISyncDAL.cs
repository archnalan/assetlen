using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface ISyncDAL
    {

        Task<ServiceResult<bool>> RetryPendingSyncJobs();
        void SyncWithOnlineApi(HttpRequest originalRequest, object? requestBody);
        Task<ServiceResult<PaginationDetails<SyncLogDto>>> GetChangesFromOnlineApi(DateTime lastSync, int offSet = 0, int batchSize = 100);
        Task<bool> IsInternetAvailable();
    }
}