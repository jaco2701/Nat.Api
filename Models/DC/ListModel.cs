using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("LISTS")]
    public class ListModel
    {
        [Key, Column("TYPE", Order = 0)]
        public string? ivcodType { get; set; }
        [Key, Column("ID", Order = 1)]
        public string? ivcodId { get; set; }
        [Column("DESCR")]
        public string ? ivstrDesc { get; set; }
    }
}