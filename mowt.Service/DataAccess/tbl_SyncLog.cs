using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DataAccess
{

    public class tbl_SyncLog : BaseEntity
    {
        // [Key]
        //public int Id { get; set; } // Primary key

        public string? UserJwt { get; set; }

        public string? Method { get; set; }

        public string? Endpoint { get; set; }

        public string? Payload { get; set; }
        public string? Headers { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
