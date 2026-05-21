namespace assetlen.Shared.Models.Models.ViewModels
{
    public class BaseDto
    {
        public virtual string? Id { get; set; }
        public bool Selected { get; set; } = false;
        public DateTime? DateTimeCreated { get; set; }
        DateTime? DateTimeModified { get; set; }
    }


}