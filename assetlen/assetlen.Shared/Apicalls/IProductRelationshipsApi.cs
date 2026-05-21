using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using assetlen.Shared.Models.Models.ViewModels;
using Refit;

namespace assetlen.Shared.Apicalls
{
    public interface IProductRelationshipsApi
    {
        [Get("/api/ProductRelationships/GetProdRelationshipbasedOnhasSubAndIssubProd")]
        Task<ApiResponse<List<ProductRelationshipDto>>> GetProdRelationshipbasedOnhasSubAndIssubProd([Query] string issubProdID, [Query] int hasSubProdID);

        [Get("/api/ProductRelationships/GetProdRelationshipBbasedIssubProd")]
        Task<ApiResponse<List<ProductRelationshipDto>>> GetProdRelationshipBbasedIssubProd([Query] string issubProdID);

        [Get("/api/ProductRelationships/GetProdRelationshipBbasedOnhasSubProductID")]
        Task<ApiResponse<List<ProductRelationshipDto>>> GetProdRelationshipBbasedOnhasSubProductID([Query] string hasSubProdID);

        [Put("/api/ProductRelationships/UpdateProductRelationShip")]
        Task<ApiResponse<ProductRelationshipDto>> UpdateProductRelationShip([Query] string relationId, [Body] ProductRelationshipDto prDto);

        [Put("/api/ProductRelationships/UpdateProductRelationShipBasedonIsSubAndHasSubIDs")]
        Task<ApiResponse<ProductRelationshipDto>> UpdateProductRelationShipBasedonIsSubAndHasSubIDs([Query] string issubProdID, [Query] string hasSubProdID);

        [Put("/api/ProductRelationships/UpdateSortOrderBasedonIsSubAndHasSubIDs")]
        Task<ApiResponse<ProductRelationshipDto>> UpdateSortOrderBasedonIsSubAndHasSubIDs([Body] ProductRelationshipDto prDto);

        [Delete("/api/ProductRelationships/HardDeleteProduRelationshipBbasedOnRelationShipID")]
        Task<ApiResponse<object>> HardDeleteProduRelationshipBbasedOnRelationShipID([Query] string id);

        [Delete("/api/ProductRelationships/HardDeleteProduRelationshipBbasedOnHasSubProdIDAndIssubProd")]
        Task<ApiResponse<object>> HardDeleteProduRelationshipBbasedOnHasSubProdIDAndIssubProd([Query] string issubProdID, [Query] string hasSubProdID);

        [Delete("/api/ProductRelationships/HardDeleteProduRelationshipBbasedOnHasSubProductID")]
        Task<ApiResponse<object>> HardDeleteProduRelationshipBbasedOnHasSubProductID([Query] string hasSubProdID);

        [Post("/api/ProductRelationships/CreateNewProductRelationShipFromDB")]
        Task<ApiResponse<ProductRelationshipDto>> CreateNewProductRelationShipFromDB([Body] ProductRelationshipDto prDto);
    }
}
