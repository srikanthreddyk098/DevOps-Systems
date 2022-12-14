USE [CalpineAzureDashboard]
GO
/****** Object:  Table [dbo].[SubnetIpConfigurationHistory]    Script Date: 4/25/2019 9:49:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubnetIpConfigurationHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SubnetIpConfigurationHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[SubnetId] [int] NULL,
	[PrivateIpAddress] [varchar](255) NULL,
	[PrivateIpAllocationMethod] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[PublicIpAddressId] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[SubnetIpConfiguration]    Script Date: 4/25/2019 9:49:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubnetIpConfiguration]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SubnetIpConfiguration](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[SubnetId] [int] NULL,
	[PrivateIpAddress] [varchar](255) NULL,
	[PrivateIpAllocationMethod] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[PublicIpAddressId] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[SubnetIpConfigurationHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[SubnetServiceEndpointHistory]    Script Date: 4/25/2019 9:49:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubnetServiceEndpointHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SubnetServiceEndpointHistory](
	[Id] [int] NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[SubnetId] [int] NULL,
	[Locations] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[Service] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[SubnetServiceEndpoint]    Script Date: 4/25/2019 9:49:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubnetServiceEndpoint]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SubnetServiceEndpoint](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[SubnetId] [int] NULL,
	[Locations] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[Service] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[SubnetServiceEndpointHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[VirtualMachineHistory]    Script Date: 4/25/2019 9:49:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualMachineHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualMachineHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[Region] [varchar](255) NULL,
	[Status] [varchar](255) NULL,
	[Os] [varchar](255) NULL,
	[OsSku] [varchar](255) NULL,
	[OsPublisher] [varchar](255) NULL,
	[OsType] [varchar](255) NULL,
	[OsVersion] [varchar](255) NULL,
	[LicenseType] [varchar](255) NULL,
	[Size] [varchar](255) NULL,
	[Cores] [int] NULL,
	[Memory] [int] NULL,
	[AvailabilitySetId] [varchar](255) NULL,
	[NumberOfNics] [int] NULL,
	[PrimaryNicId] [varchar](255) NULL,
	[OsDisk] [varchar](255) NULL,
	[OsDiskSku] [varchar](255) NULL,
	[OsDiskSize] [float] NULL,
	[OsDiskEncryptionStatus] [varchar](255) NULL,
	[IsOsDiskEncrypted] [bit] NULL,
	[IsManagedDiskEnabled] [bit] NULL,
	[NumberOfDataDisks] [int] NULL,
	[AzureAgentProvisioningState] [varchar](255) NULL,
	[AzureAgentVersion] [varchar](255) NULL,
	[Tags] [varchar](max) NULL,
	[TagApplicationName] [varchar](255) NULL,
	[TagBackupPolicy] [varchar](255) NULL,
	[TagBackupFrequency] [varchar](255) NULL,
	[TagProjectCode] [varchar](255) NULL,
	[TagProjectDurationStart] [varchar](255) NULL,
	[TagProjectDurationEnd] [varchar](255) NULL,
	[TagReservedInstance] [varchar](255) NULL,
	[TagServerType] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[VirtualMachine]    Script Date: 4/25/2019 9:49:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualMachine]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualMachine](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[Region] [varchar](255) NULL,
	[Status] [varchar](255) NULL,
	[Os] [varchar](255) NULL,
	[OsSku] [varchar](255) NULL,
	[OsPublisher] [varchar](255) NULL,
	[OsType] [varchar](255) NULL,
	[OsVersion] [varchar](255) NULL,
	[LicenseType] [varchar](255) NULL,
	[Size] [varchar](255) NULL,
	[Cores] [int] NULL,
	[Memory] [int] NULL,
	[AvailabilitySetId] [varchar](255) NULL,
	[NumberOfNics] [int] NULL,
	[PrimaryNicId] [varchar](255) NULL,
	[OsDisk] [varchar](255) NULL,
	[OsDiskSku] [varchar](255) NULL,
	[OsDiskSize] [float] NULL,
	[OsDiskEncryptionStatus] [varchar](255) NULL,
	[IsOsDiskEncrypted] [bit] NULL,
	[IsManagedDiskEnabled] [bit] NULL,
	[NumberOfDataDisks] [int] NULL,
	[AzureAgentProvisioningState] [varchar](255) NULL,
	[AzureAgentVersion] [varchar](255) NULL,
	[Tags] [varchar](max) NULL,
	[TagApplicationName] [varchar](255) NULL,
	[TagBackupPolicy] [varchar](255) NULL,
	[TagBackupFrequency] [varchar](255) NULL,
	[TagProjectCode] [varchar](255) NULL,
	[TagProjectDurationStart] [varchar](255) NULL,
	[TagProjectDurationEnd] [varchar](255) NULL,
	[TagReservedInstance] [varchar](255) NULL,
	[TagServerType] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK__VirtualM__3214EC07E48E8F39] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[VirtualMachineHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[NetworkInterfaceHistory]    Script Date: 4/25/2019 9:49:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkInterfaceHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NetworkInterfaceHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[NumberOfDnsServers] [int] NULL,
	[NumberOfAppliedDnsServers] [int] NULL,
	[InternalDnsNameLabel] [varchar](255) NULL,
	[InternalDomainNameSuffix] [varchar](255) NULL,
	[InternalFqdn] [varchar](255) NULL,
	[IsAcceleratedNetworkingEnabled] [bit] NULL,
	[IsIpForwardingEnabled] [bit] NULL,
	[MacAddress] [varchar](255) NULL,
	[NetworkSecurityGroupId] [varchar](1023) NULL,
	[PrimaryPrivateIp] [varchar](255) NULL,
	[PrimaryPrivateIpAllocationMethod] [varchar](255) NULL,
	[VirtualMachineAzureId] [varchar](1023) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[NetworkInterface]    Script Date: 4/25/2019 9:49:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkInterface]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NetworkInterface](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[NumberOfDnsServers] [int] NULL,
	[NumberOfAppliedDnsServers] [int] NULL,
	[InternalDnsNameLabel] [varchar](255) NULL,
	[InternalDomainNameSuffix] [varchar](255) NULL,
	[InternalFqdn] [varchar](255) NULL,
	[IsAcceleratedNetworkingEnabled] [bit] NULL,
	[IsIpForwardingEnabled] [bit] NULL,
	[MacAddress] [varchar](255) NULL,
	[NetworkSecurityGroupId] [varchar](1023) NULL,
	[PrimaryPrivateIp] [varchar](255) NULL,
	[PrimaryPrivateIpAllocationMethod] [varchar](255) NULL,
	[VirtualMachineAzureId] [varchar](1023) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[NetworkInterfaceHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[NetworkInterfaceIpConfigurationHistory]    Script Date: 4/25/2019 9:49:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkInterfaceIpConfigurationHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NetworkInterfaceIpConfigurationHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[NetworkInterfaceId] [varchar](1023) NOT NULL,
	[IsPrimary] [bit] NULL,
	[VirtualNetworkId] [varchar](1023) NULL,
	[SubnetId] [varchar](1023) NULL,
	[PrivateIpAddress] [varchar](50) NULL,
	[PrivateIpAddressVersion] [varchar](255) NULL,
	[PrivateIpAllocationMethod] [varchar](255) NULL,
	[PublicIpAddressId] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[NetworkInterfaceIpConfiguration]    Script Date: 4/25/2019 9:49:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkInterfaceIpConfiguration]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NetworkInterfaceIpConfiguration](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[NetworkInterfaceId] [varchar](1023) NOT NULL,
	[IsPrimary] [bit] NULL,
	[VirtualNetworkId] [varchar](1023) NULL,
	[SubnetId] [varchar](1023) NULL,
	[PrivateIpAddress] [varchar](50) NULL,
	[PrivateIpAddressVersion] [varchar](255) NULL,
	[PrivateIpAllocationMethod] [varchar](255) NULL,
	[PublicIpAddressId] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[NetworkInterfaceIpConfigurationHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[PublicIpHistory]    Script Date: 4/25/2019 9:49:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublicIpHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PublicIpHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[IpAddress] [varchar](255) NULL,
	[IpAllocationMethod] [varchar](255) NULL,
	[Version] [varchar](255) NULL,
	[Fqdn] [varchar](255) NULL,
	[HasAssignedLoadBalancer] [bit] NULL,
	[HasAssignedNetworkInterface] [bit] NULL,
	[AvailabilityZones] [varchar](255) NULL,
	[IdleTimeoutInMinutes] [int] NULL,
	[LeafDomainLabel] [varchar](255) NULL,
	[ReverseFqdn] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[PublicIp]    Script Date: 4/25/2019 9:49:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublicIp]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PublicIp](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[IpAddress] [varchar](255) NULL,
	[IpAllocationMethod] [varchar](255) NULL,
	[Version] [varchar](255) NULL,
	[Fqdn] [varchar](255) NULL,
	[HasAssignedLoadBalancer] [bit] NULL,
	[HasAssignedNetworkInterface] [bit] NULL,
	[AvailabilityZones] [varchar](255) NULL,
	[IdleTimeoutInMinutes] [int] NULL,
	[LeafDomainLabel] [varchar](255) NULL,
	[ReverseFqdn] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[PublicIpHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[VirtualNetworkHistory]    Script Date: 4/25/2019 9:49:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualNetworkHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualNetworkHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[AddressSpace] [varchar](255) NULL,
	[DdosProtectionPlan] [varchar](255) NULL,
	[DnsServers] [varchar](255) NULL,
	[EnableDdosProtection] [bit] NULL,
	[EnableVmProtection] [bit] NULL,
	[Tags] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[VirtualNetwork]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualNetwork]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualNetwork](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[AddressSpace] [varchar](255) NULL,
	[DdosProtectionPlan] [varchar](255) NULL,
	[DnsServers] [varchar](255) NULL,
	[EnableDdosProtection] [bit] NULL,
	[EnableVmProtection] [bit] NULL,
	[Tags] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[VirtualNetworkHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[SubnetHistory]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubnetHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SubnetHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[VirtualNetworkId] [int] NULL,
	[AddressPrefix] [varchar](255) NULL,
	[NetworkSecurityGroupAzureId] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[ResourceNavigationLinks] [varchar](255) NULL,
	[RouteTableAzureId] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[Subnet]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subnet]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Subnet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[VirtualNetworkId] [int] NULL,
	[AddressPrefix] [varchar](255) NULL,
	[NetworkSecurityGroupAzureId] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[ResourceNavigationLinks] [varchar](255) NULL,
	[RouteTableAzureId] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[SubnetHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_VirtualNetwork_Subnet]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VirtualNetwork_Subnet]'))
EXEC dbo.sp_executesql @statement = N'




CREATE VIEW [dbo].[vw_VirtualNetwork_Subnet]
AS
SELECT [v].[Id]
      ,[v].[SubscriptionId]
      ,[v].[Subscription]
      ,[v].[ResourceGroup]
      ,[v].[Region]
      ,[v].[Name] AS                                    [VirtualNetName]
      ,[v].[Name]+'' (''+[v].[AddressSpace]+'')'' AS        [VirtualNetNameFull]
      ,[v].[AzureId]
      ,[v].[AddressSpace]
      ,[v].[DdosProtectionPlan]
      ,[v].[DnsServers]
      ,[v].[EnableDdosProtection]
      ,[v].[EnableVmProtection]
      ,[v].[Tags]
      ,[v].[CreatedBy]
      ,[v].[CreatedDtUtc]
      ,[v].[ValidFrom]
      ,[v].[ValidTo]
      ,[s].[Name] AS                                    [SubnetName]
      ,[s].[Name]+'' (''+[s].[AddressPrefix]+'')'' AS       [SubnetNameFull]
      ,[s].[VirtualNetworkId]
      ,[s].[AddressPrefix]
      ,[s].[NetworkSecurityGroupAzureId]
      ,[s].[ProvisioningState]
      ,[s].[ResourceNavigationLinks]
      ,[s].[RouteTableAzureId]
      ,[s].[CreatedBy] AS                               [CreatedBySubnet]
      ,[s].[CreatedDtUtc] AS                            [CreatedDtUtcSubnet]
      ,[s].[ValidFrom] AS                               [ValidFromSubnet]
      ,[s].[ValidTo] AS                                 [ValidToSubnet]
      ,[nicIp].[Name] AS                                [NicIpConfigName]
      ,[nicIp].[PrivateIpAddress]
      ,[nic].[Name] AS                                  [DeviceName]
      ,[nic].[PrimaryPrivateIp]
      ,[nic].[InternalFqdn]
      ,[vm].[Name] AS                                   [VMName]
      ,[vm].[Name]+'' (PVT_IP:''+[nic].[PrimaryPrivateIp]+'') (PUB_IP:'' + p.IpAddress + '')'' AS [VMNameFull]
      ,[p].[IpAddress] AS                               [PublicIP]
      ,[sse].[Locations] AS                             [ServiceLocation]
      ,[sse].[Service] AS                               [ServiceEndpoint]
FROM [dbo].[VirtualNetwork] AS [v]
     LEFT JOIN [Subnet] AS [s] ON [v].[id] = [s].[VirtualNetworkId]
     LEFT JOIN [NetworkInterfaceIpConfiguration] AS [nicIp] ON [nicIp].[SubnetId] = [s].[AzureId]
     LEFT JOIN [NetworkInterface] AS [nic] ON [nicIp].[NetworkInterfaceId] = [nic].[Id]
     LEFT JOIN [VirtualMachine] AS [vm] ON [vm].[AzureId] = [nic].[VirtualMachineAzureId]
     LEFT JOIN [PublicIp] AS [p] ON [p].[AzureId] = [nicIp].[PublicIpAddressId]
     LEFT JOIN [SubnetServiceEndpoint] AS [sse] ON [s].[Id] = [sse].[SubnetId];






' 
GO
/****** Object:  View [dbo].[wm_VirtualNetwork_Subnet_Short]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[wm_VirtualNetwork_Subnet_Short]'))
EXEC dbo.sp_executesql @statement = N'
/****** Script for SelectTopNRows command from SSMS  ******/
CREATE view [dbo].[wm_VirtualNetwork_Subnet_Short]
as
SELECT net.[Id]
      ,net.[SubscriptionId]
      ,net.[Subscription]
      ,net.[ResourceGroup]
      ,net.[Region]
      ,net.[Name]
      ,net.[AzureId]
      ,net.[AddressSpace]
      ,net.[DdosProtectionPlan]
      ,net.[DnsServers]
      ,net.[EnableDdosProtection]
      ,net.[EnableVmProtection]
      ,net.[Tags]
      ,net.[CreatedBy]
      ,net.[CreatedDtUtc]
      ,net.[ValidFrom]
      ,net.[ValidTo]
	  ,s.AddressPrefix
	  ,s.Name SubnetName
	  ,sse.[Service] Endpoint
	  ,sse.Locations [EndpointLocation]	  
  FROM [dbo].[VirtualNetwork] net
  inner join  Subnet s
  on net.id = s.VirtualNetworkId
  full outer join [SubnetServiceEndpoint] sse
  on s.Id = sse.SubnetId

