using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class SyncLogDto : BaseDto
    {
        // [Key]
        // public int? Id { get; set; } // Primary key

        public string UserJwt { get; set; }

        public string Method { get; set; }

        public string Headers { get; set; }  // originally Dictionary<string, string>? but stored a s tring
        public string Endpoint { get; set; }

        public string? Payload { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
