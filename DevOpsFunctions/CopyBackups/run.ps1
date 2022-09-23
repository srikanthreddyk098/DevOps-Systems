using namespace System.Net

# Input bindings are passed in via param block.
param($Request, $TriggerMetadata)

##################################################################################################
#parameters
##################################################################################################
foreach ($output in $Request.Body.Output) {
   $keyVaultName = $output.keyVaultName
   $sourceKeyStorageName = $output.sourceKeyStorageName
   $destKeyStorageName = $output.destKeyStorageName
   $destStorageAccount = $output.destStorageAccount
   $sourceStorageAccount = $output.sourceStorageAccount
   $sourceStorageContainer = $output.sourceStorageContainer
   $destStorageContainer =$output.destStorageContainer
   $daysBack =$output.daysBack
   $blobName = $output.blobName
   $tempPath = [System.IO.Path]::GetTempPath()
   $date = $((Get-Date).ToString('yyyyMMdd_HHmmss'))
   $file = "$tempPath\AzureBlobMove_$date.txt"
   
   #################################################################################################################       
   #Storage Context
   #################################################################################################################
   
   $destStorageKey = Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $destKeyStorageName
   $sourceStorageKey = Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $sourceKeyStorageName
   $sourceStorageContext = New-AzStorageContext –StorageAccountName $sourceStorageAccount -StorageAccountKey $sourceStorageKey.SecretValueText
   $destStorageContext = New-AzStorageContext –StorageAccountName $destStorageAccount -StorageAccountKey $destStorageKey.SecretValueText
   
   ####################################################################################################################
   #getting  the current data
   ####################################################################################################################
   $currentDate = Get-Date
   $dateToMove = $currentDate.AddDays($daysBack)
   #$blobsList = Get-AzureStorageBlob -Context $sourceStorageContext -Container $sourceStorageContainer 
   $blobsList = Get-AzStorageBlob -Context $sourceStorageContext -Container $sourceStorageContainer -blob $blobName | Where { $_.Length -gt 0KB}
   $blobCopyArray = @()
   
   ####################################################################################################################
   #Do the copy of everything
   ####################################################################################################################
   $body = "";

   if ($null -eq $blobsList) {
      Write-Output "No files to copy. If this unexpected, please contact DevOps@calpine.com " | Out-File -File $file -Append
      $body += "<b><font color=red>No files to copy if this unexpected please contact:DevOps@calpine.com </b></font><br/>"
   }
   else {
      foreach ($blobIterator in $blobsList) {
         $blobDate = [datetime]$blobIterator.LastModified.UtcDateTime
         ####################################################################################################################
         #Check if the blob's last modified date is less than current date for move
         ####################################################################################################################
               
         if ($blobDate -gt $dateToMove) {
            Write-Output "-----------------------------------"
            Write-Output "Name of the moving blob : $($blobIterator.Name) " | Out-File -File $file -Append
            $body += "Name of the moving blob : $($blobIterator.Name)<br/>"
            Write-Output "Last Modified Date of the Blob: " $blobDate
            Write-Output "-----------------------------------"
            $blobCopy = Start-AzStorageBlobCopy -Context $sourceStorageContext -SrcContainer $sourceStorageContainer -SrcBlob $blobIterator.Name `
            -DestContext $destStorageContext -DestContainer $destStorageContainer -DestBlob $blobIterator.Name
            $blobCopyArray += $blobCopy
         }
      }
      
      Start-Sleep -Seconds 30
      $body += "<b>Copy status of each file as below:-<br/>"
         
      foreach ($blobCopy in $blobCopyArray) {
         #Could ignore all rest and just run $blobCopy | Get-AzureStorageBlobCopyState but I prefer output with % copied
         $CopyState = $blobCopy | Get-AzStorageBlobCopyState
         $body += "$($CopyState.Source.AbsolutePath + " " + $CopyState.Status + " {0:N2}%" -f (($CopyState.BytesCopied/$CopyState.TotalBytes)*100))<br/>"
         Write-Output $body
      }
   }

   if (Test-Path $file) {
      #######################################################################################
      #Send Email
      #######################################################################################
      $smtpServer = "relay.calpine.com"
      if ($env:environment -eq "dev") {
         $to = @("Mallika.Chaganti@calpine.com","Nikhil.Nandyala@calpine.com")
      }
      elseif ($env:environment -eq "prod") {
         $to = @("DevOps@calpine.com","Calpine_DBA@calpine.com")
      }
      $from = "devops@calpine.com"
      $subject = "Azure Database Backup Copy Summary From [$sourceStorageAccount][$sourceStorageContainer] to [$destStorageAccount][$destStorageContainer]"
      $attachments = @()
      $attachments += $file
      
      Send-MailMessage -SmtpServer $smtpServer -To $to -From $from -Subject $subject -Body $body -Attachments $attachments -BodyAsHtml
   }
}