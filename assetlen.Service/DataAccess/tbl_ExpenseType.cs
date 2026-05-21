using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Service.DataAccess;

public partial class tbl_ExpenseType : BaseEntity
{
    // [Key]
    //public int Id { get; set; }

    public string? Description { get; set; }

}
