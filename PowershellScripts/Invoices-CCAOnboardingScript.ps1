[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $CCA,
    [Parameter(Mandatory = $true)]
    [string] $environment
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

#Verify user is logged into Azure
$tenantId = "7406f7f1-ef6e-49f3-a9c0-002b8bc12056"
$subscriptions = $(Get-AzSubscription -TenantId $tenantId)
if ($null -eq $tenantId) { throw("Please run 'Login-AzAccount' and run script again.") }

$environment = $environment.ToLower()
#Validate Inputs and ensure that app registrations do not already exist

if (-not @("uat", "prod").Contains($environment)) {
    throw "Environment must be 'uat', or 'prod'. Exiting."
}

$cca = $cca.ToLower()
$apiName = "invoice-api-$cca-$environment"
$clientName = "invoice-client-$cca-$environment"

if ($null -ne $(Get-AzureADApplication -SearchString "$apiName")) {
    throw ("The api app registration $apiName already exists! Exiting.")
}

if ($null -ne $(Get-AzureADApplication -SearchString "$clientName")) {
    throw ("The client app registration $clientName already exists! Exiting.")
}

#####################################################################################################################################
#Create and configure API app registration
#####################################################################################################################################
#Create app registration
$apiAppRole = [Microsoft.Open.AzureAD.Model.AppRole]::new()
$apiAppRole.Id = [Guid]::NewGuid().ToString()
$apiAppRole.DisplayName = "BillingApps"
$apiAppRole.Description = "Apps that require Billing info and documents"
$apiAppRole.Value = "Billing.Read"
$apiAppRole.IsEnabled = $true
$apiAppRole.AllowedMemberTypes = @("Application")
$apiAppReg = New-AzureADApplication -DisplayName $apiName -AppRoles $apiAppRole
Write-Host "Created $apiName app registration..."

#Set app registration owner to user running this script
$signedInUser = $(Get-AzureAdUser -ObjectId $(Get-AzContext).Account.Id)
Add-AzureADApplicationOwner -ObjectId $apiAppReg.ObjectId -RefObjectId "$($signedInUser.ObjectId)"
Write-Host "    -set owner to $($signedInUser.UserPrincipalName)"

#Set API permissions and identifier uri
$apiRequiredResourceAccess = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]::new()
$apiRequiredResourceAccess.ResourceAppId = "00000003-0000-0000-c000-000000000000"
$apiRequiredResourceAccess.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]
$resourceAccess = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new()
$resourceAccess.Id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
$resourceAccess.Type = "Scope"
$apiRequiredResourceAccess.ResourceAccess.Add($resourceAccess)
$apiIdentifierUri = "api://$($apiAppReg.AppId)"
Set-AzureADApplication -ObjectId $apiAppReg.ObjectId -RequiredResourceAccess $apiRequiredResourceAccess -IdentifierUris $apiIdentifierUri
Write-Host "    -configured API permissions and identifier URL"

#Remove default user_impersonation scope
$scopes = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.OAuth2Permission]
$scope = $apiAppReg.Oauth2Permissions | Where-Object { $_.Value -eq "user_impersonation" }
$scope.IsEnabled = $false
$scopes.Add($scope)
Set-AzureADApplication -ObjectId $apiAppReg.ObjectId -Oauth2Permissions $scopes
$scopes = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.OAuth2Permission]
Set-AzureADApplication -ObjectId $apiAppReg.ObjectId -Oauth2Permissions $scopes
Write-Host "    -removed default user_impersonation scope"

#Create associated service principal
$apiSP = New-AzureADServicePrincipal -AppId $apiAppReg.AppId -DisplayName $apiName -AccountEnabled $true -AppRoleAssignmentRequired $false
Write-Host "    -created associated service principal"

#wait to allow app registrations to be fully registered before making az api calls
Write-Host "    -waiting 60 seconds for service principal to be created and registered"
Start-Sleep -Seconds 60

