using Applet.Nat.Afip.Mtxca;
using Applet.Nat.Afip.ServicesCDC;
using Applet.Nat.Afip.ServicesFEX;
using Applet.Nat.Afip.ServicesV1;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;

namespace Applet.Nat.Api.Models.BR
{
    public class DocumentTracking
    {
        #region CONST
        public DocumentTracking(NatContext vioContext, long vivlngDoc)
        {
            mioContext = vioContext;
            mivlngDoc = vivlngDoc;
        }
        public DocumentTracking(NatContext vioContext, DocumentTrackingModel vioDcModel)
        {
            mioContext = vioContext;
            ioDcModel = vioDcModel;
            mivlngDoc = vioDcModel.ivlngDoc;
        }
        #endregion
        #region PUBLIC PROPS
        public DocumentTrackingModel ioDcModel { get; set; }
        public string ivstrTrackDescr
        {
            get
            {
                if (string.IsNullOrEmpty(ioDcModel.ivstrData))
                    return string.Empty;
                try
                {
                    if (ioDcModel == null || ioDcModel.ivstrData == null) return string.Empty;
                    dynamic? lioTrackData = JsonConvert.DeserializeObject(ioDcModel?.ivstrData ?? string.Empty);
                    if (lioTrackData == null || lioTrackData?.Response == null) return string.Empty;
                    lioTrackData = JsonConvert.DeserializeObject(ioDcModel.ivstrData);
                    FECompUltimoAutorizadoResponse lioO1 = JsonConvert.DeserializeObject<FECompUltimoAutorizadoResponse>(lioTrackData.Response.ToString());
                    if (lioO1 != null && lioO1.Body != null && lioO1.Body.FECompUltimoAutorizadoResult != null)
                        return $"Ult. Doc: {lioO1.Body.FECompUltimoAutorizadoResult.CbteNro}";
                    FECompConsultarResponse lioO2 = JsonConvert.DeserializeObject<FECompConsultarResponse>(lioTrackData.Response.ToString()); ;
                    if (lioO2 != null && lioO2.Body != null && lioO2.Body.FECompConsultarResult != null)
                        return $"Cod.Aut.: {lioO2.Body.FECompConsultarResult.ResultGet?.CodAutorizacion ?? string.Empty}";
                    FECAESolicitarResponse lioO3 = JsonConvert.DeserializeObject<FECAESolicitarResponse>(lioTrackData.Response.ToString()); ;
                    if (lioO3 != null && lioO3.Body != null && lioO3.Body.FECAESolicitarResult != null  && lioO3.Body.FECAESolicitarResult.FeDetResp != null && lioO3.Body.FECAESolicitarResult.FeDetResp.Length > 0 && !string.IsNullOrEmpty(lioO3.Body.FECAESolicitarResult.FeDetResp[0].CAE))
                        return $"Cod.Aut.: {lioO3.Body.FECAESolicitarResult.FeDetResp[0].CAE}";
                    consultarUltimoComprobanteAutorizadoResponse lioO4 = JsonConvert.DeserializeObject<consultarUltimoComprobanteAutorizadoResponse>(lioTrackData.Response.ToString());
                    if (lioO4 != null && lioO4.numeroComprobante != 0)
                        return $"Ult Doc: {lioO4.numeroComprobante}";
                    consultarComprobanteResponse lioO5 = JsonConvert.DeserializeObject<consultarComprobanteResponse>(lioTrackData.Response.ToString());
                    if (lioO5 != null && lioO5.comprobante != null && lioO5.comprobante.codigoAutorizacion != 0)
                        return $"Cod.Aut.: {lioO5.comprobante.codigoAutorizacion}";
                    autorizarComprobanteResponse lioO6 = JsonConvert.DeserializeObject<autorizarComprobanteResponse>(lioTrackData.Response.ToString());
                    if (lioO6 != null && lioO6.comprobanteResponse != null && lioO6.comprobanteResponse.CAE != 0)
                        return $"Cod.Aut.: {lioO6.comprobanteResponse.CAE}";
                    FEXResponseLast_CMP lioO7 = JsonConvert.DeserializeObject<FEXResponseLast_CMP>(lioTrackData.Response.ToString());
                    if (lioO7 != null && lioO7.FEXResult_LastCMP != null && lioO7.FEXResult_LastCMP.Cbte_nro != 0)
                        return $"Ult Doc: {lioO7.FEXResult_LastCMP.Cbte_nro}";
                    FEXGetCMPResponse lioO8 = JsonConvert.DeserializeObject<FEXGetCMPResponse>(lioTrackData.Response.ToString());
                    if (lioO8 != null && lioO8.FEXResultGet != null && !string.IsNullOrEmpty(lioO8.FEXResultGet.Cae))
                        return $"Cod.Aut.: {lioO8.FEXResultGet.Cae}";
                    FEXResponseAuthorize lioO9 = JsonConvert.DeserializeObject<FEXResponseAuthorize>(lioTrackData.Response.ToString());
                    if (lioO9 != null && lioO9.FEXResultAuth != null && !string.IsNullOrEmpty(lioO9.FEXResultAuth.Cae))
                        return $"Cod.Aut.: {lioO9.FEXResultAuth.Cae}";
                    ComprobanteConstatarResponse lio10 = JsonConvert.DeserializeObject<ComprobanteConstatarResponse>(lioTrackData.Response.ToString());
                    if (lio10 != null && lio10.Body != null && lio10.Body.ComprobanteConstatarResult != null && lio10.Body?.ComprobanteConstatarResult?.CmpResp != null)
                        return $"Cod. Aut.:{lio10.Body?.ComprobanteConstatarResult?.CmpResp?.CodAutorizacion?.ToString() ?? string.Empty}";
                    return string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        #endregion
        #region PUBLICS METHODS
        public void addTrack(short vieStatus, string vivstrWsData)
        {
            mioContext.DocumentTrackings.Add(
                new DocumentTrackingModel
                {
                    ivnumTrack = NN(),
                    ivnroStatus = vieStatus,
                    ivdtmTrack = DateTime.Now,
                    ivlngDoc = mivlngDoc,
                    ivstrData = vivstrWsData
                });
            mioContext.SaveChanges();
        }
        #endregion 
        #region PRIVATE PROPS
        private NatContext mioContext { get; set; }
        private long mivlngDoc { get; set; }
        #endregion 
        #region PRIVATE METHODS
        private int NN()
        {
            int livnum = 1;
            if (mioContext.DocumentTrackings.Any(x => x.ivlngDoc == mivlngDoc))
                livnum = mioContext.DocumentTrackings.Where(x => x.ivlngDoc == mivlngDoc).Max(x => x.ivnumTrack) + 1;
            return livnum;
        }
        #endregion
    }

}
