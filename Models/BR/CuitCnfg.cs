using Applet.Nat.Api.Br.Models;
using System;
using System.Globalization;
namespace Applet.Nat.Api.Br
{
    public class CuitCnfg
    {
        public ServiceMapper[]? coServiceMappers { get; set; }
        public CuitParameter[] coParameters { get; set; }
        public TemplateVersion[]? coTemplateVersions { get; set; }
    }
    public class ServiceMapper
    {
        public string? ivstrWs { get; set; }
        public ServiceMapperItem[]? coItems { get; set; }
        public int? ivnumRecLen { get; set; }
        public string? ivstrTemplate { get; set; }
        public string? ivstrSplitter { get; set; }
        public string? ivstrInputType { get; set; }
        public short[]? cvnroDocTypes { get; set; }
        public bool? ivblnTaxInLines { get; set; }
        public bool? ivblnCalcPermisoExistente { get; set; }
        public bool? ivblnMapping { get; set; }
        public bool? ivblnSaveOnLoad { get; set; }
    }
    public class ServiceMapperItem
    {
        public string? ivstrProperty { get; set; }
        public ServiceMapperItemXPath[]? coXPaths { get; set; }
        public short[]? coStatus { get; set; }
        public Dictionary<string, string>? coConversion { get; set; }
        public int? ivnumLen { get; set; }
        public string? ivstrLPad { get; set; }
        public string? ivstrRPad { get; set; }
        public string? ivstrformat { get; set; }
        public string? ivstrCoord { get; set; }
        public bool? ivblnRequired { get; set; }
        public bool ivblnIsNumeric { get { return !string.IsNullOrEmpty(ivstrProperty) && (ivstrProperty.Contains("ivnro") || ivstrProperty.Contains("ivnum") || ivstrProperty.Contains("ivlng") || ivstrProperty.Contains("ivdbl")); } }
        public string? ivstrDefault { get; set; }
        public bool? ivblnColumnHeader { get; set; }
        public string FormatPropertyValue(string vivstrPropertyValue)
        {
            string livstr = vivstrPropertyValue;
            if ((ivstrCoord != null && ivstrCoord.Contains("FIX")))
                return ivstrformat ?? string.Empty;
            if (string.IsNullOrEmpty(ivstrProperty))
                return livstr;
            if (string.IsNullOrEmpty(livstr))
            {
                if (!string.IsNullOrEmpty(ivstrDefault))
                    livstr = ivstrDefault;
                else if (ivblnIsNumeric)
                    return "0";
                return livstr;
            }
            if (coConversion != null && coConversion.ContainsKey(livstr))
                return coConversion[livstr];
            if (ivstrformat != null && ivstrformat.Contains("{"))
                return string.Format(ivstrformat, livstr);
            string[] lcvstrFmt;
            if (ivstrformat != null && ivstrformat.Contains("["))
            {
                lcvstrFmt = ivstrformat.Replace("[", "").Replace("]", "").Split(",");
                livstr = livstr.Substring(int.Parse(lcvstrFmt[0]), Math.Min(livstr.Length, int.Parse(lcvstrFmt[1])));
            }
            else if (ivstrformat != null && ivstrformat.Contains("#R:"))
            {
                lcvstrFmt = ivstrformat.Replace("#R:", "").Split("_S_");
                livstr = lcvstrFmt.Length == 2 ? livstr.Replace(lcvstrFmt[0], lcvstrFmt[1]) : livstr;
            }
            if (ivstrProperty.Contains("dbl"))
            {
                if (livstr.Contains("-"))
                    livstr = "-" + livstr.Replace("-", string.Empty);
                if (livstr.Contains(","))
                {
                    int livnumIdxPto = livstr.IndexOf(".");
                    int livnumIdxComa = livstr.IndexOf(",");
                    if (livnumIdxPto != -1 && livnumIdxComa != -1 && livnumIdxPto > livnumIdxComa)
                        //tiene , y . y la , es antes que el .
                        livstr = livstr.Replace(",", string.Empty);
                    else if (livnumIdxPto != -1 && livnumIdxComa != -1 && livnumIdxPto < livnumIdxComa)
                        //tiene , y . y la , es despues que el .
                        livstr = livstr.Replace(".", string.Empty).Replace(",", ".");
                    else if (livnumIdxPto == -1)
                        //tiene , y no tiene .
                        livstr = livstr.Replace(",", ".");
                }
                Double livval;
                if (!Double.TryParse(livstr, out livval))
                    return string.Empty;
                return livval.ToString();
            }
            if (ivstrformat != null && ivstrformat.Contains("=>"))
            {
                if (!DateTime.TryParseExact(livstr, ivstrformat.Split("=>")[0], null, DateTimeStyles.None, out DateTime livdtm))
                    return string.Empty;
                return livdtm.ToString(ivstrformat.Split("=>")[1], null);
            }
            return livstr;
        }
    }
    public class ServiceMapperItemXPath
    {
        public string? ivstrParent { get; set; }
        public string? ivstrData { get; set; }
        public string? ivstrEnum { get; set; }
        public string? ivstrCoord { get; set; }
    }
    public class TemplateVersion
    {
        public short ivnroTipo { get; set; }
        public short ivnroTemplateVersion { get; set; }

    }
    public class CuitParameter
    {
        public string ivstrId { get; set; }
        public string ivstrValue { get; set; }

    }
}

