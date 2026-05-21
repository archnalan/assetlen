namespace mowt.Shared.Models.Models.ViewModels
{
    public class UserFavoriteDto : BaseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public bool IsFavorited { get; set; }
        public ProductsDto? Product { get; set; }
    }
}
