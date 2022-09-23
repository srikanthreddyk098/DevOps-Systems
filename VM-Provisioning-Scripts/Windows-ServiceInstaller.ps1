####################################################################################################################################
<#
.SYNOPSIS
    Downloads and installs the service specified.
.DESCRIPTION
    Downloads and installs the service specified using the 
#>
####################################################################################################################################

param (
    [Parameter(Mandatory = $true)]
    #The name of the service to install.
	[string] $ServiceName,

    [Parameter(Mandatory = $true)]
    #The base URL to download URL of the agent zip.
    [string] $BaseUrl = "http://download.calpine.com",

    [Parameter(Mandatory = $true)]
    #The name of the file to download.
	[string] $DownloadFile,

    [Parameter(Mandatory = $true)]
    #The base path to install the agent.
	[string] $BaseInstallDir,

    #The main directory for the agent install.
    [string] $AgentInstallDir = $DownloadFile.Split(".")[0],

    [Parameter(Mandatory = $true)]
    #The name of the executable to run.
	[string] $ExeToInstall,

    #Arguments to pass to the executable.
	[string] $ExeArguments = $null
)


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

    $logFile = "C:\Windows\Temp\BuildLog_$($env:COMPUTERNAME).txt"
	$formattedString = "$(Get-Date)|$($MyInvocation.ScriptName)|$logString"
	Add-Content -Path $logFile -Value $formattedString
	if ($Foregroundcolor) { Write-Host $formattedString -ForegroundColor $Foregroundcolor } else { Write-Host $formattedString }
}


####################################################################################################################################
#Start main script
####################################################################################################################################

trap {
    Log "An exception occurred:"
    Log "$($_.Exception | Select *)"
    Exit(1)
}

Log -Logstring "Checking if service '$ServiceName' already exists..."
$service = Get-Service -Name $ServiceName -ErrorAction 'silentlycontinue'

if ($service -ne $null) {
    Log -Logstring "Service '$ServiceName' is already installed. Exiting..."
    Exit
}

Log "Service '$ServiceName' has not been installed yet."


####################################################################################################################################
#Download and extract agent files.
####################################################################################################################################

Log "Downloading $DownloadFile..."

$installPath = "$BaseInstallDir\$AgentInstallDir"
New-Item -Path $installPath -ItemType Directory -Force | Out-Null

$isZip = $DownloadFile -like "*.zip"

if ($isZip) {
    $downloadPath = "$BaseInstallDir\$DownloadFile"
}
else {
    $downloadPath = "$installPath\$DownloadFile"
}

if (Test-Path "$downloadPath") {
    Log "$downloadPath already exists. Skipping download..."
}
else {
    $downloadSource = "$BaseUrl/$DownloadFile"
    Log "Starting download of $downloadSource to $downloadPath"
	(New-Object System.Net.WebClient).DownloadFile($downloadSource, "$downloadPath")

    if (Test-Path "$downloadPath") {
        Log "Download completed successfully."
    }
    else {
        $errorString = "Error downloading $downloadSource to $downloadPath"
		Log $errorString
        throw $errorString
    }
}

if ($isZip) {
    if (Test-Path "$installPath") {
        Log -Logstring "Unzip directory, $installPath, already exists. Removing before unzipping."
        Remove-Item $installPath -Recurse -Confirm:$False
    }
    
    Log "Creating unzip directory $installPath"
    New-Item -Path $installPath -ItemType Directory -Force | Out-Null
    
    #Add .NET Framework class
    Add-Type -Assembly "System.IO.Compression.FileSystem"
    
    #Extract zip file
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$BaseInstallDir\$DownloadFile", $installPath)
    
    Log "Finished downloading and unzipping $DownloadFile"
}
else {
    Log "Finished downloading $DownloadFile"
}


####################################################################################################################################
#Install agent
####################################################################################################################################

if(-not $ExeToInstall.contains("msiexec")) {
    $ExeToInstall = "$installPath\$ExeToInstall"
}
    
if($ExeArguments) {
	$ExeArguments = $ExeArguments.replace("BaseDir",$BaseInstallDir)
	$ExeArguments = $ExeArguments.replace("AgentDir",$AgentInstallDir)

    Log "Installing $ServiceName with command: $ExeToInstall $ExeArguments"
    Start-Process -FilePath $ExeToInstall -ArgumentList $ExeArguments
}
else {
    Log "Installing $ServiceName with command $ExeToInstall and no arguments."
    Start-Process -FilePath $ExeToInstall
}

$maxWait = 300
$waitDelay = 5
$currentWaitTime=0

$foundService = $false
$maxWaitTimeReached = $false
while (-not $foundService -and -not $maxWaitTimeReached) {
    $services = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

	if($services){
        $foundService = $true
	}
    else {
        if ($currentWaitTime -ge $maxWait) {
            $maxWaitTimeReached=$true
            Log "INSTALLATION FAILED - Reached max wait time of $maxWait seconds for service '$ServiceName' to appear."
            exit
        }
        else {
            $currentWaitTime = $currentWaitTime + $waitDelay
            
            Log "Waiting $waitDelay seconds for service '$ServiceName' to appear..."
            Sleep $waitDelay
        }
    }
}

if ($foundService) {
    Log "Finished successful installation of service '$ServiceName'."
}


####################################################################################################################################
#End main script
####################################################################################################################################