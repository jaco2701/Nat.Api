using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Text;

namespace Nat.API.Models.BR
{
    public class OperaFiscalPayload 
    {
        #region CONS
        #endregion
        #region PUBLIC METHODS  
        #endregion
        #region PRIVATE PROPS  
        #endregion
        #region PUBLIC PROPS  
        public Header? Header { get; set; }
        public DepositInfo[]? DepositsInfo { get; set; }
        public DocumentInfo? DocumentInfo { get; set; }
        public AdditionalInfo? AdditionalInfo { get; set; }
        public UserDefinedFields? UserDefinedFields { get; set; }
        public FiscalTerminalInfo? FiscalTerminalInfo { get; set; }
        public FolioInfo? FolioInfo { get; set; }
        public HotelInfo? HotelInfo { get; set; }
        public ReservationInfo? ReservationInfo { get; set; }
        public FiscalFolioUserInfo? FiscalFolioUserInfo { get; set; }
        public CollectingAgentPropertyInfo? CollectingAgentPropertyInfo { get; set; }
        public VersionInfo? VersionInfo { get; set; }
        public FiscalPartnerResponse? FiscalPartnerResponse { get; set; }
        [JsonIgnore]
        public long ivlngCuitEmisor
        {
            get
            {
                if (DocumentInfo == null) return 0;
                if (string.IsNullOrEmpty(DocumentInfo.PropertyTaxNumber)) return 0;
                if (!long.TryParse(DocumentInfo.PropertyTaxNumber.ToString(), out long livlngCuit)) return 0;
                return livlngCuit;
            }
        }
        #endregion

    }
#region HEADER
    public class Header
{
    public Partners[]? Partners { get; set; }
}
public class Partners
{
    public Partner? Partner { get; set; }
}
public class Partner
{
    public UserDefinedFields? UserDefinedFields { get; set; }
    public FiscalTerminalInfo? FiscalTerminalInfo { get; set; }
    public string? Name { get; set; }
    public string? Priority { get; set; }
    public string? FolioRequired { get; set; }
}

