This executable generates and writes a user delegation SAS token to the standard console output. The SAS token is valid for 24 hours.

It requires two input arguments:
    1. The name of the storage account to generate the key against
    2. The container that will be operated on

e.g. AzureStorageAccountSASGenerator.exe <storageAccountName> <container>