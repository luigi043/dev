-- ============================================
-- INSUREX COMPLETE DATABASE SCHEMA
-- SQL Server 2019+ / Azure SQL Database
-- ============================================

-- ============================================
-- PART 1: DATABASE CREATION
-- ============================================

CREATE DATABASE InsureX;
GO

USE InsureX;
GO

-- ============================================
-- PART 2: SCHEMA CREATION
-- ============================================

CREATE SCHEMA tenant;
GO

CREATE SCHEMA audit;
GO

CREATE SCHEMA compliance;
GO

-- ============================================
-- PART 3: TABLES
-- ============================================

-- 3.1 TENANT MANAGEMENT
CREATE TABLE tenant.Tenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    TenantType NVARCHAR(50) NOT NULL, -- Bank, Insurer, Broker, Admin
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active, Suspended, Inactive
    SubscriptionTier NVARCHAR(50) NOT NULL DEFAULT 'Basic', -- Basic, Professional, Enterprise
    MaxAssets INT NULL,
    MaxUsers INT NULL,
    ContactName NVARCHAR(200) NULL,
    ContactEmail NVARCHAR(200) NULL,
    ContactPhone NVARCHAR(50) NULL,
    AddressLine1 NVARCHAR(200) NULL,
    AddressLine2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Country NVARCHAR(100) NULL,
    TaxId NVARCHAR(50) NULL,
    RegistrationNumber NVARCHAR(50) NULL,
    Settings NVARCHAR(MAX) NULL, -- JSON configuration
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL
);
GO

-- 3.2 ASP.NET CORE IDENTITY TABLES (AspNetUsers extended)
CREATE TABLE dbo.AspNetUsers (
    Id NVARCHAR(450) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    JobTitle NVARCHAR(100) NULL,
    Department NVARCHAR(100) NULL,
    ProfilePicture NVARCHAR(500) NULL,
    LastLoginAt DATETIME2 NULL,
    MustChangePassword BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_AspNetUsers_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id)
);
GO

-- Standard Identity tables (minimal implementation)
CREATE TABLE dbo.AspNetRoles (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    TenantId UNIQUEIDENTIFIER NULL,
    Description NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetRoleClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_Roles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetUserClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetUserLogins (
    LoginProvider NVARCHAR(450) NOT NULL,
    ProviderKey NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetUserTokens (
    UserId NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name NVARCHAR(450) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- 3.3 USER PERMISSIONS
CREATE TABLE tenant.UserPermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Permission NVARCHAR(100) NOT NULL, -- e.g., "assets.view", "policies.create", "compliance.manage"
    IsGranted BIT NOT NULL DEFAULT 1,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    GrantedBy NVARCHAR(100) NULL,
    ExpiresAt DATETIME2 NULL,
    CONSTRAINT FK_UserPermissions_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_UserPermissions_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserPermission UNIQUE (TenantId, UserId, Permission)
);
GO

-- 3.4 API KEYS FOR SYSTEM INTEGRATION
CREATE TABLE tenant.ApiKeys (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    KeyName NVARCHAR(100) NOT NULL,
    ApiKeyHash NVARCHAR(256) NOT NULL, -- Store hash, not actual key
    ApiKeyPrefix NVARCHAR(20) NOT NULL, -- First few chars for identification
    Permissions NVARCHAR(MAX) NOT NULL, -- JSON array of permissions
    AllowedIPs NVARCHAR(MAX) NULL, -- JSON array of allowed IPs
    ExpiresAt DATETIME2 NULL,
    LastUsedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    RevokedAt DATETIME2 NULL,
    RevokedBy NVARCHAR(100) NULL,
    RevocationReason NVARCHAR(500) NULL,
    CONSTRAINT FK_ApiKeys_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id)
);
GO

-- 3.5 ASSET REGISTRY
CREATE TABLE dbo.Assets (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AssetTag NVARCHAR(100) NOT NULL,
    AssetType NVARCHAR(100) NOT NULL, -- Vehicle, Equipment, Property, etc.
    Make NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    Year INT NULL,
    SerialNumber NVARCHAR(100) NULL,
    VIN NVARCHAR(50) NULL,
    RegistrationNumber NVARCHAR(50) NULL,
    LicensePlate NVARCHAR(20) NULL,
    Color NVARCHAR(50) NULL,
    FuelType NVARCHAR(50) NULL,
    EngineNumber NVARCHAR(100) NULL,
    PurchasePrice DECIMAL(18,2) NULL,
    CurrentValue DECIMAL(18,2) NULL,
    InsuredValue DECIMAL(18,2) NULL,
    PurchaseDate DATE NULL,
    InServiceDate DATE NULL,
    LastInspectionDate DATE NULL,
    NextInspectionDate DATE NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Inactive, Sold, WrittenOff, Deleted
    Condition NVARCHAR(50) NULL, -- Excellent, Good, Fair, Poor
    Location NVARCHAR(500) NULL,
    GPSLocation NVARCHAR(100) NULL,
    OwnerType NVARCHAR(50) NULL, -- Customer, Financer, Insurer
    OwnerId NVARCHAR(100) NULL, -- External ID
    OwnerName NVARCHAR(200) NULL,
    FinancerReference NVARCHAR(100) NULL,
    ContractNumber NVARCHAR(100) NULL,
    Notes NVARCHAR(MAX) NULL,
    CustomFields NVARCHAR(MAX) NULL, -- JSON for tenant-specific fields
    ComplianceStatus NVARCHAR(50) NULL, -- Compliant, NonCompliant, Warning, Pending
    ComplianceScore INT NULL, -- 0-100
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_Assets_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT UQ_AssetTag_Tenant UNIQUE (TenantId, AssetTag)
);
GO

-- Asset documents
CREATE TABLE dbo.AssetDocuments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIET NOT NULL,
    AssetId INT NOT NULL,
    DocumentType NVARCHAR(100) NOT NULL, -- PurchaseInvoice, Registration, Photo, Inspection
    DocumentName NVARCHAR(500) NOT NULL,
    DocumentUrl NVARCHAR(1000) NOT NULL,
    MimeType NVARCHAR(100) NULL,
    FileSize INT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedBy NVARCHAR(100) NULL,
    IsPublic BIT NOT NULL DEFAULT 0,
    ExpiryDate DATE NULL,
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AssetDocuments_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_AssetDocuments_Assets FOREIGN KEY (AssetId) REFERENCES dbo.Assets(Id) ON DELETE CASCADE
);
GO

