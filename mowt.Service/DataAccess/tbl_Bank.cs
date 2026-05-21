using mowt.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace mowt.Service.DataAccess;

public partial class tbl_Bank : BaseEntity
{
    //public string Id { get; set; }
    public string BankName { get; set; } = string.Empty;

    public string? SwiftCode { get; set; }

    public string? Address { get; set; }

    public bool? IsActive { get; set; }

    public string? Description { get; set; }
}