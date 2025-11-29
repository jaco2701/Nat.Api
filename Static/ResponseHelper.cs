using System.Xml.Serialization;
using System.Xml;
using Applet.Nat.Api.Br;

namespace Applet.Nat.Api.Static
{
    [XmlInclude(typeof(Nat.Afip.ServicesV1.FEAuthRequest))]
    public static class ResponseHelper
    {
        public static Response Get(int vivnumStatus, Exception vioE)
        {
            try
            {
                string livstrStack = vioE.ToString();
                return new Response
                {
                    ivnumStatus = vivnumStatus,
                    ioData = new ResponseException
                    {
                        ivstrMsg = vioE.Message,
                        ivstrStack = vioE.ToString()
                    }
                };
            }
            catch (Exception lioE)
            {
                throw new Exception("Error en generacion de Response:", lioE);
            }
        }
        public static Response Get(int vivnumStatus, string vivstrMsg)
        {
            try
            {
                return new Response
                {
                    ivnumStatus = vivnumStatus,
                    ioData = new ResponseException
                    {
                        ivstrMsg = vivstrMsg,
                        ivstrStack = string.Empty
                    }
                };
            }
            catch (Exception lioE)
            {
                throw new Exception("Error en generacion de Response:", lioE);
            }
        }
        public static Response Get(object vioResponse)
        {
            return new Response
            {
                ivnumStatus = 0,
                ioData = vioResponse
            };
        }
        public static Response Get(List<object> vcoResponse)
        {
            return new Response
            {
                ivnumStatus = 0,
                ioData = vcoResponse
            };

        }
    }

}