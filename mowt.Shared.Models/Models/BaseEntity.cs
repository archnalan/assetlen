using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models
{
    public interface IBaseEntity
    {
        DateTime? DateTimeCreated { get; set; }
        DateTime? DateTimeModified { get; set; }
        bool? IsDeleted { get; set; }
        string? LastModifiedBy { get; set; }
        string? TenantId { get; set; }
        Access? Access { get; set; }
        //string Id { get; set; }
    }

    public class BaseEntity : IBaseEntity
    {
        [Key]
        [MaxLength(40)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public bool? IsDeleted { get; set; }
        public virtual string? TenantId { get; set; }
        public DateTime? DateTimeCreated { get; set; }
        public DateTime? DateTimeModified { get; set; }
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// Controls multi-tenant visibility. Null or Private = owning tenant only.
        /// </summary>
        public Access? Access { get; set; }
    }
}
