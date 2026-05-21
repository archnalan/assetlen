using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface IBankDAL
    {
        Task<ServiceResult<BankDto>> AddBank(BankDto bank);
        Task<ServiceResult<bool>> DeleteBankById(string id);
        Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchBanksFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<PaginationDetails<BankDto>>> GetBanksFromDB(int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<BankDto>> GetBankById(string id);
        Task<ServiceResult<BankDto>> GetBankByName(string name);
        Task<ServiceResult<string>> GetBankIDBasedOnBankName(string bankName);
        Task<ServiceResult<BankDto>> UpdateBank(BankDto bankDto);
        Task<ServiceResult<PaginationDetails<BankDto>>> SearchBanksFromDB(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<MemoryStream>> GetBanksForCSVExportBySelectedFields(List<string> selectedColumnNames);
        Task<ServiceResult<ImportResultSummary>> ImportBanksFromExcel(ImportDataDto importData);
    }
}