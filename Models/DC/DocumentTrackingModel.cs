using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("DOCUMENTTRACKING")]
    public class DocumentTrackingModel
    {
        [Key, Column("lngdoc", Order = 0)]
        public long ivlngDoc { get; set; }
        [Column("dtmtrack")]
        public DateTime ivdtmTrack { get; set; }
        [Column("nrostatus")]
        public short ivnroStatus { get; set; }
        [Column("strdata")]
        public string? ivstrData { get; set; }
        [Column("wsdata")]
        public string? ivstrwsData{ get; set; }
        [Key, Column("numtrack", Order = 1)]
        public int ivnumTrack { get; set; }
    }
}