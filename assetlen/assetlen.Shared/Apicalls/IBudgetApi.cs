using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

public interface IBudgetApi
{
    [Get("/api/Budget/GetSummary")]
    Task<IApiResponse<ProjectBudgetSummaryDto>> GetSummary([Query] string projectId);

    [Post("/api/Budget/AddLineItem")]
    Task<IApiResponse<BudgetLineItemDto>> AddLineItem([Body] BudgetLineItemCreateDto dto);

    [Put("/api/Budget/UpdateLineItem")]
    Task<IApiResponse<BudgetLineItemDto>> UpdateLineItem([Body] BudgetLineItemUpdateDto dto);

    [Delete("/api/Budget/DeleteLineItem")]
    Task<IApiResponse<bool>> DeleteLineItem([Query] string lineItemId);

    [Post("/api/Budget/AddReceipt")]
    Task<IApiResponse<ReceiptDto>> AddReceipt([Body] ReceiptCreateDto dto);

    [Get("/api/Budget/GetReceiptsByLineItem")]
    Task<IApiResponse<List<ReceiptDto>>> GetReceiptsByLineItem([Query] string lineItemId);

    [Delete("/api/Budget/DeleteReceipt")]
    Task<IApiResponse<bool>> DeleteReceipt([Query] string receiptId);
}
