using System;
using System.IO;

namespace Applet.Nat.Api.Static
{
    internal class LogInternal
    {
        public LogInternal()
        {
            mioPath = "./log";
            if (!Directory.Exists(mioPath))
                Directory.CreateDirectory(mioPath);
            mioPath += "/";
            DateTime livdtm = DateTime.Today;
            mioPath += livdtm.ToString("yyyyMMdd");
            mioPath += ".log";
            if (!File.Exists(mioPath))
                mioStreamWriter = File.CreateText(mioPath);
            else
                mioStreamWriter = File.AppendText(mioPath);

        }
        #region 'Propiedades'
        private System.IO.StreamWriter mioStreamWriter;
        private string mioPath;
        #endregion

        #region 'Metodos'
        public void escribir(string _linea)
        {
            short livnroTry = 0;
            while (true)
            {
                try
                {
                    mioStreamWriter.WriteLine(_linea);
                    break;
                }
                catch  
                {
                    if (livnroTry > 3) throw;
                    livnroTry++;
                    System.Threading.Thread.Sleep(Random.Shared.Next(1, 5) * 50 * livnroTry);
                }
            }
        }
        public void cerrar()
        {
            mioStreamWriter.Close();
            mioStreamWriter.Dispose();
        }


        #endregion
    }

    public static class LogHelper
{
    public static void write(Exception e)
    {

        if (e != null)
        {
            LogInternal liologinternal = new LogInternal();
            liologinternal.escribir($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}==>{e.Message}");
            liologinternal.escribir(e.StackTrace);
            if (e.InnerException != null)
            {
                liologinternal.escribir(e.InnerException.Message);
                liologinternal.escribir(e.InnerException.StackTrace);
                if (e.InnerException.InnerException != null)
                {
                    liologinternal.escribir(e.InnerException.InnerException.Message);
                    liologinternal.escribir(e.InnerException.InnerException.StackTrace);
                }
            }
            liologinternal.cerrar();
        }
    }
    public static void writeinfo(string _message, bool livblnVerbose)
    {
        if (!livblnVerbose) return;
        LogInternal liologinternal = new LogInternal();
        liologinternal.escribir($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}==>{_message}");
        liologinternal.cerrar();
    }

}
}