-- 3.6 POLICY MANAGEMENT
CREATE TABLE dbo.Policies (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PolicyNumber NVARCHAR(100) NOT NULL,
    AssetId INT NOT NULL,
    InsurerCode NVARCHAR(50) NOT NULL,
    InsurerName NVARCHAR(200) NOT NULL,
    PolicyType NVARCHAR(100) NOT NULL, -- Comprehensive, ThirdParty, FireTheft, etc.
    PolicySubType NVARCHAR(100) NULL,
    ProductCode NVARCHAR(50) NULL,
    SumInsured DECIMAL(18,2) NOT NULL,
    Premium DECIMAL(18,2) NOT NULL,
    PremiumFrequency NVARCHAR(20) NULL, -- Monthly, Quarterly, Annually
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    RenewalDate DATE NULL,
    CancellationDate DATE NULL,
    CancellationReason NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Expired, Cancelled, Pending
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Paid, Pending, Overdue, Failed
    PaymentMethod NVARCHAR(50) NULL,
    LastPaymentDate DATE NULL,
    NextPaymentDate DATE NULL,
    CoverageDetails NVARCHAR(MAX) NULL,
    Exclusions NVARCHAR(MAX) NULL,
    TermsAndConditionsUrl NVARCHAR(1000) NULL,
    Documents NVARCHAR(MAX) NULL, -- JSON array of document URLs
    ClaimsCount INT NOT NULL DEFAULT 0,
    TotalClaimsAmount DECIMAL(18,2) NULL,
    LastClaimDate DATE NULL,
    UnderwriterReference NVARCHAR(100) NULL,
    BrokerReference NVARCHAR(100) NULL,
    CommissionRate DECIMAL(5,2) NULL,
    CommissionAmount DECIMAL(18,2) NULL,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_Policies_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_Policies_Assets FOREIGN KEY (AssetId) REFERENCES dbo.Assets(Id),
    CONSTRAINT UQ_PolicyNumber_Tenant UNIQUE (TenantId, PolicyNumber)
);
GO

-- Policy claims
CREATE TABLE dbo.PolicyClaims (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PolicyId INT NOT NULL,
    ClaimNumber NVARCHAR(100) NOT NULL,
    ClaimDate DATE NOT NULL,
    ClaimType NVARCHAR(100) NOT NULL, -- Accident, Theft, Fire, Damage, etc.
    ClaimAmount DECIMAL(18,2) NOT NULL,
    ApprovedAmount DECIMAL(18,2) NULL,
    ExcessAmount DECIMAL(18,2) NULL,
    SettlementAmount DECIMAL(18,2) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Submitted', -- Submitted, UnderReview, Approved, Rejected, Settled
    IncidentDate DATE NULL,
    IncidentDescription NVARCHAR(MAX) NULL,
    IncidentLocation NVARCHAR(500) NULL,
    PoliceReportNumber NVARCHAR(100) NULL,
    AssessorName NVARCHAR(200) NULL,
    AssessorReportUrl NVARCHAR(1000) NULL,
    SettlementDate DATE NULL,
    SettlementReference NVARCHAR(100) NULL,
    Documents NVARCHAR(MAX) NULL, -- JSON array
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_PolicyClaims_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_PolicyClaims_Policies FOREIGN KEY (PolicyId) REFERENCES dbo.Policies(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ClaimNumber_Tenant UNIQUE (TenantId, ClaimNumber)
);
GO

-- 3.7 COMPLIANCE ENGINE
CREATE TABLE compliance.Rules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    RuleName NVARCHAR(200) NOT NULL,
    RuleCode NVARCHAR(50) NOT NULL,
    Description NVARCHAR(1000) NULL,
    RuleType NVARCHAR(50) NOT NULL, -- Policy, Payment, Inspection, Documentation, Custom
    RuleCategory NVARCHAR(50) NOT NULL, -- Mandatory, Recommended, Optional
    ConditionExpression NVARCHAR(MAX) NOT NULL, -- JSON or expression
    ActionExpression NVARCHAR(MAX) NULL, -- JSON or expression
    Severity INT NOT NULL DEFAULT 1, -- 1-Low, 2-Medium, 3-High, 4-Critical
    IsActive BIT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 0,
    DaysToExpiry INT NULL, -- For expiry-based rules
    ApplicableAssetTypes NVARCHAR(MAX) NULL, -- JSON array
    ApplicablePolicyTypes NVARCHAR(MAX) NULL, -- JSON array
    MinValue INT NULL,
    MaxValue INT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    CustomScript NVARCHAR(MAX) NULL,
    SuccessMessage NVARCHAR(500) NULL,
    FailureMessage NVARCHAR(500) NULL,
    Recommendation NVARCHAR(500) NULL,
    EffectiveFrom DATE NULL,
    EffectiveTo DATE NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_Rules_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT UQ_RuleCode_Tenant UNIQUE (TenantId, RuleCode)
);
GO

