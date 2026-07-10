using Newtonsoft.Json;
namespace Nat.API.Models.BR
{

    public class OHIPPayload
    {
        [JsonProperty("propertyId")]
        public string PropertyId { get; set; }

        [JsonProperty("folioId")]
        public string FolioId { get; set; }

        [JsonProperty("businessDate")]
        public string BusinessDate { get; set; }

        [JsonProperty("billGenerationDate")]
        public DateTime BillGenerationDate { get; set; }

        [JsonProperty("billNumber")]
        public string BillNumber { get; set; }

        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("payeeProfile")]
        public PayeeProfile PayeeProfile { get; set; }

        [JsonProperty("totalInfo")]
        public TotalInfo TotalInfo { get; set; }

        [JsonProperty("taxBreakdown")]
        public List<TaxBreakdown> TaxBreakdown { get; set; }

        [JsonProperty("invoiceLines")]
        public List<InvoiceLine> InvoiceLines { get; set; }
    }

    public class PayeeProfile
    {
        [JsonProperty("profileId")]
        public string ProfileId { get; set; }

        [JsonProperty("profileType")]
        public string ProfileType { get; set; }

        [JsonProperty("taxRegistrationNumber")]
        public string TaxRegistrationNumber { get; set; }

        [JsonProperty("taxRegistrationType")]
        public string TaxRegistrationType { get; set; }

        [JsonProperty("fiscalCondition")]
        public string FiscalCondition { get; set; }

        [JsonProperty("postalAddress")]
        public PostalAddress PostalAddress { get; set; }
    }

    public class PostalAddress
    {
        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }
    }

    public class TotalInfo
    {
        [JsonProperty("grossAmount")]
        public decimal GrossAmount { get; set; }

        [JsonProperty("netAmount")]
        public decimal NetAmount { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
    }

    public class TaxBreakdown
    {
        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("taxType")]
        public string TaxType { get; set; }

        [JsonProperty("taxRate")]
        public decimal TaxRate { get; set; }

        [JsonProperty("taxableAmount")]
        public decimal TaxableAmount { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("afipTaxCode")]
        public string AfipTaxCode { get; set; }
    }

    public class InvoiceLine
    {
        [JsonProperty("lineNumber")]
        public int LineNumber { get; set; }

        [JsonProperty("chargeCode")]
        public string ChargeCode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("netAmount")]
        public decimal NetAmount { get; set; }

        [JsonProperty("taxRate")]
        public decimal TaxRate { get; set; }

        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }
    }
}


