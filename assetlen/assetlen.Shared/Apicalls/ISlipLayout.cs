using assetlen.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ISlipLayout
    {
        [Put("/api/Slips/UpdateOrCreateSlipsUsingSlipID")]
        Task<IApiResponse<SizeDto>> UpdateOrCreateSlipsUsingSlipID([Body] SizeDto input);
        [Get("/api/Slips/GetSlipdetailsFromDBbasedOnslipID")]
        Task<IApiResponse<SizeDto>> GetSlipdetailsFromDBbasedOnslipID([Query] string sizeId);
    }
}
