using assetlen.Shared.Models.Models.DocumentModels;
using assetlen.Shared.Models.Models.ViewModels;
using Refit;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Apicalls
{
    public interface IProductDetailApi
    {
        [Get("/api/ProductDetail/GetPreviewSectionByProductId")]
        Task<IApiResponse<List<ProductDetailDto>>> GetPreviewSectionByProductId([Query][Required] string productId, [Query] CancellationToken cancellationToken = default);

        [Get("/api/ProductDetail/GetSectionsByProductId")]
        Task<IApiResponse<List<ProductDetailDto>>> GetSectionsByProductId([Query][Required] string productId, [Query] CancellationToken cancellationToken = default);

        [Get("/api/ProductDetail/GetSectionById")]
        Task<IApiResponse<ProductDetailDto>> GetSectionById([Query][Required] string id, [Query] CancellationToken cancellationToken = default);

        [Post("/api/ProductDetail/AddSection")]
        Task<IApiResponse<ProductDetailDto>> AddSection([Body] ProductDetailCreateDto dto, [Query] CancellationToken cancellationToken = default);

        [Post("/api/ProductDetail/AddSectionsBulk")]
        Task<IApiResponse<bool>> AddSectionsBulk([Query][Required] string productId, [Body] List<ProductDetailCreateDto> sections, [Query] CancellationToken cancellationToken = default);

        [Put("/api/ProductDetail/UpdateSection")]
        Task<IApiResponse<ProductDetailDto>> UpdateSection([Body] ProductDetailUpdateDto dto, [Query] CancellationToken cancellationToken = default);

        [Put("/api/ProductDetail/UpdateSectionContent")]
        Task<IApiResponse<bool>> UpdateSectionContent([Query][Required] string id, [Body][Required] string content, [Query] CancellationToken cancellationToken = default);

        [Put("/api/ProductDetail/UpdateSectionTitle")]
        Task<IApiResponse<bool>> UpdateSectionTitle([Query][Required] string id, [Query][Required] string title, [Query] CancellationToken cancellationToken = default);

        [Put("/api/ProductDetail/ReorderSections")]
        Task<IApiResponse<bool>> ReorderSections([Query][Required] string productId, [Body] List<SectionOrderChangeDto> newOrder, [Query] CancellationToken cancellationToken = default);

        [Delete("/api/ProductDetail/DeleteSection")]
        Task<IApiResponse<bool>> DeleteSection([Query][Required] string id, [Query] CancellationToken cancellationToken = default);

        [Delete("/api/ProductDetail/DeleteAllSectionsForProduct")]
        Task<IApiResponse<bool>> DeleteAllSectionsForProduct([Query][Required] string productId, [Query] CancellationToken cancellationToken = default);

        [Post("/api/ProductDetail/UpsertSection")]
        Task<IApiResponse<ProductDetailDto>> UpsertSection([Body] ProductDetailUpsertDto dto, [Query] CancellationToken cancellationToken = default);

        [Post("/api/ProductDetail/DuplicateSection")]
        Task<IApiResponse<ProductDetailDto>> DuplicateSection([Query][Required] string id, [Query] CancellationToken cancellationToken = default);

        [Post("/api/ProductDetail/NormalizeSortOrder")]
        Task<IApiResponse<bool>> NormalizeSortOrder([Query][Required] string productId, [Query] CancellationToken cancellationToken = default);

        [Get("/api/ProductDetail/SearchSections")]
        Task<IApiResponse<List<ProductDetailDto>>> SearchSections([Query][Required] string productId, [Query] string keyword, [Query] CancellationToken cancellationToken = default);

        [Get("/api/ProductDetail/GetDocumentSnapshot")]
        Task<IApiResponse<Dictionary<string, string>>> GetDocumentSnapshot([Query][Required] string productId, [Query] CancellationToken cancellationToken = default);

        [Post("/api/ProductDetail/SaveDocument")]
        Task<IApiResponse<bool>> SaveDocument([Query][Required] string productId, [Body] List<ProductDetailPersistDto> sections, [Query] CancellationToken cancellationToken = default);
    }
}