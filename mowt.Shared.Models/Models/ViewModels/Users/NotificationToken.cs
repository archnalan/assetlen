
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Shared.Models.Models
{
    public class NotificationToken
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")] public AppUser User { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
    }

}