CREATE TABLE compliance.Checks (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AssetId INT NOT NULL,
    RuleId INT NULL,
    CheckDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Status NVARCHAR(50) NOT NULL, -- Compliant, NonCompliant, Warning, Error
    Score INT NOT NULL DEFAULT 0, -- 0-100
    Findings NVARCHAR(MAX) NULL, -- JSON details
    Recommendations NVARCHAR(MAX) NULL,
    Evidence NVARCHAR(MAX) NULL, -- JSON references
    NextCheckDate DATETIME2 NULL,
    CheckedBy NVARCHAR(100) NULL,
    IsAutomatic BIT NOT NULL DEFAULT 1,
    ExecutionTimeMs INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Checks_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_Checks_Assets FOREIGN KEY (AssetId) REFERENCES dbo.Assets(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Checks_Rules FOREIGN KEY (RuleId) REFERENCES compliance.Rules(Id)
);
GO

CREATE TABLE compliance.History (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AssetId INT NOT NULL,
    ChangeDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FromStatus NVARCHAR(50) NOT NULL,
    ToStatus NVARCHAR(50) NOT NULL,
    FromScore INT NOT NULL,
    ToScore INT NOT NULL,
    Reason NVARCHAR(MAX) NOT NULL,
    TriggeredBy NVARCHAR(100) NULL, -- Rule, User, System
    TriggerId NVARCHAR(100) NULL, -- Rule ID, User ID, etc.
    Evidence NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_History_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_History_Assets FOREIGN KEY (AssetId) REFERENCES dbo.Assets(Id) ON DELETE CASCADE
);
GO

CREATE TABLE compliance.Alerts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AssetId INT NOT NULL,
    RuleId INT NULL,
    AlertType NVARCHAR(50) NOT NULL, -- Warning, Violation, Expiry, Critical
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Severity INT NOT NULL DEFAULT 1,
    Status NVARCHAR(50) NOT NULL DEFAULT 'New', -- New, Acknowledged, Resolved, Ignored
    AssignedTo NVARCHAR(450) NULL,
    AcknowledgedAt DATETIME2 NULL,
    AcknowledgedBy NVARCHAR(100) NULL,
    ResolvedAt DATETIME2 NULL,
    ResolvedBy NVARCHAR(100) NULL,
    ResolutionNotes NVARCHAR(MAX) NULL,
    ResolutionType NVARCHAR(50) NULL, -- Fixed, Waived, FalseAlarm, etc.
    DueDate DATETIME2 NULL,
    EscalationLevel INT NOT NULL DEFAULT 0,
    RequiresAction BIT NOT NULL DEFAULT 1,
    ActionTaken NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Alerts_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_Alerts_Assets FOREIGN KEY (AssetId) REFERENCES dbo.Assets(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Alerts_Rules FOREIGN KEY (RuleId) REFERENCES compliance.Rules(Id),
    CONSTRAINT FK_Alerts_Users FOREIGN KEY (AssignedTo) REFERENCES dbo.AspNetUsers(Id)
);
GO

-- 3.8 WORKFLOW & CASE MANAGEMENT
CREATE TABLE workflow.Cases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    CaseNumber NVARCHAR(100) NOT NULL,
    CaseType NVARCHAR(50) NOT NULL, -- NonCompliance, Claim, Renewal, Inspection
    AssetId INT NULL,
    PolicyId INT NULL,
    AlertId INT NULL,
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- Low, Medium, High, Critical
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open', -- Open, InProgress, Pending, Resolved, Closed
    Stage NVARCHAR(50) NULL, -- Initial, Investigation, Resolution, Review
    AssignedTo NVARCHAR(450) NULL,
    AssignedTeam NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    DueDate DATETIME2 NULL,
    ClosedAt DATETIME2 NULL,
    ClosedBy NVARCHAR(100) NULL,
    ResolutionSummary NVARCHAR(MAX) NULL,
    EscalationCount INT NOT NULL DEFAULT 0,
    LastEscalatedAt DATETIME2 NULL,
    SLAHours INT NULL,
    SLAExpiresAt DATETIME2 NULL,
    SLABreached BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Cases_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_Cases_Assets FOREIGN KEY (AssetId) REFERENCES dbo.Assets(Id),
    CONSTRAINT FK_Cases_Policies FOREIGN KEY (PolicyId) REFERENCES dbo.Policies(Id),
    CONSTRAINT FK_Cases_Alerts FOREIGN KEY (AlertId) REFERENCES compliance.Alerts(Id),
    CONSTRAINT FK_Cases_AssignedUser FOREIGN KEY (AssignedTo) REFERENCES dbo.AspNetUsers(Id),
    CONSTRAINT UQ_CaseNumber_Tenant UNIQUE (TenantId, CaseNumber)
);
GO