#The New-AzureADApplication module sets the oauth2AllowIdTokenImplicitFlow to true by default and cannot be changed through 
#the available PowerShell modules, so we need to set it to false using a Microsoft Graph rest API call
#https://stackoverflow.com/questions/64370204/azuread-oauth2allowidtokenimplicitflow-for-app-registrations
az rest --method PATCH `
        --uri "https://graph.microsoft.com/v1.0/applications/$($apiAppReg.ObjectId)" `
        --headers 'Content-Type=application/json' `
        --body '{\"web\":{\"implicitGrantSettings\":{\"enableIdTokenIssuance\":false}}}'
        Write-Host "    -set implicit grant settings to false"

#####################################################################################################################################

#####################################################################################################################################
#Create and configure client app registration
#####################################################################################################################################
#Create app registration
$clientAppReg = New-AzureADApplication -DisplayName $clientName
Write-Host "Created $clientName app registration..."

#Set app registration owner to user running this script
Add-AzureADApplicationOwner -ObjectId $clientAppReg.ObjectId -RefObjectId "$($signedInUser.ObjectId)"
Write-Host "    -set owner to $($signedInUser.UserPrincipalName)"

#Set API permissions
$clientRequiredResourceAccessList = [System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]]::new()

$requiredResourceAccess = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]::new()
$requiredResourceAccess.ResourceAppId = "00000003-0000-0000-c000-000000000000"
$requiredResourceAccess.ResourceAccess = [System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]]::new()
$resourceAccess = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new()
$resourceAccess.Id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
$resourceAccess.Type = "Scope"
$requiredResourceAccess.ResourceAccess.Add($resourceAccess)
$clientRequiredResourceAccessList.Add($requiredResourceAccess)

$requiredResourceAccess = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]::new()
$requiredResourceAccess.ResourceAppId = "$($apiAppReg.AppId)"
$requiredResourceAccess.ResourceAccess = [System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]]::new()
$resourceAccess = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new()
$resourceAccess.Id = "$($($apiAppReg.AppRoles | Where-Object { $_.Value -eq "Billing.Read" } | Select-Object Id).Id)"
$resourceAccess.Type = "Role"
$requiredResourceAccess.ResourceAccess.Add($resourceAccess)
$clientRequiredResourceAccessList.Add($requiredResourceAccess)
Set-AzureADApplication -ObjectId $clientAppReg.ObjectId -RequiredResourceAccess $clientRequiredResourceAccessList
Write-Host "    -configured API permissions"

#Remove default user_impersonation scope
$scopes = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.OAuth2Permission]
$scope = $clientAppReg.Oauth2Permissions | Where-Object { $_.Value -eq "user_impersonation" }
$scope.IsEnabled = $false
$scopes.Add($scope)
Set-AzureADApplication -ObjectId $clientAppReg.ObjectId -Oauth2Permissions $scopes
$scopes = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.OAuth2Permission]
Set-AzureADApplication -ObjectId $clientAppReg.ObjectId -Oauth2Permissions $scopes
Write-Host "    -removed default user_impersonation scope"

#Create associated service principal
$clientSP = New-AzureADServicePrincipal -AppId $clientAppReg.AppId -DisplayName $clientName -AccountEnabled $true -AppRoleAssignmentRequired $false
Write-Host "    -created associated service principal"

#wait to allow app registrations to be fully registered before making az api calls
Write-Host "    -waiting 60 seconds for service principal to be created and registered"
Start-Sleep -Seconds 60

#The New-AzureADApplication module sets the oauth2AllowIdTokenImplicitFlow to true by default and cannot be changed through 
#the available PowerShell modules, so we need to set it to false using a Microsoft Graph rest API call
#https://stackoverflow.com/questions/64370204/azuread-oauth2allowidtokenimplicitflow-for-app-registrations
az rest --method PATCH `
        --uri "https://graph.microsoft.com/v1.0/applications/$($clientAppReg.ObjectId)" `
        --headers 'Content-Type=application/json' `
        --body '{\"web\":{\"implicitGrantSettings\":{\"enableIdTokenIssuance\":false}}}'
Write-Host "    -set implicit grant settings to false"

#Grant admin consent for API permissions
az ad app permission admin-consent --id $clientAppReg.AppId
Write-Host "    -granted admin consent"

#####################################################################################################################################

#####################################################################################################################################
#Add secret to key vault
#####################################################################################################################################
if ($environment -eq "prod") {
    $subscriptionName = "CCA"
    $keyVaultName = "APP-INVOICE-PROD"
}
elseif ($environment -eq "uat") {
    $subscriptionName = "CCA Dev"
    $keyVaultName = "APP-INVOICE-UAT"
}

$keyVaultSecretName = "$($cca.ToUpper())-AzureAd--ClientId"

$secret = az keyvault secret set --subscription $subscriptionName `
                                 --vault-name $keyVaultName `
                                 --name $keyVaultSecretName `
                                 --value $apiIdentifierUri

if ($null -eq $secret) { 
    throw ("Failed to add the secret '$secretName' to the key vault '$keyVaultName'. See console output for error details.")
}
Write-Host "Added the following secret to $($keyVaultName): $keyVaultSecretName"

#####################################################################################################################################

#####################################################################################################################################
#Add virtual directory to invoice app
#####################################################################################################################################
if ($environment -eq "prod") {
    $subscriptionName = "CCA"
    $resourceGroup = "PZ_RG_CCA_PROD_INVOICE_02"
    $appName = "invoices"
}
elseif ($environment -eq "uat") {
    $subscriptionName = "CCA Dev"
    $appName = "invoices-uat"
    $resourceGroup = "PZ_RG_CCA_TEST_INVOICE_01"
}

Set-AzContext -TenantId $tenantId -Subscription "$subscriptionName"
$webApp = Get-AzWebApp -ResourceGroupName $resourceGroup -Name $appName

$newVirtualApp = [Microsoft.Azure.Management.WebSites.Models.VirtualApplication]::new()
$newVirtualApp.VirtualPath = "/$cca"
$newVirtualApp.PhysicalPath = "site\wwwroot\$cca"
$newVirtualApp.PreloadEnabled = [boolean] "True"
$newVirtualApp.VirtualDirectories = $null

$webApp.SiteConfig.VirtualApplications.Add($newVirtualApp)

Set-AzWebApp -WebApp $webApp