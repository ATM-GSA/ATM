using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TABS.Data
{
    [Table("AuditLog", Schema = "sali")]
    public class AuditLogging
    {
        [Key]
        public int Id { get; set; }

        [MaxLength]
        public string Message { get; set; }

        [MaxLength]
        public string MessageTemplate { get; set; }

        [MaxLength(128)]
        public string Level { get; set; }

        public DateTime TimeStamp { get; set; }

        [MaxLength]
        public string Exception { get; set; }

        [MaxLength]
        public string LogEvent { get; set; }
    }
}
