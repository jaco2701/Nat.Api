using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;

namespace Applet.Nat.Api.Ifaces
{
    public interface IDocsIO  //entrada y salida de documentos  
    {
        Task DocsI(int vivnumUserOriginator);
        Task DocO(Document[] vcoDocuments);
        string ivstrB64Rta { get; set; }
    }
}

