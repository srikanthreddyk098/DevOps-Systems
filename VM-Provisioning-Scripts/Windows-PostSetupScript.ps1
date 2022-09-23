####################################################################################################################################
<#
.SYNOPSIS
    Runs post setup commands.
.DESCRIPTION
    This script adds a local administrator group if requested, builds the body of the VM provisioning email with the status of the
    the security agents, and calls an Azure function to start the Rapid7 scan and email the results of the VM provisioning.
#>
####################################################################################################################################

param (    
    #The Azure subscription the VM is deployed to.
    [Parameter(Mandatory=$true)]
    [string] $Subscription,
    
    #Domain group to add to the VM local administrators.
    $AdministratorGroupToAdd = "None",
    
    #The base URL to download URL of the agent zip.
    $BaseUrl = 'http://download.calpine.com'
)

$nameOfCurrentScript = "Windows-PostSetupScript.ps1"
$tempDir = "C:\Windows\Temp"
$logFile = "$tempDir\BuildLog_$($env:COMPUTERNAME).txt"

####################################################################################################################################
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
	if($Foregroundcolor) { Write-Host $formattedString -ForegroundColor $Foregroundcolor } else { Write-Host $formattedString }
}


####################################################################################################################################
Function Get-ServiceStatus {
    <#
    .SYNOPSIS
        Gets status of a service.
    .DESCRIPTION
        Gets the status of a service and if stopped, tries to start the service up to three times.
    #>
####################################################################################################################################
	param (
		$ServiceName,
        $NumberOfAttempts = 1
	)

	$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

    if ($service) {
        $status = $service.Status
		if ($status -eq 'Stopped') {
	        try {
		        Log "Service '$ServiceName' is stopped. Attempting to start (attempt $NumberOfAttempts)."
		        Start-Service -Name $ServiceName
                Sleep 120

                if ($NumberOfAttempts -lt 3) {
		            return Get-ServiceStatus -ServiceName $ServiceName -NumberOfAttempts $($NumberOfAttempts + 1)
                }
	        } 
            catch {
                Log "Error occurred starting service '$ServiceName' (attempt $($NumberOfAttempts + 1))."
                Log "$($_.Exception | ConvertTo-Json)"
	        }
		}

        Log "Service '$ServiceName' has status of: $status"
        return "Service '$ServiceName' has status of: $status"
    }
    else {
        return "Service '$ServiceName' was not found."
    }
}

####################################################################################################################################
#Start main script
####################################################################################################################################

trap {
    Log "An exception occurred:"
    Log "$($_.Exception | Select *)"
    Log "Exiting..."
    Exit(1)
}

#Add AD group to local administrators
if ($AdministratorGroupToAdd -ne "None" -and $env:USERDOMAIN -ne $env:COMPUTERNAME) {
    net localgroup administrators $env:USERDOMAIN\$AdministratorGroupToAdd /add  
    $localAdminGroupAdded = "$env:USERDOMAIN\$AdministratorGroupToAdd"
}

#Get ip address
foreach ($config in (Get-WmiObject Win32_NetworkAdapterConfiguration)) {
    $ipAddresses = $config.IPAddress

    if ($ipAddresses) {
        foreach ($ip in $ipAddresses) {
            if ($ip.contains(".")) {
                $ipAddress = $ip
                break
            }
        }
    }
}

####################################################################################################################################
#Build email body
####################################################################################################################################

$body = "<b>Computer</b>: $env:COMPUTERNAME<br/>"
$body += "<b>Operating System</b>: $((Get-WmiObject -Class Win32_OperatingSystem -Namespace root/cimv2).Caption)<br/>"
$body += "<b>IP Address</b>: $ipAddress<br/>"
$body += "<br/><b>Agent Services</b>:<br/>"
$body += "&nbsp;&nbsp;&nbsp;&nbsp;$(Get-ServiceStatus -ServiceName 'EracentEPAService')<br/>"
$body += "&nbsp;&nbsp;&nbsp;&nbsp;$(Get-ServiceStatus -ServiceName 'EracentEUAService')<br/>"
$body += "&nbsp;&nbsp;&nbsp;&nbsp;$(Get-ServiceStatus -ServiceName 'SplunkForwarder')<br/>"
$body += "&nbsp;&nbsp;&nbsp;&nbsp;$(Get-ServiceStatus -ServiceName 'Tanium Client')<br/>"
#$body += "&nbsp;&nbsp;&nbsp;&nbsp;$(Get-ServiceStatus -ServiceName 'CarbonBlack')<br/>"
$body += "&nbsp;&nbsp;&nbsp;&nbsp;$(Get-ServiceStatus -ServiceName 'ir_agent')<br/>"

if ($env:USERDOMAIN -eq $env:COMPUTERNAME) {
    $body += "&nbsp;&nbsp;&nbsp;&nbsp;Service 'CcmExec' has status of: N/A. Domain is $env:USERDOMAIN.<br/>"
}
else {
    $body += "&nbsp;&nbsp;&nbsp;&nbsp;$(Get-ServiceStatus -ServiceName 'CcmExec')<br/>"
}

switch((Get-WmiObject -Class Win32_OperatingSystem -Namespace root/cimv2).BuildNumber){
    "14393" {}
    "17763" {}
    default {
	    $body += "&nbsp;&nbsp;&nbsp;&nbsp;MsMpSvc - $(Get-ServiceStatus -ServiceName 'MsMpSvc')<br/>"
    }
}

$body += "<br/><b>Local Admin Group Added</b>: $localAdminGroupAdded</br>"

####################################################################################################################################
#Send request to Azure function to start rapid 7 scan
####################################################################################################################################

$attachments = 
    @(
        @{
            FileName = "BuildLog_$($env:COMPUTERNAME).txt";
            Attachment = [Convert]::ToBase64String([IO.File]::ReadAllBytes($logFile))
        }
    ) | ConvertTo-Json

$postBody = @{
    Subscription = $Subscription
    ComputerName = $env:COMPUTERNAME
    IpAddress = $ipAddress
    EmailBody = [Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes($body))
    EmailAttachments = [Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes($attachments))
} | ConvertTo-Json

$rapid7FunctionUrl = "https://devopspowershellfunctions.app.calpine.com/api/Rapid7Scan?code=Q3XK1Hrz8tffySegoGAPEMUoC96dS48nChkVGXeutvTBIQVqbP9TJw=="

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Log "Sending http request to $rapid7FunctionUrl"
try {
    if($Subscription -like "CCA*") {
        Add-Type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
        [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    }
    
    Invoke-WebRequest -Uri $rapid7FunctionUrl -Method POST -Body $postBody -ContentType "application/json"
}
catch {
    Log "$($_.Exception.Message)"
}
Log "Completed http request to $rapid7FunctionUrl"

####################################################################################################################################
#Remove old scheduled task and temp direcories
####################################################################################################################################

schtasks /delete /tn $nameOfCurrentScript /f

if (Test-Path "$tempDir\Windows-SetupScript.ps1") {
    Remove-Item "$tempDir\Windows-SetupScript.ps1" -Confirm:$false -Force
}

if (Test-Path "$tempDir\$nameOfCurrentScript") {
    Remove-Item "$tempDir\$nameOfCurrentScript" -Confirm:$false -Force
    Log "Removed $tempDir\$nameOfCurrentScript"
}

Log "Finished processing $nameOfCurrentScript"

####################################################################################################################################
#End main script
####################################################################################################################################