    #endregion
#region DEPOSIT INFO
public class DepositInfo
{
    public FolioHeaderInfo? FolioHeaderInfo { get; set; }
    public Posting? Postings { get; set; }
    public RevenueBucketInfo? RevenueBucketInfo { get; set; }
    public TrxInfo? TrxInfo { get; set; }
}
public class FolioHeaderInfo
{
    public DateTime? BillGenerationDate { get; set; }
    public string? FolioType { get; set; }
    public bool? CreditBill { get; set; }
    public int? FolioNo { get; set; }
    public string? QueueName { get; set; }
    public int? BillNo { get; set; }
    public int? InvoiceNo { get; set; }
    public string? InvoiceCurrencyCode { get; set; }
    public string? InvoiceCurrencyRate { get; set; }
    public string? FiscalBillNo { get; set; }
    public string? FiscalBillGenerationDate { get; set; }
    public string? FiscalBillGenerationTime { get; set; }
    public int? Window { get; set; }
    public int? CashierNumber { get; set; }
    public string? FiscalFolioStatus { get; set; }
    public string? SignatureHash { get; set; }
    public string? LastSignatureHash { get; set; }
    public string? ServiceType { get; set; }
    public DateTime? LocalBillGenerationDate { get; set; }
    public string? PropertyBillPrefix { get; set; }
    public string? OriginalBillNo { get; set; }
    public string? SampleFolioText1 { get; set; }
    public string? FolioTypeUniqueCode { get; set; }
    public AmountUDFsType[]? CollectingAgentTaxes { get; set; }
}
public class GenerateInfo
{
    public double? TrxNo { get; set; }
    public string? TrxCode { get; set; }
    public DateTime? TrxDate { get; set; }
    public string? TrxType { get; set; }
    public double? UnitPrice { get; set; }
    public double? Quantity { get; set; }
    public string? Currency { get; set; }
    public bool? TaxInclusive { get; set; }
    public double? ExchangeRate { get; set; }
    public double? TaxRate { get; set; }
    public DateTime? TrxDateTime { get; set; }
    public DateTime? LocalTrxDateTime { get; set; }
    public double? NetAmount { get; set; }
    public double? GrossAmount { get; set; }
    public double? TrxNoAdjust { get; set; }
    public double? TrxNoAddedBy { get; set; }
    public double? TrxNoAgainstPackage { get; set; }
    public string? Remark { get; set; }
    public string? Reference { get; set; }
    public string? CouponNo { get; set; }
    public double? GuestAccountDebit { get; set; }
    public double? GuestAccountCredit { get; set; }
    public string? ArrangementCode { get; set; }
    public string? Product { get; set; }
    public double? PackageDebit { get; set; }
    public double? PackageCredit { get; set; }
    public double? PackageAllowance { get; set; }
    public string? PackageArrangementCode { get; set; }
    public double? DepositTrxNo { get; set; }
    public string? PackageTrxType { get; set; }
    public string? CreditCardNumberMasked { get; set; }
    public double? DepLedDebit { get; set; }
    public double? DepLedCredit { get; set; }
    public double? ARLedDebit { get; set; }
    public double? ARLedCredit { get; set; }
    public double? TranActionId { get; set; }
    public double? FinDmlSeqNo { get; set; }
    public double? ParallelGuestDebit { get; set; }
    public double? ParallelGuestCredit { get; set; }
    public string? ParallelCurrency { get; set; }
    public double? ParallelNetAmount { get; set; }
    public double? ParallelGrossAmount { get; set; }
    public string? ProfitOrLoss { get; set; }
    public string? ReasonCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReceiptType { get; set; }
    public string? ChequeNumber { get; set; }
    public int? VendorTransactionNo { get; set; }
    public int? CashierId { get; set; }
    public string? OrgResvNameId { get; set; }
    public int? VendorPositionNo { get; set; }

}
public class RevenueBucketInfo
{
    public string? BucketCode { get; set; }
    public string? BucketType { get; set; }
    public string? BucketValue { get; set; }
    public string? Description { get; set; }
    public double? BucketCodeTotalGross { get; set; }
    public double? BucketCodeTotalNet { get; set; }
    public string? TrxCode { get; set; }
}
public class FiscalArticleInfo
{
    public string? ArticleId { get; set; }
    public string? ArticleCode { get; set; }
    public string? Description { get; set; }
    public double? Price { get; set; }
    public string? LanguageCode { get; set; }

}
#endregion
#region DOCUMENT INFO
public class DocumentInfo
{
    public string? HotelCode { get; set; }
    public int? BillNo { get; set; }
    public string? FolioType { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? TerminalId { get; set; }
    public string? ProgramName { get; set; }
    public string? Version { get; set; }
    public int? FiscalFolioId { get; set; }
    public int? OperaFiscalBillNo { get; set; }
    public string? Application { get; set; }
    public string? PropertyTaxNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? BankIdType { get; set; }
    public string? BankIdCode { get; set; }
    public DateTime? BusinessDate { get; set; }
    public DateTime? BusinessDateTime { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryName { get; set; }
    public string? SupportingDocumentSeqNo { get; set; }
    public string? DocumentType { get; set; }
    public LastSupportingDocumentInfo? LastSupportingDocumentInfo { get; set; }
    public string? Command { get; set; }
    public string? FiscalTimeoutPeriod { get; set; }
    public string? FolioTaxSeqNo { get; set; }
    public string? CallbackURL { get; set; }
    public string? FiscalParameterstring { get; set; }
}
public class LastSupportingDocumentInfo
{
    public string? DocumentNo1 { get; set; }
    public string? DocumentNo2 { get; set; }
    public string? SpecialId { get; set; }
}
#endregion
#region ADDITIONAL INFO
public class AdditionalInfo
{
    public BeforeSettlement? BeforeSettlement { get; set; }
    public ProfileOptions? ProfileOptions { get; set; }
    public ReservationOptions? ReservationOptions { get; set; }
    public MappedValues? MappedValues { get; set; }
}
public class BeforeSettlement
{
    public NV? NV { get; set; }
}
public class ProfileOptions
{
    public NV? NV { get; set; }
}
public class ReservationOptions
{
    public NV? NV { get; set; }
}
public class MappedValues
{
    public MappingType[]? MappingType { get; set; }
}
public class MappingType
{
    public string? Type { get; set; }
    public MappedValue? MappedValue { get; set; }
}
public class MappedValue
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public NV? NV { get; set; }
}
#endregion
#region FISCAL TERMINAL INFO
public class FiscalTerminalInfo
{
    public string? TerminalID { get; set; }
    public string? TransactionType { get; set; }
    public DateTime? TimeStamp { get; set; }
    public string? LastReceiptNumber { get; set; }
    public string? TerminalAddress { get; set; }
    public string? DeviceValue { get; set; }
    public string? TerminalAddressAndPort { get; set; }
}
#endregion
#region FOLIO INFO
public class FolioInfo
{
    public FolioHeaderInfo? FolioHeaderInfo { get; set; }
    public PayeeInfo? PayeeInfo { get; set; }
    public Posting[]? Postings { get; set; }
    public RevenueBucketInfo[]? RevenueBucketInfo { get; set; }
    public ArrangementInfo[]? ArrangementInfo { get; set; }
    public TotalInfo? TotalInfo { get; set; }
    public TrxInfo? TrxInfo { get; set; }
    public PosChequeInfo? PosChequeInfo { get; set; }
}
public class PayeeInfo
{
    public string? NameId { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Name2 { get; set; }
    public string? Name3 { get; set; }
    public string? Passport { get; set; }
    public string? Tax1No { get; set; }
    public string? Tax2No { get; set; }
    public string? PaymentDueDate { get; set; }
    public string? NameType { get; set; }
    public string? Nationality { get; set; }
    public string? BillingContactName { get; set; }
    public string? Language { get; set; }
    public string? NameTaxType { get; set; }
    public ExternalRefInfo? ExternalRefInfo { get; set; }
    public string? BusinessId { get; set; }
    public string? BusinessRegistration { get; set; }
    public string? NameCo { get; set; }
    public string? Salutation { get; set; }
    public string? ECommerceID { get; set; }
    public string? Industry { get; set; }
    public int? FiscalGuestType { get; set; }
    public string? TaxCategory { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxOfficeDesc { get; set; }
    public double? TaxPercent1 { get; set; }
    public double? TaxPercent2 { get; set; }
    public double? TaxPercent3 { get; set; }
    public double? TaxPercent4 { get; set; }
    public double? TaxPercent5 { get; set; }
    public string? AlternateLastName { get; set; }
    public string? AlternateFirstName { get; set; }
    public string? AlternateSalutation { get; set; }
    public string? MiddleName { get; set; }
    public string? LegalCompany { get; set; }
    public string? FiscalClientId { get; set; }
    public string? Profession { get; set; }
    public string? Email { get; set; }
    public IdentificationInfo[]? IdentificationInfos { get; set; }
    public AddressInfo? Address { get; set; }
    public Phone? Phone { get; set; }
    public WebPage? WebPage { get; set; }
    public UserDefinedFields? UserDefinedFields { get; set; }
}
public class ExternalRefInfo
{
    public string? ReferenceName { get; set; }
    public string? ReferenceType { get; set; }
}
public class AddressInfo
{
    public string? Type { get; set; }
    public string? Address { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? Address4 { get; set; }
    public string? AddresseeStateDesc { get; set; }
    public string? AddresseeCountryDesc { get; set; }
    public string? IsoCode { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Province { get; set; }
    public bool Primary { get; set; }
}
public class Phone
{
    public string? Type { get; set; }
    public string? Number { get; set; }
}
public class WebPage
{
    public string? Type { get; set; }
    public string? Value { get; set; }
}
public class Posting
{
    public double? TrxNo { get; set; }
    public string? ArticleId { get; set; }
    public string? TrxCode { get; set; }
    public DateTime? TrxDate { get; set; }
    public string? TrxType { get; set; }
    public double? UnitPrice { get; set; }
    public double? Quantity { get; set; }
    public string? Currency { get; set; }
    public bool? TaxInclusive { get; set; }
    public double? ExchangeRate { get; set; }
    public double? TaxRate { get; set; }
    public DateTime? TrxDateTime { get; set; }
    public DateTime? LocalTrxDateTime { get; set; }
    public double? NetAmount { get; set; }
    public double? GrossAmount { get; set; }
    public double? TrxNoAdjust { get; set; }
    public double? TrxNoAddedBy { get; set; }
    public double? TrxNoAgainstPackage { get; set; }
    public string? Remark { get; set; }
    public string? Reference { get; set; }
    public string? CouponNo { get; set; }
    public double? GuestAccountDebit { get; set; }
    public double? GuestAccountCredit { get; set; }
    public string? ArrangementCode { get; set; }
    public string? Product { get; set; }
    public double? PackageDebit { get; set; }
    public double? PackageCredit { get; set; }
    public double? PackageAllowance { get; set; }
    public string? PackageArrangementCode { get; set; }
    public double? DepositTrxNo { get; set; }
    public string? PackageTrxType { get; set; }
    public string? CreditCardNumberMasked { get; set; }
    public double? DepLedDebit { get; set; }
    public double? DepLedCredit { get; set; }
    public double? ARLedDebit { get; set; }
    public double? ARLedCredit { get; set; }
    public double? TranActionId { get; set; }
    public double? FinDmlSeqNo { get; set; }
    public double? ParallelGuestDebit { get; set; }
    public double? ParallelGuestCredit { get; set; }
    public string? ParallelCurrency { get; set; }
    public double? ParallelNetAmount { get; set; }
    public double? ParallelGrossAmount { get; set; }
    public string? ProfitOrLoss { get; set; }
    public string? ReasonCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReceiptType { get; set; }
    public string? ChequeNumber { get; set; }
    public string? VendorTransactionNo { get; set; }
    public int? VendorPositionNo { get; set; }
    public string? CashierId { get; set; }
    public string? TrxNoHeader { get; set; }
    public string? OrgResvNameId { get; set; }
    public Generates[]? Generates { get; set; }
    public string? Display { get; set; }

}
public class Generates
{
    public GenerateInfo? Generate { get; set; }
}
public class ArrangementInfo
{
    public string? Description { get; set; }
    public string? ArrangementCode { get; set; }
}
public class TotalInfo
{
    public double? NetAmount { get; set; }
    public double? GrossAmount { get; set; }
    public double? NonTaxableAmount { get; set; }
    public double? PaidOut { get; set; }
    public Taxes? Taxes { get; set; }
}
public class Taxes
{
    public List<Tax>? Tax { get; set; }
}
public class Tax
{
    public string? Name { get; set; }
    public int? Value { get; set; }
    public double? NetAmount { get; set; }
    public string? Percent { get; set; }
    public string? Amount { get; set; }
}
public class TrxInfo
{
    public string? HotelCode { get; set; }
    public string? Group { get; set; }
    public string? SubGroup { get; set; }
    public string? Code { get; set; }
    public string? TrxType { get; set; }
    public double? TaxCodeNo { get; set; }
    public double? QuantityCode { get; set; }
    public string? Description { get; set; }
    public FiscalArticleInfo[]? Articles { get; set; }
    public FiscalArticleInfo[]? TranslatedDescriptions { get; set; }
    public string? TrxCodeType { get; set; }
    public bool? RevenueGroup { get; set; }
    public string? ServiceType { get; set; }
    public string? FiscalTrxCodeType { get; set; }
}
public class PosChequeInfo
{
    public string? TrxNo { get; set; }
    public string? ChequeNo { get; set; }
    public string? ChequeDetails { get; set; }
    public DateTime? ChequeDate { get; set; }
}
#endregion
#region HOTEL INFO
public class HotelInfo
{
    public string? HotelCode { get; set; }
    public AddressInfo? Address { get; set; }
    public string? HotelName { get; set; }
    public string? LegalOwner { get; set; }
    public string? LocalCurrency { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? TimeZoneRegion { get; set; }
    public string? Decimals { get; set; }
    public string? PhoneNo { get; set; }
    public string? Email { get; set; }
    public string? FaxNo { get; set; }
    public string? TollFreeNo { get; set; }
    public string? WebPage { get; set; }
    public DateTime PropertyDateTime { get; set; }
    public string? GovernmentId { get; set; }
    public string? BusinessPremiseId1 { get; set; }
    public string? BusinessPremiseId2 { get; set; }
    public ExchangeRates[]? ExchangeRates { get; set; }
}
public class ExchangeRateInfo
{
    public string? ExchangeRate { get; set; }
    public string? CurrencyCode { get; set; }
}
public class ExchangeRates
{
    public ExchangeRateInfo? ExchangeRateInfo { get; set; }
}
#endregion
#region RESERVATION INFO
public class ReservationInfo
{
    public string? ConfirmationNo { get; set; }
    public string? ResvNameID { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public int? NumberOfNights { get; set; }
    public DateTime? DepartureDate { get; set; }
    public int? NumAdults { get; set; }
    public int? NumChilds { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? ChildAgeBucket1 { get; set; }
    public string? ChildAgeBucket2 { get; set; }
    public string? ChildAgeBucket3 { get; set; }
    public string? NameTaxType { get; set; }
    public double? RoomRate { get; set; }
    public string? RatePlanCode { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomClass { get; set; }
    public string? RoomType { get; set; }
    public int? NumberOfRooms { get; set; }
    public string? Guarantee { get; set; }
    public string? MarketCode { get; set; }
    public string? SourceOfBusiness { get; set; }
    public string? ResStatus { get; set; }
    public string? GuestType { get; set; }
    public string? Origin { get; set; }
    public string? SourceCode { get; set; }
    public string? SourceGroup { get; set; }
    public string? CustomReference { get; set; }
    public string? TARecordLocator { get; set; }
    public DateTime? EntryDate { get; set; }
    public GuestInfo? GuestInfo { get; set; }
    public IdentificationInfoListValue[]? IdentificationInfos { get; set; }
    public KeywordInfos[]? KeywordInfos { get; set; }
    public AddressInfo? Address { get; set; }
    public Phone? Phone { get; set; }
    public WebPage? WebPage { get; set; }
    public ExternalRefInfo? ExternalRefInfo { get; set; }
    public LinkedProfileListValue[]? LinkedProfiles { get; set; }
    public AccompanyingGuestInfo[]? AccompanyingGuestInfo { get; set; }

}
public class GuestInfo
{
    public string? NameId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Name2 { get; set; }
    public string? Name3 { get; set; }
    public string? Passport { get; set; }
    public string? Tax1No { get; set; }
    public string? Tax2No { get; set; }
    public string? PaymentDueDate { get; set; }
    public string? NameTaxType { get; set; }
    public string? Nationality { get; set; }
    public string? BusinessId { get; set; }
    public int? FiscalGuestType { get; set; }
    public string? TaxCategory { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxOfficeDesc { get; set; }
    public double? TaxPercent1 { get; set; }
    public double? TaxPercent2 { get; set; }
    public double? TaxPercent3 { get; set; }
    public double? TaxPercent4 { get; set; }
    public double? TaxPercent5 { get; set; }
    public string? ArNumber { get; set; }
    public string? Email { get; set; }
    public string? AlternateLastName { get; set; }
    public string? AlternateFirstName { get; set; }
    public string? AlternateSalutation { get; set; }
    public string? MiddleName { get; set; }
    public string? LegalCompany { get; set; }
    public string? FiscalClientId { get; set; }
    public string? Profession { get; set; }
    public string? Salutation { get; set; }
    public string? BusinessRegistration { get; set; }

}
public class IdentificationInfoListValue
{
    public IdentificationInfo? IdentificationInfo { get; set; }
}
public class IdentificationInfo
{
    public string? IdType { get; set; }
    public int? IdNumber { get; set; }
    public DateTime? IdDate { get; set; }
    public string? IdCountry { get; set; }
    public string? IdPlace { get; set; }
    public bool? Primary { get; set; }
}
public class KeywordInfo
{
    public string? KeywordType { get; set; }
    public string? KeyWord { get; set; }
}
public class KeywordInfos
{
    public KeywordInfo? KeywordInfo { get; set; }
}
public class AccompanyingGuestInfo
{
    public GuestInfo? AccompanyingGuest { get; set; }
}
public class LinkedProfileListValue
{
    public LinkedProfileInfo? LinkedProfile { get; set; }
}
public class LinkedProfileInfo
{
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? Name2 { get; set; }
    public string? NameType { get; set; }
    public string? NameId { get; set; }
}
#endregion
#region FISCAL FOLIO USER INFO
public class FiscalFolioUserInfo
{
    public string? AppUser { get; set; }
    public string? AppUserId { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? CashierId { get; set; }
    public string? CashierName { get; set; }
}
#endregion
#region COLLECTING AGENT PROPERTY INFO
public class CollectingAgentPropertyInfo
{
    public NV? TaxPercents { get; set; }
    public NV? TriggerAmounts { get; set; }
}
#endregion
#region VERSION INFO
public class VersionInfo
{
    public string? PayloadVersion { get; set; }
    public string? ApplicationPath { get; set; }
    public OPERACloud? OPERACloud { get; set; }
    public OPERA5? OPERA5 { get; set; }
}
public class OPERACloud
{
    public string? OPERACloudVersion { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patchset { get; set; }
    public string? Patch { get; set; }
}
public class OPERA5
{
    public string? OPERA5Version { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patchset { get; set; }
    public string? Patch { get; set; }
}
#endregion
#region FISCAL PARTNER RESPONSE
public class FiscalPartnerResponse
{
    public FiscalResponse? FiscalResponse { get; set; }
}
public class FiscalResponse
{
    public string? ID { get; set; }
    public NV? NV { get; set; }
    public DateTime? BusinessDate { get; set; }
    public int? ResponseSeqNo { get; set; }
}
#endregion
#region OPERA FOLIO PDF PAYLOAD
public class OperaFolioPDFPayload
{
    public string? BillGenerationDate { get; set; }
    public int? BillNo { get; set; }
    public string? FolioType { get; set; }
    public string? Folio { get; set; }
}
#endregion
#region USER DEFINED FIELDS & NAME VALUE
public class UserDefinedFields
{
    public CharacterUDFs[]? CharacterUDFs { get; set; }
    public CharacterUDFs[]? NumericUDFs { get; set; }
    public CharacterUDFs[]? DateUDFs { get; set; }
}
public class NV
{
    public string? Name { get; set; }
    public string? Value { get; set; }
}
public class AmountUDFsType
{
    public NV[]? Amounts { get; set; }
}
public class CharacterUDFs
{
    public NV? UDF { get; set; }
}

    #endregion
}


