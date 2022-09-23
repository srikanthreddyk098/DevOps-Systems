####################################################################################################################################
<#
.SYNOPSIS
    Installs security agents.
.DESCRIPTION
    This script installs the necessary Windows security agents and schedules the Windows post-setup script to be run after a reboot.
#>
####################################################################################################################################

param (
    #The Azure subscription the VM is deployed to.
    [Parameter(Mandatory=$true)]
    [string] $Subscription,

    #The Azure subscription the VM is deployed to.
    [Parameter(Mandatory=$true)]
    [string] $Timezone,
    
    #Domain the VM will be added to. 
    [string] $Domain = "WORKGROUP",
    
    #Domain group to add to the VM local administrators.
    [string] $AdministratorGroupToAdd = "None",
    
    #The base URL to download URL of the agent zip.
    [string] $BaseUrl = "http://download.calpine.com",

    #Switch to indicate that the patch script should be executed.
    [bool] $PatchNow = $true,

    #Switch to indicate that this script was run manually.
    [switch] $ManualRun
)

$nameOfCurrentScript = "Windows-SetupScript.ps1"
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
        $OverwritePs1Files = $true
	)
	
	$download = $true

    if (Test-Path "$BaseInstallDir\$Script") {
		if ($OverwritePs1Files) {
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
	
	if($download = $true) {
		Log "Downloading script $DownloadUrl/$Script"
		(New-Object System.Net.WebClient).DownloadFile("$DownloadUrl/$Script", "$BaseInstallDir\$Script")
		
		if (Test-Path "$baseInstallDir\$Script") {
			Log "Download of $DownloadUrl/$Script to $baseInstallDir\$Script completed successfully."
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
    Exit(1)
}

#Set timezone
if ($Timezone -eq "PST") {
    tzutil /s "Pacific Standard Time"
}
elseif ($Timezone -eq "EST") {
    tzutil /s "Eastern Standard Time"
}
else {
    tzutil /s "Central Standard Time"
}

$downloadUrl = $BaseUrl + "/devops/scripts"
$baseInstallDir = "D:\~Install"
New-Item -Path $baseInstallDir -ItemType "directory" -Force | Out-Null

$serviceInstallerScript = "Windows-ServiceInstaller.ps1"
Get-Script -DownloadUrl "$downloadUrl" -Script "$serviceInstallerScript" -BaseInstallDir "$baseInstallDir"

$patchScript = "miscupdates.ps1"
Get-Script -DownloadUrl "$BaseUrl/dchosting/scripts" -Script "$patchScript" -BaseInstallDir "$baseInstallDir"


$computername = $env:COMPUTERNAME
$userDomainName = $env:USERDOMAIN

if ($userDomainName -eq $env:COMPUTERNAME) {
    $isAttachedToDomain = $false
}
else {
    $isAttachedToDomain = $true
}


####################################################################################################################################
#EracentEPAService
####################################################################################################################################

$serviceName		= 'EracentEPAService'
$agentUrl 			= "$BaseUrl" + "/DevOps/SecurityAgents/Eracent/Windows"
$downloadFile		= 'eracent-windows-clientsInstaller_1_EPA_EUA.msi'
$exeToInstall   	= 'msiexec.exe'
$exeArguments   	= "/i `"BaseDir\AgentDir\$downloadFile`"" + " /qn"

$argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$serviceInstallerScript -ServiceName '$serviceName' -BaseUrl '$agentUrl' -DownloadFile '$downloadFile' -BaseInstallDir '$baseInstallDir' -ExeToInstall '$exeToInstall' -ExeArguments '$exeArguments'"
Log "Calling $serviceInstallerScript with command:"
Log "$argumentList"
Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow

####################################################################################################################################
#SCCM
####################################################################################################################################

$serviceName		= 'CcmExec'
$agentUrl 			= "$BaseUrl" + "/DevOps/SecurityAgents/SCCM/Windows"
$downloadFile		= 'sccm-windows.zip'
$exeToInstall		= 'ccmsetup.exe'

if ($isAttachedToDomain) {
    Log "Is attached to domain $userDomainName - SCCM Installer"
	$argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$serviceInstallerScript -ServiceName '$serviceName' -BaseUrl '$agentUrl' -DownloadFile '$downloadFile' -BaseInstallDir '$baseInstallDir' -ExeToInstall '$exeToInstall'"
    Log "Calling $serviceInstallerScript with command:"
    Log "$argumentList"
    Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow
}
else {
    Log "Not attached to domain. Skipping install of SCCM."
}

####################################################################################################################################
#SCEP
####################################################################################################################################

$serviceName		= 'MsMpSvc'
$agentUrl 			= "$BaseUrl" + "/DevOps/SecurityAgents/SCCM/Windows"
$downloadFile		= 'sccm-windows.zip'
$exeToInstall		= 'scepinstall.exe'
$exeArguments   	= '/s /q /NoSigsUpdateAtInitialEXP /policy BaseDir\AgentDir\ep_defaultpolicy.xml'

$buildNumber = (Get-WmiObject -Class Win32_OperatingSystem -Namespace root/cimv2).BuildNumber

switch($buildNumber){
	"14393" {
		Log "Skipping SCEP install due to unsupported OS buildNumber: $buildNumber"
	}
	"17763" {
		Log "Skipping SCEP install due to unsupported OS buildNumber: $buildNumber"
	}
	default{
        if ($isAttachedToDomain) {
            $argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$serviceInstallerScript -ServiceName '$serviceName' -BaseUrl '$agentUrl' -DownloadFile '$downloadFile' -BaseInstallDir '$baseInstallDir' -ExeToInstall '$exeToInstall' -ExeArguments '$exeArguments'"
            Log "Calling $serviceInstallerScript with command:"
            Log "$argumentList"
			Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow
        }
        else {
            Log "Not attached to domain. Skipping install of SCEP."
        }
	}
}

####################################################################################################################################
#Splunk Forwarder
####################################################################################################################################

$serviceName		= 'SplunkForwarder'
$agentUrl			= "$BaseUrl" + "/DevOps/SecurityAgents/Splunk/Windows"
$downloadFile		= 'splunkforwarder-windows-x64.msi'
$exeToInstall		= 'msiexec.exe'
$exeArguments   	= '/i "BaseDir\AgentDir\splunkforwarder-windows-x64.msi" DEPLOYMENT_SERVER="10.221.0.135:8089" AGREETOLICENSE=Yes /quiet'

$argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$serviceInstallerScript -ServiceName '$serviceName' -BaseUrl '$agentUrl' -DownloadFile '$downloadFile' -BaseInstallDir '$baseInstallDir' -ExeToInstall '$exeToInstall' -ExeArguments '$exeArguments'"
Log "Calling $serviceInstallerScript with command:"
Log "$argumentList"
Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow

####################################################################################################################################
#Tanium Client
####################################################################################################################################

$serviceName		= 'Tanium Client'
$agentUrl			= "$BaseUrl" + "/DevOps/SecurityAgents/Tanium/Windows"
$downloadFile		= 'tanium-windows.exe'
$exeToInstall		= 'tanium-windows.exe'

$argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$serviceInstallerScript -ServiceName '$serviceName' -BaseUrl '$agentUrl' -DownloadFile '$downloadFile' -BaseInstallDir '$baseInstallDir' -ExeToInstall '$exeToInstall'"
Log "Calling $serviceInstallerScript with command:"
Log "$argumentList"
Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow

####################################################################################################################################
#Rapid7
####################################################################################################################################

$tempRegKey = "HKLM:\Software\Policies\Microsoft\Windows\Installer"
if (-Not(Test-Path “$tempRegKey”)) {
    New-Item -Path $tempRegKey -Force
}
Set-ItemProperty -Path $tempRegKey -Name "DisableMSI" -Type Dword -Value 0

$serviceName		= 'ir_agent'
$agentUrl			= "$BaseUrl" + "/DevOps/SecurityAgents/Rapid7/Windows"
$downloadFile		= 'rapid7-windows-x86_64.msi'
$exeToInstall		= 'msiexec.exe'
$exeArguments   	= '/i "BaseDir\AgentDir\rapid7-windows-x86_64.msi" /l*v ' + $tempDir + '\insight_agent_install_log.log /quiet CUSTOMTOKEN=us:30fc5a4b-de8e-4b6e-b46a-3a979b7781e8'

$argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$serviceInstallerScript -ServiceName '$serviceName' -BaseUrl '$agentUrl' -DownloadFile '$downloadFile' -BaseInstallDir '$baseInstallDir' -ExeToInstall '$exeToInstall' -ExeArguments '$exeArguments'"
Log "Calling $serviceInstallerScript with command:"
Log "$argumentList"
Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow

Remove-Item -Path $tempRegKey

####################################################################################################################################
#Carbon Black Response
####################################################################################################################################

#$serviceName		= 'CarbonBlack'
#$agentUrl	    	= "$BaseUrl" + "/DevOps/SecurityAgents/CarbonBlackResponse/Windows"
#$downloadFile		= 'carbon-black-response-windows.zip'
#$exeToInstall		= 'msiexec.exe'
#$exeArguments   	= '/i "BaseDir\AgentDir\cbsetup.msi" /qn /L*V BaseDir\AgentDir\msi.log'
#
#$argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$serviceInstallerScript -ServiceName '$serviceName' -BaseUrl '$agentUrl' -DownloadFile '$downloadFile' -BaseInstallDir '$baseInstallDir' -ExeToInstall '$exeToInstall' -ExeArguments '$exeArguments'"
#Log "Calling $serviceInstallerScript with command:"
#Log "$argumentList"
#Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow


####################################################################################################################################
#Run patching and Windows activation script
####################################################################################################################################

if ($PatchNow){
    $argumentList = "-ExecutionPolicy Bypass $baseInstallDir\$patchScript -BaseHttp $BaseUrl -BaseDir $baseInstallDir"
    Log "Calling $patchScript with command:"
    Log "$argumentList"
	Start-Process -FilePath "powershell.exe" -ArgumentList $argumentList -Wait -NoNewWindow
}
else {
	Log "Skipping patch update because PatchNow variable is set to false."
}


####################################################################################################################################
#Schedule task to run post reboot script
####################################################################################################################################

$postSetupScript = 'Windows-PostSetupScript.ps1'
$downloadSource = $downloadUrl + "/" + $postSetupScript
Invoke-WebRequest $downloadSource -OutFile "$tempDir\$postSetupScript"

$arguments = "-ExecutionPolicy Bypass -NoProfile -WindowStyle Hidden -File `'$tempDir\$postSetupScript`' -Subscription `'$Subscription`' -AdministratorGroupToAdd `'$AdministratorGroupToAdd`' -BaseUrl `'$BaseUrl`'"

if ($ManualRun) {
    Log "Running: $postSetupScript $arguments"
    Start-Process -FilePath "powershell.exe" -ArgumentList $arguments -Wait -NoNewWindow
}
else {
    schtasks.exe /create /f /tn $postSetupScript /ru SYSTEM /sc ONSTART /tr "powershell.exe $arguments" /rl highest
    Log "Created schedule to run the following command at next startup:"
    Log "$postSetupScript $arguments"
}


####################################################################################################################################
#Remove old scheduled task and temp direcories
####################################################################################################################################

schtasks /delete /tn $nameOfCurrentScript /f

if (Test-Path "$tempDir\$nameOfCurrentScript") {
    Remove-Item "$tempDir\$nameOfCurrentScript" -Confirm:$false -Force
}

if (Test-Path $baseInstallDir) {
    Remove-Item $baseInstallDir -Confirm:$false -Force -Recurse
}


####################################################################################################################################
#Reboot VM
####################################################################################################################################

if (-not $ManualRun) {
    Restart-Computer -Force
}


####################################################################################################################################
#End main script
####################################################################################################################################