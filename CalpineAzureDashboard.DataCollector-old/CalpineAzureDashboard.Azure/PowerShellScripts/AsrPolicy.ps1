param (
    $username,
	$password,
    $tenantId,
    $subscriptionId
)

$securePassword = $password | ConvertTo-SecureString -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($username, $securePassword)
Connect-AzAccount -Credential $credential -ServicePrincipal -Tenant $tenantId | Out-Null

$items = @()
Set-AzContext -SubscriptionId $subscriptionId | Out-Null

$vaults = Get-AzRecoveryServicesVault
foreach ($vault in $vaults) {
    #Write-Host "  Checking vault: $($vault.Name)..."
    Set-AzRecoveryServicesAsrVaultContext -Vault $vault | Out-Null

    $items += Get-AzRecoveryServicesAsrPolicy
}

return $items