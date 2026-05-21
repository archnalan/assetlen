using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.API.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace assetlen.API.Domain
{
    public class RecoveryActionService : IRecoveryActionService
    {
        private readonly IServiceProvider _services;

        public RecoveryActionService(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Logic that gets called when ever internet connection is restored
        /// </summary>
        /// <returns></returns>
        public async Task PerformRecoveryAsync()
        {
            // Implement your recovery logic here
            var _syncDAL = _services.GetRequiredService<ISyncDAL>(); // Resolve ISyncDAL
            var syncResult = await _syncDAL.RetryPendingSyncJobs();
        }
    }
}
