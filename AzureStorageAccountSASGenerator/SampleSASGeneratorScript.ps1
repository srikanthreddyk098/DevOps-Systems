#filepath to the AzureStorageAccountSASGenerator.exe executable
$exeFilepath = "C:\Users\sa_mreag032\Downloads\AzureStorageAccountSASGenerator.exe"

#name of the storage account and containter to generate the SAS key against
$storageAccountName = "pzpcomdbackup01"
$containerName = "test"

#call the generator passing storage account and container as arguments. the SAS key will be returned
$sas = cmd.exe /c $exeFilepath $storageAccountName $containerName

#example azcopy directory upload
$directoryToUpload = "C:\Users\sa_mreag032\Downloads\test\*"
.\Downloads\azcopy.exe copy "$directoryToUpload" "https://$storageAccountName.blob.core.windows.net/$containerName/?$sas"

#example azcopy file upload
$fileToUpload = "C:\Users\sa_mreag032\Downloads\test\test1.txt"
.\Downloads\azcopy.exe copy "$fileToUpload" "https://$storageAccountName.blob.core.windows.net/$containerName/?$sas"
