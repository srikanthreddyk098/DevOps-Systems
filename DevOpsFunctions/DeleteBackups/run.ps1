using namespace System.Net

# Input bindings are passed in via param block.
param($Request, $TriggerMetadata)

##################################################################################################
#parameter
##################################################################################################
foreach ($output in $Request.Body.Output) {
    $keyVaultName = $output.keyVaultName
    $sourceStorageAccount = $output.sourceStorageAccount         
    $sourceKeyStorageName = $output.sourceKeyStorageName
    $sourceStorageContainer = $output.sourceStorageContainer
    $blobName=$Output.blobName
    $daysBack = $output.daysBack
    $tempPath = [System.IO.Path]::GetTempPath()
    $date = $((Get-Date).ToString('yyyyMMdd_HHmmss'))
    $file = "$tempPath\AzureBlobcleanup_$date.txt"
    
    ####################################################################################################################
    #getting  the current data
    ####################################################################################################################
    $currentDate = Get-Date
    $dateBeforeBlobsToBeDeleted = $currentDate.AddDays($daysBack)
    # Number of blobs deleted
    $blobCountDeleted = 0;
    #<font color="red">This is some text!</font>
    
    $sub01 = "Below blob backups will be deleted as per 'Calpine Standard Backup Policy' :-"
    $body = "<b><font color=red>$sub01</b></font><br/>"
    #####################################################################################################################
    ## Creating Storage context for Source, destination and log storage accounts
    #####################################################################################################################
    
    $sourceStorageKey = Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $sourceKeyStorageName
    $context = New-AzStorageContext -StorageAccountName $sourceStorageAccount -StorageAccountKey $sourceStorageKey.SecretValueText
    $blobList = Get-AzStorageBlob -Context $context -Container $sourceStorageContainer -blob $blobName
    
    #####################################################################################################################
    ## Iterate through each blob
    #######################################################################################################################
    
    foreach($blobIterator in $blobList) {
        $blobDate = [datetime]$blobIterator.LastModified.UtcDateTime
        ########################################################################################################################
        # Check if the blob's last modified date is less than the threshold date for deletion
        ########################################################################################################################

        if ($blobDate -le $dateBeforeBlobsToBeDeleted) {
            Write-Output "Purging blob from Storage: " $blobIterator.name $blobDate | Out-File -File $file -Append
            $body += "<font color=navy>Removing blob : $($blobIterator.name)_$blobDate</font><br/>" 
            #$body += "Purging blob from Storage:  $($blobIterator.name)<br/>" 
            #$body =
            #write-output "Last Modified Date of the Blob: " $blobDate  | Out-File -File $file -Append
            
            #########################################################################################################
            # Cmdlet to delete the blob
            #########################################################################################################
            
            if ($env:environment -eq "prod") {
                Remove-AzStorageBlob -Container $sourceStorageContainer -Blob $blobIterator.Name -Context $context
            }
            
            $blobCountDeleted += 1;
        }
    }

    Write-output "Blobs deleted: " $blobCountDeleted | Out-File -File $file -Append
    $body += "Total Blobs deleted: $blobCountDeleted "

    if (Test-Path $file) {
        #########################################################################################################
        ##Send Email
        #########################################################################################################
        
        $smtpServer    = "relay.calpine.com"
        if ($env:environment -eq "dev") {
        $to = @("Mallika.Chaganti@calpine.com","Nikhil.Nandyala@calpine.com")
        }
        elseif ($env:environment -eq "prod") {
        $to = @("DevOps@calpine.com","Calpine_DBA@calpine.com")
        }
        
        $from  = "devops@calpine.com"
        $subject = "Azure Database Backup Delete Summary From [$sourceStorageAccount][$sourceStorageContainer]"
        $attachments = @()
        $attachments += $file
        
        Send-MailMessage -SmtpServer $smtpServer -To $to -From $from -Subject $subject -Body $body -Attachments $attachments -BodyAsHtml
    }
}