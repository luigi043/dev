namespace InsureX.Application.Common.Constants;

public static class AssetStatusValues
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Pending = "Pending";
    public const string Sold = "Sold";
    public const string Transferred = "Transferred";
    public const string WrittenOff = "WrittenOff";
    public const string Stolen = "Stolen";
    public const string UnderRepair = "UnderRepair";
    public const string Retired = "Retired";
    
    public static readonly string[] All = new[]
    {
        Active, Inactive, Pending, Sold, Transferred, 
        WrittenOff, Stolen, UnderRepair, Retired
    };
}

public static class ComplianceStatusValues
{
    public const string Compliant = "Compliant";
    public const string NonCompliant = "NonCompliant";
    public const string Pending = "Pending";
    public const string Warning = "Warning";
    public const string Expiring = "Expiring";
    public const string Expired = "Expired";
    public const string UnderReview = "UnderReview";
    
    public static readonly string[] All = new[]
    {
        Compliant, NonCompliant, Pending, Warning, 
        Expiring, Expired, UnderReview
    };
}