CREATE TABLE workflow.Tasks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    CaseId INT NOT NULL,
    TaskNumber NVARCHAR(100) NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    TaskType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, InProgress, Completed, Cancelled
    AssignedTo NVARCHAR(450) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    CompletedBy NVARCHAR(100) NULL,
    DueDate DATETIME2 NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    EstimatedHours DECIMAL(5,2) NULL,
    ActualHours DECIMAL(5,2) NULL,
    Outcome NVARCHAR(MAX) NULL,
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Tasks_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_Tasks_Cases FOREIGN KEY (CaseId) REFERENCES workflow.Cases(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Tasks_AssignedUser FOREIGN KEY (AssignedTo) REFERENCES dbo.AspNetUsers(Id),
    CONSTRAINT UQ_TaskNumber_Tenant UNIQUE (TenantId, TaskNumber)
);
GO

CREATE TABLE workflow.Comments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(50) NOT NULL, -- Case, Task, Alert, Asset
    EntityId INT NOT NULL,
    UserId NVARCHAR(450) NULL,
    Comment NVARCHAR(MAX) NOT NULL,
    IsInternal BIT NOT NULL DEFAULT 0,
    Mentions NVARCHAR(MAX) NULL, -- JSON array of mentioned user IDs
    Attachments NVARCHAR(MAX) NULL, -- JSON array
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Comments_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
);
GO

CREATE TABLE workflow.Attachments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId INT NOT NULL,
    FileName NVARCHAR(500) NOT NULL,
    FileUrl NVARCHAR(1000) NOT NULL,
    FileSize INT NULL,
    MimeType NVARCHAR(100) NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_Attachments_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id)
);
GO

-- 3.9 AUDIT & EVIDENCE
CREATE TABLE audit.Logs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NULL,
    UserId NVARCHAR(450) NULL,
    EventType NVARCHAR(100) NOT NULL, -- Create, Update, Delete, View, Login, Export
    EntityType NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(100) NULL,
    OldValues NVARCHAR(MAX) NULL, -- JSON
    NewValues NVARCHAR(MAX) NULL, -- JSON
    Changes NVARCHAR(MAX) NULL, -- JSON of changed fields
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    RequestPath NVARCHAR(500) NULL,
    RequestMethod NVARCHAR(20) NULL,
    ResponseStatusCode INT NULL,
    ExecutionTimeMs INT NULL,
    CorrelationId NVARCHAR(100) NULL,
    SessionId NVARCHAR(100) NULL,
    AdditionalData NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AuditLogs_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id)
);
GO

CREATE TABLE audit.Evidence (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EvidenceType NVARCHAR(50) NOT NULL, -- Webhook, Document, Report, Image
    Source NVARCHAR(100) NOT NULL, -- System, API, Upload, Integration
    SourceId NVARCHAR(100) NULL, -- External reference
    EntityType NVARCHAR(100) NULL,
    EntityId INT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    ContentLength INT NULL,
    ContentHash NVARCHAR(256) NOT NULL,
    StoragePath NVARCHAR(1000) NOT NULL,
    StorageContainer NVARCHAR(100) NOT NULL,
    FileName NVARCHAR(500) NULL,
    Metadata NVARCHAR(MAX) NULL, -- JSON
    ExpiresAt DATETIME2 NULL,
    IsArchived BIT NOT NULL DEFAULT 0,
    ArchivedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_Evidence_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id)
);
GO

-- 3.10 INTEGRATION & WEBHOOKS
CREATE TABLE integration.WebhookEvents (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EventId NVARCHAR(100) NOT NULL,
    EventType NVARCHAR(100) NOT NULL,
    Source NVARCHAR(100) NOT NULL, -- Insurer, Bank, System
    SourceSystem NVARCHAR(100) NULL,
    WebhookUrl NVARCHAR(1000) NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    Headers NVARCHAR(MAX) NULL,
    Signature NVARCHAR(256) NULL,
    SignatureVersion NVARCHAR(20) NULL,
    IdempotencyKey NVARCHAR(100) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Processed, Failed, Ignored
    ProcessingStatus NVARCHAR(50) NULL,
    AttemptCount INT NOT NULL DEFAULT 0,
    LastAttemptAt DATETIME2 NULL,
    ProcessedAt DATETIME2 NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    ResponseCode INT NULL,
    ResponseBody NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_WebhookEvents_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT UQ_WebhookEventId_Tenant UNIQUE (TenantId, EventId)
);
GO

CREATE TABLE integration.ConnectorConfigs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ConnectorType NVARCHAR(50) NOT NULL, -- Insurer, Bank, Custom
    ConnectorName NVARCHAR(200) NOT NULL,
    ConnectorCode NVARCHAR(50) NOT NULL,
    BaseUrl NVARCHAR(500) NOT NULL,
    AuthType NVARCHAR(50) NOT NULL, -- OAuth2, ApiKey, Basic, None
    AuthConfig NVARCHAR(MAX) NULL, -- JSON
    Headers NVARCHAR(MAX) NULL, -- JSON
    TimeoutSeconds INT NOT NULL DEFAULT 30,
    RetryCount INT NOT NULL DEFAULT 3,
    RetryDelayMs INT NOT NULL DEFAULT 1000,
    IsActive BIT NOT NULL DEFAULT 1,
    LastTestedAt DATETIME2 NULL,
    LastTestStatus NVARCHAR(50) NULL,
    ErrorCount INT NOT NULL DEFAULT 0,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_ConnectorConfigs_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT UQ_ConnectorCode_Tenant UNIQUE (TenantId, ConnectorCode)
);
GO

