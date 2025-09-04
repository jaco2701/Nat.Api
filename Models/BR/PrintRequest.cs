using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Applet.Nat.Api.Models
{
    public class PrintRequest
    {
        public string? ivstrB64Document { get; set; }
        public string? ivstrTemplatePath { get; set; }
    }
}