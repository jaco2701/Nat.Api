using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Nat.API.Models.BR;
using Newtonsoft.Json;
using System.Text;
namespace Applet.Nat.Api.Br.Models
{
    public class CuitCuit
    {
        #region CONSTRUCT
        public CuitCuit() { }
        public CuitCuit(long vivlngCuit, long vivlngCuitReceptor, NatContext vioContext)
        {
            mioContext = vioContext;
            CuitCuitModel lioCuitCuitModel = mioContext.CuitCuits.Find(vivlngCuit,vivlngCuitReceptor);
            if (lioCuitCuitModel == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Relacion C.U.I.T.", "a"));
            ioDcModel = lioCuitCuitModel;
        }
        public CuitCuit(CuitCuitModel vioCuitCuitModel, NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioConfiguration = vioConfiguration;
            mioContext = vioContext;
            ioDcModel = vioCuitCuitModel;
        }
        #endregion
        #region PUBLIC PROPS
        public CuitCuitModel ioDcModel { get; set; }
        public eTask ieTask { get; set; }

        #endregion
        #region PRIVATE PROPS
        private IConfiguration mioConfiguration;
        private NatContext mioContext;
        #endregion
        #region PUBLICS METHODS
        public void SetDC(NatContext vioContext)
        {
            mioContext = vioContext;
        }
        public async Task Task()
        {
            switch (ieTask)
            {
                case eTask.Delete:
                    mioContext.CuitCuits.Remove(ioDcModel);
                    mioContext.SaveChanges();
                    break;
                case eTask.Save:
                    CuitCuitModel lioDbCuitCuitModel = mioContext.CuitCuits.Find(ioDcModel.ivlngCuit, ioDcModel.ivlngCuitReceptor);
                    if (lioDbCuitCuitModel != null)
                    {
                        lioDbCuitCuitModel.ivstrEmail = ioDcModel.ivstrEmail;
                        lioDbCuitCuitModel.ivstrRs = ioDcModel.ivstrRs;
                        mioContext.CuitCuits.Update(lioDbCuitCuitModel);
                    }
                    else
                        mioContext.CuitCuits.Add(ioDcModel);
                    mioContext.SaveChanges();
                    break;
            }
        }
           #endregion
        #region PRIVATE METHODS
        #endregion
    }
}