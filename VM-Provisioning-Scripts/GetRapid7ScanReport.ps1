####################################################################################################################################
<#
.SYNOPSIS
    Gets the Rapid7 report for a specified scan id.
.DESCRIPTION
    Gets the Rapid7 report for a specified scan id.
#>
####################################################################################################################################

param (
	#The IP address of the machine that was scanned in the scan ID..
	$IpAddressToScan,

	#The url of the rapid 7 host.
	$Rapid7Host = "https://rapid7.calpine.com",
	
	#The ID of the scan to retrieve the report for.
	$ScanId,

	#The directory to write the report.
	$ReportDir = "C:\Windows\Temp",
	
	#Path to log file
	$LogFile = "C:\Windows\Temp\GetRapid7ScanReport.txt"
)

####################################################################################################################################

$nameOfCurrentScript = "GetRapid7ScanReport.ps1"

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
	Add-Content -Path $LogFile -Value $formattedString
	if ($Foregroundcolor) { Write-Host $formattedString -ForegroundColor $Foregroundcolor } else { Write-Host $formattedString }
}


####################################################################################################################################
#Start main script
####################################################################################################################################

trap {
    Log "An exception occurred:"
    Log "$($_.Exception | Select *)"
    Log "Exiting..."
    return $null
}

Log "IpAddressToScan: $IpAddressToScan, Rapid7Host: $Rapid7Host, ScanId: $ScanId, ReportDir: $ReportDir"

#Verify that ScanId is valid
if ($ScanId -eq "FailedToScan" -or $ScanId -eq "NoScan") {
	Log "Invalid ScanId: $ScanId. Exiting..."
	return $null
}

if ($null -eq $ScanId  -or $ScanId -eq "") {
	Log "Error: ScanId cannot be null. Exiting..."
	return $null
}

$reportFormat = 'pdf'
$rapid7api = $Rapid7Host + "/api/3"
$dateString = (Get-Date).ToString('yyyyMMdd_HHmmss')

####################################################################################################################################
#Get rapid 7 account credentials
####################################################################################################################################

$apiVersion = "2017-09-01"
$resourceUri = "https://vault.azure.net"
$tokenAuthUri = "$($env:MSI_ENDPOINT)?resource=$resourceUri&api-version=$apiVersion"
$tokenResponse = Invoke-RestMethod -Method Get -Headers @{ "Secret" = "$($env:MSI_SECRET)" } -Uri $tokenAuthUri
$accessToken = $tokenResponse.access_token

$rapid7AccountUri = "https://cpn-devops.vault.azure.net/secrets/Rapid7Account?api-version=2016-10-01"
$rapid7AccountResponse = Invoke-RestMethod -Method 'GET' -Headers @{ "Authorization" = "Bearer $accessToken" } -Uri $rapid7AccountUri
$rapid7Account = $rapid7AccountResponse.Value

$rapid7AccountPasswordUri = "https://cpn-devops.vault.azure.net/secrets/Rapid7AccountPassword?api-version=2016-10-01"
$rapid7AccountPasswordResponse = Invoke-RestMethod -Method 'GET' -Headers @{ "Authorization" = "Bearer $accessToken" } -Uri $rapid7AccountPasswordUri
$rapid7AccountPassword = $rapid7AccountPasswordResponse.Value

#Set credentials in header
$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes("$($rapid7Account):$($rapid7AccountPassword)"))
$Headers = @{ Authorization = "Basic $encodedCreds"	}

