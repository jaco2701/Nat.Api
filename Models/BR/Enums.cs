using Applet.Nat.Api.DC;
using System.Reflection.Emit;
using System.Reflection;

namespace Applet.Nat.Api.Br.Models
{
    public enum eTask : short
    {
        GetPdf = 1,
        Share = 2,
        Tracking = 3,
        Original = 4,
        PdfGen = 5,
        RePrint = 6,
        ETsts = 7,
        Auth=8,
        Delete= 9,
        Save= 10,
        Pass =11,
        Rta = 12,
        UploadDocs = 13,
    }
    public enum eLoadMethod : short
    {
        Manual = 0,
        File = 1,
        OracleCanonical = 2,
        Api = 3,
    }

}