' 
GO
/****** Object:  View [dbo].[vw_SubnetServiceEndpoint_Subnet]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SubnetServiceEndpoint_Subnet]'))
EXEC dbo.sp_executesql @statement = N'create view [dbo].[vw_SubnetServiceEndpoint_Subnet] as
SELECT sse.[Id]
      ,sse.[AzureId]
      ,sse.[SubnetId]
      ,sse.[Locations]
      ,sse.[ProvisioningState]
      ,sse.[Service]
      ,sse.[CreatedBy]
      ,sse.[CreatedDtUtc]
      ,sse.[ValidFrom]
      ,sse.[ValidTo]
	  ,s.AddressPrefix
	  ,s.Name
  FROM [dbo].[SubnetServiceEndpoint] sse
  inner join Subnet s 
  on s.Id = sse.SubnetId' 
GO
/****** Object:  Table [dbo].[LoadBalancerHistory]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadBalancerHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LoadBalancerHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[LoadBalancer]    Script Date: 4/25/2019 9:49:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadBalancer]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LoadBalancer](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[LoadBalancerHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[LoadBalancerBackendHistory]    Script Date: 4/25/2019 9:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadBalancerBackendHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LoadBalancerBackendHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[LoadBalancerId] [int] NOT NULL,
	[IpConfigurationName] [varchar](255) NULL,
	[NetworkInterfaceName] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[LoadBalancerBackend]    Script Date: 4/25/2019 9:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadBalancerBackend]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LoadBalancerBackend](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[LoadBalancerId] [int] NOT NULL,
	[IpConfigurationName] [varchar](255) NULL,
	[NetworkInterfaceName] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[LoadBalancerBackendHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[LoadBalancerFrontendHistory]    Script Date: 4/25/2019 9:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadBalancerFrontendHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LoadBalancerFrontendHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[LoadBalancerId] [int] NOT NULL,
	[PrivateIpAddress] [varchar](255) NULL,
	[PrivateIpAllocationMethod] [varchar](255) NULL,
	[PublicIpAddressId] [varchar](1023) NULL,
	[SubnetId] [varchar](1023) NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[LoadBalancerFrontend]    Script Date: 4/25/2019 9:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadBalancerFrontend]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[LoadBalancerFrontend](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[LoadBalancerId] [int] NOT NULL,
	[PrivateIpAddress] [varchar](255) NULL,
	[PrivateIpAllocationMethod] [varchar](255) NULL,
	[PublicIpAddressId] [varchar](1023) NULL,
	[SubnetId] [varchar](1023) NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[LoadBalancerFrontendHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_Loadbalancer_F_B]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_Loadbalancer_F_B]'))
EXEC dbo.sp_executesql @statement = N'/****** Script for SelectTopNRows command from SSMS  ******/

CREATE view [dbo].[vw_Loadbalancer_F_B] as
SELECT lb.[Id]
      ,lb.[Name] LoadBalancer
 	  ,lbb.Name LBBackend
	  ,lbf.Name LBFrontend
	  ,lbf.PrivateIpAddress [LBFPrivateIP]
	  ,p.IpAddress [LBFPublicIP]
      ,lb.[ValidFrom]
      ,lb.[ValidTo]
	  ,lb.Region
	  ,lb.Subscription
	  ,lb.ResourceGroup
	  ,lb.CreatedBy
	  ,lb.CreatedDtUtc
  FROM [dbo].[LoadBalancer] lb
  left join LoadBalancerBackend lbb on lb.id = lbb.LoadBalancerId
  left join LoadBalancerFrontend lbf on lb.id = lbf.LoadBalancerId
  left join PublicIp p on lbf.PublicIpAddressId=p.AzureId


' 
GO
/****** Object:  Table [dbo].[ActivityLogHistory]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLogHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActivityLogHistory](
	[Id] [int] NOT NULL,
	[Action] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[Caller] [varchar](255) NULL,
	[Category] [varchar](255) NULL,
	[ClientIpAddress] [varchar](255) NULL,
	[ClientRequestId] [varchar](255) NULL,
	[CorrelationId] [varchar](255) NULL,
	[Description] [varchar](255) NULL,
	[Event] [varchar](255) NULL,
	[EventDataId] [varchar](255) NULL,
	[EventTimestampUtc] [datetime] NULL,
	[HttpMethod] [varchar](255) NULL,
	[Level] [varchar](255) NULL,
	[Operation] [varchar](255) NULL,
	[OperationId] [varchar](255) NULL,
	[Properties] [varchar](max) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[ResourceId] [varchar](255) NULL,
	[ResourceProvider] [varchar](255) NULL,
	[ResourceType] [varchar](255) NULL,
	[Role] [varchar](255) NULL,
	[Scope] [varchar](255) NULL,
	[Status] [varchar](255) NULL,
	[SubStatus] [varchar](255) NULL,
	[SubmissionTimeStampUtc] [datetime] NULL,
	[SubscriptionId] [varchar](255) NULL,
	[TenantId] [varchar](255) NULL,
	[Uri] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[ActivityLog]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActivityLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Action] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[Caller] [varchar](255) NULL,
	[Category] [varchar](255) NULL,
	[ClientIpAddress] [varchar](255) NULL,
	[ClientRequestId] [varchar](255) NULL,
	[CorrelationId] [varchar](255) NULL,
	[Description] [varchar](255) NULL,
	[Event] [varchar](255) NULL,
	[EventDataId] [varchar](255) NULL,
	[EventTimestampUtc] [datetime] NULL,
	[HttpMethod] [varchar](255) NULL,
	[Level] [varchar](255) NULL,
	[Operation] [varchar](255) NULL,
	[OperationId] [varchar](255) NULL,
	[Properties] [varchar](max) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[ResourceId] [varchar](255) NULL,
	[ResourceProvider] [varchar](255) NULL,
	[ResourceType] [varchar](255) NULL,
	[Role] [varchar](255) NULL,
	[Scope] [varchar](255) NULL,
	[Status] [varchar](255) NULL,
	[SubStatus] [varchar](255) NULL,
	[SubmissionTimeStampUtc] [datetime] NULL,
	[SubscriptionId] [varchar](255) NULL,
	[TenantId] [varchar](255) NULL,
	[Uri] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[ActivityLogHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_ActivityLog]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ActivityLog]'))
EXEC dbo.sp_executesql @statement = N'/****** Script for SelectTopNRows command from SSMS  ******/
create view [dbo].[vw_ActivityLog] as 
SELECT [Id]
      ,[Action]
      ,[AzureId]
      ,[Caller]
      ,[Category]
      ,[ClientIpAddress]
      ,[ClientRequestId]
      ,[CorrelationId]
      ,[Description]
      ,[Event]
      ,[EventDataId]
      ,[EventTimestampUtc]
      ,[HttpMethod]
      ,[Level]
      ,[Operation]
      ,[OperationId]
      ,[Properties]
      ,[ResourceGroup]
      ,[ResourceId]
      ,[ResourceProvider]
      ,[ResourceType]
      ,[Role]
      ,[Scope]
      ,[Status]
      ,[SubStatus]
      ,[SubmissionTimeStampUtc]
      ,[SubscriptionId]
      ,[TenantId]
      ,[Uri]
      ,[ValidFrom]
      ,[ValidTo]
  FROM [dbo].[ActivityLog]' 
