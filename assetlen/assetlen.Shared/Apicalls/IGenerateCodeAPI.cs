using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface IGenerateCodeAPI
    {
        [Get("/api/GenerateBarCode/GetUniqueBarcodeNumberFromDB")]
        Task<IApiResponse<UniqueFieldDto>> GetUniqueBarcodeNumberFromDB();

        [Get("/api/GenerateBarCode/GenerateNextBarcode")]
        Task<IApiResponse<string>> GenerateNextBarcode(string companyCode);

        [Post("/api/GenerateBarCode/CreateBarcodeNumberInDB")]
        Task<IApiResponse<UniqueFieldDto>> CreateBarcodeNumberInDB(UniqueFieldDto uniqueFieldDto);

        [Put("/api/GenerateBarCode/UpdateBarcodeNumberInDB")]
        Task<IApiResponse<UniqueFieldDto>> UpdateBarcodeNumberInDB(UniqueFieldDto uniqueFieldDto);

    }
}
