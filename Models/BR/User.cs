using Applet.Nat.Api.DC;
using Applet.Nat.Api.Static;
using Microsoft.EntityFrameworkCore;
using Nat.API.Properties;
namespace Applet.Nat.Api.Br.Models
{
    public class User
    {
        #region CONSTRUCT
        public User() { }
        public User(int vivnumUser, NatContext vioContext, Token vioToken = null)
        {
            mioToken = vioToken;
            mioContext = vioContext;
            UserModel lioUserModel = mioContext.Users.Find(vivnumUser);
            if (lioUserModel == null)
                throw new Exception(Resources.lioE_NoCreds);
            ioDcModel = lioUserModel;
            FillCuits();
        }
        public User(string vivstrUser, NatContext vioContext, Token vioToken = null)
        {
            mioToken = vioToken;
            mioContext = vioContext;
            UserModel lioUserModel;
            if (MailHelper.IsValidEmail(vivstrUser))
                lioUserModel = mioContext.Users.FirstOrDefault(x => x.ivstrUserEmail == vivstrUser);
            else
                lioUserModel = mioContext.Users.FirstOrDefault(x => x.ivstrUserId == vivstrUser);
            if (lioUserModel == null)
                throw new Exception(Resources.lioE_NoCreds);
            ioDcModel = lioUserModel;
            FillCuits();
        }
        public User(UserModel vioUserModel, NatContext vioContext, Token vioToken = null)
        {
            mioToken = vioToken;
            mioContext = vioContext;
            ioDcModel = vioUserModel;
            FillCuits();
        }
        public List<UserCuitModel> coCuitsModels { get; set; }
        #endregion
        #region PUBLIC PROPS
        public UserModel ioDcModel { get; set; }
        public string? ivstrPass { get; set; }
        public eTask ieTask { get; set; }
        #endregion
        #region PRIVATE PROPS
        private NatContext mioContext;
        private Token mioToken;
        #endregion
        #region PUBLICS METHODS
        public void SetDC(NatContext vioContext)
        {
            mioContext = vioContext;
        }
        public int Task()
        {
            switch (ieTask)
            {
                case eTask.Delete:
                    {
                        mioContext.Users.Remove(ioDcModel);
                        mioContext.SaveChanges();
                        break;
                    }
                case eTask.Save:
                    {
                        UserModel lioDbUserModel = mioContext.Users.Find(ioDcModel.ivnumUser);
                        if (lioDbUserModel != null)
                        {
                            lioDbUserModel.ivstrUserName = ioDcModel.ivstrUserName;
                            lioDbUserModel.ivstrUserEmail = ioDcModel.ivstrUserEmail;
                            lioDbUserModel.ivblnEnable = ioDcModel.ivblnEnable;
                            lioDbUserModel.ivnrologonFails = ioDcModel.ivnrologonFails;
                            mioContext.Users.Update(lioDbUserModel);
                        }
                        else
                        {
                            if (ioDcModel.ivnumUser == 0)
                                ioDcModel.ivnumUser = NN();
                            mioContext.Users.Add(ioDcModel);
                        }
                        mioContext.SaveChanges();
                        if (coCuitsModels != null && coCuitsModels.Count > 0)
                        {
                            mioContext.UserCuits.RemoveRange(mioContext.UserCuits.Where(x => x.ivnumUser == ioDcModel.ivnumUser));
                            foreach (UserCuitModel lioCuitModel in coCuitsModels)
                            {
                                lioCuitModel.ivnumUser = ioDcModel.ivnumUser;
                                mioContext.UserCuits.Add(lioCuitModel);
                            }
                            mioContext.SaveChanges();
                        }
                        break;
                    }
                case eTask.Auth:
                    {
                        string livstrQry = $"SELECT * FROM users WHERE numuser={ioDcModel.ivnumUser} AND PWDCOMPARE('{ivstrPass}',struserPass) = 1";
                        int livnum = mioContext.Users.FromSqlRaw(livstrQry).Count();
                        if (livnum == 0)
                        {
                            ioDcModel.ivnrologonFails++;
                            if (ioDcModel.ivnrologonFails > short.Parse(ListHelper.GetValue("EXPIRE", "LOGINFAIL", mioContext)))
                            {
                                ioDcModel.ivblnEnable = false;
                                mioContext.Users.Update(ioDcModel);
                                mioContext.SaveChanges();
                                throw new Exception(Resources.lioE_UserDisabled);
                            }
                            mioContext.Users.Update(ioDcModel);
                            mioContext.SaveChanges();
                            throw new Exception(Resources.lioE_NoCreds + $" numuser={ioDcModel.ivnumUser} pass={ivstrPass}");
                        }
                        ioDcModel.ivblnEnable = true;
                        ioDcModel.ivnrologonFails = 0;
                        mioContext.Users.Update(ioDcModel);
                        mioContext.SaveChanges();
                        break;
                    }
                case eTask.Pass:
                    {
                        string livstr = $"UPDATE users SET struserPass = PWDENCRYPT('{ivstrPass}')  WHERE numUser = {ioDcModel.ivnumUser}";
                        mioContext.Database.ExecuteSqlRaw(livstr);
                        mioContext.SaveChanges();
                        break;
                    }
            }
            return ioDcModel.ivnumUser;
        }
        #endregion
        #region PRIVATE METHODS
        private int NN()
        {
            UserModel? lio = this.mioContext.Users.OrderByDescending(x => x.ivnumUser).FirstOrDefault();
            if (lio == null)
                return 1;
            return lio.ivnumUser + 1;
        }
        private void FillCuits()
        {
            coCuitsModels = mioContext.UserCuits.Where(x => x.ivnumUser == ioDcModel.ivnumUser).ToList();
            Cuit lioCuit;
            foreach (UserCuitModel lioO in coCuitsModels)
            {
                lioCuit= new Cuit(lioO.ivlngCuit,mioContext,null);
                lioO.ivstrRS = lioCuit.ioDcModel.ivstrCuitRS;
                lioO.ivstrEncoding = lioCuit.ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Encoding")?.ivstrValue ??string.Empty;
            }
        }
        #endregion
    }
}