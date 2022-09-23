[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $CCA,
    [Parameter(Mandatory = $true)][string] $environment,
    [Parameter(Mandatory = $true)][string] $defaultConnectionString,
    [Parameter(Mandatory = $true)][string] $crmConnectionString
)

trap {
    "Exception encountered:"
    $_.Exception
    Exit(1)
}

#Verify user is logged into az cli
$user = $(az ad signed-in-user show)
if ($null -eq $user) { throw("Please run 'az-login' and run script again.") }

#Verify user is logged into AzureAD
$tenantDetail = $(Get-AzureADTenantDetail)
if ($null -eq $tenantDetail) { throw("Please run 'Connect-AzureAD' and run script again.") }

$environment = $environment.ToLower()
#Validate Inputs and ensure that app registrations do not already exist
if (-not @("uat", "prod").Contains($environment)) {
    throw "Environment must be 'uat', or 'prod'. Exiting."
}

$cca = $cca.ToLower()

#####################################################################################################################################
#Create IFrame app service
#####################################################################################################################################

if ($environment -eq "uat") {
    $subscriptionName = "CCA Dev"
    $iFramesResourceGroup = "PZ_RG_CCA_TEST_IFRAMES_01"
    $appName = "opt$cca-uat"
    $aseDomainName = "ase-cca-westus-dev-dmz-01.appserviceenvironment.net"
    $serverFarmId = "/subscriptions/f3c4b539-7c05-49b3-8657-479ab0846f66/resourceGroups/PZ_RG_CCA_DEV_APP_SERVICE_PLANS_01/providers/Microsoft.Web/serverfarms/plan-cca-westus-dev-dmz-02"
    $aseId = "/subscriptions/f3c4b539-7c05-49b3-8657-479ab0846f66/resourceGroups/PZ_RG_CCA_DEV_VNET_02/providers/Microsoft.Web/hostingEnvironments/ase-cca-westus-dev-dmz-01"
    $appInsightsInstrumentationKey = "cb5a1e23-acc0-4751-8e39-007bcd2ccb33"
}
elseif ($environment -eq "prod") {
    $subscriptionName = "CCA"
    $iFramesResourceGroup = "PZ_RG_CCA_PROD_IFRAMES_02"
    $appName = "opt$cca"
    $aseDomainName = "ase-cca-westus-prod-dmz-01.appserviceenvironment.net"
    $serverFarmId = "/subscriptions/97de0298-ca3c-4a78-878f-9f181b482059/resourceGroups/PZ_RG_CCA_PROD_APP_SERVICE_PLANS_01/providers/Microsoft.Web/serverfarms/plan-cca-westus-prod-dmz-01"
    $aseId = "/subscriptions/97de0298-ca3c-4a78-878f-9f181b482059/resourceGroups/PZ_RG_CCA_PROD_VNET_01/providers/Microsoft.Web/hostingEnvironments/ase-cca-westus-prod-dmz-01"
    $appInsightsInstrumentationKey = "3ae2c966-e40f-4882-bf88-0199249e8e75"
}

