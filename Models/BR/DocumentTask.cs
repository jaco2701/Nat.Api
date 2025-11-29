using Applet.Nat.Api.Br.Models;
using Newtonsoft.Json;

namespace Nat.Api.Models.BR
{
    public class DocumentTask
    {
        public long[]? cvlngDocs { get; set; }
        [JsonProperty("Documentos")]
        public DocumentKey[]? coKeys { get; set; }
        [JsonProperty("Task")]
        public eTask ieTask { get; set; }
    }
    public class DocumentTaskResponse
    {
        [JsonProperty("Id.Nat")]
        public long ivlngDoc { get; set; }
        [JsonProperty("Respuesta")]
        public string? ioData { get; set; }
        [JsonProperty("Documento")]
        public DocumentKey? ioKey { get; set; }
    }
    public class DocumentKey
    {
        [JsonProperty("CuitEmisor")]
        public long ivlngCuitEmisor { get; set; }

        [JsonProperty("Tipo")]
        public short ivnroTipo { get; set; }

        [JsonProperty("PuntoDeVenta")]
        public int ivnumPvta { get; set; }

        [JsonProperty("Numero")]
        public long ivlngCbte { get; set; }
    }
}
