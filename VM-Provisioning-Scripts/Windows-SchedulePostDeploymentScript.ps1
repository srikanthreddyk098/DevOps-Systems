####################################################################################################################################
<#
.SYNOPSIS
    Downloads and schedules a task to run the specified script.
.DESCRIPTION
    This script creates a scheduled task to run the Windows setup script after the machine is rebooted due to joining to the AD.
#>
####################################################################################################################################

param (
    #The Azure subscription the VM is deployed to.
    [Parameter(Mandatory=$true)]
    [string] $Subscription,

    #The Azure subscription the VM is deployed to.
    [Parameter(Mandatory=$true)]
    [string] $Timezone,
    
    #AD group to add to the VM local administrators.
    [string] $AdministratorGroupToAdd = "None",

    #Domain the VM will be added to. 
    [string] $Domain ="WORKGROUP",
    
    #The base URL to download URL of the agent zip.
    [string] $BaseUrl = "http://download.calpine.com",

    #The name of the script to dowload and create a scheduled task for.
    [string] $ScriptToRun = "Windows-SetupScript.ps1"
)

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
		[string]$LogString,

        #The foreground color of the host to use for this log string.
		[string]$ForegroundColor=$null
	)

    $logFile = "$tempDir\BuildLog_$($env:COMPUTERNAME).txt"
	$formattedString = "$(Get-Date)|$($MyInvocation.ScriptName)|$logString"
	Add-Content -Path $logFile -Value $formattedString
	if ($Foregroundcolor) { Write-Host $formattedString -ForegroundColor $Foregroundcolor } else { Write-Host $formattedString }
}


####################################################################################################################################
#Start main script
####################################################################################################################################

try {    
    $downloadSource = "$BaseUrl/devops/scripts/$ScriptToRun"
    (New-Object System.Net.WebClient).DownloadFile("$downloadSource", "$tempDir\$ScriptToRun")

    $arguments = "-ExecutionPolicy Bypass -NoProfile -WindowStyle Hidden -File `'$tempDir\$ScriptToRun`' -Subscription `'$Subscription`' -Timezone `'$Timezone`' -AdministratorGroupToAdd `'$AdministratorGroupToAdd`' -Domain `'$Domain`' -BaseUrl `'$BaseUrl`'"

    schtasks.exe /create /f /tn $ScriptToRun /ru SYSTEM /sc ONSTART /tr "powershell.exe $arguments" /rl highest
    Log "Created schedule to run the following command at next startup:"
    Log "$ScriptToRun $arguments"
}
catch {
    Log "An exception occurred:"
    Log "$($_.Exception | Select *)"
}


####################################################################################################################################
#End main script
####################################################################################################################################