$template = @"
{
    "`$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {},
    "resources": [
        {
            "apiVersion": "2020-09-01",
            "type": "Microsoft.Web/sites",
            "name": "$appName",
            "location": "West US",
            "tags": {
                "Application_Name": "CCA IFrames"
            },
            "kind": "app",
            "identity": {
                "type": "SystemAssigned"
            },
            "properties": {
                "enabled": true,
                "hostNameSslStates": [
                    {
                        "name": "$appName.$aseDomainName",
                        "sslState": "Disabled",
                        "hostType": "Standard"
                    },
                    {
                        "name": "$appName.scm.$aseDomainName",
                        "sslState": "Disabled",
                        "hostType": "Repository"
                    }
                ],
                "serverFarmId": "$serverFarmId",
                "reserved": false,
                "isXenon": false,
                "hyperV": false,
                "siteConfig": {
                    "numberOfWorkers": 1,
                    "defaultDocuments": [
                        "Default.htm",
                        "Default.html",
                        "Default.asp",
                        "index.htm",
                        "index.html",
                        "iisstart.htm",
                        "default.aspx",
                        "index.php",
                        "hostingstart.html"
                    ],
                    "netFrameworkVersion": "v4.0",
                    "phpVersion": "5.6",
                    "requestTracingEnabled": false,
                    "remoteDebuggingEnabled": false,
                    "remoteDebuggingVersion": "VS2019",
                    "httpLoggingEnabled": false,
                    "logsDirectorySizeLimit": 35,
                    "detailedErrorLoggingEnabled": false,
                    "publishingUsername": "`$$appName",
                    "azureStorageAccounts": {},
                    "scmType": "VSTSRM",
                    "use32BitWorkerProcess": true,
                    "webSocketsEnabled": false,
                    "alwaysOn": true,
                    "managedPipelineMode": "Integrated",
                    "virtualApplications": [
                        {
                            "virtualPath": "/",
                            "physicalPath": "site\\wwwroot",
                            "preloadEnabled": true
                        },
                        {
                            "virtualPath": "/api",
                            "physicalPath": "site\\wwwroot\\api",
                            "preloadEnabled": false
                        },
                        {
                            "virtualPath": "/ui",
                            "physicalPath": "site\\wwwroot\\ui",
                            "preloadEnabled": false
                        }
                    ],
                    "loadBalancing": "LeastRequests",
                    "experiments": {
                        "rampUpRules": []
                    },
                    "autoHealEnabled": false,
                    "localMySqlEnabled": false,
                    "managedServiceIdentityId": 5,
                    "ipSecurityRestrictions": [
                        {
                            "ipAddress": "Any",
                            "action": "Allow",
                            "priority": 1,
                            "name": "Allow all",
                            "description": "Allow all access"
                        }
                    ],
                    "scmIpSecurityRestrictions": [
                        {
                            "ipAddress": "Any",
                            "action": "Allow",
                            "priority": 1,
                            "name": "Allow all",
                            "description": "Allow all access"
                        }
                    ],
                    "scmIpSecurityRestrictionsUseMain": false,
                    "http20Enabled": false,
                    "minTlsVersion": "1.2",
                    "ftpsState": "FtpsOnly",
                    "appSettings": [
                        {
                            "name": "APPINSIGHTS_INSTRUMENTATIONKEY",
                            "value": "$appInsightsInstrumentationKey"
                        },
                        {
                            "name": "APPINSIGHTS_PROFILERFEATURE_VERSION",
                            "value": "disabled"
                        },
                        {
                            "name": "APPINSIGHTS_SNAPSHOTFEATURE_VERSION",
                            "value": "1.0.0"
                        },
                        {
                            "name": "APPLICATIONINSIGHTS_CONNECTION_STRING",
                            "value": "InstrumentationKey=$appInsightsInstrumentationKey;IngestionEndpoint=https://westus-0.in.applicationinsights.azure.com/"
                        },
                        {
                            "name": "ApplicationInsightsAgent_EXTENSION_VERSION",
                            "value": "~2"
                        },
                        {
                            "name": "CCA",
                            "value": "$($cca.ToUpper())"
                        },
                        {
                            "name": "DiagnosticServices_EXTENSION_VERSION",
                            "value": "~3"
                        },
                        {
                            "name": "InstrumentationEngine_EXTENSION_VERSION",
                            "value": "disabled"
                        },
                        {
                            "name": "SnapshotDebugger_EXTENSION_VERSION",
                            "value": "disabled"
                        },
                        {
                            "name": "XDT_MicrosoftApplicationInsights_BaseExtensions",
                            "value": "disabled"
                        },
                        {
                            "name": "XDT_MicrosoftApplicationInsights_Mode",
                            "value": "recommended"
                        },
                        {
                            "name": "XDT_MicrosoftApplicationInsights_PreemptSdk",
                            "value": "disabled"
                        }
                    ],
                    "metadata": [
                        {
                            "name": "CURRENT_STACK",
                            "value": "dotnetcore"
                        }
                    ]
                },
                "scmSiteAlsoStopped": false,
                "hostingEnvironmentProfile": {
                    "id": "$aseId"
                },
                "clientAffinityEnabled": true,
                "clientCertEnabled": false,
                "hostNamesDisabled": false,
                "containerSize": 0,
                "dailyMemoryTimeQuota": 0,
                "httpsOnly": true,
                "redundancyMode": "None"
            }
        }
    ],
    "outputs": {
        "managedIdentityPrincipalId": {
            "type": "string",
            "value": "[reference(resourceId('Microsoft.Web/sites','$appName'),'2019-08-01','full').identity.principalId]"
        }
    }
}
"@