GO
/****** Object:  Table [dbo].[AvailabilitySetHistory]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AvailabilitySetHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AvailabilitySetHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[Sku] [varchar](255) NULL,
	[FaultDomainCount] [int] NULL,
	[UpdateDomainCount] [int] NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[AvailabilitySet]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AvailabilitySet]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AvailabilitySet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[Sku] [varchar](255) NULL,
	[FaultDomainCount] [int] NULL,
	[UpdateDomainCount] [int] NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[AvailabilitySetHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_Availabilitysets]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_Availabilitysets]'))
EXEC dbo.sp_executesql @statement = N'
/****** Script for SelectTopNRows command from SSMS  ******/
create view [dbo].[vw_Availabilitysets] as
SELECT  [Id]
      ,[SubscriptionId]
      ,[Subscription]
      ,[ResourceGroup]
      ,[Region]
      ,[Name]
      ,[AzureId]
      ,[Sku]
      ,[FaultDomainCount]
      ,[UpdateDomainCount]
      ,[CreatedBy]
      ,[CreatedDtUtc]
      ,[ValidFrom]
      ,[ValidTo]
  FROM [dbo].[AvailabilitySet]' 
GO
/****** Object:  Table [dbo].[VaultBackupJobHistory]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VaultBackupJobHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VaultBackupJobHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[VaultName] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[ActivityId] [varchar](255) NULL,
	[BackupManagementType] [varchar](255) NULL,
	[Duration] [varchar](255) NULL,
	[EndTimeUtc] [datetime] NULL,
	[EntityFriendlyName] [varchar](255) NULL,
	[ErrorCode] [int] NULL,
	[ErrorString] [varchar](255) NULL,
	[ErrorTitle] [varchar](255) NULL,
	[ErrorRecommendation] [varchar](max) NULL,
	[MabServerName] [varchar](255) NULL,
	[MabServerType] [varchar](255) NULL,
	[Operation] [varchar](255) NULL,
	[StartTimeUtc] [datetime] NULL,
	[Status] [varchar](255) NULL,
	[VirtualMachineVersion] [varchar](255) NULL,
	[WorkloadType] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[VaultBackupJob]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VaultBackupJob]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VaultBackupJob](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[VaultName] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[ActivityId] [varchar](255) NULL,
	[BackupManagementType] [varchar](255) NULL,
	[Duration] [varchar](255) NULL,
	[EndTimeUtc] [datetime] NULL,
	[EntityFriendlyName] [varchar](255) NULL,
	[ErrorCode] [int] NULL,
	[ErrorString] [varchar](255) NULL,
	[ErrorTitle] [varchar](255) NULL,
	[ErrorRecommendation] [varchar](max) NULL,
	[MabServerName] [varchar](255) NULL,
	[MabServerType] [varchar](255) NULL,
	[Operation] [varchar](255) NULL,
	[StartTimeUtc] [datetime] NULL,
	[Status] [varchar](255) NULL,
	[VirtualMachineVersion] [varchar](255) NULL,
	[WorkloadType] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[VaultBackupJobHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[AdApplicationHistory]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdApplicationHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdApplicationHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[AdditionalProperties] [varchar](max) NULL,
	[ApplicationId] [varchar](255) NULL,
	[ApplicationPermissions] [varchar](255) NULL,
	[AvailableToOtherTenants] [bit] NULL,
	[DeletionTimestamp] [datetime] NULL,
	[Homepage] [varchar](255) NULL,
	[IdentifierUris] [varchar](255) NULL,
	[Oauth2AllowImplicitFlow] [bit] NULL,
	[ObjectId] [varchar](255) NULL,
	[ReplyUrls] [varchar](max) NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[AdApplication]    Script Date: 4/25/2019 9:49:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdApplication]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdApplication](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[AdditionalProperties] [varchar](max) NULL,
	[ApplicationId] [varchar](255) NULL,
	[ApplicationPermissions] [varchar](255) NULL,
	[AvailableToOtherTenants] [bit] NULL,
	[DeletionTimestamp] [datetime] NULL,
	[Homepage] [varchar](255) NULL,
	[IdentifierUris] [varchar](255) NULL,
	[Oauth2AllowImplicitFlow] [bit] NULL,
	[ObjectId] [varchar](255) NULL,
	[ReplyUrls] [varchar](max) NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[AdApplicationHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[VirtualMachineExtensionHistory]    Script Date: 4/25/2019 9:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualMachineExtensionHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualMachineExtensionHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[VirtualMachineId] [int] NOT NULL,
	[Publisher] [varchar](255) NULL,
	[ImageName] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[AutoUpgradeMinorVersion] [bit] NULL,
	[Version] [varchar](255) NULL,
	[PublicSettings] [varchar](max) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[VirtualMachineExtension]    Script Date: 4/25/2019 9:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualMachineExtension]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualMachineExtension](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[VirtualMachineId] [int] NOT NULL,
	[Publisher] [varchar](255) NULL,
	[ImageName] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[AutoUpgradeMinorVersion] [bit] NULL,
	[Version] [varchar](255) NULL,
	[PublicSettings] [varchar](max) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK__VirtualM__3214EC07C8E7C28A] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[VirtualMachineExtensionHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[DataDiskHistory]    Script Date: 4/25/2019 9:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataDiskHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DataDiskHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[SizeInGb] [int] NULL,
	[Os] [varchar](255) NULL,
	[CreationMethod] [varchar](255) NULL,
	[ImageId] [varchar](1023) NULL,
	[Sku] [varchar](255) NULL,
	[IsAttachedToVm] [bit] NULL,
	[VirtualMachineAzureId] [varchar](1023) NULL,
	[IsEncryptionEnabled] [bit] NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[DataDisk]    Script Date: 4/25/2019 9:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataDisk]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DataDisk](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[SizeInGb] [int] NULL,
	[Os] [varchar](255) NULL,
	[CreationMethod] [varchar](255) NULL,
	[ImageId] [varchar](1023) NULL,
	[Sku] [varchar](255) NULL,
	[IsAttachedToVm] [bit] NULL,
	[VirtualMachineAzureId] [varchar](1023) NULL,
	[IsEncryptionEnabled] [bit] NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK_DataDisk] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[DataDiskHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[AdRoleDefinitionHistory]    Script Date: 4/25/2019 9:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdRoleDefinitionHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdRoleDefinitionHistory](
	[Id] [int] NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[Name] [varchar](255) NULL,
	[FriendlyName] [varchar](255) NULL,
	[Description] [varchar](max) NULL,
	[Type] [varchar](255) NULL,
	[Scopes] [varchar](max) NULL,
	[Actions] [varchar](max) NULL,
	[NotActions] [varchar](max) NULL,
	[DataActions] [varchar](max) NULL,
	[NotDataActions] [varchar](max) NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[AdRoleDefinition]    Script Date: 4/25/2019 9:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdRoleDefinition]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdRoleDefinition](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[Name] [varchar](255) NULL,
	[FriendlyName] [varchar](255) NULL,
	[Description] [varchar](max) NULL,
	[Type] [varchar](255) NULL,
	[Scopes] [varchar](max) NULL,
	[Actions] [varchar](max) NULL,
	[NotActions] [varchar](max) NULL,
	[DataActions] [varchar](max) NULL,
	[NotDataActions] [varchar](max) NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[AdRoleDefinitionHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[NetworkSecurityGroupHistory]    Script Date: 4/25/2019 9:49:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkSecurityGroupHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NetworkSecurityGroupHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[NetworkInterfaceAzureIds] [varchar](max) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[ResourceGuid] [varchar](255) NULL,
	[SubnetAzureIds] [varchar](1023) NULL,
	[Tags] [varchar](max) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[NetworkSecurityGroup]    Script Date: 4/25/2019 9:49:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NetworkSecurityGroup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NetworkSecurityGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[NetworkInterfaceAzureIds] [varchar](max) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[ResourceGuid] [varchar](255) NULL,
	[SubnetAzureIds] [varchar](1023) NULL,
	[Tags] [varchar](max) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK_NetworkSecurityGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[NetworkSecurityGroupHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[NsgSecurityRuleHistory]    Script Date: 4/25/2019 9:49:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NsgSecurityRuleHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NsgSecurityRuleHistory](
	[Id] [int] NOT NULL,
	[NetworkSecurityGroupId] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[Access] [varchar](255) NULL,
	[Description] [varchar](max) NULL,
	[DestinationAddressPrefix] [varchar](255) NULL,
	[DestinationAddressPrefixes] [varchar](1023) NULL,
	[DestinationApplicationSecurityGroupAzureIds] [varchar](max) NULL,
	[DestinationPortRange] [varchar](255) NULL,
	[DestinationPortRanges] [varchar](1023) NULL,
	[Direction] [varchar](255) NULL,
	[IsDefaultSecurityRule] [bit] NULL,
	[Priority] [int] NULL,
	[Protocol] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[SourceAddressPrefix] [varchar](255) NULL,
	[SourceAddressPrefixes] [varchar](1023) NULL,
	[SourceApplicationSecurityGroupAzureIds] [varchar](max) NULL,
	[SourcePortRange] [varchar](255) NULL,
	[SourcePortRanges] [varchar](1023) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[NsgSecurityRule]    Script Date: 4/25/2019 9:49:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NsgSecurityRule]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NsgSecurityRule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NetworkSecurityGroupId] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[Access] [varchar](255) NULL,
	[Description] [varchar](max) NULL,
	[DestinationAddressPrefix] [varchar](255) NULL,
	[DestinationAddressPrefixes] [varchar](1023) NULL,
	[DestinationApplicationSecurityGroupAzureIds] [varchar](max) NULL,
	[DestinationPortRange] [varchar](255) NULL,
	[DestinationPortRanges] [varchar](1023) NULL,
	[Direction] [varchar](255) NULL,
	[IsDefaultSecurityRule] [bit] NULL,
	[Priority] [int] NULL,
	[Protocol] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[SourceAddressPrefix] [varchar](255) NULL,
	[SourceAddressPrefixes] [varchar](1023) NULL,
	[SourceApplicationSecurityGroupAzureIds] [varchar](max) NULL,
	[SourcePortRange] [varchar](255) NULL,
	[SourcePortRanges] [varchar](1023) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK_NsgSecurityRule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[NsgSecurityRuleHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[AdRoleAssignmentHistory]    Script Date: 4/25/2019 9:49:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdRoleAssignmentHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdRoleAssignmentHistory](
	[Id] [int] NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[Name] [varchar](255) NULL,
	[PrincipalId] [varchar](255) NULL,
	[RoleDefinitionId] [varchar](255) NULL,
	[Scope] [varchar](255) NULL,
	[CanDelegate] [bit] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[AdRoleAssignment]    Script Date: 4/25/2019 9:49:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdRoleAssignment]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdRoleAssignment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[Name] [varchar](255) NULL,
	[PrincipalId] [varchar](255) NULL,
	[RoleDefinitionId] [varchar](255) NULL,
	[Scope] [varchar](255) NULL,
	[CanDelegate] [bit] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[AdRoleAssignmentHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_NetworkSecurity]    Script Date: 4/25/2019 9:49:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_NetworkSecurity]'))
EXEC dbo.sp_executesql @statement = N'--The NetworkSecurityGroupId column in the NsgSecurityRule table can be used to join the two tables together.
CREATE view [dbo].[vw_NetworkSecurity] as

SELECT 
      [NetworkSecurityGroupId]
      ,nsg.[Name] NsgName
      ,[Access]
      ,[Description]
      ,[DestinationAddressPrefix]
      ,[DestinationAddressPrefixes]
      ,[DestinationApplicationSecurityGroupAzureIds]
      ,[DestinationPortRange]
      ,[DestinationPortRanges]
      ,[Direction]
      ,[IsDefaultSecurityRule]
      ,[Priority]
      ,[Protocol]
      ,nsg.[ProvisioningState]
      ,[SourceAddressPrefix]
      ,[SourceAddressPrefixes]
      ,[SourceApplicationSecurityGroupAzureIds]
      ,[SourcePortRange]
      ,[SourcePortRanges]
      ,nsg.[CreatedBy]
      ,nsg.[CreatedDtUtc]
      ,nsg.[ValidFrom]
      ,nsg.[ValidTo]
	  ,nsg.ResourceGroup
	  ,nsg.Subscription
	  ,nsgr.Name
	  ,nsg.Region
	  
  FROM [dbo].[NsgSecurityRule] nsgr
  INNER JOIN NetworkSecurityGroup nsg
  on nsg.Id=nsgr.NetworkSecurityGroupId


' 
GO
/****** Object:  View [dbo].[vw_Vnet_Subnet_NSG]    Script Date: 4/25/2019 9:49:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_Vnet_Subnet_NSG]'))
EXEC dbo.sp_executesql @statement = N'


CREATE view [dbo].[vw_Vnet_Subnet_NSG] as


SELECT v.[Id]
      ,v.[SubscriptionId]
      ,v.[Subscription]
      ,v.[ResourceGroup]
      ,v.[Region]
      ,v.[Name] as VirtualNetName
      ,v.[AzureId]
      ,v.[AddressSpace]
      ,v.[DdosProtectionPlan]
      ,v.[DnsServers]
      ,v.[EnableDdosProtection]
      ,v.[EnableVmProtection]
      ,v.[Tags]
      ,v.[CreatedBy]
      ,v.[CreatedDtUtc]
      ,v.[ValidFrom]
      ,v.[ValidTo]
	  ,s.[Name] SubnetName
      ,s.[VirtualNetworkId]
      ,s.[AddressPrefix]
      ,s.[NetworkSecurityGroupAzureId]
      ,s.[ProvisioningState]
      ,s.[ResourceNavigationLinks]
      ,s.[RouteTableAzureId]
      ,s.CreatedBy CreatedBySubnet
      ,s.CreatedDtUtc CreatedDtUtcSubnet
      ,s.ValidFrom ValidFromSubnet
      ,s.ValidTo ValidToSubnet
	  ,nsg.Name NsgName
	  ,nsgr.Access NsgAccess
	  ,nsgr.Description NsgDesc
	  ,nsgr.DestinationAddressPrefix
	  ,nsgr.DestinationPortRange
	  ,nsgr.Direction
	  ,nsgr.Name NsgRuleName
	  ,nsgr.Priority 
	  ,nsgr.Protocol
	  ,nsgr.SourcePortRange
	 
  FROM [dbo].[VirtualNetwork] v
  inner join Subnet s
  on v.id=s.VirtualNetworkId
  left join [dbo].NetworkSecurityGroup nsg
  on s.NetworkSecurityGroupAzureId = nsg.AzureId
  left JOIN [dbo].[NsgSecurityRule] nsgr
  ON nsg.Id=nsgr.NetworkSecurityGroupId


' 
GO
/****** Object:  Table [dbo].[AdGroupHistory]    Script Date: 4/25/2019 9:49:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdGroupHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdGroupHistory](
	[Id] [int] NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[AdditionalProperties] [varchar](max) NULL,
	[DeletionTimestamp] [datetime] NULL,
	[DisplayName] [varchar](255) NULL,
	[Mail] [varchar](255) NULL,
	[ObjectId] [varchar](255) NULL,
	[SecurityEnabled] [bit] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[AdGroup]    Script Date: 4/25/2019 9:49:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdGroup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AzureId] [varchar](1023) NULL,
	[AdditionalProperties] [varchar](max) NULL,
	[DeletionTimestamp] [datetime] NULL,
	[DisplayName] [varchar](255) NULL,
	[Mail] [varchar](255) NULL,
	[ObjectId] [varchar](255) NULL,
	[SecurityEnabled] [bit] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[AdGroupHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[StorageAccountHistory]    Script Date: 4/25/2019 9:49:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StorageAccountHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StorageAccountHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[SkuName] [varchar](255) NULL,
	[SkuTier] [varchar](255) NULL,
	[Kind] [varchar](255) NULL,
	[AccessTier] [varchar](255) NULL,
	[CreationTime] [datetime] NULL,
	[CustomDomain] [varchar](255) NULL,
	[UseSubDomain] [bit] NULL,
	[EnableHttpsTrafficOnly] [bit] NULL,
	[KeySource] [varchar](255) NULL,
	[KeyVaultName] [varchar](255) NULL,
	[KeyVaultUri] [varchar](255) NULL,
	[KeyVaultVersion] [varchar](255) NULL,
	[BlobEncryptionEnabled] [bit] NULL,
	[BlobEncryptionLastEnabledTime] [datetime] NULL,
	[FileEncryptionEnabled] [bit] NULL,
	[FileEncryptionLastEnabledTime] [datetime] NULL,
	[QueueEncryptionEnabled] [bit] NULL,
	[QueueEncryptionLastEnabledTime] [datetime] NULL,
	[TableEncryptionEnabled] [bit] NULL,
	[TableEncryptionLastEnabledTime] [datetime] NULL,
	[SystemAssignedManagedServiceIdentityPrincipalId] [varchar](1023) NULL,
	[SystemAssignedManagedServiceIdentityTenantId] [varchar](1023) NULL,
	[LastGeoFailoverTime] [datetime] NULL,
	[IsAccessAllowedFromAllNetworks] [bit] NULL,
	[CanAccessFromAzureServices] [bit] NULL,
	[CanReadMetricsFromAnyNetwork] [bit] NULL,
	[CanReadLogEntriesFromAnyNetwork] [bit] NULL,
	[IPAddressesWithAccess] [varchar](max) NULL,
	[IPAddressRangesWithAccess] [varchar](max) NULL,
	[NetworkSubnetsWithAccess] [varchar](max) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[PrimaryLocation] [varchar](255) NULL,
	[SecondaryLocation] [varchar](255) NULL,
	[PrimaryStatus] [varchar](255) NULL,
	[SecondaryStatus] [varchar](255) NULL,
	[PrimaryBlobEndpoint] [varchar](255) NULL,
	[PrimaryFileEndpoint] [varchar](255) NULL,
	[PrimaryQueueEndpoint] [varchar](255) NULL,
	[PrimaryTableEndpoint] [varchar](255) NULL,
	[SecondaryBlobEndpoint] [varchar](255) NULL,
	[SecondaryFileEndpoint] [varchar](255) NULL,
	[SecondaryQueueEndpoint] [varchar](255) NULL,
	[SecondaryTableEndpoint] [varchar](255) NULL,
	[Tags] [varchar](max) NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[StorageAccount]    Script Date: 4/25/2019 9:49:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StorageAccount]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StorageAccount](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NOT NULL,
	[Subscription] [varchar](255) NOT NULL,
	[ResourceGroup] [varchar](255) NOT NULL,
	[Region] [varchar](255) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[SkuName] [varchar](255) NULL,
	[SkuTier] [varchar](255) NULL,
	[Kind] [varchar](255) NULL,
	[AccessTier] [varchar](255) NULL,
	[CreationTime] [datetime] NULL,
	[CustomDomain] [varchar](255) NULL,
	[UseSubDomain] [bit] NULL,
	[EnableHttpsTrafficOnly] [bit] NULL,
	[KeySource] [varchar](255) NULL,
	[KeyVaultName] [varchar](255) NULL,
	[KeyVaultUri] [varchar](255) NULL,
	[KeyVaultVersion] [varchar](255) NULL,
	[BlobEncryptionEnabled] [bit] NULL,
	[BlobEncryptionLastEnabledTime] [datetime] NULL,
	[FileEncryptionEnabled] [bit] NULL,
	[FileEncryptionLastEnabledTime] [datetime] NULL,
	[QueueEncryptionEnabled] [bit] NULL,
	[QueueEncryptionLastEnabledTime] [datetime] NULL,
	[TableEncryptionEnabled] [bit] NULL,
	[TableEncryptionLastEnabledTime] [datetime] NULL,
	[SystemAssignedManagedServiceIdentityPrincipalId] [varchar](1023) NULL,
	[SystemAssignedManagedServiceIdentityTenantId] [varchar](1023) NULL,
	[LastGeoFailoverTime] [datetime] NULL,
	[IsAccessAllowedFromAllNetworks] [bit] NULL,
	[CanAccessFromAzureServices] [bit] NULL,
	[CanReadMetricsFromAnyNetwork] [bit] NULL,
	[CanReadLogEntriesFromAnyNetwork] [bit] NULL,
	[IPAddressesWithAccess] [varchar](max) NULL,
	[IPAddressRangesWithAccess] [varchar](max) NULL,
	[NetworkSubnetsWithAccess] [varchar](max) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[PrimaryLocation] [varchar](255) NULL,
	[SecondaryLocation] [varchar](255) NULL,
	[PrimaryStatus] [varchar](255) NULL,
	[SecondaryStatus] [varchar](255) NULL,
	[PrimaryBlobEndpoint] [varchar](255) NULL,
	[PrimaryFileEndpoint] [varchar](255) NULL,
	[PrimaryQueueEndpoint] [varchar](255) NULL,
	[PrimaryTableEndpoint] [varchar](255) NULL,
	[SecondaryBlobEndpoint] [varchar](255) NULL,
	[SecondaryFileEndpoint] [varchar](255) NULL,
	[SecondaryQueueEndpoint] [varchar](255) NULL,
	[SecondaryTableEndpoint] [varchar](255) NULL,
	[Tags] [varchar](max) NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK_StorageAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[StorageAccountHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[WebAppHistory]    Script Date: 4/25/2019 9:49:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WebAppHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[WebAppHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[Region] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[AlwaysOn] [bit] NULL,
	[AppServicePlanId] [varchar](255) NULL,
	[AutoSwapSlotName] [varchar](255) NULL,
	[AvailabilityState] [varchar](255) NULL,
	[ClientAffinityEnabled] [bit] NULL,
	[ClientCertEnabled] [bit] NULL,
	[CloningInfo] [varchar](max) NULL,
	[ContainerSize] [int] NULL,
	[DailyMemoryTimeQuota] [int] NULL,
	[DefaultDocuments] [varchar](max) NULL,
	[DefaultHostName] [varchar](255) NULL,
	[DiagnosticLogsConfig] [varchar](max) NULL,
	[DocumentRoot] [varchar](255) NULL,
	[Enabled] [bit] NULL,
	[EnabledHostNames] [varchar](255) NULL,
	[FtpsState] [varchar](max) NULL,
	[HostingEnvironmentProfile] [varchar](max) NULL,
	[HostNames] [varchar](max) NULL,
	[HostNamesDisabled] [bit] NULL,
	[HostNameSslStates] [varchar](max) NULL,
	[Http20Enabled] [bit] NULL,
	[HttpsOnly] [bit] NULL,
	[IsDefaultContainer] [bit] NULL,
	[IsXenon] [bit] NULL,
	[JavaContainer] [varchar](255) NULL,
	[JavaContainerVersion] [varchar](255) NULL,
	[JavaVersion] [varchar](255) NULL,
	[Key] [varchar](255) NULL,
	[Kind] [varchar](255) NULL,
	[LastModifiedTimeUtc] [datetime] NULL,
	[LastSwapDestination] [varchar](255) NULL,
	[LastSwapSource] [varchar](255) NULL,
	[LastSwapTimestampUtc] [datetime] NULL,
	[LinuxFxVersion] [varchar](255) NULL,
	[LocalMySqlEnabled] [bit] NULL,
	[ManagedPipelineMode] [varchar](255) NULL,
	[MaxNumberOfWorkers] [int] NULL,
	[NetFrameworkVersion] [varchar](255) NULL,
	[NodeVersion] [varchar](255) NULL,
	[OperatingSystem] [varchar](255) NULL,
	[OutboundIpAddresses] [varchar](max) NULL,
	[ParentId] [varchar](1023) NULL,
	[PhpVersion] [varchar](255) NULL,
	[PossibleOutboundIpAddresses] [varchar](max) NULL,
	[PlatformArchitecture] [varchar](255) NULL,
	[PythonVersion] [varchar](255) NULL,
	[RemoteDebuggingEnabled] [bit] NULL,
	[RemoteDebuggingVersion] [varchar](255) NULL,
	[RepositorySiteName] [varchar](255) NULL,
	[Reserved] [bit] NULL,
	[ScmSiteAlsoStopped] [bit] NULL,
	[ScmType] [varchar](255) NULL,
	[ServerFarmId] [varchar](1023) NULL,
	[SiteConfig] [varchar](max) NULL,
	[SnapshotInfo] [varchar](max) NULL,
	[State] [varchar](255) NULL,
	[SuspendedTill] [datetime] NULL,
	[SystemAssignedManagedServiceIdentityPrincipalId] [varchar](255) NULL,
	[SystemAssignedManagedServiceIdentityTenantId] [varchar](255) NULL,
	[Tags] [varchar](max) NULL,
	[TargetSwapSlot] [varchar](255) NULL,
	[TrafficManagerHostNames] [varchar](max) NULL,
	[Type] [varchar](255) NULL,
	[UsageState] [varchar](255) NULL,
	[VirtualApplications] [varchar](max) NULL,
	[WebSocketsEnabled] [bit] NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[WebApp]    Script Date: 4/25/2019 9:49:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WebApp]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[WebApp](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[Region] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[AlwaysOn] [bit] NULL,
	[AppServicePlanId] [varchar](255) NULL,
	[AutoSwapSlotName] [varchar](255) NULL,
	[AvailabilityState] [varchar](255) NULL,
	[ClientAffinityEnabled] [bit] NULL,
	[ClientCertEnabled] [bit] NULL,
	[CloningInfo] [varchar](max) NULL,
	[ContainerSize] [int] NULL,
	[DailyMemoryTimeQuota] [int] NULL,
	[DefaultDocuments] [varchar](max) NULL,
	[DefaultHostName] [varchar](255) NULL,
	[DiagnosticLogsConfig] [varchar](max) NULL,
	[DocumentRoot] [varchar](255) NULL,
	[Enabled] [bit] NULL,
	[EnabledHostNames] [varchar](255) NULL,
	[FtpsState] [varchar](max) NULL,
	[HostingEnvironmentProfile] [varchar](max) NULL,
	[HostNames] [varchar](max) NULL,
	[HostNamesDisabled] [bit] NULL,
	[HostNameSslStates] [varchar](max) NULL,
	[Http20Enabled] [bit] NULL,
	[HttpsOnly] [bit] NULL,
	[IsDefaultContainer] [bit] NULL,
	[IsXenon] [bit] NULL,
	[JavaContainer] [varchar](255) NULL,
	[JavaContainerVersion] [varchar](255) NULL,
	[JavaVersion] [varchar](255) NULL,
	[Key] [varchar](255) NULL,
	[Kind] [varchar](255) NULL,
	[LastModifiedTimeUtc] [datetime] NULL,
	[LastSwapDestination] [varchar](255) NULL,
	[LastSwapSource] [varchar](255) NULL,
	[LastSwapTimestampUtc] [datetime] NULL,
	[LinuxFxVersion] [varchar](255) NULL,
	[LocalMySqlEnabled] [bit] NULL,
	[ManagedPipelineMode] [varchar](255) NULL,
	[MaxNumberOfWorkers] [int] NULL,
	[NetFrameworkVersion] [varchar](255) NULL,
	[NodeVersion] [varchar](255) NULL,
	[OperatingSystem] [varchar](255) NULL,
	[OutboundIpAddresses] [varchar](max) NULL,
	[ParentId] [varchar](1023) NULL,
	[PhpVersion] [varchar](255) NULL,
	[PossibleOutboundIpAddresses] [varchar](max) NULL,
	[PlatformArchitecture] [varchar](255) NULL,
	[PythonVersion] [varchar](255) NULL,
	[RemoteDebuggingEnabled] [bit] NULL,
	[RemoteDebuggingVersion] [varchar](255) NULL,
	[RepositorySiteName] [varchar](255) NULL,
	[Reserved] [bit] NULL,
	[ScmSiteAlsoStopped] [bit] NULL,
	[ScmType] [varchar](255) NULL,
	[ServerFarmId] [varchar](1023) NULL,
	[SiteConfig] [varchar](max) NULL,
	[SnapshotInfo] [varchar](max) NULL,
	[State] [varchar](255) NULL,
	[SuspendedTill] [datetime] NULL,
	[SystemAssignedManagedServiceIdentityPrincipalId] [varchar](255) NULL,
	[SystemAssignedManagedServiceIdentityTenantId] [varchar](255) NULL,
	[Tags] [varchar](max) NULL,
	[TargetSwapSlot] [varchar](255) NULL,
	[TrafficManagerHostNames] [varchar](max) NULL,
	[Type] [varchar](255) NULL,
	[UsageState] [varchar](255) NULL,
	[VirtualApplications] [varchar](max) NULL,
	[WebSocketsEnabled] [bit] NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
 CONSTRAINT [PK_WebApp] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[WebAppHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_VirtualNetwork_Subnet_Backuo]    Script Date: 4/25/2019 9:49:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VirtualNetwork_Subnet_Backuo]'))
EXEC dbo.sp_executesql @statement = N'

create view [dbo].[vw_VirtualNetwork_Subnet_Backuo] as


SELECT v.[Id]
      ,v.[SubscriptionId]
      ,v.[Subscription]
      ,v.[ResourceGroup]
      ,v.[Region]
      ,v.[Name] as VirtualNetName
      ,v.[AzureId]
      ,v.[AddressSpace]
      ,v.[DdosProtectionPlan]
      ,v.[DnsServers]
      ,v.[EnableDdosProtection]
      ,v.[EnableVmProtection]
      ,v.[Tags]
      ,v.[CreatedBy]
      ,v.[CreatedDtUtc]
      ,v.[ValidFrom]
      ,v.[ValidTo]
	  ,s.[Name] SubnetName
      ,s.[VirtualNetworkId]
      ,s.[AddressPrefix]
      ,s.[NetworkSecurityGroupAzureId]
      ,s.[ProvisioningState]
      ,s.[ResourceNavigationLinks]
      ,s.[RouteTableAzureId]
      ,s.CreatedBy CreatedBySubnet
      ,s.CreatedDtUtc CreatedDtUtcSubnet
      ,s.ValidFrom ValidFromSubnet
      ,s.ValidTo ValidToSubnet
	  ,niic.Name [NicIpConfigName]
	  ,niic.PrivateIpAddress
	  ,ni.Name as DeviceName
	  ,ni.PrimaryPrivateIp
	  ,ni.InternalFqdn
	  ,vm.Name VMName
	  ,p.IpAddress PublicIP
	  ,sse.Locations [ServiceLocation]
	  ,sse.Service [ServiceEndpoint]
  FROM [dbo].[VirtualNetwork] v
  inner join Subnet  s
  on v.id=s.VirtualNetworkId
  inner join NetworkInterfaceIpConfiguration niic 
  on niic.SubnetId = s.AzureId
  inner join NetworkInterface ni
  on niic.NetworkInterfaceId = ni.Id
  left join VirtualMachine vm
  on vm.AzureId = ni.VirtualMachineAzureId
  inner join PublicIp p
  on p.AzureId = niic.PublicIpAddressId
  
  left join [SubnetServiceEndpoint] sse
  on s.Id = sse.SubnetId
  



' 
GO
/****** Object:  Table [dbo].[VirtualNetworkPeeringHistory]    Script Date: 4/25/2019 9:49:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualNetworkPeeringHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualNetworkPeeringHistory](
	[Id] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[VirtualNetworkId] [int] NULL,
	[AllowForwardedTraffic] [bit] NULL,
	[AllowGatewayTransit] [bit] NULL,
	[AllowVirtualNetworkAccess] [bit] NULL,
	[PeeringState] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[RemoteAddressSpace] [varchar](255) NULL,
	[RemoteVirtualNetwork] [varchar](255) NULL,
	[UseRemoteGateways] [bit] NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[VirtualNetworkPeering]    Script Date: 4/25/2019 9:49:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VirtualNetworkPeering]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VirtualNetworkPeering](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[VirtualNetworkId] [int] NULL,
	[AllowForwardedTraffic] [bit] NULL,
	[AllowGatewayTransit] [bit] NULL,
	[AllowVirtualNetworkAccess] [bit] NULL,
	[PeeringState] [varchar](255) NULL,
	[ProvisioningState] [varchar](255) NULL,
	[RemoteAddressSpace] [varchar](255) NULL,
	[RemoteVirtualNetwork] [varchar](255) NULL,
	[UseRemoteGateways] [bit] NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[VirtualNetworkPeeringHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_VirtualNetwork_Peerings]    Script Date: 4/25/2019 9:49:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VirtualNetwork_Peerings]'))
EXEC dbo.sp_executesql @statement = N'




CREATE VIEW [dbo].[vw_VirtualNetwork_Peerings]
AS

  --This query will include peerings where one of the peered VNets may not exist
  SELECT *
  FROM
       (
         SELECT [vnet].[SubscriptionId]
               ,[vnet].[Subscription]
               ,[peer].[AzureId]
               ,[peer].[Name] + '' ('' + CONVERT(VARCHAR, [peer].[Id]) + '')'' AS [PeeringName]
               ,[peer].[PeeringState]
               ,[peer].[ProvisioningState]
               ,[vnet].[Name] AS [VirtualNetName]
         FROM [VirtualNetworkPeering] AS [peer]
              LEFT JOIN [VirtualNetwork] AS [vnet] ON [peer].[VirtualNetworkId] = [vnet].[Id]
         WHERE [vnet].[Name] IS NOT NULL
       ) AS [a]
  UNION
        (
          SELECT [vnet].[SubscriptionId]
                ,[vnet].[Subscription]
                ,[peer].[AzureId]
               ,[peer].[Name] + '' ('' + CONVERT(VARCHAR, [peer].[Id]) + '')'' AS [PeeringName]
                ,[peer].[PeeringState]
                ,[peer].[ProvisioningState]
                ,[vnet].[Name] AS [VirtualNetName]
          FROM [VirtualNetworkPeering] AS [peer]
               LEFT JOIN [VirtualNetwork] AS [vnet] ON [peer].[RemoteVirtualNetwork] = [vnet].[AzureId]
          WHERE [vnet].[Name] IS NOT NULL
        );

' 
GO
/****** Object:  Table [dbo].[AdUserHistory]    Script Date: 4/25/2019 9:49:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdUserHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdUserHistory](
	[Id] [int] NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[AccountEnabled] [bit] NULL,
	[AdditionalProperties] [varchar](max) NULL,
	[DeletionTimestamp] [datetime] NULL,
	[DisplayName] [varchar](255) NULL,
	[GivenName] [varchar](255) NULL,
	[Mail] [varchar](255) NULL,
	[MailNickname] [varchar](255) NULL,
	[ObjectId] [varchar](255) NULL,
	[SignInNames] [varchar](1023) NULL,
	[Surname] [varchar](255) NULL,
	[UsageLocation] [varchar](255) NULL,
	[UserPrincipalName] [varchar](255) NULL,
	[UserType] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[AdUser]    Script Date: 4/25/2019 9:49:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdUser]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AdUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AzureId] [varchar](1023) NOT NULL,
	[AccountEnabled] [bit] NULL,
	[AdditionalProperties] [varchar](max) NULL,
	[DeletionTimestamp] [datetime] NULL,
	[DisplayName] [varchar](255) NULL,
	[GivenName] [varchar](255) NULL,
	[Mail] [varchar](255) NULL,
	[MailNickname] [varchar](255) NULL,
	[ObjectId] [varchar](255) NULL,
	[SignInNames] [varchar](1023) NULL,
	[Surname] [varchar](255) NULL,
	[UsageLocation] [varchar](255) NULL,
	[UserPrincipalName] [varchar](255) NULL,
	[UserType] [varchar](255) NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[AdUserHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[VaultBackupPolicyHistory]    Script Date: 4/25/2019 9:49:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VaultBackupPolicyHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VaultBackupPolicyHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[VaultName] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[ProtectedItemsCount] [int] NULL,
	[TimeZone] [varchar](255) NULL,
	[InstantRpRetentionRangeInDays] [int] NULL,
	[IsCompression] [bit] NULL,
	[IsSqlCompression] [bit] NULL,
	[WorkloadType] [varchar](255) NULL,
	[DailyRetentionDurationType] [varchar](255) NULL,
	[DailyRetentionDurationCount] [int] NULL,
	[DailyRetentionDurationTime] [datetime] NULL,
	[WeeklyRetentionDurationType] [varchar](255) NULL,
	[WeeklyRetentionDurationCount] [int] NULL,
	[WeeklyRetentionDaysOfTheWeek] [varchar](255) NULL,
	[WeeklyRetentionDurationTime] [datetime] NULL,
	[MonthlyRetentionFormatType] [varchar](255) NULL,
	[MonthlyRetentionDurationType] [varchar](255) NULL,
	[MonthlyRetentionDurationCount] [int] NULL,
	[MonthlyRetentionDaysOfTheWeek] [varchar](255) NULL,
	[MonthlyRetentionWeeksOfTheMonth] [varchar](255) NULL,
	[MonthlyRetentionDaysOfTheMonth] [varchar](255) NULL,
	[MonthlyRetentionDurationTime] [datetime] NULL,
	[YearlyRetentionFormatType] [varchar](255) NULL,
	[YearlyRetentionDurationType] [varchar](255) NULL,
	[YearlyRetentionDurationCount] [int] NULL,
	[YearlyRetentionDaysOfTheWeek] [varchar](255) NULL,
	[YearlyRetentionWeeksOfTheMonth] [varchar](255) NULL,
	[YearlyRetentionDaysOfTheMonth] [varchar](255) NULL,
	[YearlyRetentionMonthsOfTheYear] [varchar](255) NULL,
	[YearlyRetentionDurationTime] [datetime] NULL,
	[ScheduleRunDays] [varchar](255) NULL,
	[ScheduleRunFrequency] [varchar](255) NULL,
	[ScheduleRunTime] [datetime] NULL,
	[ScheduleWeeklyFrequency] [int] NULL,
	[LogRetentionDurationType] [varchar](255) NULL,
	[LogRetentionDurationCount] [int] NULL,
	[LogScheduleFrequencyInMins] [int] NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[VaultBackupPolicy]    Script Date: 4/25/2019 9:49:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VaultBackupPolicy]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VaultBackupPolicy](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[VaultName] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[ProtectedItemsCount] [int] NULL,
	[TimeZone] [varchar](255) NULL,
	[InstantRpRetentionRangeInDays] [int] NULL,
	[IsCompression] [bit] NULL,
	[IsSqlCompression] [bit] NULL,
	[WorkloadType] [varchar](255) NULL,
	[DailyRetentionDurationType] [varchar](255) NULL,
	[DailyRetentionDurationCount] [int] NULL,
	[DailyRetentionDurationTime] [datetime] NULL,
	[WeeklyRetentionDurationType] [varchar](255) NULL,
	[WeeklyRetentionDurationCount] [int] NULL,
	[WeeklyRetentionDaysOfTheWeek] [varchar](255) NULL,
	[WeeklyRetentionDurationTime] [datetime] NULL,
	[MonthlyRetentionFormatType] [varchar](255) NULL,
	[MonthlyRetentionDurationType] [varchar](255) NULL,
	[MonthlyRetentionDurationCount] [int] NULL,
	[MonthlyRetentionDaysOfTheWeek] [varchar](255) NULL,
	[MonthlyRetentionWeeksOfTheMonth] [varchar](255) NULL,
	[MonthlyRetentionDaysOfTheMonth] [varchar](255) NULL,
	[MonthlyRetentionDurationTime] [datetime] NULL,
	[YearlyRetentionFormatType] [varchar](255) NULL,
	[YearlyRetentionDurationType] [varchar](255) NULL,
	[YearlyRetentionDurationCount] [int] NULL,
	[YearlyRetentionDaysOfTheWeek] [varchar](255) NULL,
	[YearlyRetentionWeeksOfTheMonth] [varchar](255) NULL,
	[YearlyRetentionDaysOfTheMonth] [varchar](255) NULL,
	[YearlyRetentionMonthsOfTheYear] [varchar](255) NULL,
	[YearlyRetentionDurationTime] [datetime] NULL,
	[ScheduleRunDays] [varchar](255) NULL,
	[ScheduleRunFrequency] [varchar](255) NULL,
	[ScheduleRunTime] [datetime] NULL,
	[ScheduleWeeklyFrequency] [int] NULL,
	[LogRetentionDurationType] [varchar](255) NULL,
	[LogRetentionDurationCount] [int] NULL,
	[LogScheduleFrequencyInMins] [int] NULL,
	[CreatedBy] [varchar](255) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[VaultBackupPolicyHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  Table [dbo].[VaultBackupHistory]    Script Date: 4/25/2019 9:49:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VaultBackupHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VaultBackupHistory](
	[Id] [int] NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[VaultName] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[BackupManagementType] [varchar](255) NULL,
	[BackupSetName] [varchar](255) NULL,
	[ComputerName] [varchar](255) NULL,
	[ContainerName] [varchar](255) NULL,
	[CreateMode] [varchar](255) NULL,
	[DeferredDeleteSyncTimeInUTC] [datetime] NULL,
	[ExtendedInfo] [varchar](255) NULL,
	[FriendlyName] [varchar](255) NULL,
	[IsScheduledForDeferredDelete] [bit] NULL,
	[HealthDetailsCount] [int] NULL,
	[HealthCode] [int] NULL,
	[HealthMessage] [varchar](255) NULL,
	[HealthRecommendationsCount] [int] NULL,
	[HealthStatus] [varchar](255) NULL,
	[LastBackupStatus] [varchar](255) NULL,
	[LastBackupTimeUtc] [datetime] NULL,
	[LastRecoveryPointUtc] [datetime] NULL,
	[PolicyId] [varchar](1023) NULL,
	[ProtectedItemDataId] [varchar](255) NULL,
	[ProtectionState] [varchar](255) NULL,
	[ProtectionStatus] [varchar](255) NULL,
	[SourceResourceId] [varchar](1023) NULL,
	[WorkloadType] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) NOT NULL,
	[ValidTo] [datetime2](2) NOT NULL
)
END
GO
/****** Object:  Table [dbo].[VaultBackup]    Script Date: 4/25/2019 9:49:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VaultBackup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VaultBackup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionId] [varchar](255) NULL,
	[Subscription] [varchar](255) NULL,
	[ResourceGroup] [varchar](255) NULL,
	[VaultName] [varchar](255) NULL,
	[Name] [varchar](255) NULL,
	[AzureId] [varchar](1023) NULL,
	[BackupManagementType] [varchar](255) NULL,
	[BackupSetName] [varchar](255) NULL,
	[ComputerName] [varchar](255) NULL,
	[ContainerName] [varchar](255) NULL,
	[CreateMode] [varchar](255) NULL,
	[DeferredDeleteSyncTimeInUTC] [datetime] NULL,
	[ExtendedInfo] [varchar](255) NULL,
	[FriendlyName] [varchar](255) NULL,
	[IsScheduledForDeferredDelete] [bit] NULL,
	[HealthDetailsCount] [int] NULL,
	[HealthCode] [int] NULL,
	[HealthMessage] [varchar](255) NULL,
	[HealthRecommendationsCount] [int] NULL,
	[HealthStatus] [varchar](255) NULL,
	[LastBackupStatus] [varchar](255) NULL,
	[LastBackupTimeUtc] [datetime] NULL,
	[LastRecoveryPointUtc] [datetime] NULL,
	[PolicyId] [varchar](1023) NULL,
	[ProtectedItemDataId] [varchar](255) NULL,
	[ProtectionState] [varchar](255) NULL,
	[ProtectionStatus] [varchar](255) NULL,
	[SourceResourceId] [varchar](1023) NULL,
	[WorkloadType] [varchar](255) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDtUtc] [datetime] NULL,
	[ValidFrom] [datetime2](2) GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo] [datetime2](2) GENERATED ALWAYS AS ROW END NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[VaultBackupHistory] , DATA_CONSISTENCY_CHECK = ON )
)
END
GO
/****** Object:  View [dbo].[vw_DataDisk_VM]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_DataDisk_VM]'))
EXEC dbo.sp_executesql @statement = N'
CREATE view [dbo].[vw_DataDisk_VM] as 

SELECT d.[Id]
      ,d.[SubscriptionId]
      ,d.[Subscription]
      ,d.[ResourceGroup]
      ,d.[Region]
      ,d.[Name]
      ,d.[AzureId]
      ,d.[SizeInGb]
      ,d.[Os]
      ,d.[CreationMethod]
      ,d.[ImageId]
      ,d.[Sku]
      ,d.[IsAttachedToVm]
      ,d.[VirtualMachineAzureId]
      ,d.[IsEncryptionEnabled]
      ,d.[CreatedBy]
      ,d.[CreatedDtUtc]
      ,d.[ValidFrom]
      ,d.[ValidTo],
	  v.Name as Name2
  FROM [dbo].[DataDisk] d, [dbo].[VirtualMachine] v
  where VirtualMachineAzureId = v.AzureId
  and d.SubscriptionId=v.SubscriptionId
  and d.ResourceGroup = v.ResourceGroup

  UNION ALL

  SELECT distinct d.[Id]
      ,d.[SubscriptionId]
      ,d.[Subscription]
      ,d.[ResourceGroup]
      ,d.[Region]
      ,d.[Name]
      ,d.[AzureId]
      ,d.[SizeInGb]
      ,d.[Os]
      ,d.[CreationMethod]
      ,d.[ImageId]
      ,d.[Sku]
      ,d.[IsAttachedToVm]
      ,d.[VirtualMachineAzureId]
      ,d.[IsEncryptionEnabled]
      ,d.[CreatedBy]
      ,d.[CreatedDtUtc]
      ,d.[ValidFrom]
      ,d.[ValidTo]
	  ,'''' 
  FROM [dbo].[DataDisk] d, [dbo].[VirtualMachine] v
  where VirtualMachineAzureId is NULL
  and d.SubscriptionId=v.SubscriptionId
  and VirtualMachineAzureId = v.AzureId
  and d.ResourceGroup = v.ResourceGroup



' 
GO
/****** Object:  View [dbo].[vw_VirtualMachine_History]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VirtualMachine_History]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [dbo].[vw_VirtualMachine_History]
AS
SELECT        [Id], [SubscriptionId], [Subscription], [ResourceGroup], [Name], [AzureId], [Region], [Status], [Os], [OsSku], [OsPublisher], [OsType], [OsVersion], [LicenseType], [Size], [Cores], [Memory], [AvailabilitySetId], 
                         [NumberOfNics], [PrimaryNicId], [OsDisk], [OsDiskSku], [OsDiskSize], [OsDiskEncryptionStatus], [IsOsDiskEncrypted], [IsManagedDiskEnabled], [NumberOfDataDisks], [AzureAgentProvisioningState], [AzureAgentVersion], [Tags], 
                         [TagApplicationName], [TagBackupPolicy], [TagBackupFrequency], [TagProjectCode], [TagProjectDurationStart], [TagProjectDurationEnd], [TagReservedInstance], [TagServerType], [CreatedBy], [CreatedDtUtc], [ValidFrom], 
                         [ValidTo],cast([ValidFrom] as date) as ChangeDate
FROM            [dbo].[VirtualMachine]
UNION ALL
SELECT        [Id], [SubscriptionId], [Subscription], [ResourceGroup], [Name], [AzureId], [Region], [Status], [Os], [OsSku], [OsPublisher], [OsType], [OsVersion], [LicenseType], [Size], [Cores], [Memory], [AvailabilitySetId], 
                         [NumberOfNics], [PrimaryNicId], [OsDisk], [OsDiskSku], [OsDiskSize], [OsDiskEncryptionStatus], [IsOsDiskEncrypted], [IsManagedDiskEnabled], [NumberOfDataDisks], [AzureAgentProvisioningState], [AzureAgentVersion], [Tags], 
                         [TagApplicationName], [TagBackupPolicy], [TagBackupFrequency], [TagProjectCode], [TagProjectDurationStart], [TagProjectDurationEnd], [TagReservedInstance], [TagServerType], [CreatedBy], [CreatedDtUtc], [ValidFrom], 
                         [ValidTo],cast([ValidFrom] as date) as ChangeDate
FROM            [dbo].[VirtualMachineHistory]
' 
GO
/****** Object:  View [dbo].[vw_VaultBackup_History]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VaultBackup_History]'))
EXEC dbo.sp_executesql @statement = N'

CREATE view [dbo].[vw_VaultBackup_History] as
/*
SELECT [Id]
      ,[SubscriptionId]
      ,[Subscription]
      ,[ResourceGroup]
      ,[VaultName]
      ,[Name]
      ,[AzureId]
      ,[BackupManagementType]
      ,[BackupSetName]
      ,[ComputerName]
      ,[ContainerName]
      ,[CreateMode]
      ,[DeferredDeleteSyncTimeInUTC]
      ,[ExtendedInfo]
      ,[FriendlyName]
      ,[IsScheduledForDeferredDelete]
      ,[HealthDetailsCount]
      ,[HealthCode]
      ,[HealthMessage]
      ,[HealthRecommendationsCount]
      ,[HealthStatus]
      ,[LastBackupStatus]
      ,[LastBackupTimeUtc]
      ,[LastRecoveryPointUtc]
      ,[PolicyId]
      ,[ProtectedItemDataId]
      ,[ProtectionState]
      ,[ProtectionStatus]
      ,[SourceResourceId]
      ,[WorkloadType]
      ,[CreatedBy]
      ,[CreatedDtUtc]
      ,[ValidFrom]
      ,[ValidTo]
  FROM [dbo].[VaultBackupHistory]

  UNION ALL

  SELECT [Id]
      ,[SubscriptionId]
      ,[Subscription]
      ,[ResourceGroup]
      ,[VaultName]
      ,[Name]
      ,[AzureId]
      ,[BackupManagementType]
      ,[BackupSetName]
      ,[ComputerName]
      ,[ContainerName]
      ,[CreateMode]
      ,[DeferredDeleteSyncTimeInUTC]
      ,[ExtendedInfo]
      ,[FriendlyName]
      ,[IsScheduledForDeferredDelete]
      ,[HealthDetailsCount]
      ,[HealthCode]
      ,[HealthMessage]
      ,[HealthRecommendationsCount]
      ,[HealthStatus]
      ,[LastBackupStatus]
      ,[LastBackupTimeUtc]
      ,[LastRecoveryPointUtc]
      ,[PolicyId]
      ,[ProtectedItemDataId]
      ,[ProtectionState]
      ,[ProtectionStatus]
      ,[SourceResourceId]
      ,[WorkloadType]
      ,[CreatedBy]
      ,[CreatedDtUtc]
      ,[ValidFrom]
      ,[ValidTo]
  FROM [dbo].[VaultBackup]
*/

	SELECT 	
	   Id
      ,[Subscription]
	  --,[SubscriptionId] as SubscriptionId1
      ,[ResourceGroup]
      --,[VaultName] as VaultName1
      --,[Name]
      ,[AzureId]
      ,[BackupManagementType]
      ,[BackupSetName]
      ,[ComputerName]
      ,[ContainerName]
      ,[CreateMode]
      ,[DeferredDeleteSyncTimeInUTC]
      ,[ExtendedInfo]
      ,[FriendlyName]
      ,[IsScheduledForDeferredDelete]
      ,[HealthDetailsCount]
      ,[HealthCode]
      ,[HealthMessage]
      ,[HealthRecommendationsCount]
      ,[HealthStatus]
      ,[LastBackupStatus]
      ,CAST([LastBackupTimeUtc] as DATETIME) as LastBackupTimeUtc
      ,[LastRecoveryPointUtc]
      ,[ProtectionState]
      ,[ProtectionStatus]
	  ,D.DailyRetentionDurationType
	  ,D.ValidFrom, D.VaultName,D.SubscriptionId, WorkloadType,D.ScheduleRunFrequency,CAST(D.ScheduleRunTime as TIME) as ScheduleRunTime,D.ScheduleRunDays,D.Name,D.DailyRetentionDurationCount, D.MonthlyRetentionDurationCount,D.MonthlyRetentionDurationType,D.YearlyRetentionDurationCount,d.YearlyRetentionDurationType
	 FROM VaultBackup N
	 INNER JOIN 
		(SELECT RANK() OVER (PARTITION BY VaultName ORDER BY ValidFrom  DESC) r,ValidFrom, [DailyRetentionDurationType],VaultName,SubscriptionId, ScheduleRunFrequency,ScheduleRunTime,ScheduleRunDays,Name,DailyRetentionDurationCount, MonthlyRetentionDurationCount,MonthlyRetentionDurationType,YearlyRetentionDurationType,YearlyRetentionDurationCount FROM VaultBackupPolicy) D
		ON (N.VaultName = D.VaultName and N.SubscriptionId=D.SubscriptionId)
	WHERE D.r = 1
		


