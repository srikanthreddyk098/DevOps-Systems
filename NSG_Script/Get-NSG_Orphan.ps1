Clear-Host

##Email input variables
$smtpServer = "relay.calpine.com"
$to = @("srikanth.kandala@calpine.com")
$from = "srikanth.kandala@calpine.com"
$subject = "Orphan Network Security Groups - $((Get-Date).ToString("02/22/2022"))"
$attachments = @()

##nsg details output file path 
$nsgUsageOutFile = 'C:\Temp\NetworkSecurityGroups.csv'

##declaring Output variables
[System.Collections.ArrayList]$nsgUsage = New-Object -TypeName System.Collections.ArrayList


##Azure Tenant ID
$tenantId = "17474e16-44cc-4a07-8b13-4d65ff4e9797"
##Azure Subscription Name
$SubscriptionName = 'Calpine DevLabs'
##Service Principal Object ID
$clientId = "33a96879-0a9d-4806-ac98-41e721f1eed4"
##Service Principal secret\password
$clientSecret = "lR-7Q~jzqP25sRs4C8JdJx_xF3uRCTW-HWFNL"

##Create Credential object for service Principal
$PasswordSecureString = ConvertTo-SecureString "$clientSecret" -AsPlainText -Force
$Credential = New-Object System.Management.Automation.PSCredential($clientId , $PasswordSecureString)

##Conect to Azure Account using TenantId and ServicePrincipal Credential object
Connect-AzAccount -Credential $Credential -TenantId $tenantId -ServicePrincipal

##Get Subscription details
$subscription = Select-AzSubscription -Subscription $SubscriptionName

##Set Azure context to the required Subscription
Set-AzContext -Subscription $subscription.Subscription.Name

##Get all Azure NSG details
$networksecuritygroups = Get-AzNetworkSecurityGroup

foreach ($networksecuritygroup in $networksecuritygroups)
{
    Write-Host "`r`nchecking '$($networksecuritygroup.Name)'.."
    $subnets = $networksecuritygroup.SubnetsText
    if ($subnets -eq $null -or $subnets -eq '[]')
    {
        Write-Host "'$($networksecuritygroup.Name)' is orphan"
        $NSGDetails = [ordered]@{
            SubscriptionName = $subscription.Subscription.Name
            ResourceGroup = $networksecuritygroup.ResourceGroupName
            NSG_Name = $networksecuritygroup.Name
        }
        $nsgUsage.add((New-Object psobject -Property $NSGDetails)) | Out-Null
    }
}

##Email body
$body = "Please refer to the attached document for Orphan Network Security Groups"

##Export NSG details to a csv file
$nsgUsage | Export-Csv -Path $nsgUsageOutFile -NoTypeInformation

$attachments += $nsgUsageOutFile

##Send email
Send-MailMessage -SmtpServer $smtpServer -To $to -From $from -Subject $subject -Body ($body | Out-String) -Attachments $attachments -BodyAsHtml