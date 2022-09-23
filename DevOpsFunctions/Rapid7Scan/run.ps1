####################################################################################################################################
<#
.SYNOPSIS
    Initiates Rapid7 scan and emails VM provisioning results.
.DESCRIPTION
    This script initiates the Rapid7 scan, gets the PDF report, and emails the VM provisioning summary.
#>
####################################################################################################################################

param (
    $Request,
    $TriggerMetadata
)

####################################################################################################################################

$nameOfCurrentScript = "InitiateRapid7ScanFunction.ps1"
$tempPath = [System.IO.Path]::GetTempPath()
$date = $((Get-Date).ToString('yyyyMMdd_HHmmss'))
$logFile = "$tempPath\Rapid7ScanLog_$date.txt"

if (Test-Path $logFile) {
    Remove-Item $logFile -Confirm:$false -Force
}

Function Log {
    <#
    .SYNOPSIS
        Helper log function.
    .DESCRIPTION
        Logs the supplied to both the console and a log file. The caller can optionally specify the foreground color of the console 
        for each log output.
    #>
####################################################################################################################################
	param (
        #The string to log.
		[string] $LogString,

        #The foreground color of the host to use for this log string.
		[string] $ForegroundColor=$null
	)

	$formattedString = "$(Get-Date)|$nameOfCurrentScript|$logString"
	Add-Content -Path $logFile -Value $formattedString
	if ($Foregroundcolor) { Write-Host $formattedString -ForegroundColor $Foregroundcolor } else { Write-Host $formattedString }
}


####################################################################################################################################

Function Get-Script {
    <#
    .SYNOPSIS
        Downloads script.
    .DESCRIPTION
        Downloads the specified script to the specified directory.
    #>
####################################################################################################################################
	param (
		$DownloadUrl,
		$Script,
		$BaseInstallDir,
        $OverwriteFiles = $true
	)
	
	$download = $true

	if (-not (Test-Path $BaseInstallDir)) {
	    New-Item -Path $BaseInstallDir -ItemType "directory" -Force | Out-Null
	}

    if (Test-Path "$BaseInstallDir\$Script") {
		if ($OverwriteFiles) {
        	Log "$BaseInstallDir\$Script already exists. Deleting."
			Remove-Item "$BaseInstallDir\$Script"
		}
        else {
			Log "$BaseInstallDir\$Script already downloaded. Not overwriting."
			$download = $false
		}
	}
    else {
		Log "Downloading $BaseInstallDir\$Script"
	}
	
	if ($download) {
		Log "Downloading script $DownloadUrl/$Script"
		(New-Object System.Net.WebClient).DownloadFile("$DownloadUrl/$Script", "$BaseInstallDir\$Script")
		
		if (Test-Path "$BaseInstallDir\$Script") {
			Log "Download of $DownloadUrl/$Script to $BaseInstallDir\$Script completed successfully."
		}
        else {
			$errorMessage = "Error downloading $DownloadUrl/$Script to $BaseInstallDir\$Script"
			Log $errorMessage
			throw $errorMessage
		}
	}
}


####################################################################################################################################
#Start main script
####################################################################################################################################

trap {
    Log "An exception occurred:"
    Log "$($_.Exception | Select *)"
    Log "Exiting..."

    $status = [System.Net.HttpStatusCode]::BadRequest
    $body = "An error occurred."
    Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
        StatusCode = $status
        Body = $body
    })

    Exit(1)
}


####################################################################################################################################
#Check for ip address in request
####################################################################################################################################

$ipAddress = $Request.Body.IpAddress

if (!$ipAddress) {
    $status = [System.Net.HttpStatusCode]::OK
    $body = "No ip address was provided to scan."
    Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
        StatusCode = $status
        Body = $body
    })
    exit 1
}

Log "IP Address: $ipAddress"

####################################################################################################################################
#Start Rapid7 scan and get report
####################################################################################################################################

$baseInstallDir = "$tempPath\rapid7"
Log "baseInstallDir: $baseInstallDir"
$downloadUrl = "http://download.calpine.com/devops/scripts"

$rapid7Host = "https://rapid7.calpine.com"
$rapid7ScanScriptName = 'StartRapid7Scan.ps1'
$rapid7ReportScriptName ='GetRapid7ScanReport.ps1'

