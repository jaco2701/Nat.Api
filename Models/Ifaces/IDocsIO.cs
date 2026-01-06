using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;

namespace Applet.Nat.Api.Ifaces
{
    public interface IDocsIO
    {
        Task DocsGet();
        Task DocsUpdate(Document[] vcoDocuments);
    }
}