' 
GO
/****** Object:  View [dbo].[vw_PublicIP]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_PublicIP]'))
EXEC dbo.sp_executesql @statement = N'/****** Script for SelectTopNRows command from SSMS  ******/
create view [dbo].[vw_PublicIP] as

SELECT [Id]
      ,[SubscriptionId]
      ,[Subscription]
      ,[ResourceGroup]
      ,[Region]
      ,[Name]
      ,[AzureId]
      ,[IpAddress]
      ,[IpAllocationMethod]
      ,[Version]
      ,[Fqdn]
      ,[HasAssignedLoadBalancer]
      ,[HasAssignedNetworkInterface]
      ,[AvailabilityZones]
      ,[IdleTimeoutInMinutes]
      ,[LeafDomainLabel]
      ,[ReverseFqdn]
      ,[CreatedBy]
      ,[CreatedDtUtc]
      ,[ValidFrom]
      ,[ValidTo]
  FROM [dbo].[PublicIp]' 
GO
/****** Object:  View [dbo].[vw_WebApp]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_WebApp]'))
EXEC dbo.sp_executesql @statement = N'/****** Script for SelectTopNRows command from SSMS  ******/

create view [dbo].[vw_WebApp] as 

SELECT [Id]
      ,[SubscriptionId]
      ,[Subscription]
      ,[ResourceGroup]
      ,[Region]
      ,[Name]
      ,[AzureId]
      ,[AlwaysOn]
      ,[AppServicePlanId]
      ,[AutoSwapSlotName]
      ,[AvailabilityState]
      ,[ClientAffinityEnabled]
      ,[ClientCertEnabled]
      ,[CloningInfo]
      ,[ContainerSize]
      ,[DailyMemoryTimeQuota]
      ,[DefaultDocuments]
      ,[DefaultHostName]
      ,[DiagnosticLogsConfig]
      ,[DocumentRoot]
      ,[Enabled]
      ,[EnabledHostNames]
      ,[FtpsState]
      ,[HostingEnvironmentProfile]
      ,[HostNames]
      ,[HostNamesDisabled]
      ,[HostNameSslStates]
      ,[Http20Enabled]
      ,[HttpsOnly]
      ,[IsDefaultContainer]
      ,[IsXenon]
      ,[JavaContainer]
      ,[JavaContainerVersion]
      ,[JavaVersion]
      ,[Key]
      ,[Kind]
      ,[LastModifiedTimeUtc]
      ,[LastSwapDestination]
      ,[LastSwapSource]
      ,[LastSwapTimestampUtc]
      ,[LinuxFxVersion]
      ,[LocalMySqlEnabled]
      ,[ManagedPipelineMode]
      ,[MaxNumberOfWorkers]
      ,[NetFrameworkVersion]
      ,[NodeVersion]
      ,[OperatingSystem]
      ,[OutboundIpAddresses]
      ,[ParentId]
      ,[PhpVersion]
      ,[PossibleOutboundIpAddresses]
      ,[PlatformArchitecture]
      ,[PythonVersion]
      ,[RemoteDebuggingEnabled]
      ,[RemoteDebuggingVersion]
      ,[RepositorySiteName]
      ,[Reserved]
      ,[ScmSiteAlsoStopped]
      ,[ScmType]
      ,[ServerFarmId]
      ,[SiteConfig]
      ,[SnapshotInfo]
      ,[State]
      ,[SuspendedTill]
      ,[SystemAssignedManagedServiceIdentityPrincipalId]
      ,[SystemAssignedManagedServiceIdentityTenantId]
      ,[Tags]
      ,[TargetSwapSlot]
      ,[TrafficManagerHostNames]
      ,[Type]
      ,[UsageState]
      ,[VirtualApplications]
      ,[WebSocketsEnabled]
      ,[CreatedBy]
      ,[CreatedDtUtc]
      ,[ValidFrom]
      ,[ValidTo]
  FROM [dbo].[WebApp]' 
