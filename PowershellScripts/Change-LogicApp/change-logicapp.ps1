<#
.SYNOPSIS
Change the state of Logic Apps to Enabled or Disabled

.DESCRIPTION
This script Enables or Disables Logic Apps based on the input xml config file.  The config file has details of Azure Subscription, Resource Group and what Logic Apps to change.

.PARAMETER ConfigFile
Name of the xml config file. The xml file need to be in the same folder as this script.

.PARAMETER Action
Action (Enable or Disable) to take on the Logic Apps.

.PARAMETER IsTest
Test run the script. No real changes are executed.

.INPUTS
None.

.OUTPUTS
None.

.EXAMPLE
PS> .\change-logicapp.ps1 -ConfigFile .\dev.xml -Action Disabled

.EXAMPLE
PS> .\change-logicapp.ps1 -ConfigFile .\prod.xml -Action Enabled

.EXAMPLE
PS> .\change-logicapp.ps1 -ConfigFile .\prod.xml -Action Enabled -IsTest

#>


# Script Parameters  
Param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigFile = "",
    [Parameter(Mandatory=$true)]
    [ValidateSet("Enabled", "Disabled")]
    [string]$Action,
    [switch]$IsTest = $false
)

if(!(Test-Path $ConfigFile)) {
    Write-Error "File $ConfigFile does not exist ... quiting"
    exit 1
}

[XML]$xmlconfig = Get-Content $ConfigFile

$Tenant = $xmlconfig.Tenant.name

$loginstatus = Login-AzAccount -Tenant "$Tenant"

if(!$loginstatus.Context) {
    Write-Error "Trouble logging you into $Tenant tenant ... quiting"
    exit 1
}


"Below Logic Apps will be " + $Action | Write-Host -ForegroundColor Yellow

foreach($sub in $xmlconfig.Tenant.Subscription) {
    foreach($app in $sub.LogicApp) {
        Write-Host "Subscripton:" $sub.name "  Logic App:" $app.name
    }
}

$msg = "Are you sure to " + $Action + " these Logic Apps (Y/N)? "
$answer = Read-Host $msg

if($answer -match "y") {
    foreach($sub in $xmlconfig.Tenant.Subscription) {
        #Select-AzSubscription -Subscription $sub.name
        Write-Host "Switching to subscription " $sub.name
        $sb = Set-AzContext -Subscription $sub.name
        if($sb) {
            foreach($app in $sub.LogicApp) {
                if($IsTest) {
                    Set-AzLogicApp -State $Action -Name $app.name -ResourceGroupName $app.resourcegroup -Force -WhatIf
                }
                else {
                    $wsobj = Set-AzLogicApp -State $Action -Name $app.name -ResourceGroupName $app.resourcegroup -Force
                    if(!$wsobj) { "Changes to " + $app.name + " failed " | Write-Host -ForegroundColor Red }
                    $la = Get-AzLogicApp -Name $app.name -ResourceGroupName $app.resourcegroup
                    "State of " + $app.name + " after update is " + $la.State | Write-Host -ForegroundColor Yellow
                }
            }
        }
        else {
            Write-Error "Cannot switch to Subscription" $sub.name
        }
    }
}
else {
    "Doing nothing ..."
}
