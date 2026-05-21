namespace mowt.Shared.Models.Models.ViewModels
{
    public class UserDocumentDto : BaseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public bool IsInCollection { get; set; }
        public ProductsDto? Product { get; set; }
    }
}
