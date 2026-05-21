namespace assetlen.Shared.Models.Models
{
    public class NotificationsDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public int NotificationType { get; set; }
        public string? Message { get; set; }
        public string? Title { get; set; }
        public long NotificationForeignID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DateModified { get; set; }
        public bool isRead { get; set; } = false;
    }
}
