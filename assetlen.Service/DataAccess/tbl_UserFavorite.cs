using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_UserFavorite : BaseEntity
{
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string ProductId { get; set; } = string.Empty;

    [ForeignKey("ProductId")]
    public tbl_Product? Product { get; set; }
}
