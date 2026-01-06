using Applet.Nat.Api.Br.Models;
using System;
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
    }
    public class ServiceMapperItem
    {
        public string? ivstrProperty { get; set; }
        public ServiceMapperItemXPath[]? coXPaths { get; set; }
        public Dictionary<string,string>? coConversion { get; set; }
        public int? ivnumLen { get; set; }
        public string? ivstrLPad { get; set; }
        public string? ivstrRPad { get; set; }
        public string? ivstrformat { get; set; }
        public string? ivstrCoord { get; set; }
        public bool? ivblnRequired { get; set; }
        public bool ivblnIsNumeric { get { return ivstrProperty.Contains("ivnro") || ivstrProperty.Contains("ivnum") || ivstrProperty.Contains("ivlng") || ivstrProperty.Contains("ivdbl"); } }
        public string? ivstrDefault { get; set; }
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

