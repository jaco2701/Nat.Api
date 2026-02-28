using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace Applet.Nat.Api.Static
{
    public static class XmlHelper
    {

        public static string RemoveNameSpace(string vivstrXml)
        {
            XmlDocument lioXmlDocumentFrom = new XmlDocument();
            lioXmlDocumentFrom.LoadXml(vivstrXml.ToString());
            XmlDocument lioXmlDocumentTo = new XmlDocument();
            XmlNode lioXmlNode = RecursiveRemoveNameSpace(lioXmlDocumentTo, lioXmlDocumentFrom.FirstChild);
            lioXmlDocumentTo.AppendChild(lioXmlNode);
            return (lioXmlDocumentTo.OuterXml);
        }
        private static System.Xml.XmlElement RecursiveRemoveNameSpace(XmlDocument vioXmlDocument, XmlNode vioXmlChildNode)
        {
            string livNewElementNamespace = (vioXmlChildNode.NamespaceURI != String.Empty ? String.Empty : String.Empty);
            XmlElement lioXmlElement = vioXmlDocument.CreateElement(vioXmlChildNode.LocalName, livNewElementNamespace);
            if (vioXmlChildNode.ChildNodes != null)
                foreach (System.Xml.XmlNode lioXmlNode in vioXmlChildNode.ChildNodes)
                {
                    if (lioXmlNode is System.Xml.XmlElement)
                        lioXmlElement.AppendChild(RecursiveRemoveNameSpace(vioXmlDocument, (System.Xml.XmlElement)lioXmlNode));
                    else
                        lioXmlElement.InnerXml = vioXmlChildNode.InnerXml;
                }
            if (vioXmlChildNode.Attributes != null)
                foreach (XmlAttribute lioXmlAttribute in vioXmlChildNode.Attributes)
                {
                    if (lioXmlAttribute.Name != "xmlns" && lioXmlAttribute.Name != "xmlns:xsi" && lioXmlAttribute.Name != "xmlns:xsd")
                    {
                        XmlAttribute lioXmlAttributeNew = vioXmlDocument.CreateAttribute(lioXmlAttribute.Name);
                        lioXmlAttributeNew.Value = lioXmlAttribute.Value;
                        lioXmlElement.Attributes.Append(lioXmlAttributeNew);
                    }
                }
            return lioXmlElement;
        }

    }
}

