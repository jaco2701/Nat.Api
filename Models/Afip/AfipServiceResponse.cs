using System;
using System.Collections.Generic;
using System.Xml;

namespace Applet.Nat.Api.AFIP.Model
{
    public class AfipServiceResponse
    {
        #region Ctor
        public AfipServiceResponse() { }
        public AfipServiceResponse(string _StrXmlAfipResponse, string _Method)
        {
            mivstrMethod = _Method;
            mivxmlAfipRessponse = new System.Xml.XmlDocument();
            mivxmlAfipRessponse.LoadXml(_StrXmlAfipResponse);
        }
        #endregion
        #region Propiedades
        private XmlDocument mivxmlAfipRessponse;
        private string mivstrMethod;
        private List<string> mcvstrError;
        public List<string> cvstrError
        {
            get
            {
                if (mcvstrError == null)
                    mcvstrError = GetNodos("E");
                return (mcvstrError);
            }
        }
        private List<string> mcvstrObs;
        public List<string> cvstrObs
        {
            get
            {
                if (mcvstrObs == null)
                    mcvstrObs = GetNodos("O");
                return (mcvstrObs);
            }
        }
        public string mivstrRes;
        public string ivstrRes
        {
            get
            {
                if (mivstrRes == null)
                    mivstrRes = GetNodos("R")[0];
                return (mivstrRes);
            }
        }

        #endregion
        #region Metodos
        private List<string> GetNodos(string vivstrTipoNodo)
        {
            List<string> lcvstrResult = new List<string>();
            XmlNode lioxmlNode;
            XmlNodeList lioxmlNodeList;
            switch (mivstrMethod)
            {
                case "FECompConsultar":
                    {

                        switch (vivstrTipoNodo)
                        {
                            case "R":
                                {
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode("/FECompConsultaResponse/ResultGet");
                                    if (lioxmlNode != null)
                                    {
                                        string[] lcoNodeExclude = { "CbteDesde", "CbteHasta", "PtoVta", "CbteTipo" };

                                        foreach (XmlNode lioxmlNodechild in lioxmlNode.ChildNodes)
                                            if (!Array.Exists(lcoNodeExclude, t => t == lioxmlNode.Name))
                                                lcvstrResult.Add(lioxmlNodechild.InnerXml);

                                    }
                                    break;

                                }
                            case "E":
                                {
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode("/FECompConsultaResponse/Errors");
                                    if (lioxmlNode != null)
                                    {
                                        foreach (XmlNode lioxmlNodeListnode in lioxmlNode.ChildNodes)
                                        {
                                            lioxmlNode = lioxmlNodeListnode.SelectSingleNode("./Msg");
                                            if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                                lcvstrResult.Add(lioxmlNode.InnerXml);
                                        }
                                    }
                                    break;
                                }
                            case "O":
                                {
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode("/FECompConsultaResponse/Events");
                                    if (lioxmlNode != null)
                                    {
                                        foreach (XmlNode lioxmlNodeListnode in lioxmlNode.ChildNodes)
                                        {
                                            lioxmlNode = lioxmlNodeListnode.SelectSingleNode("./Msg");
                                            if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                                lcvstrResult.Add(lioxmlNode.InnerXml);
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }

                case "FECAESolicitar":
                    {
                        switch (vivstrTipoNodo)
                        {
                            case "R":
                                {
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode("/FECAEResponse/FeDetResp/FECAEDetResponse/Resultado");
                                    if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                        lcvstrResult.Add(lioxmlNode.InnerXml);
                                    break;
                                }
                            case "E":
                                {
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode("/FECAEResponse/FeDetResp/FECAEDetResponse/Errors");
                                    if (lioxmlNode != null)
                                    {
                                        foreach (XmlNode lioxmlNodeListnode in lioxmlNode.ChildNodes)
                                        {
                                            lioxmlNode = lioxmlNodeListnode.SelectSingleNode("./Msg");
                                            if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                                lcvstrResult.Add(lioxmlNode.InnerXml);
                                        }
                                    }
                                    break;
                                }
                            case "O":
                                {
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode(("/FECAEResponse/FeDetResp/FECAEDetResponse/Observaciones"));
                                    if (lioxmlNode != null)
                                    {
                                        foreach (XmlNode lioxmlNodeListnode in lioxmlNode.ChildNodes)
                                        {
                                            lioxmlNode = lioxmlNodeListnode.SelectSingleNode("./Msg");
                                            if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                                lcvstrResult.Add(lioxmlNode.InnerXml);
                                        }
                                    }
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode("/FECAEResponse/FeDetResp/FECAEDetResponse/CAEFchVto");
                                    if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                        lcvstrResult.Add(lioxmlNode.InnerXml);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case "ComprobanteConstatar":
                    {
                        switch (vivstrTipoNodo)
                        {
                            case "R":
                                {
                                    lioxmlNode = mivxmlAfipRessponse.SelectSingleNode("/CmpResponse/Resultado");
                                    if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                        lcvstrResult.Add(lioxmlNode.InnerXml);
                                    else
                                        lcvstrResult.Add(string.Empty);

                                    break;
                                }
                            case "E":
                                {
                                    lioxmlNodeList = mivxmlAfipRessponse.SelectNodes("/CmpResponse/Errors");
                                    if (lioxmlNodeList != null)
                                    {
                                        foreach (XmlNode lioxmlNodeListnode in lioxmlNodeList)
                                        {
                                            lioxmlNode = lioxmlNodeListnode.SelectSingleNode("./Err");
                                            if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                                lcvstrResult.Add(lioxmlNode.InnerXml);
                                        }
                                    }
                                    break;
                                }
                            case "O":
                                {
                                    lioxmlNodeList = mivxmlAfipRessponse.SelectNodes("/CmpResponse/Observaciones");
                                    if (lioxmlNodeList != null)
                                    {
                                        foreach (XmlNode lioxmlNodeListnode in lioxmlNodeList)
                                        {
                                            lioxmlNode = lioxmlNodeListnode.SelectSingleNode("./Obs/Msg");
                                            if (lioxmlNode != null && lioxmlNode.InnerXml != string.Empty)
                                                lcvstrResult.Add(lioxmlNode.InnerXml);
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
            return lcvstrResult;
        }
        #endregion
    }
}
