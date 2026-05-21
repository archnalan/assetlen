using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface IPaymentsDAL
	{
		Task<ServiceResult<PaymentsDto>> AddPayments(PaymentsDto p);
		Task<ServiceResult<TransactionDto>> AddPaymentsAndCloseSale(List<PaymentsDto> p);
		Task<ServiceResult<bool>> DeletePayment(string id);
		Task<ServiceResult<List<PaymentModeDto>>> GetAllPaymentModes();
		Task<ServiceResult<List<PaymentAccountDto>>> GetBANKPaymentAccountFromDB();
		Task<ServiceResult<List<PaymentAccountDto>>> GetCARDPaymentAccountFromDB();
		Task<ServiceResult<string>> GetPaymentModeNameUsingID(string id);
		Task<ServiceResult<List<PaymentsDto>>> GetPaymentsBasedOnSaleID(string saleId);
		Task<ServiceResult<List<PaymentsDto>>> GetPaymentsFromDB();
		Task<ServiceResult<decimal>> GetSumOfPaymentsBasedOnSaleID(string saleID);
		Task<ServiceResult<List<PaymentAccountDto>>> SearchBANKAccountFromDBUsingKeyword(string keywords);
		Task<ServiceResult<List<PaymentAccountDto>>> SearchCARDPaymentAccountFromDBUsingKeyword(string keywords);
	}
}