GO
/****** Object:  View [dbo].[vw_NetworkInterface_Configuration]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_NetworkInterface_Configuration]'))
EXEC dbo.sp_executesql @statement = N'
CREATE view [dbo].[vw_NetworkInterface_Configuration] as

SELECT [SubscriptionId]
      ,[Subscription]
      ,[ResourceGroup]
      ,[Region]
      ,[NumberOfDnsServers]
      ,[NumberOfAppliedDnsServers]
      ,[InternalDnsNameLabel]
      ,[InternalDomainNameSuffix]
      ,[InternalFqdn]
      ,[IsAcceleratedNetworkingEnabled]
      ,[IsIpForwardingEnabled]
      ,[MacAddress]
      ,[NetworkSecurityGroupId]
      ,[PrimaryPrivateIp]
      ,[PrimaryPrivateIpAllocationMethod]
      ,[VirtualMachineAzureId]
      ,[CreatedBy]
      ,[CreatedDtUtc]
	  ,nic.IsPrimary
	  ,nic.Name
	  ,nic.PrivateIpAddress
	  ,nic.PublicIpAddressId
	  ,nic.SubnetId
	  ,vm.Name as VMName

FROM [dbo].[NetworkInterface] ni

INNER JOIN 
(
SELECT [Id]
      ,[Name]
      ,[AzureId]
      ,[NetworkInterfaceId]
      ,[IsPrimary]
      ,[VirtualNetworkId]
      ,[SubnetId]
      ,[PrivateIpAddress]
      ,[PrivateIpAddressVersion]
      ,[PrivateIpAllocationMethod]
      ,[PublicIpAddressId]
      ,[ValidFrom]
      ,[ValidTo]
  FROM [dbo].[NetworkInterfaceIpConfiguration] 
  
  ) nic on  ni.Id = nic.NetworkInterfaceId
  inner join
  (
	select Name,AzureId from VirtualMachine
  ) vm on vm.AzureId = ni.VirtualMachineAzureId' 
GO
/****** Object:  View [dbo].[vw_VirtualMachine]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VirtualMachine]'))
EXEC dbo.sp_executesql @statement = N'create view [dbo].[vw_VirtualMachine] as

	SELECT vm.[Id]
      ,vm.[SubscriptionId]
      ,vm.[Subscription]
      ,vm.[ResourceGroup]
      ,vm.[Name]
      ,vm.[AzureId]
      ,vm.[Region]
      ,vm.[Status]
      ,vm.[Os]
      ,vm.[OsSku]
      ,vm.[OsPublisher]
      ,vm.[OsType]
      ,vm.[OsVersion]
      ,vm.[LicenseType]
      ,vm.[Size]
      ,vm.[Cores]
      ,vm.[Memory]
      ,vm.[AvailabilitySetId]
      ,vm.[NumberOfNics]
      ,vm.[PrimaryNicId]
      ,vm.[OsDisk]
      ,vm.[OsDiskSku]
      ,vm.[OsDiskSize]
      ,vm.[OsDiskEncryptionStatus]
      ,vm.[IsOsDiskEncrypted]
      ,vm.[IsManagedDiskEnabled]
      ,vm.[NumberOfDataDisks]
      ,vm.[AzureAgentProvisioningState]
      ,vm.[AzureAgentVersion]
      ,vm.[Tags]
      ,vm.[TagApplicationName]
      ,vm.[TagBackupPolicy]
      ,vm.[TagBackupFrequency]
      ,vm.[TagProjectCode]
      ,vm.[TagProjectDurationStart]
      ,vm.[TagProjectDurationEnd]
      ,vm.[TagReservedInstance]
      ,vm.[TagServerType]
      ,vm.[CreatedBy]
      ,vm.[CreatedDtUtc]
      ,vm.[ValidFrom]
      ,vm.[ValidTo]
  FROM [dbo].[VirtualMachine] vm
  left join VirtualMachineHistory vmh
  ON vm.name = vmh.name and vm.ResourceGroup=vmh.ResourceGroup and vm.SubscriptionId=vmh.SubscriptionId



' 
GO
/****** Object:  View [dbo].[vw_Disk]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_Disk]'))
EXEC dbo.sp_executesql @statement = N'
create view [dbo].[vw_Disk] as

SELECT d.[Id]
      ,d.[SubscriptionId]
      ,d.[Subscription]
      ,d.[ResourceGroup]
      ,d.[Region]
      ,d.[Name]
      ,d.[AzureId]
      ,d.[SizeInGb]
      ,d.[Os]
      ,d.[CreationMethod]
      ,d.[ImageId]
      ,d.[Sku]
      ,d.[IsAttachedToVm]
      ,d.[VirtualMachineAzureId]
      ,d.[IsEncryptionEnabled]
      ,d.[CreatedBy]
      ,d.[CreatedDtUtc]
      ,d.[ValidFrom]
      ,d.[ValidTo]
  FROM [dbo].[DataDisk] d
  left join DataDiskHistory dh
  ON d.name = dh.name and d.ResourceGroup=dh.ResourceGroup and d.SubscriptionId=dh.SubscriptionId

' 
GO
/****** Object:  View [dbo].[vw_VirtualMachinesDeleted]    Script Date: 4/25/2019 9:49:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_VirtualMachinesDeleted]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_VirtualMachinesDeleted]
AS
     SELECT VirtualMachineHistory.*, 
            ActivityLog.Caller AS DeletedBy, 
            ActivityLog.EventTimestampUtc AS DeletedDtUtc
     FROM
     (
         SELECT Id, 
                MAX(ValidTo) AS ValidTo
         FROM VirtualMachineHistory
		 WHERE AzureId IS NOT NULL
         GROUP BY id
     ) VirtualMachineHistoryLatest
     INNER JOIN VirtualMachineHistory ON VirtualMachineHistory.Id = VirtualMachineHistoryLatest.Id
                                         AND VirtualMachineHistory.ValidTo = VirtualMachineHistoryLatest.ValidTo
     LEFT JOIN VirtualMachine ON VirtualMachine.Id = VirtualMachineHistory.Id
     INNER JOIN
     (
         SELECT ResourceId, 
                MAX(EventTimeStampUtc) AS EventTimestampUtc
         FROM ActivityLog
         WHERE Operation = ''Delete Virtual Machine'' AND Status = ''Succeeded''
         GROUP BY ResourceId
     ) ActivityLogLatest ON ActivityLogLatest.ResourceId = VirtualMachineHistory.AzureId
     INNER JOIN ActivityLog ON ActivityLog.ResourceId = ActivityLogLatest.ResourceId
                               AND ActivityLog.EventTimestampUtc = ActivityLogLatest.EventTimestampUtc
     WHERE VirtualMachine.Id IS NULL;
' 
GO
