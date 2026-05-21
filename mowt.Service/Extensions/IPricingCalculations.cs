using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using System.ComponentModel.DataAnnotations;

namespace mowt.Service.Extensions
{
    public interface IPricingCalculations
    {
        ServiceResult<ProductPricing> ProductCalculationChecks([Required] ProductPricing p, bool? ignoreCheck, bool? takeCalculated);
        ServiceResult<TransactionDetailDto> FullCalculationsCheck(TransactionDetailDto transactionDetail, ProductPricing productPricing, bool takeCalculated);
    }
}