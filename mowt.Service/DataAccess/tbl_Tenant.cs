using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DataAccess
{
    public class tbl_Tenant
    {

        public bool? IsDeleted { get; set; }
        public DateTime? DateTimeCreated { get; set; }
        public DateTime? DateTimeModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public string? CocurrencyKey { get; set; }
        public string? keyHarsh { get; set; }
        public DateTime LastRenewal { get; set; }

        [Required]
        [Key]
        public string TenantId { get; set; }
        public string Name { get; set; }  // Name of the company
        public string? BussinessRegNumber { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public string? Address { get; set; }  // Street address of the company
        public string? City { get; set; }  // City where the company is located
        public string? State { get; set; }  // State or region of the company
        public string? PostalCode { get; set; }  // Postal code
        public string? Country { get; set; }  // Country
        public string? PhoneNumber { get; set; }  // Contact phone number
        public string? Email { get; set; }  // Contact email address
        public string? Website { get; set; }  // Company website URL
        public DateTime? EstablishedDate { get; set; }  // Date the company was established
        public string? Industry { get; set; }  // Industry sector of the company
        public int? NumberOfEmployees { get; set; }  // Total number of employees
        public decimal? AnnualRevenue { get; set; }  // Annual revenue of the company
        public string? CEO { get; set; }  // Chief Executive Officer
        //public List<string> Departments { get; set; }  // List of company departments
        public bool? IsPublic { get; set; }  // Indicates if the company is publicly traded
        public string? StockSymbol { get; set; }  // Stock symbol (if applicable)

        public string? Description { get; set; }  // Brief description of the company

    }
}
