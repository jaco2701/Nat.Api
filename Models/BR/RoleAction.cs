using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Nat.API.Models.BR;
using Newtonsoft.Json;
using System.Text;
namespace Applet.Nat.Api.Br.Models
{
    public class RoleAction
    {
        #region CONSTRUCT
        public RoleAction() { }
        public RoleAction(RoleActionModel vioRoleActionModel, NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioConfiguration = vioConfiguration;
            mioContext = vioContext;
            ioDcModel = vioRoleActionModel;
        }
        #endregion
        #region PUBLIC PROPS
        public RoleActionModel ioDcModel { get; set; }
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
                case eTask.Save:
                    RoleActionModel lioDbRoleActionModel = mioContext.RoleActions.Find(ioDcModel.ivnroRole, ioDcModel.ivnroAction);
                    if (lioDbRoleActionModel == null)
                        mioContext.RoleActions.Add(ioDcModel);
                    mioContext.SaveChanges();
                    break;
            }
        }
        #endregion
        #region PRIVATE METHODS
        #endregion
    }

}