# Set-AzContext -Subscription "$subscriptionName"
# New-AzResourceGroupDeployment -ResourceGroupName $iFramesResourceGroup -TemplateObject $template

$templateFilePath = "$env:TEMP\iFrames_template_$([Guid]::NewGuid().ToString()).json"
Set-Content -Path "$templateFilePath" -Value $template
$deploymentResult = az deployment group create --subscription $subscriptionName --resource-group $iFramesResourceGroup --template-file "$templateFilePath"
Remove-Item -Path "$templateFilePath"

$managedIdentityPrincipalId = $($deploymentResult | ConvertFrom-Json).properties.outputs.managedIdentityPrincipalId.value

if ($null -eq $managedIdentityPrincipalId) {
    throw ("Managed identity for $appName is null. Please check deployment.")
}
Write-Host "Created the following app: $appName"

#####################################################################################################################################

#####################################################################################################################################
#Add secrets to IFrames key vault
#####################################################################################################################################
if ($environment -eq "prod") { $keyVaultName = "APP-IFRAME-PROD" }
elseif ($environment -eq "uat") { $keyVaultName = "APP-IFRAME-UAT" }

#Cors-Origin Secret
$secretName = "$($cca.ToUpper())-Cors-Origin"
$secretValue = "https://$appName.communityenergysolutions.com"
$secret = az keyvault secret set --subscription $subscriptionName `
                                 --vault-name $keyVaultName `
                                 --name $secretName `
                                 --value $secretValue

if ($null -eq $secret) { throw ("Failed to add the secret '$secretName' to the key vault '$keyVaultName'. See console output for error details.") }
Write-Host "Added the following secret to $($keyVaultName): $secretName"

#DefaultConnection Secret
$secretName = "$($cca.ToUpper())-DefaultConnection"
$secretValue = "$defaultConnectionString"
$secret = az keyvault secret set --subscription $subscriptionName `
                                 --vault-name $keyVaultName `
                                 --name $secretName `
                                 --value $secretValue

if ($null -eq $secret) { throw ("Failed to add the secret '$secretName' to the key vault '$keyVaultName'. See console output for error details.") }
Write-Host "Added the following secret to $($keyVaultName): $secretName"

#CRMConnectionString Secret
$secretName = "$($cca.ToUpper())-CRMConnectionString"
$secretValue = "$crmConnectionString"
$secret = az keyvault secret set --subscription $subscriptionName `
                                 --vault-name $keyVaultName `
                                 --name $secretName `
                                 --value $secretValue

if ($null -eq $secret) { throw ("Failed to add the secret '$secretName' to the key vault '$keyVaultName'. See console output for error details.") }
Write-Host "Added the following secret to $($keyVaultName): $secretName"


#Add identity to access policy
$policy = az keyvault set-policy --subscription $subscriptionName `
                                 --name $keyVaultName `
                                 --secret-permissions get list `
                                 --object-id $managedIdentityPrincipalId
Write-Host "Updated access policy"