using mowt.ServiceHandler;
using mowt.Shared.Models.Models.DocumentModels;
using mowt.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IProductDetailDAL
    {
        Task<ServiceResult<List<ProductDetailDto>>> GetPreviewSectionByProductId(string productId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ProductDetailDto>>> GetSectionsByProductId(string productId, CancellationToken cancellationToken = default);
        Task<ServiceResult<ProductDetailDto>> GetSectionById(string id, CancellationToken cancellationToken = default);
        Task<ServiceResult<ProductDetailDto>> AddSection(ProductDetailCreateDto dto, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> AddSectionsBulk(string productId, List<ProductDetailCreateDto> sections, CancellationToken cancellationToken = default);
        Task<ServiceResult<ProductDetailDto>> UpdateSection(ProductDetailUpdateDto dto, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> UpdateSectionContent(string id, string content, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> UpdateSectionTitle(string id, string title, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> ReorderSections(string productId, List<SectionOrderChangeDto> newOrder, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> DeleteSection(string id, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> DeleteAllSectionsForProduct(string productId, CancellationToken cancellationToken = default);
        Task<ServiceResult<ProductDetailDto>> UpsertSection(ProductDetailUpsertDto dto, CancellationToken cancellationToken = default);
        Task<ServiceResult<ProductDetailDto>> DuplicateSection(string id, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> NormalizeSortOrder(string productId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ProductDetailDto>>> SearchSections(string productId, string keyword, CancellationToken cancellationToken = default);
        Task<ServiceResult<Dictionary<string, string>>> GetDocumentSnapshot(string productId, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> SaveDocument(string productId, List<ProductDetailPersistDto> sections, CancellationToken cancellationToken = default);
    }
}
