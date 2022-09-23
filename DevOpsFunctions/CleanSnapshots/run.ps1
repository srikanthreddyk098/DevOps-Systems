# Input bindings are passed in via param block.
param($Timer)

# Get the current universal time in the default string format
$currentUTCtime = (Get-Date).ToUniversalTime()

# The 'IsPastDue' porperty is 'true' when the current function invocation is later than scheduled.
if ($Timer.IsPastDue) {
    Write-Host "PowerShell timer is running late!"
}

# Write an information log with the current time.
Write-Host "PowerShell timer trigger function running at UTC time: $currentUTCtime"

$tokenRequestParams = @{
    Method = "Post"
    Uri = "https://login.microsoftonline.com/$env:tenantId/oauth2/token"
    Body = @{
        grant_type = "client_credentials"
        resource = "https://management.azure.com"
        client_id = $env:clientId
        client_secret = $env:clientSecret
    }
}

$tokenResponse = Invoke-RestMethod @tokenRequestParams
$token = $tokenResponse.access_token

$snapshotsDeleted = @()
$snapshotsWithErrors = @()
$snapshotsToDeleteTomorrow = @()
$snapshotsMissingDateToDeleteTag = @()
$snapshotsWithInvalidDateToDeleteTag = @()
$tagName = "DateToDelete"

function GetCustomSnapshotObject($snapshot) {
    return [PSCustomObject]@{
        Subscription = ($snapshot.id -split "/")[2]
        ResourceGroup = ($snapshot.id -split "/")[4]
        Snapshot = $snapshot.name
    }
}

################################################################################################################################################################
#Get subscriptions
################################################################################################################################################################
$subscriptionsRequestParams = @{
    Method= "Get"
    Uri = "https://management.azure.com/subscriptions?api-version=2020-01-01"
    Headers = @{ "Authorization" = "Bearer $token" }
}

$subscriptions = Invoke-RestMethod @subscriptionsRequestParams

################################################################################################################################################################
foreach($subscription in $subscriptions.value | where { $_.state -eq "Enabled" } | sort displayName) {
    echo $subscription.displayName
    $snapshotRequestParams = @{
        Method= "Get"
        Uri = "https://management.azure.com/subscriptions/$($subscription.subscriptionId)/providers/Microsoft.Compute/snapshots?api-version=2020-06-30"
        Headers = @{ "Authorization" = "Bearer $token" }
    }
################################################################################################################################################################
#Get snapshots for subscription
################################################################################################################################################################
    $snapshots = $null 
    $snapshots = Invoke-RestMethod @snapshotRequestParams

    if ($snapshots.value.Count-eq 0) { continue }

    foreach ($snapshot in $snapshots.value) {
        #skip snapshot if it does not have the expected tag
        if (!$snapshot.tags.$tagName) {
            $snapshotsMissingDateToDeleteTag += GetCustomSnapshotObject($snapshot)
            continue
        }

        $dateToDelete = New-Object DateTime
        
        #skip snapshot if the date tag cannot be parsed
        if (![DateTime]::TryParse($snapshot.tags.$tagName, [ref] $dateToDelete)) {
            $snapshotsWithInvalidDateToDeleteTag += GetCustomSnapshotObject($snapshot)
            continue
        }

        #check if snapshot should be deleted
        if ($dateToDelete.Date -le (Get-Date).Date) {
            try {
                if ($env:environment -eq "prod") {
                    $subscriptionId = ($snapshot.id -split "/")[2]
                    $resourceGroup = ($snapshot.id -split "/")[4]
                    $snapshotDeleteRequestParams = @{
                        Method= "Delete"
                        Uri = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Compute/snapshots/$($snapshot.name)?api-version=2020-06-30"
                        Headers = @{ "Authorization" = "Bearer $token" }
                    }

                    $snapshotDeleteResponse = Invoke-RestMethod @snapshotDeleteRequestParams

                    Start-Sleep 10

                    #check that snapshot was deleted
                    $snapshotCheckRequestParams = @{
                        Method= "Get"
                        Uri = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Compute/snapshots/$($snapshot.name)?api-version=2020-06-30"
                        Headers = @{ "Authorization" = "Bearer $token" }
                    }

                    try {
                        $snapshotDeleteCheck = Invoke-RestMethod @snapshotCheckRequestParams
                        if ($null -eq $snapshotCheck) {
                            $snapshotsDeleted += GetCustomSnapshotObject($snapshot)
                        }
                        else {
                            Write-Error "Could not verify that the following snapshot was deleted successfully: $($snapshot.id)"
                            $snapshotsWithErrors += GetCustomSnapshotObject($snapshot)
                        }
                    }
                    catch {
                        if ($_.Exception.Response.StatusCode -eq "NotFound") {
                            $snapshotsDeleted += GetCustomSnapshotObject($snapshot)
                        }
                        else {
                            Write-Error "An unexpected exception occurred checking if $($snapshot.id) was deleted."
                            Write-Error $_.Exception | Format-List -Force
                            $snapshotsWithErrors += GetCustomSnapshotObject($snapshot)
                        }
                    }
                }
                else {
                    $snapshotsDeleted += GetCustomSnapshotObject($snapshot)
                }
            }
            catch [Exception] {
                Write-Error "An exception occurred deleting snapshot: $($snapshot.id)"
                Write-Error $_.Exception | Format-List -Force
                $snapshotsWithErrors += GetCustomSnapshotObject($snapshot)
            }
        }                
        #save snapshot if it is to be deleted tomorrow
        elseif ($dateToDelete.Date -eq (Get-Date).AddDays(1).Date) {
            $snapshotsToDeleteTomorrow += GetCustomSnapshotObject($snapshot)
        }
    }
}

