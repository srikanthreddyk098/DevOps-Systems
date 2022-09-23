Login-AzAccount

Select-AzSubscription -Subscription ""

Set-AzContext -Subscription ""

$rg = ""

$vmnames = Get-Content -Path 'C:\Temp\servers.txt'

foreach($vmname in $vmnames)
{
    
    $VM = Get-AzVM -ResourceGroupName $rg -Name $vmname

    Stop-AzVM -ResourceGroupName $rg -Name $vmname -Force

    $disk = Get-AzDisk -ResourceGroupName $rg -DiskName $VM.StorageProfile.OsDisk.Name

    $disk.DiskSizeGB = 

    Update-AzDisk -ResourceGroupName $rg -Disk $disk -DiskName $Disk.Name

    Start-AzVM -ResourceGroupName $rg -Name $vmname
}