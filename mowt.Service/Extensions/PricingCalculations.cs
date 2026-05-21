using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using Mapster;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.Extensions
{
    public class PricingCalculations : IPricingCalculations
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PricingCalculations(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public ServiceResult<ProductPricing> ProductCalculationChecks([Required] ProductPricing p, bool? ignoreCheck, bool? takeCalculated)
        {
            if (ignoreCheck == false || ignoreCheck == null)
            {
                if (p.Tax == null && (p.CostInclusive != null || p.PriceInclusive != null))
                    return ServiceResult<ProductPricing>.Failure(new BadRequestException("Tax is required for tax-dependent fields."));
                // Validate Cost Inclusive and Exclusive relationship
                if (p.CostExclusive.HasValue && p.Tax!.TaxValue != null && p.Tax.TaxValue.Value > 0)
                {
                    var expectedCostInc = Math.Round(p.CostExclusive.Value * ((p.Tax.TaxValue.Value / 100m) + 1), 2);
                    if (p.CostInclusive.HasValue && p.CostInclusive.Value != expectedCostInc)
                    {
                        if (takeCalculated == true)
                        {
                            p.CostInclusive = expectedCostInc;
                        }
                        else
                        {
                            return ServiceResult<ProductPricing>.Failure(
                                new BadRequestException($"Cost Inclusive ({p.CostInclusive.Value}) does not match calculated Cost Inclusive ({expectedCostInc}) based on Cost Exclusive and Tax."));
                        }
                    }
                }

                // Validate Selling Price Inclusive and Exclusive relationship
                if (p.PriceExclusive.HasValue && p.Tax != null && p.Tax.TaxValue != null && p.Tax.TaxValue.HasValue && p.Tax.TaxValue.Value > 0)
                {
                    var expectedPriceInc = Math.Round(p.PriceExclusive.Value * ((p.Tax.TaxValue.Value / 100m) + 1), 2);
                    if (p.PriceInclusive.HasValue && p.PriceInclusive.Value != expectedPriceInc)
                    {
                        if (takeCalculated == true)
                        {
                            p.PriceInclusive = expectedPriceInc;
                        }
                        else
                        {
                            return ServiceResult<ProductPricing>.Failure(
                                new BadRequestException($"Selling Price Inclusive ({p.PriceInclusive.Value}) does not match calculated Selling Price Inclusive ({expectedPriceInc}) based on Price Exclusive and Tax."));
                        }
                    }
                }

                // Validate markup effect on Price Exclusive
                if (p.CostExclusive.HasValue && p.PriceExclusive.HasValue && p.MarkUp.HasValue)
                {
                    var expectedPriceExc = Math.Round(p.CostExclusive.Value * ((p.MarkUp.Value / 100m) + 1), 2);
                    if (p.PriceExclusive.Value != expectedPriceExc)
                    {
                        if (takeCalculated == true)
                        {
                            p.PriceExclusive = expectedPriceExc;
                        }
                        else
                        {
                            return ServiceResult<ProductPricing>.Failure(
                                new BadRequestException($"Price Exclusive ({p.PriceExclusive.Value}) does not match calculated Price Exclusive ({expectedPriceExc}) based on Cost Exclusive and markup."));
                        }
                    }
                }

                // Validate Wholesale Price Inclusive and Exclusive relationship
                if (p.PriceExclusive2.HasValue && p.Tax != null && p.Tax.TaxValue != null && p.Tax.TaxValue.HasValue && p.Tax.TaxValue.Value > 0)
                {
                    var expectedWholesalePriceInc = Math.Round(p.PriceExclusive2.Value * ((p.Tax.TaxValue.Value / 100m) + 1), 2);
                    if (p.PriceInclusive2.HasValue && p.PriceInclusive2.Value != expectedWholesalePriceInc)
                    {
                        if (takeCalculated == true)
                        {
                            p.PriceInclusive2 = expectedWholesalePriceInc;
                        }
                        else
                        {
                            return ServiceResult<ProductPricing>.Failure(
                                new BadRequestException($"Wholesale Price Inclusive ({p.PriceInclusive2.Value}) does not match calculated Wholesale Price Inclusive ({expectedWholesalePriceInc}) based on Wholesale Price Exclusive and Tax."));
                        }
                    }
                }
            }

            return ServiceResult<ProductPricing>.Success(p);
        }

        public ServiceResult<TransactionDetailDto> FullCalculationsCheck(TransactionDetailDto transactionDetail, ProductPricing productPricing, bool takeCalculated = true)
        {
            // use changed prices if changed
            productPricing.Adapt(transactionDetail);

            // Perform product calculation checks
            var calculationResult = ProductCalculationChecks(productPricing, false, takeCalculated);

            if (!calculationResult.IsSuccess)
            {
                return ServiceResult<TransactionDetailDto>.Failure(calculationResult.Error);
            }

            var validatedProductPricing = calculationResult.Data;
            bool hasMismatch = false;

            // Validate and optionally update transaction details
            if ((int)transactionDetail.CostExc != (int)validatedProductPricing.CostExclusive)
            {
                if (takeCalculated)
                {
                    transactionDetail.CostExc = validatedProductPricing.CostExclusive;
                }
                else
                {
                    hasMismatch = true;
                }
            }

            if ((int)transactionDetail.CostInc != (int)validatedProductPricing.CostInclusive)
            {
                if (takeCalculated)
                {
                    transactionDetail.CostInc = validatedProductPricing.CostInclusive;
                }
                else
                {
                    hasMismatch = true;
                }
            }

            if ((int)transactionDetail.PriceExc != (int)validatedProductPricing.PriceExclusive)
            {
                if (takeCalculated)
                {
                    transactionDetail.PriceExc = validatedProductPricing.PriceExclusive;
                }
                else
                {
                    hasMismatch = true;
                }
            }

            if ((int)transactionDetail.PriceInc != (int)validatedProductPricing.PriceInclusive)
            {
                if (takeCalculated)
                {
                    transactionDetail.PriceInc = validatedProductPricing.PriceInclusive;
                }
                else
                {
                    hasMismatch = true;
                }
            }

            if (hasMismatch && !takeCalculated)
            {
                return ServiceResult<TransactionDetailDto>.Failure(
                    new BadRequestException("Invalid values, Please re-check your tax, cost, and selling prices"));
            }


            // Recalculate totals if values were updated or are mismatched
            if (transactionDetail.Qty.HasValue)
            {

                transactionDetail.TotalPriceExc = transactionDetail.PriceExc * transactionDetail.Qty;
                transactionDetail.TotalPriceInc = transactionDetail.PriceInc * transactionDetail.Qty;


            }

            return ServiceResult<TransactionDetailDto>.Success(transactionDetail);
        }

    }

}
