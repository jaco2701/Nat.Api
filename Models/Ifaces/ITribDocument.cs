using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;

namespace Applet.Nat.Api.Ifaces
{
    public interface ITribDocument
    {
        void SetData(DocumentUser vioDocumentUser);
        Task<short> Auth();
        void Validate();
        bool AuthDataModified(ITribDocument vioIDocument);
        void SetContext(NatContext vioContext);
        UxAuth GetAuth();
        Task<double> GetCotizacion(string ivstrCurrency, DateTime livdtm);
        long ivCuitAutorizante { get; }
        string ivstrSR { get; set; }
    }
}