################################################################################################################################################################
#Send email
################################################################################################################################################################

$smtpServer	= "relay.calpine.com"
$from = "devops@calpine.com"

if ($env:environment -eq "prod") {
    $to	= @("devops@calpine.com")
}
else {
    $to = @("adam.gomes@calpine.com")
}

$subject = "Daily Snapshot Cleanup Summary - $((Get-Date).ToString("MM/dd/yy"))"
$body = ""

function GenerateEmailBodyStringFromSnapshotArray($snapshotArray) {
    $str = ""
    if ($snapshotArray.Count -gt 0) {
        foreach ($snapshot in $snapshotArray | Sort-Object Snapshot) {
            $str += "&nbsp;&nbsp;&nbsp;&nbsp;$($snapshot.Snapshot)<br/>"
        }
    }
    else {
        $str += "&nbsp;&nbsp;&nbsp;&nbsp;None<br/>"
    }
    return $str
}

$body += "<b>Snapshots deleted today:</b><br/>"
$body += GenerateEmailBodyStringFromSnapshotArray($snapshotsDeleted)
$body += "<br/>"
$body += "<b>Snapshots that failed to delete:</b><br/>"
$body += GenerateEmailBodyStringFromSnapshotArray($snapshotsWithErrors)
$body += "<br/>"
$body += "<b>Snapshots that will be deleted tomorrow:</b><br/>"
$body += GenerateEmailBodyStringFromSnapshotArray($snapshotsToDeleteTomorrow)
$body += "<br/>"
$body += "<b>Snapshots missing a date to delete tag:</b><br/>"
$body += GenerateEmailBodyStringFromSnapshotArray($snapshotsMissingDateToDeleteTag)
$body += "<br/>"
$body += "<b>Snapshots with an invalid date to delete tag:</b><br/>"
$body += GenerateEmailBodyStringFromSnapshotArray($snapshotsWithInvalidDateToDeleteTag)

Send-MailMessage -SmtpServer $smtpServer -To $to -From $from -Subject $subject -Body $body -BodyAsHtml