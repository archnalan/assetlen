using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface IProductRelationshipsDAL
	{
		Task<ServiceResult<ProductRelationshipDto>> AddProductRelationship(ProductRelationshipDto pr);
		Task<ServiceResult<List<ProductRelationshipDto>>> AddProductRelationships(List<ProductRelationshipDto> productRelationships);
		Task<ServiceResult<List<ProductRelationshipDto>>> CreateUpdateRelationshipsByParentId(string parentId, List<ProductRelationshipDto> relationsDto);
		Task<ServiceResult<ProductRelationshipDto>> GetProdRelationshipbasedOnhasSubAndIssubProd(string issubProd, string hasSubProd);
		Task<ServiceResult<List<ProductRelationshipDto>>> GetProdRelationshipBbasedIssubProd(string issubProd);
		Task<ServiceResult<List<ProductRelationshipDto>>> GetRelationsByHasSubProdID(string hasSubProd);
		Task<ServiceResult<List<string>>> GetSubProductIds(string patentProductId);
		Task<ServiceResult<bool>> HardDeleteProduRelationshipBbasedOnHasSubProdIDAndIssubProd(string issubProd, string hasSubProd);
		Task<ServiceResult<bool>> HardDeleteProduRelationshipBbasedOnHasSubProductID(string hasSubProd);
		Task<ServiceResult<bool>> HardDeleteProduRelationshipBbasedOnRelationShipID(string relationId);
		Task<ServiceResult<ProductRelationshipDto>> UpdateProductRelationShip(string id, ProductRelationshipDto pr);
		Task<ServiceResult<ProductRelationshipDto>> UpdateProductRelationShipBasedonIsSubAndHasSubIDs(string isSubId, string hasSubId);
		Task<ServiceResult<ProductRelationshipDto>> UpdateSortOrderBasedonIsSubAndHasSubIDs(ProductRelationshipDto pr);
	}
}