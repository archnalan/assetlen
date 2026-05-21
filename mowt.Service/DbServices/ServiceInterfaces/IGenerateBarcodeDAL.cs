using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels.Users;
using System.ComponentModel.DataAnnotations;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IGenerateBarcodeDAL
    {
        Task<ServiceResult<UniqueFieldDto>> GetUniqueBarcodeNumberFromDB();

        Task<ServiceResult<string>> GenerateNextBarcode(string companyCode);

        Task<ServiceResult<UniqueFieldDto>> CreateBarcodeNumberInDB(UniqueFieldDto uniqueFieldDto);

        Task<ServiceResult<UniqueFieldDto>> UpdateBarcodeNumberInDB(UniqueFieldDto uniqueFieldDto);

        Task<ServiceResult<List<string>>> GenerateBarcodes(int n, string companyCode = "");
    }
}