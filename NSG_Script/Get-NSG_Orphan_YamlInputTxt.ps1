Clear-Host

##Email input variables
$smtpServer = "relay.calpine.com"
$to = @("srikanth.kandala@calpine.com")
$from = "srikanth.kandala@calpine.com"
$subject = "Orphan Network Security Groups - $((Get-Date).ToString("02/28/2022"))"
$attachments = @()

##nsg details output file path 
$nsgUsageOutFile = 'C:\Temp\NetworkSecurityGroups.csv'

##declaring Output variables
[System.Collections.ArrayList]$nsgUsage = New-Object -TypeName System.Collections.ArrayList

$AzureInputFiePath = 'C:\temp\AzureDetails.txt'
$SubscriptionNames = Get-Content -Path $AzureInputFiePath | Sort-Object -Unique

<#
##Azure Tenant ID
$tenantId = '7406f7f1-ef6e-49f3-a9c0-002b8bc12056'
##Service Principal Object ID
$clientId = ''
##Service Principal secret\password
$clientSecret = ''

##Create Credential object for service Principal
$PasswordSecureString = ConvertTo-SecureString "$clientSecret" -AsPlainText -Force
$Credential = New-Object System.Management.Automation.PSCredential($clientId , $PasswordSecureString)

##Conect to Azure Account using TenantId and ServicePrincipal Credential object
Connect-AzAccount -Credential $Credential -TenantId $tenantId -ServicePrincipal
#>
$tenantId = '7406f7f1-ef6e-49f3-a9c0-002b8bc12056'
Connect-AzAccount -TenantId $tenantId

$TenantSubscriptions = Get-AzSubscription

Foreach($SubscriptionName in $SubscriptionNames)
{
    $subscriptions = $TenantSubscriptions | Where-Object {$_.Name -eq $SubscriptionName}
    
    Foreach($subscription in $subscriptions)
    {
        Write-Host "`r`nProcessing subscription '$($subscription.name)' with id '$($subscription.Id)'"

        ##Set Azure context to the required Subscription
        $AzContext = Set-AzContext -Subscription $subscription.Id

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
                    SubscriptionName = $subscription.Name
                    ResourceGroup = $networksecuritygroup.ResourceGroupName
                    NSG_Name = $networksecuritygroup.Name
                }
                $nsgUsage.add((New-Object psobject -Property $NSGDetails)) | Out-Null
            }
        }
    }
}

##Email body
$body = "Please refer to the attached document for Orphan Network Security Groups"

##Export NSG details to a csv file
$nsgUsage | Export-Csv -Path $nsgUsageOutFile -NoTypeInformation

$attachments += $nsgUsageOutFile

##Send email
Send-MailMessage -SmtpServer $smtpServer -To $to -From $from -Subject $subject -Body ($body | Out-String) -Attachments $attachments -BodyAsHtml
