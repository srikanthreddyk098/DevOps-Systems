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
    if ($subscriptionId -eq "97de0298-ca3c-4a78-878f-9f181b482059" -and $vault.Name -eq "RSV-CCAProd-General") { continue }

    #Write-Host "  Checking vault: $($vault.Name)..."
    Set-AzRecoveryServicesAsrVaultContext -Vault $vault | Out-Null

    $fabrics = Get-AzRecoveryServicesAsrFabric
    foreach ($fabric in $fabrics) {
        $protectionContainers = Get-AzRecoveryServicesAsrProtectionContainer -Fabric $fabric
        foreach ($container in $protectionContainers) {
            #Write-Host "    Checking container: $($container.Name)..."
            $replicationProtectedItems = Get-AzRecoveryServicesAsrReplicationProtectedItem -ProtectionContainer $container

            foreach ($item in $replicationProtectedItems) {
                #Write-Host "      Found replication item: $($item.FriendlyName)"
				$items +=  $item
            }
        }
    }
}

return $items