$rapid7ScanScriptNameFullPath = "$baseInstallDir\$rapid7ScanScriptName"
$rapid7ReportScriptNameFullPath = "$baseInstallDir\$rapid7ReportScriptName"

if ($Request.Body.Subscription -like "CCA*") {
    $siteId = 145
}
else {
    $siteId = 148
}

Get-Script -DownloadUrl $downloadUrl -Script $rapid7ScanScriptName -BaseInstallDir $baseInstallDir
Log "Starting script: $rapid7ScanScriptName..."
$scanId = Invoke-Expression "$rapid7ScanScriptNameFullPath -IpAddressToScan $ipAddress -Rapid7Host $rapid7Host -SiteId $siteId -LogFile $logFile"
Log "Rapid7 scan started with scanId: $scanId"

if ($scanId -ne "FailedToScan") {
    Get-Script -DownloadUrl $downloadUrl -Script $rapid7ReportScriptName -BaseInstallDir $baseInstallDir
    Log "Starting script: $rapid7ReportScriptName..."
    $rapid7ReportObject = Invoke-Expression "$rapid7ReportScriptNameFullPath -IpAddressToScan $ipAddress -Rapid7Host $rapid7Host -SiteId $siteId -ScanId $scanId -ReportDir $baseInstallDir -LogFile $logFile"
    Log "Got Rapid7 report object for scanId: $scanId"
}

####################################################################################################################################
#Send email
####################################################################################################################################

$smtpServer	= "relay.calpine.com"
$to			= @("devops@calpine.com")
$from		= "devops@calpine.com"
$subject    = "Azure Provisioning Summary - $($Request.Body.ComputerName)"

$body = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($Request.Body.EmailBody))
if ($scanId -eq 'FailedToScan') {
	$body += "<br/><b>Rapid7 Scan</b>: Rapid7 scan failed. Check logs for errors. Please run manually!<br/>"
    Log "Rapid7 failed to scan. Please run manually."
}
else {
	$body += "<br/><b>Rapid7 Scan</b>: Started with scan id: $scanId<br/>"
}

$attachments = @()
$attachments += $logFile
$requestAttachments = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($Request.Body.EmailAttachments))

if (-not ($null -eq $requestAttachments -or $requestAttachments -eq "")) {
    foreach ($requestAttachment in $requestAttachments) {
        $attachment = ConvertFrom-Json $requestAttachment
        [IO.File]::WriteAllBytes("$baseInstallDir\$($attachment.FileName)", [Convert]::FromBase64String($attachment.Attachment))
        Log "Adding attachment $baseInstallDir\$($attachment.FileName)"
        $attachments += "$baseInstallDir\$($attachment.FileName)"
    }
}

if ($null -ne $rapid7ReportObject) {
    foreach ($object in $rapid7ReportObject) {
        $fullPath = $object.FullPath
        $body += "&nbsp;&nbsp;&nbsp;&nbsp;$fullPath<br/>"
        $body += "&nbsp;&nbsp;&nbsp;&nbsp;Critical vulnerbilities: $($object.critical)<br/>"
        $body += "&nbsp;&nbsp;&nbsp;&nbsp;Severe vulnerbilities: $($object.severe)<br/>"
        $body += "&nbsp;&nbsp;&nbsp;&nbsp;Total vulnerbilities: $($object.total)<br/>"
        Log "Adding attachment $fullPath"
        $attachments += $fullPath
    }
}

Log "Email subject: $subject"
Log "Email body: $body"
Send-MailMessage -SmtpServer $smtpServer -To $to -From $from -Subject $subject -Body $body -Attachments $attachments –BodyAsHtml
Log "Sent email to $to."

####################################################################################################################################
#Remove temp direcories
####################################################################################################################################

if (Test-Path $rapid7ReportScriptNameFullPath) {
    Remove-Item $rapid7ReportScriptNameFullPath -Confirm:$false -Force
}

if (Test-Path $rapid7ScanScriptNameFullPath) {
    Remove-Item $rapid7ScanScriptNameFullPath -Confirm:$false -Force
}

foreach ($object in $rapid7ReportObject) {
    $fullPath = $object.FullPath

    if (Test-Path $fullPath) {
        Remove-Item $FullPath -Confirm:$false -Force
    }
}

####################################################################################################################################

Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
    StatusCode = [System.Net.HttpStatusCode]::OK
    Body = "Finished"
})


####################################################################################################################################
#End main script
####################################################################################################################################