-- 3.11 NOTIFICATIONS
CREATE TABLE dbo.Notifications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId NVARCHAR(450) NULL,
    NotificationType NVARCHAR(50) NOT NULL, -- Email, SMS, InApp, Push
    Channel NVARCHAR(50) NOT NULL,
    Subject NVARCHAR(500) NULL,
    Body NVARCHAR(MAX) NOT NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal',
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Sent, Failed, Delivered, Read
    SentAt DATETIME2 NULL,
    DeliveredAt DATETIME2 NULL,
    ReadAt DATETIME2 NULL,
    FailedAt DATETIME2 NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    ReferenceType NVARCHAR(50) NULL, -- Asset, Policy, Case, Alert
    ReferenceId INT NULL,
    Metadata NVARCHAR(MAX) NULL, -- JSON
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Notifications_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- 3.12 DASHBOARD & REPORTING
CREATE TABLE reporting.DashboardSnapshots (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SnapshotDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TotalAssets INT NOT NULL,
    ActiveAssets INT NOT NULL,
    TotalPolicies INT NOT NULL,
    ActivePolicies INT NOT NULL,
    ExpiringPolicies INT NOT NULL,
    TotalClaims INT NOT NULL,
    PendingClaims INT NOT NULL,
    CompliantAssets INT NOT NULL,
    NonCompliantAssets INT NOT NULL,
    WarningAssets INT NOT NULL,
    OverallComplianceRate DECIMAL(5,2) NOT NULL,
    ActiveAlerts INT NOT NULL,
    CriticalAlerts INT NOT NULL,
    OpenCases INT NOT NULL,
    OverdueTasks INT NOT NULL,
    TotalSumInsured DECIMAL(18,2) NOT NULL,
    TotalPremium DECIMAL(18,2) NOT NULL,
    TotalClaimsAmount DECIMAL(18,2) NOT NULL,
    AssetTypeBreakdown NVARCHAR(MAX) NULL, -- JSON
    PolicyTypeBreakdown NVARCHAR(MAX) NULL,
    ComplianceTrend NVARCHAR(MAX) NULL, -- JSON
    TopIssues NVARCHAR(MAX) NULL, -- JSON
    RecentActivities NVARCHAR(MAX) NULL, -- JSON
    Data NVARCHAR(MAX) NULL, -- Full snapshot data
    CONSTRAINT FK_DashboardSnapshots_Tenants FOREIGN KEY (TenantId) REFERENCES tenant.Tenants(Id)
);
GO

-- ============================================
-- PART 4: INDEXES
-- ============================================

-- Tenants
CREATE INDEX IX_Tenants_Code ON tenant.Tenants(Code);
CREATE INDEX IX_Tenants_Status ON tenant.Tenants(Status);

-- Users
CREATE INDEX IX_AspNetUsers_TenantId ON dbo.AspNetUsers(TenantId);
CREATE INDEX IX_AspNetUsers_Email ON dbo.AspNetUsers(Email);
CREATE INDEX IX_AspNetUsers_UserName ON dbo.AspNetUsers(UserName);
CREATE INDEX IX_AspNetUsers_IsActive ON dbo.AspNetUsers(IsActive);

-- Assets
CREATE INDEX IX_Assets_TenantId ON dbo.Assets(TenantId);
CREATE INDEX IX_Assets_AssetTag ON dbo.Assets(AssetTag);
CREATE INDEX IX_Assets_Status ON dbo.Assets(Status);
CREATE INDEX IX_Assets_ComplianceStatus ON dbo.Assets(ComplianceStatus);
CREATE INDEX IX_Assets_OwnerId ON dbo.Assets(OwnerId);
CREATE INDEX IX_Assets_VIN ON dbo.Assets(VIN);
CREATE INDEX IX_Assets_SerialNumber ON dbo.Assets(SerialNumber);
CREATE INDEX IX_Assets_CreatedAt ON dbo.Assets(CreatedAt);

-- Policies
CREATE INDEX IX_Policies_TenantId ON dbo.Policies(TenantId);
CREATE INDEX IX_Policies_AssetId ON dbo.Policies(AssetId);
CREATE INDEX IX_Policies_PolicyNumber ON dbo.Policies(PolicyNumber);
CREATE INDEX IX_Policies_Status ON dbo.Policies(Status);
CREATE INDEX IX_Policies_StartDate ON dbo.Policies(StartDate);
CREATE INDEX IX_Policies_EndDate ON dbo.Policies(EndDate);
CREATE INDEX IX_Policies_InsurerCode ON dbo.Policies(InsurerCode);

-- Claims
CREATE INDEX IX_PolicyClaims_TenantId ON dbo.PolicyClaims(TenantId);
CREATE INDEX IX_PolicyClaims_PolicyId ON dbo.PolicyClaims(PolicyId);
CREATE INDEX IX_PolicyClaims_Status ON dbo.PolicyClaims(Status);
CREATE INDEX IX_PolicyClaims_ClaimDate ON dbo.PolicyClaims(ClaimDate);

