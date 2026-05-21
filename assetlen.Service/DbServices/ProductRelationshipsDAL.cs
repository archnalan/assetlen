using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
	public class ProductRelationshipsDAL : IProductRelationshipsDAL
	{
		private readonly AssetlenDbContext _context;
		private readonly ILogger<ProductRelationshipsDAL> _logger;

		public ProductRelationshipsDAL(ILogger<ProductRelationshipsDAL> logger, AssetlenDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		#region Read prodRelationSHipIID from Database based on bothIDS
		public async Task<ServiceResult<ProductRelationshipDto>> GetProdRelationshipbasedOnhasSubAndIssubProd(string issubProd, string hasSubProd)
		{
			try
			{
				var prodRelationship = await _context.tbl_ProductRelationships
												.FirstOrDefaultAsync(c => c.HasAsubProductId == hasSubProd
												&& c.IsAsubProductId == issubProd);

				if (prodRelationship == null)
				{
					return ServiceResult<ProductRelationshipDto>.Failure(
						new NotFoundException($"Product relationship that has sub-Product ID: {hasSubProd} and is a sub-product ID: {issubProd} not found."));
				}

				return ServiceResult<ProductRelationshipDto>.Success(prodRelationship.Adapt<ProductRelationshipDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching product relationship for HasAsubProductId: {HasSubProd}, IsAsubProductId: {IsSubProd}", hasSubProd, issubProd);
				return ServiceResult<ProductRelationshipDto>.Failure(
					new ServerErrorException("Could not fetch product relationship."));
			}
		}
		#endregion

		#region Read prodRelationSHipIID from Database based on isSubProductID
		public async Task<ServiceResult<List<ProductRelationshipDto>>> GetProdRelationshipBbasedIssubProd(string issubProd)
		{
			try
			{
				var prodRelationship = await _context.tbl_ProductRelationships
												.Where(p => p.IsAsubProductId == issubProd)
												.ToListAsync();

				if (prodRelationship == null)
				{
					return ServiceResult<List<ProductRelationshipDto>>.Failure(
						new NotFoundException($"Product relationships with is a sub-product ID: {issubProd} not found."));
				}

				var prodRelationshipDto = prodRelationship.Adapt<List<ProductRelationshipDto>>();

				return ServiceResult<List<ProductRelationshipDto>>.Success(prodRelationshipDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching product relationships for IsAsubProductId: {IsSubProd}", issubProd);
				return ServiceResult<List<ProductRelationshipDto>>.Failure(
					new ServerErrorException("Could not fetch product relationships."));
			}
		}
		#endregion

		#region Read prodRelationSHipIID from Database based on hasProdID
		public async Task<ServiceResult<List<ProductRelationshipDto>>> GetRelationsByHasSubProdID(string hasSubProd)
		{
			try
			{
				var prodRelationship = await _context.tbl_ProductRelationships
												.Where(p => p.HasAsubProductId == hasSubProd)
												.ToListAsync();

				var prodRelationshipDto = prodRelationship.Adapt<List<ProductRelationshipDto>>();

				return ServiceResult<List<ProductRelationshipDto>>.Success(prodRelationshipDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching product relationships for HasAsubProductId: {HasSubProd}", hasSubProd);
				return ServiceResult<List<ProductRelationshipDto>>.Failure(
					new ServerErrorException("Could not fetch product relationships."));
			}
		}
		#endregion

		#region Get Product Ids based on parent product ID
		public async Task<ServiceResult<List<string?>>> GetSubProductIds(string patentProductId)
		{
			try
			{
				var subProductIds = await _context.tbl_ProductRelationships
					.Where(p => p.HasAsubProductId == patentProductId)
					.Select(p => p.IsAsubProductId)
					.ToListAsync();
				if (subProductIds == null || !subProductIds.Any())
				{
					return ServiceResult<List<string?>>.Failure(
						new NotFoundException($"No sub-products found for parent product ID: {patentProductId}."));
				}
				return ServiceResult<List<string?>>.Success(subProductIds);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching sub-product IDs for parent product ID: {ParentProductId}", patentProductId);
				return ServiceResult<List<string?>>.Failure(
					new ServerErrorException("Could not fetch sub-product IDs."));
			}
		}
		#endregion

		#region update productRelationShip in the  DB

		public async Task<ServiceResult<ProductRelationshipDto>> UpdateProductRelationShipBasedonIsSubAndHasSubIDs(string isSubProdId, string hasSubProdID)

		{
			var relationInDb = await _context.tbl_ProductRelationships
												.FirstOrDefaultAsync(c => c.HasAsubProductId == hasSubProdID
												&& c.IsAsubProductId == isSubProdId);


			if (relationInDb == null) return ServiceResult<ProductRelationshipDto>.Failure(
				new NotFoundException($"Product relationship with IDs: {isSubProdId} and {hasSubProdID} not found."));

			try
			{
				//Updating the fields

				relationInDb.SortOrder = relationInDb.SortOrder ?? relationInDb.SortOrder;
				relationInDb.Qty = relationInDb.Qty ?? relationInDb.Qty;

				await _context.SaveChangesAsync();

				return ServiceResult<ProductRelationshipDto>.Success(relationInDb.Adapt<ProductRelationshipDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while updating product relationship for IsAsubProductId: {IsSubProdId}, HasAsubProductId: {HasSubProdID}", isSubProdId, hasSubProdID);
				return ServiceResult<ProductRelationshipDto>.Failure(
							new ServerErrorException("Could not update product relationship."));
			}
		}
		#endregion

		#region update productRelationShip in the  DB

		public async Task<ServiceResult<ProductRelationshipDto>> UpdateProductRelationShip(string id, ProductRelationshipDto pr)

		{
			if (pr == null) return ServiceResult<ProductRelationshipDto>.Failure(
								new BadRequestException("Product relationship data required"));

			if (pr.Id != id) return ServiceResult<ProductRelationshipDto>.Failure(
					new BadRequestException($"Product relationship with ID: {id} is not the same as product relationship with ID: {pr.Id}"));

			var relationInDb = await _context.tbl_ProductRelationships
												.FirstOrDefaultAsync(c => c.Id == id);

			if (relationInDb == null) return ServiceResult<ProductRelationshipDto>.Failure(
				new NotFoundException($"Product relationship with ID: {id} not found."));

			if (!string.IsNullOrEmpty(pr.HasAsubProductId))
			{
				var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.HasAsubProductId);
				if (!productExists)
				{
					return ServiceResult<ProductRelationshipDto>.Failure(
						new NotFoundException($"Product with ID:{pr.HasAsubProductId} does not exist. No Relationship was created."));
				}
			}

			if (!string.IsNullOrEmpty(pr.IsAsubProductId))
			{
				var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.IsAsubProductId);
				if (!productExists)
				{
					return ServiceResult<ProductRelationshipDto>.Failure(
						new NotFoundException($"Product with ID:{pr.IsAsubProductId} does not exist. No Relationship was created."));
				}
			}

			relationInDb = await _context.tbl_ProductRelationships
											   .FirstOrDefaultAsync(c => c.Id == id);

			if (relationInDb == null) return ServiceResult<ProductRelationshipDto>.Failure(
				new NotFoundException($"Product relationship with ID: {id} not found."));

			try
			{
				//Updating the fields only when they have a value to update
				relationInDb.IsAsubProductId = pr.IsAsubProductId ?? relationInDb.IsAsubProductId;
				relationInDb.HasAsubProductId = pr.HasAsubProductId ?? relationInDb.HasAsubProductId;
				relationInDb.SortOrder = pr.SortOrder ?? relationInDb.SortOrder;
				relationInDb.Qty = pr.Qty ?? relationInDb.Qty;

				await _context.SaveChangesAsync();

				return ServiceResult<ProductRelationshipDto>.Success(relationInDb.Adapt<ProductRelationshipDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while updating product relationship with ID: {Id}", id);
				return ServiceResult<ProductRelationshipDto>.Failure(
							new ServerErrorException("Could not update product relationship."));
			}
		}
		#endregion

		#region update SortOrder in the  DB
		public async Task<ServiceResult<ProductRelationshipDto>> UpdateSortOrderBasedonIsSubAndHasSubIDs(ProductRelationshipDto pr)

		{
			if (pr == null) return ServiceResult<ProductRelationshipDto>.Failure(
								new BadRequestException("Product relationship data required"));


			var relationInDb = await _context.tbl_ProductRelationships
												.FirstOrDefaultAsync(c => c.HasAsubProductId == pr.HasAsubProductId
												&& c.IsAsubProductId == pr.IsAsubProductId);


			if (relationInDb == null) return ServiceResult<ProductRelationshipDto>.Failure(
				new NotFoundException($"Product relationship with is a sub-product ID:{pr.IsAsubProductId} and has a sub-product ID:{pr.HasAsubProductId} not found."));

			if (!string.IsNullOrEmpty(pr.HasAsubProductId))
			{
				var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.HasAsubProductId);
				if (!productExists)
				{
					return ServiceResult<ProductRelationshipDto>.Failure(
						new NotFoundException($"Product with ID:{pr.HasAsubProductId} does not exist. No Relationship was created."));
				}
			}

			if (!string.IsNullOrEmpty(pr.IsAsubProductId))
			{
				var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.IsAsubProductId);
				if (!productExists)
				{
					return ServiceResult<ProductRelationshipDto>.Failure(
						new NotFoundException($"Product with ID:{pr.IsAsubProductId} does not exist. No Relationship was created."));
				}
			}


			try
			{
				//Updating the fields
				relationInDb.SortOrder = relationInDb.SortOrder ?? relationInDb.SortOrder;
				//relationInDb.Qty = relationInDb.Qty ?? relationInDb.Qty;

				_context.tbl_ProductRelationships.Update(relationInDb);

				await _context.SaveChangesAsync();

				return ServiceResult<ProductRelationshipDto>.Success(relationInDb.Adapt<ProductRelationshipDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while updating sort order for product relationship with IsAsubProductId: {IsSubProdId}, HasAsubProductId: {HasSubProdId}", pr.IsAsubProductId, pr.HasAsubProductId);
				return ServiceResult<ProductRelationshipDto>.Failure(
							new ServerErrorException("Could not update product relationship."));
			}
		}
		#endregion

		#region Delete ProductRelationShip in the  DB
		public async Task<ServiceResult<bool>> HardDeleteProduRelationshipBbasedOnRelationShipID(string relationId)
		{
			var relationInDb = await _context.tbl_ProductRelationships.FindAsync(relationId);

			if (relationInDb == null) return ServiceResult<bool>
					.Failure(new NotFoundException($"Product relationship with ID: {relationId} not found."));

			try
			{
				_context.tbl_ProductRelationships.Remove(relationInDb);

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while deleting product relationship with ID: {RelationId}", relationId);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete product relationship."));
			}
		}
		#endregion

		#region Delete ProductRelationShip in the  DB
		public async Task<ServiceResult<bool>> HardDeleteProduRelationshipBbasedOnHasSubProdIDAndIssubProd(string issubProd, string hasSubProd)
		{
			var relationInDb = await _context.tbl_ProductRelationships
								.FirstOrDefaultAsync(c => c.IsAsubProductId == issubProd
								&& c.HasAsubProductId == hasSubProd);

			if (relationInDb == null) return ServiceResult<bool>.Failure(
				new NotFoundException($"Product relationship that has sub-Product ID: {hasSubProd} and is a sub-product ID: {issubProd} not found."));


			try
			{
				_context.tbl_ProductRelationships.Remove(relationInDb);

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while deleting product relationship with IsAsubProductId: {IsSubProd}, HasAsubProductId: {HasSubProd}", issubProd, hasSubProd);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete product relationship."));
			}
		}
		#endregion

		#region Delete ProductRelationShip in the  DB using hasSubProdID
		public async Task<ServiceResult<bool>> HardDeleteProduRelationshipBbasedOnHasSubProductID(string hasSubProd)
		{
			var relationInDb = await _context.tbl_ProductRelationships
								.FirstOrDefaultAsync(c => c.HasAsubProductId == hasSubProd);

			if (relationInDb == null) return ServiceResult<bool>.Failure(
				new NotFoundException($"Product relationship that has sub-Product ID: {hasSubProd} not found."));
			try
			{
				_context.tbl_ProductRelationships.Remove(relationInDb);

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while deleting product relationship with HasAsubProductId: {HasSubProd}", hasSubProd);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete product relationship."));
			}
		}
		#endregion

		#region Create New ProductRelationShip in DB

		public async Task<ServiceResult<ProductRelationshipDto>> AddProductRelationship(ProductRelationshipDto pr)
		{
			if (pr == null) return ServiceResult<ProductRelationshipDto>.Failure(
				new BadRequestException($"Product relationship data is required."));

			if (!string.IsNullOrEmpty(pr.HasAsubProductId))
			{
				var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.HasAsubProductId);
				if (!productExists)
				{
					return ServiceResult<ProductRelationshipDto>.Failure(
						new NotFoundException($"Product with ID:{pr.HasAsubProductId} does not exist. No Relationship was created."));
				}
			}

			if (!string.IsNullOrEmpty(pr.IsAsubProductId))
			{
				var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.IsAsubProductId);
				if (!productExists)
				{
					return ServiceResult<ProductRelationshipDto>.Failure(
						new NotFoundException($"Product with ID:{pr.IsAsubProductId} does not exist. No Relationship was created."));
				}
			}
			try
			{

				var prodRelation = pr.Adapt<tbl_ProductRelationship>();

				await _context.tbl_ProductRelationships.AddAsync(prodRelation);
				await _context.SaveChangesAsync();

				var prodRelationDto = prodRelation.Adapt<ProductRelationshipDto>();

				return ServiceResult<ProductRelationshipDto>.Success(prodRelationDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while creating product relationship for IsAsubProductId: {IsSubProdId}, HasAsubProductId: {HasSubProdId}", pr.IsAsubProductId, pr.HasAsubProductId);
				return ServiceResult<ProductRelationshipDto>.Failure(
							new ServerErrorException("Could not create product relationship."));
			}

		}
		#endregion

		#region Create Multiple ProductRelationShips in DB

		public async Task<ServiceResult<List<ProductRelationshipDto>>> AddProductRelationships(List<ProductRelationshipDto> productRelationships)
		{
			if (productRelationships == null || !productRelationships.Any())
			{
				return ServiceResult<List<ProductRelationshipDto>>.Failure(new BadRequestException("Product relationships data is required."));
			}
			var productRelationshipEntities = new List<tbl_ProductRelationship>();
			foreach (var pr in productRelationships)
			{
				if (!string.IsNullOrEmpty(pr.HasAsubProductId))
				{
					var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.HasAsubProductId);
					if (!productExists)
					{
						return ServiceResult<List<ProductRelationshipDto>>.Failure(
							new NotFoundException($"Product with ID:{pr.HasAsubProductId} does not exist. No Relationship was created."));
					}
				}
				if (!string.IsNullOrEmpty(pr.IsAsubProductId))
				{
					var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.IsAsubProductId);
					if (!productExists)
					{
						return ServiceResult<List<ProductRelationshipDto>>.Failure(
							new NotFoundException($"Product with ID:{pr.IsAsubProductId} does not exist. No Relationship was created."));
					}
				}
				var prodRelation = pr.Adapt<tbl_ProductRelationship>();
				productRelationshipEntities.Add(prodRelation);
			}
			try
			{
				await _context.tbl_ProductRelationships.AddRangeAsync(productRelationshipEntities);
				await _context.SaveChangesAsync();
				var prodRelationDtos = productRelationshipEntities.Adapt<List<ProductRelationshipDto>>();
				return ServiceResult<List<ProductRelationshipDto>>.Success(prodRelationDtos);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while creating multiple product relationships.");
				return ServiceResult<List<ProductRelationshipDto>>.Failure(
							new ServerErrorException("Could not create product relationships."));
			}
		}

		#endregion

		#region Update Multiple Relationships in Db
		public async Task<ServiceResult<List<ProductRelationshipDto>>> CreateUpdateRelationshipsByParentId(string parentId, List<ProductRelationshipDto> relationsDto)
		{
			if (relationsDto == null || !relationsDto.Any())
			{
				return ServiceResult<List<ProductRelationshipDto>>.Failure(new BadRequestException("Product relationships data is required."));
			}

			if (string.IsNullOrEmpty(parentId))
			{
				return ServiceResult<List<ProductRelationshipDto>>.Failure(new BadRequestException("Parent product ID is required."));
			}

			var parentProductExists = await _context.tbl_Products.AnyAsync(x => x.Id == parentId);
			if (!parentProductExists)
			{
				return ServiceResult<List<ProductRelationshipDto>>.Failure(
					new NotFoundException($"Parent product with ID: {parentId} does not exist."));
			}
			try
			{
				var existingRelations = await _context.tbl_ProductRelationships
					.Where(r => r.HasAsubProductId == parentId)
					.ToListAsync();

				// Handle deletions - relationships that exist but aren't in the input list
				var toDelete = existingRelations
					.Where(er => !relationsDto.Any(r => r.IsAsubProductId == er.IsAsubProductId))
					.ToList();

				if (toDelete.Any())
				{
					_context.tbl_ProductRelationships.RemoveRange(toDelete);
				}

				// Handle creations - relationships in the input that don't exist yet
				var toCreate = relationsDto
					.Where(r => !existingRelations.Any(er => er.IsAsubProductId == r.IsAsubProductId && er.HasAsubProductId == parentId))
					.ToList();

				foreach (var pr in toCreate)
				{
					// Validate products exist
					if (!string.IsNullOrEmpty(pr.IsAsubProductId))
					{
						var productExists = await _context.tbl_Products.AnyAsync(x => x.Id == pr.IsAsubProductId);
						if (!productExists)
						{
							return ServiceResult<List<ProductRelationshipDto>>.Failure(
								new NotFoundException($"Product with ID:{pr.IsAsubProductId} does not exist. No Relationship was created."));
						}
					}

					var prodRelation = pr.Adapt<tbl_ProductRelationship>();
					prodRelation.HasAsubProductId = parentId; // Ensure parent ID is set
					await _context.tbl_ProductRelationships.AddAsync(prodRelation);
				}

				// Handle updates - relationships that exist and need updating
				var toUpdate = relationsDto
					.Where(r => existingRelations.Any(er => er.IsAsubProductId == r.IsAsubProductId && er.HasAsubProductId == parentId))
					.ToList();

				foreach (var pr in toUpdate)
				{
					// Find by relationship key fields, not by ID which might be unreliable
					var relationInDb = existingRelations.FirstOrDefault(er =>
						er.IsAsubProductId == pr.IsAsubProductId && er.HasAsubProductId == parentId);

					if (relationInDb != null)
					{
						// Update fields
						relationInDb.SortOrder = pr.SortOrder ?? relationInDb.SortOrder;
						relationInDb.Qty = pr.Qty ?? relationInDb.Qty;
						_context.tbl_ProductRelationships.Update(relationInDb);
					}
				}

				// Save all changes
				await _context.SaveChangesAsync();

				// Return complete list of updated/created relationships
				var finalRelationships = await _context.tbl_ProductRelationships
					.Where(r => r.HasAsubProductId == parentId)
					.ToListAsync();

				return ServiceResult<List<ProductRelationshipDto>>.Success(finalRelationships.Adapt<List<ProductRelationshipDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while managing product relationships for parent product ID: {ParentId}", parentId);
				return ServiceResult<List<ProductRelationshipDto>>.Failure(
					new ServerErrorException("Could not update product relationships."));
			}
		}
		#endregion
	}
}
