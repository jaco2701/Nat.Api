using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Configuration;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Text;
using System.Text.Unicode;

namespace Nat.API.Models.BR
{
    public class ApiIO : IDocsIO
    {
        #region CONS
        public ApiIO() { }
        public ApiIO(IConfiguration vioConfiguration, NatContext vioContext)
        {
            mioConfiguration = vioConfiguration;
            if (vioContext != null)
                mioContext = vioContext;
        }
        #endregion
        #region PRIVATE PROPS
        private IConfiguration mioConfiguration { get; set; }
        private NatContext mioContext { get; set; }

        #endregion
        #region PUBLIC PROPS
        public long ivlngCuit { get; set; }
        public string ivstrB64Rta { get; set; }
        public ServiceMapper ioMapper { get; set; }
        #endregion
        #region PUBLIC METHODS  
        public async Task DocsI(int vivnumUserOriginator)
        {
        }
        public async Task DocO(Document[] vcoDocuments)
        {
            foreach (Document lioDocument in vcoDocuments)
            {
                if (ioMapper != null)
                    ivstrB64Rta = DocHelper.BuildDocumentResponse(lioDocument, mioConfiguration, ioMapper);
                break; // Solo procesa de a un documento
            }
        }
        #endregion
    }
}