-- Compliance
CREATE INDEX IX_ComplianceRules_TenantId ON compliance.Rules(TenantId);
CREATE INDEX IX_ComplianceRules_IsActive ON compliance.Rules(IsActive);
CREATE INDEX IX_ComplianceChecks_TenantId ON compliance.Checks(TenantId);
CREATE INDEX IX_ComplianceChecks_AssetId ON compliance.Checks(AssetId);
CREATE INDEX IX_ComplianceChecks_CheckDate ON compliance.Checks(CheckDate);
CREATE INDEX IX_ComplianceHistory_AssetId ON compliance.History(AssetId);
CREATE INDEX IX_ComplianceAlerts_TenantId ON compliance.Alerts(TenantId);
CREATE INDEX IX_ComplianceAlerts_AssetId ON compliance.Alerts(AssetId);
CREATE INDEX IX_ComplianceAlerts_Status ON compliance.Alerts(Status);

-- Workflow
CREATE INDEX IX_Cases_TenantId ON workflow.Cases(TenantId);
CREATE INDEX IX_Cases_AssetId ON workflow.Cases(AssetId);
CREATE INDEX IX_Cases_Status ON workflow.Cases(Status);
CREATE INDEX IX_Cases_AssignedTo ON workflow.Cases(AssignedTo);
CREATE INDEX IX_Tasks_CaseId ON workflow.Tasks(CaseId);
CREATE INDEX IX_Tasks_AssignedTo ON workflow.Tasks(AssignedTo);

-- Audit
CREATE INDEX IX_AuditLogs_TenantId ON audit.Logs(TenantId);
CREATE INDEX IX_AuditLogs_UserId ON audit.Logs(UserId);
CREATE INDEX IX_AuditLogs_EntityType ON audit.Logs(EntityType);
CREATE INDEX IX_AuditLogs_CreatedAt ON audit.Logs(CreatedAt);
CREATE INDEX IX_AuditLogs_CorrelationId ON audit.Logs(CorrelationId);

-- Integration
CREATE INDEX IX_WebhookEvents_TenantId ON integration.WebhookEvents(TenantId);
CREATE INDEX IX_WebhookEvents_Status ON integration.WebhookEvents(Status);
CREATE INDEX IX_WebhookEvents_CreatedAt ON integration.WebhookEvents(CreatedAt);

-- Notifications
CREATE INDEX IX_Notifications_TenantId ON dbo.Notifications(TenantId);
CREATE INDEX IX_Notifications_UserId ON dbo.Notifications(UserId);
CREATE INDEX IX_Notifications_Status ON dbo.Notifications(Status);

-- ============================================
-- PART 5: ROW-LEVEL SECURITY (RLS)
-- ============================================

-- Create function to get current tenant ID from session context
CREATE FUNCTION dbo.fn_GetCurrentTenantId()
RETURNS UNIQUEIDENTIFIER
WITH EXECUTE AS CALLER
AS
BEGIN
    RETURN CAST(SESSION_CONTEXT(N'TenantId') AS UNIQUEIDENTIFIER);
END
GO

