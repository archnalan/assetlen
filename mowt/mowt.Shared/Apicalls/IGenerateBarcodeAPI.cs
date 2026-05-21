using mowt.Shared.Models.Models.ViewModels.Users;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface IGenerateBarcodeAPI
    {
        [Post("/api/GenerateBarCode/CreateBarcodeNumberInDB")]
        Task<IApiResponse<UniqueFieldDto>> CreateBarcodeNumberInDB([Body] UniqueFieldDto uniqueFieldDto);

        [Get("/api/GenerateBarCode/GenerateNextBarcode")]
        Task<IApiResponse<string>> GenerateNextBarcode([Query] string companyCode = "");

        [Put("/api/GenerateBarCode/UpdateBarcodeNumberInDB")]
        Task<IApiResponse<UniqueFieldDto>> UpdateBarcodeNumberInDB([Body] UniqueFieldDto uniqueFieldDto);

        [Get("/api/GenerateBarCode/GenerateBarcodes")]
        Task<IApiResponse<List<string>>> GenerateBarcodes([Query] int n, [Query] string companyCode = "");
    }
}
