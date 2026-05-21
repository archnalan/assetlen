using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Service.Extensions
{
    public interface IPricingCalculations
    {
        ServiceResult<ProductPricing> ProductCalculationChecks([Required] ProductPricing p, bool? ignoreCheck, bool? takeCalculated);
        ServiceResult<TransactionDetailDto> FullCalculationsCheck(TransactionDetailDto transactionDetail, ProductPricing productPricing, bool takeCalculated);
    }
}