-- Create security predicate
CREATE FUNCTION dbo.fn_TenantSecurityPredicate(@TenantId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
    RETURN SELECT 1 AS AccessResult
    WHERE 
        (@TenantId = dbo.fn_GetCurrentTenantId()) -- Regular tenant access
        OR 
        (dbo.fn_GetCurrentTenantId() IS NULL AND IS_MEMBER('db_owner') = 1) -- Admin access
GO

-- Apply RLS to tenant-scoped tables
CREATE SECURITY POLICY TenantAccessPolicy
    ADD FILTER PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON dbo.Assets,
    ADD BLOCK PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON dbo.Assets,
    ADD FILTER PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON dbo.Policies,
    ADD BLOCK PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON dbo.Policies,
    ADD FILTER PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON dbo.PolicyClaims,
    ADD BLOCK PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON dbo.PolicyClaims,
    ADD FILTER PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON compliance.Checks,
    ADD BLOCK PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON compliance.Checks,
    ADD FILTER PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON compliance.Alerts,
    ADD BLOCK PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON compliance.Alerts,
    ADD FILTER PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON workflow.Cases,
    ADD BLOCK PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON workflow.Cases,
    ADD FILTER PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON workflow.Tasks,
    ADD BLOCK PREDICATE dbo.fn_TenantSecurityPredicate(TenantId) ON workflow.Tasks
    WITH (STATE = ON);
GO

-- ============================================
-- PART 6: STORED PROCEDURES
-- ============================================

-- Set tenant context for connection
CREATE PROCEDURE dbo.sp_SetTenantContext
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC sp_set_session_context 'TenantId', @TenantId;
END
GO

-- Get compliance summary
CREATE PROCEDURE compliance.sp_GetComplianceSummary
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS TotalAssets,
        SUM(CASE WHEN ComplianceStatus = 'Compliant' THEN 1 ELSE 0 END) AS CompliantAssets,
        SUM(CASE WHEN ComplianceStatus = 'NonCompliant' THEN 1 ELSE 0 END) AS NonCompliantAssets,
        SUM(CASE WHEN ComplianceStatus = 'Warning' THEN 1 ELSE 0 END) AS WarningAssets,
        AVG(CAST(ComplianceScore AS FLOAT)) AS AverageComplianceScore,
        (SELECT COUNT(*) FROM compliance.Alerts WHERE TenantId = @TenantId AND Status IN ('New', 'Acknowledged')) AS ActiveAlerts
    FROM dbo.Assets
    WHERE TenantId = @TenantId;
END
GO

-- Get expiring policies
CREATE PROCEDURE dbo.sp_GetExpiringPolicies
    @TenantId UNIQUEIDENTIFIER,
    @Days INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.*,
        a.AssetTag,
        a.Make,
        a.Model
    FROM dbo.Policies p
    INNER JOIN dbo.Assets a ON p.AssetId = a.Id
    WHERE p.TenantId = @TenantId
        AND p.Status = 'Active'
        AND p.EndDate <= DATEADD(DAY, @Days, GETUTCDATE())
        AND p.EndDate >= GETUTCDATE()
    ORDER BY p.EndDate;
END
GO

-- ============================================
-- PART 7: VIEWS
-- ============================================

-- Asset compliance view
CREATE VIEW dbo.vw_AssetCompliance
AS
SELECT 
    a.Id AS AssetId,
    a.TenantId,
    a.AssetTag,
    a.Make,
    a.Model,
    a.Status,
    a.ComplianceStatus,
    a.ComplianceScore,
    (SELECT COUNT(*) FROM dbo.Policies p WHERE p.AssetId = a.Id AND p.Status = 'Active') AS ActivePolicies,
    (SELECT COUNT(*) FROM compliance.Alerts al WHERE al.AssetId = a.Id AND al.Status IN ('New', 'Acknowledged')) AS ActiveAlerts,
    (SELECT TOP 1 CheckDate FROM compliance.Checks c WHERE c.AssetId = a.Id ORDER BY CheckDate DESC) AS LastChecked
FROM dbo.Assets a;
GO

-- Policy status view
CREATE VIEW dbo.vw_PolicyStatus
AS
SELECT 
    p.*,
    a.AssetTag,
    a.Make,
    a.Model,
    CASE 
        WHEN p.EndDate < GETUTCDATE() THEN 'Expired'
        WHEN p.EndDate <= DATEADD(DAY, 30, GETUTCDATE()) THEN 'ExpiringSoon'
        ELSE 'Valid'
    END AS ValidityStatus,
    DATEDIFF(DAY, GETUTCDATE(), p.EndDate) AS DaysRemaining
FROM dbo.Policies p
INNER JOIN dbo.Assets a ON p.AssetId = a.Id;
GO

-- Dashboard summary view
CREATE VIEW reporting.vw_DashboardSummary
AS
SELECT 
    a.TenantId,
    COUNT(DISTINCT a.Id) AS TotalAssets,
    COUNT(DISTINCT CASE WHEN a.ComplianceStatus = 'Compliant' THEN a.Id END) AS CompliantAssets,
    COUNT(DISTINCT CASE WHEN a.ComplianceStatus = 'NonCompliant' THEN a.Id END) AS NonCompliantAssets,
    COUNT(DISTINCT p.Id) AS TotalPolicies,
    COUNT(DISTINCT CASE WHEN p.Status = 'Active' THEN p.Id END) AS ActivePolicies,
    COUNT(DISTINCT CASE WHEN p.EndDate <= DATEADD(DAY, 30, GETUTCDATE()) AND p.EndDate >= GETUTCDATE() THEN p.Id END) AS ExpiringPolicies,
    COUNT(DISTINCT c.Id) AS TotalClaims,
    COUNT(DISTINCT CASE WHEN c.Status IN ('Submitted', 'UnderReview') THEN c.Id END) AS PendingClaims,
    COUNT(DISTINCT al.Id) AS ActiveAlerts,
    SUM(p.SumInsured) AS TotalSumInsured,
    SUM(p.Premium) AS TotalPremium
FROM dbo.Assets a
LEFT JOIN dbo.Policies p ON a.Id = p.AssetId
LEFT JOIN dbo.PolicyClaims c ON p.Id = c.PolicyId
LEFT JOIN compliance.Alerts al ON a.Id = al.AssetId AND al.Status IN ('New', 'Acknowledged')
GROUP BY a.TenantId;
GO

-- ============================================
-- PART 8: SEED DATA
-- ============================================

-- Seed tenants
INSERT INTO tenant.Tenants (Id, Name, Code, TenantType, SubscriptionTier, ContactEmail)
VALUES 
    (NEWID(), 'Demo Bank', 'DEMOBANK', 'Bank', 'Enterprise', 'admin@demobank.com'),
    (NEWID(), 'Sample Insurer', 'SAMPLEINS', 'Insurer', 'Professional', 'admin@sampleinsurer.com'),
    (NEWID(), 'Test Broker', 'TESTBROKER', 'Broker', 'Basic', 'admin@testbroker.com');
GO

-- Seed roles
INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName)
VALUES 
    (NEWID(), 'Admin', 'ADMIN'),
    (NEWID(), 'BankUser', 'BANKUSER'),
    (NEWID(), 'InsurerUser', 'INSURERUSER'),
    (NEWID(), 'BrokerUser', 'BROKERUSER'),
    (NEWID(), 'Viewer', 'VIEWER');
