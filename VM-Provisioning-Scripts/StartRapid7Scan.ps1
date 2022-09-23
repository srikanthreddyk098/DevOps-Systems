####################################################################################################################################
<#
.SYNOPSIS
    Initiates Rapid7 scan.
.DESCRIPTION
    Initiates Rapid7 scan.
#>
####################################################################################################################################
param (
	#The IP address to initiate a rapid 7 scan for.
	$IpAddressToScan,

	#The url of the rapid 7 host.
	$Rapid7Host = 'https://rapid7.calpine.com',
	
	#The rapid7 site ID for the IP address to scan.
	$SiteId,
	
	#Path to log file
	$LogFile = "C:\Windows\Temp\StartRapid7Scan.txt"
)

####################################################################################################################################

$nameOfCurrentScript = "StartRapid7Scan.ps1"

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
    return "FailedToScan"
}

Log "IpAddressToScan: $IpAddressToScan, Rapid7Host: $Rapid7Host, SiteId: $SiteId"

if ($null -eq $IpAddressToScan -or $IpAddressToScan -eq "") {
	Log "Error: IpAddressToScan cannot be null. Exiting..."
	return "FailedToScan"
}

if ($null -eq $SiteId  -or $SiteId -eq "") {
	Log "Error: SiteId cannot be null. Exiting..."
	return "FailedToScan"
}

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
$headers = @{ Authorization = "Basic $encodedCreds"	}

####################################################################################################################################
Function Wait-ForScan {
####################################################################################################################################
	param (
		$ScanId
	)

	$status = $null
	$rapid7api = $Rapid7Host + "/api/3"
	$uri = "$rapid7api/scans/$ScanId"
    $waitIncrement = 30
	$currentWaitTime = 0
	$maxWaitTime = 7200

    while($currentWaitTime -lt $maxWaitTime) {
	    while (($status -ne 'finished') -and ($status -ne 'error')) {
            $currentWaitTime = $currentWaitTime + $waitIncrement
		    Sleep $waitIncrement
		    $response = Invoke-WebRequest -Uri $uri -Method 'GET' -Headers $Headers -ContentType "application/json" -SkipCertificateCheck
		    $responseContent = ConvertFrom-Json $response.Content
		    $scanName = $responseContent.scanName
		    $scanStatus = $responseContent.status
		    Log "WaitForScan|$currentWaitTime/$maxWaitTime|ScanStatus - $scanStatus - `"$scanName`""
		    $status = $scanStatus
		}
		
        Log "Scan status is $status"
        return $status        
	}
	
    Log "WaitForScan|$currentWaitTime/$maxWaitTime|Max WaitTime $maxWaitTime seconds reached"
    $status = "TimeOut"
    $currentWaitTime=$maxWaitTime
	return $status
}

####################################################################################################################################

####################################################################################################################################
#Start Rapid7 discovery scan (the machine needs to be discovered before it can be scanned)
#NOTE: No longer needed as of 01/2020
####################################################################################################################################

# try {
# 	$body = "{`r`n"
# 	$body+= "    `"hosts`": [`r`n"
# 	$body+=	"        `"$IpAddressToScan`"`r`n"
# 	$body+=	"    ],`r`n"
# 	$body+=	"    `"name`": `"Build discovery scan $IpAddressToScan at $((Get-Date).ToString('yyyyMMdd_HHmmss')) `",`r`n"
# 	$body+=	"    `"templateId`": `"_calpine-_-discovery-scan`"`r`n"
# 	$body+=	"}"

# 	$uri = "$Rapid7Host/api/3/sites/6/scans"
# 	Log "Executing $uri"
# 	Log "Body: $body"

# 	$response = Invoke-WebRequest -Uri $uri -Method 'POST' -Body $body -Headers $headers -ContentType "application/json" -SkipCertificateCheck
# 	$responseContent = ConvertFrom-Json $response.Content
#     $scanId = $responseContent.id
		
# 	if ($response.StatusCode -eq "201") {
# 		Log "Waiting for discover scan with scan ID $scanId to finish"
# 		$scanResult = Wait-ForScan -ScanId $scanId
# 		Log "Scan with scan ID $scanId finished with result: $scanResult"
# 	}
# 	else {
# 		Log "Discovery scan response returned status code $($response.StatusCode). Expected 201. Exiting..."
# 		return "FailedToScan"
# 	}
		
# 	if ($scanResult -ne "finished") {
# 		Log "Discovery scan returned scan result $scanResult. Expected 'finished'. Exiting..."
# 		return "FailedToScan"
# 	}
# }
# catch {
#     Log "An exception occurred sending request to $($uri):"
#     Log "$($_.Exception | Select *)"
# 	return "FailedToScan"
# }

####################################################################################################################################
#Start Rapid7 scan
####################################################################################################################################

try {
	$body = "{`r`n"
	$body+= "    `"hosts`": [`r`n"
	$body+=	"        `"$IpAddressToScan`"`r`n"
	$body+=	"    ],`r`n"
	$body+=	"    `"name`": `"Build scan $IpAddressToScan at $((Get-Date).ToString('yyyyMMdd_HHmmss'))`",`r`n"
	$body+=	"    `"templateId`": `"_calpine-_-full-audit-without-web-spider`"`r`n"
	$body+=	"}"

	$uri = "$Rapid7Host/api/3/sites/$SiteId/scans"
	Log "Executing $uri"
	Log "Body: $body"

	$response = Invoke-WebRequest -Uri $uri -Method 'POST' -Body $body -Headers $headers -ContentType "application/json" -SkipCertificateCheck
	$responseContent = ConvertFrom-Json $response.Content
	$scanId = $responseContent.id

	if ($response.StatusCode -eq "201") {
		Log "Waiting for vulnerability scan with scan ID $scanId to finish"
		$scanResult = Wait-ForScan -ScanId $scanId
		Log "Scan with scan ID $scanId finished with result: $scanResult"
	}
	else {
		Log "Received reponse code: $($response.StatusCode). Expected 201."
	}
		
	if ($scanResult -ne "finished") {
		return "FailedToScan";
	}
	
	return $scanId
}
catch {
    Log "An exception occurred sending request to $($uri):"
    Log "$($_.Exception | Select *)"
	return "FailedToScan"
}

####################################################################################################################################
#End main script
####################################################################################################################################