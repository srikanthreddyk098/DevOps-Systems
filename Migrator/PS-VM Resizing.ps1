Login-AzAccount

Select-AzSubscription -Subscription ""

Set-AzContext -Subscription ""

$rg = "" 
$vmname = "" 
$newvmsize = ""

$vm = Get-AzVM -ResourceGroupName $rg -Name $vmname

$vm.HardwareProfile.vmSize = $newvmsize

Update-AzVM -ResourceGroupName $rg -VM $vm