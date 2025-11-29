using System.Xml;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
namespace Applet.Nat.Api.Ifaces
{
    public interface IRawDocument
    {
        DocumentUser ToDocumentUser();
        string? ivstrRaw { get; set; }
        string? ivstrName { get; set; }
        public string ivstrKey { get; set; }
        string ToPrint();
    }
}

