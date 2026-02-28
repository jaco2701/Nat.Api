using System;


namespace Applet.Nat.Api.AFIP.Model
{
    public class AfipLoginResponse
    {
        #region Ctor
        public AfipLoginResponse() { }
        public AfipLoginResponse(System.Xml.XmlDocument vioxml)
        {
            mivstrSource = vioxml.SelectSingleNode("/loginTicketResponse/header/source").InnerXml;
            mivstrDestinatio = vioxml.SelectSingleNode("/loginTicketResponse/header/destination").InnerXml;
            mivstrUniqueid = vioxml.SelectSingleNode("/loginTicketResponse/header/uniqueId").InnerXml;
            mivstrGenerationtime = vioxml.SelectSingleNode("/loginTicketResponse/header/generationTime").InnerXml;
            mivstrExpirationtime = vioxml.SelectSingleNode("/loginTicketResponse/header/expirationTime").InnerXml;
            mivstrToken = vioxml.SelectSingleNode("/loginTicketResponse/credentials/token").InnerXml;
            mivstrSign = vioxml.SelectSingleNode("/loginTicketResponse/credentials/sign").InnerXml;
        }
        public AfipLoginResponse(string vivstrSource, string vivstrDestination, string vivstrUniqueId, string vivstrGenerationTime, string vivstrExpirationTime, string vivstrToken, string vivstrSign)
        {
            mivstrSource = vivstrSource;
            mivstrDestinatio = vivstrDestination;
            mivstrUniqueid = vivstrUniqueId;
            mivstrGenerationtime = vivstrGenerationTime;
            mivstrExpirationtime = vivstrExpirationTime;
            mivstrToken = vivstrToken;
            mivstrSign = vivstrSign;
        }
        #endregion
        #region Propiedades
        private string mivstrSource;
        public string ivstrSource
        {
            get
            {
                if (mivstrSource == null)
                    mivstrSource = string.Empty;
                return (mivstrSource);
            }
        }
        private string mivstrDestinatio;
        public string ivstrDestination
        {
            get
            {
                if (mivstrDestinatio == null)
                    mivstrDestinatio = string.Empty;
                return (mivstrDestinatio);
            }
        }
        private string mivstrUniqueid;
        public string ivstrUniqueId
        {
            get
            {
                if (mivstrUniqueid == null)
                    mivstrUniqueid = string.Empty;
                return (mivstrUniqueid);
            }
        }
        private string mivstrGenerationtime;
        public string ivstrGenerationTime
        {
            get
            {
                if (mivstrGenerationtime == null)
                    mivstrGenerationtime = string.Empty;
                return (mivstrGenerationtime);
            }
        }
        private string mivstrExpirationtime;
        public string ivstrExpirationTime
        {
            get
            {
                if (mivstrExpirationtime == null)
                    mivstrExpirationtime = string.Empty;
                return (mivstrExpirationtime);
            }
        }
        private string mivstrToken;
        public string ivstrToken
        {
            get
            {
                if (mivstrToken == null)
                    mivstrToken = string.Empty;
                return (mivstrToken);
            }
        }
        private string mivstrSign;
        public string ivstrSign
        {
            get
            {
                if (mivstrSign == null)
                    mivstrSign = string.Empty;
                return (mivstrSign);
            }
        }
        #endregion
    }
}