####################################################################################################################################
Function Get-Rapid7Asset {
	param (
		$TargetIP
	)

	$body = "{`r`n"
	$body += "	`"match`": `"all`",`r`n"
	$body += "	`"filters`": [`r`n"
	$body += "		{ `"field`": `"ip-address`", `"operator`": `"is`", `"value`": `"" + $TargetIP + "`" }`r`n"
	$body += "	]`r`n"
	$body += "}"

	$response = Invoke-WebRequest -Uri "$rapid7api/assets/search" -Method 'POST' -Headers $Headers -Body $body -ContentType "application/json" -SkipCertificateCheck
	$statusCode = $response.StatusCode
	
	if ($statusCode -eq 200) {
		return ConvertFrom-Json $response.Content
	}
	else {
		Throw "Invoke-WebRequest POST $rapid7api/assets/search generated StatusCode $statusCode"
	}
}
####################################################################################################################################

####################################################################################################################################
Function Get-Report {
	param(
		$AssetId,
		$Hostname,
		$Report,
		$OsFamily
	)
	switch ($OsFamily) {
		"Linux" {
			$reportTemplate = 'audit-report'
			$friendlyTitle = "Audit Report"
		}
		default {
			$reportTemplate = 'devops-32---32-remediation-32-plan'
			$friendlyTitle = "Remediation Report"
		}
	}

	$reportName = "$friendlyTitle for $Hostname ($AssetId) $dateString"

	$body =	"{`r`n"
	$body +=	"	`"format`": `"$reportFormat`",`r`n"
	$body +=	"	`"name`": `"" + $reportName + "`",`r`n"
	$body +=	"	`"scope`": {`r`n"
	$body +=	"		`"scan`": " + $ScanId + "`r`n"
	$body +=	"	},`r`n"
	$body +=	"	`"filters`": {`r`n"

	if ($OsFamily -ne "Linux") {
		$body +=	"		`"categories`": {`r`n"
		$body +=	"			`"included`": [ `r`n"
		$body +=	"				`"Microsoft Patch`"`r`n"
		$body +=	"			]`r`n"
		$body +=	"		},`r`n"
	}

	$body +=	"		`"severity`": `"critical-and-severe`"`r`n"
	$body +=	"	},`r`n"
	$body +=	"	`"template`": `"" + $reportTemplate + "`"`r`n"
	$body +=	"}"
	#write-host $body
	
	#Create The Report
	$uri = "$rapid7api/reports"
	Log "Executing $uri"
	Log "Body: $body"
	$response = Invoke-WebRequest -Uri $uri -Method POST -Body $body -Headers $Headers -ContentType "application/json" -SkipCertificateCheck
	$responseContent = ConvertFrom-Json $response.Content
	$ReportID = $responseContent.id
	Log "ReportID $ReportID returned from $uri"
	
	#Run the report
	Sleep 2
	$uri = "$rapid7api/reports/$ReportID/generate"
	Log "Executing $uri"
	$response = Invoke-WebRequest -Uri $uri -Method POST -Headers $Headers -ContentType "application/json" -SkipCertificateCheck
	$responseContent = ConvertFrom-Json $response.Content
	$ReportInstanceID = $responseContent.id
	Log "ReportInstanceID $ReportInstanceID returned from $uri"

	#Get the File
	Sleep 2
	$uri = "$rapid7api/reports/$ReportID/history/$ReportInstanceID/output"
	
	$retryCount = 0	
	$maxRetries = 10
	while ($retryCount -lt $maxRetries) {
		Log "RetryCount: $retryCount :Executing $uri"
		try {
			$response = Invoke-WebRequest -Uri $uri -Method GET -Headers $Headers -OutFile $Report -SkipCertificateCheck
			$retryCount = $maxRetries
		}
		catch {
			$retryCount++
			Sleep 10
		}
	}

	#Delete the report
	Sleep 5
	$uri = "$rapid7api/reports/$ReportID"
	Log "DeleteReport: Executing $uri"
	$response = Invoke-WebRequest -Uri $uri -Method DELETE -Headers $Headers -SkipCertificateCheck
	Log "Get-Report: DeleteReport : Delete ReportID $ReportID with StatusCode $($response.StatusCode)"
}
####################################################################################################################################

####################################################################################################################################
Function Process-Target {
	param (
		$TargetAsset
	)

	#Get Asset Information
	$rapid7Assets = Get-Rapid7Asset -TargetIP $TargetAsset
	$FoundCount = $rapid7Assets.resources.Count
	Log "Rapid7Assets found : $FoundCount"
	if ($FoundCount -eq 0) {
		Log "Resource $TargetAsset is not found - $FoundCount"
		return "NoTargetFound"
	}
	
	$objects = @()	
	foreach ($rapid7Asset in $rapid7Assets.resources) {
		$assetID = $rapid7Asset.id
		$assetHostname = $rapid7Asset.hostname		
		if ($assetHostname) {
			Log "AssetHostname : $assetHostname"
			$shortHostname = ($assetHostname.split(".")[0]).ToUpper()
		}
		else {
			$shortHostname = 'NOHOSTNAME'
		}

		$osFamily = $rapid7Asset.osFingerprint.family
		Log "OsFamily is $osFamily" -Foregroundcolor Green
		
		$fullPath = "$ReportDir" + "\" +$shortHostname + "_" + $dateString + "_" + $AssetId + ".$reportFormat"

		$propertyTable = @{            
			FullPath    = $fullPath
			critical    = $rapid7Asset.vulnerabilities.critical
			exploits    = $rapid7Asset.vulnerabilities.exploits
			malwareKits = $rapid7Asset.vulnerabilities.malwareKits
			moderate    = $rapid7Asset.vulnerabilities.moderate
			severe      = $rapid7Asset.vulnerabilities.severe
			total       = $rapid7Asset.vulnerabilities.total
		}		
		$objects += New-Object PSObject -Property $propertyTable

		Log "CreateReportForAssetBegin - Hostname: $shortHostname to $fullPath"
		Log "Get-Report -AssetId $assetID -Hostname $shortHostname -Report $fullPath"
		Get-Report -AssetId $assetID -Hostname $shortHostname -Report $fullPath -OsFamily $osFamily
		Log "CreateReportForScanEnd - Hostname: $shortHostname to $fullPath"
	}
	$ObjectsCount = $objects.Count
	Log "ObjectsCount is $ObjectsCount"
	return $objects
}

####################################################################################################################################

$returnObjects = Process-Target -TargetAsset $IpAddressToScan
foreach ($returnObject in $returnObjects){
	Log "Process target complete - Report $($returnObject.FullPath)" -ForegroundColor Yellow
}
return $returnObjects