GO

-- Seed compliance rules
DECLARE @TenantId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM tenant.Tenants WHERE Code = 'DEMOBANK');

INSERT INTO compliance.Rules (TenantId, RuleName, RuleCode, RuleType, Severity, ConditionExpression, FailureMessage, Recommendation)
VALUES 
    (@TenantId, 'Active Policy Required', 'POL001', 'Policy', 3, '{"type":"hasActivePolicy"}', 'Asset has no active insurance policy', 'Purchase a new policy immediately'),
    (@TenantId, 'Payment Status Check', 'PAY001', 'Payment', 3, '{"type":"paymentStatus","expected":"Paid"}', 'Policy payment is overdue', 'Process pending payment'),
    (@TenantId, 'Annual Inspection Required', 'INSP001', 'Inspection', 2, '{"type":"inspectionDays","max":365}', 'Asset has not been inspected in over 365 days', 'Schedule inspection'),
    (@TenantId, 'Documentation Complete', 'DOC001', 'Documentation', 1, '{"type":"hasRequiredFields","fields":["VIN","PurchaseDate","InsuredValue"]}', 'Asset missing required documentation', 'Complete all asset information'),
    (@TenantId, 'Policy Expiry Warning', 'EXP001', 'Policy', 2, '{"type":"expiresIn","days":30}', 'Policy will expire within 30 days', 'Renew policy before expiration');
GO

-- ============================================
-- PART 9: TRIGGERS
-- ============================================

-- Update asset compliance status when policies change
CREATE TRIGGER dbo.trg_Policy_UpdateAssetCompliance
ON dbo.Policies
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AssetId INT;
    
    -- Handle inserted/updated policies
    DECLARE asset_cursor CURSOR FOR
        SELECT DISTINCT AssetId FROM inserted
        UNION
        SELECT DISTINCT AssetId FROM deleted;
    
    OPEN asset_cursor;
    FETCH NEXT FROM asset_cursor INTO @AssetId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Mark asset for compliance recheck
        UPDATE dbo.Assets 
        SET UpdatedAt = GETUTCDATE(),
            UpdatedBy = 'system_trigger'
        WHERE Id = @AssetId;
        
        FETCH NEXT FROM asset_cursor INTO @AssetId;
    END
    
    CLOSE asset_cursor;
    DEALLOCATE asset_cursor;
END
GO

-- Auto-create audit log for asset changes
CREATE TRIGGER dbo.trg_Asset_Audit
ON dbo.Assets
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO audit.Logs (TenantId, UserId, EventType, EntityType, EntityId, OldValues, NewValues)
    SELECT 
        COALESCE(i.TenantId, d.TenantId),
        COALESCE(i.UpdatedBy, d.UpdatedBy, 'system'),
        CASE 
            WHEN i.Id IS NOT NULL AND d.Id IS NULL THEN 'Create'
            WHEN i.Id IS NOT NULL AND d.Id IS NOT NULL THEN 'Update'
            WHEN i.Id IS NULL AND d.Id IS NOT NULL THEN 'Delete'
        END,
        'Asset',
        COALESCE(i.Id, d.Id),
        (SELECT * FROM deleted d2 WHERE d2.Id = COALESCE(i.Id, d.Id) FOR JSON AUTO),
        (SELECT * FROM inserted i2 WHERE i2.Id = COALESCE(i.Id, d.Id) FOR JSON AUTO)
    FROM inserted i
    FULL OUTER JOIN deleted d ON i.Id = d.Id;
END
GO

-- ============================================
-- PART 10: MAINTENANCE JOBS
-- ============================================

-- Clean up old audit logs (keep 90 days)
CREATE PROCEDURE audit.sp_CleanupOldLogs
    @DaysToKeep INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM audit.Logs
    WHERE CreatedAt < DATEADD(DAY, -@DaysToKeep, GETUTCDATE());
    
    RETURN @@ROWCOUNT;
END
GO

-- Update expired policies daily
CREATE PROCEDURE dbo.sp_UpdateExpiredPolicies
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Policies
    SET Status = 'Expired',
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'system_job'
    WHERE Status = 'Active' 
        AND EndDate < GETUTCDATE();
    
    RETURN @@ROWCOUNT;
END
GO

-- ============================================
-- PART 11: PERMISSIONS
-- ============================================

-- Create application roles
CREATE ROLE InsureX_App;
CREATE ROLE InsureX_Admin;
CREATE ROLE InsureX_Reader;

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO InsureX_App;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::tenant TO InsureX_App;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::compliance TO InsureX_App;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::workflow TO InsureX_App;
GRANT SELECT, INSERT ON SCHEMA::audit TO InsureX_App;
GRANT EXECUTE ON SCHEMA::dbo TO InsureX_App;

-- Admin gets full access
GRANT CONTROL ON DATABASE::InsureX TO InsureX_Admin;

-- Readers get read-only
GRANT SELECT ON SCHEMA::dbo TO InsureX_Reader;
GRANT SELECT ON SCHEMA::tenant TO InsureX_Reader;
GRANT SELECT ON SCHEMA::compliance TO InsureX_Reader;
GRANT SELECT ON SCHEMA::workflow TO InsureX_Reader;
GRANT SELECT ON SCHEMA::audit TO InsureX_Reader;
GO

PRINT 'InsureX database creation completed successfully!';
GO