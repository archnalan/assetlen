using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface ISupplierPaymentDAL
    {
        Task<ServiceResult<SupplierPaymentDto>> AddSupplierPaymentToDB([Required] SupplierPaymentDto spDto);
        Task<ServiceResult<decimal>> GetSupplierInvoiceSUMUsingSupplierIDAndEndDate(string SupplierID, DateTime EndDate);
        Task<ServiceResult<decimal>> GetSupplierPaymentSUMLowerThanEndDate(string SupplierID, DateTime EndDate);
    }
}