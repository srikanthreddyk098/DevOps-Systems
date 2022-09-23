#!/usr/bin/env bash
# set -x
# trap read debug

if [[ "$1" = '--help' ]] || [[ "$1" = '-help' ]] || [[ "$1" = help ]] || [[ "$1" = '-h' ]] || [[ "$1" = '-H' ]] ; then
Yellow='\e[93m'
printf "${Yellow}"

cat << EOF
This script automates the installation or removal of Centrify Express.

Usage:
  To add the system from AD, use the command: ./InstallCentrify.sh password AzureSubscriptionName

  To remove the system from AD and delete the Centrify packages: ./InstallCentrify.sh password remove

Description of parmeters:
  password - This is the password to the account with AD join privileges.
  AzureSubscriptionName - This is the Azure subscription name without spaces or slashes. The subscriptions that this script supports are:
    calpinedevtest
    commercialdev
    commercialprod
    corporatedev
    corporateprod
    poweropsdev
    poweropsprod
    retaildev
    retailprod
    retaildmzdev
    retaildmzprod
    sharedservices

To manually add a VM to AD, remove a VM from AD, or remove all the Centrify packages run the script: ./install-express.sh
EOF
Default='\033[0m'
printf "${Default}"
exit 0
fi

logFile="/var/tmp/BuildLog_$(hostname).log"
log() {
    formattedString="$(date)|InstallCentrify.sh|$1"
    echo "$formattedString" >> $logFile
    echo "$formattedString"
}

###################################################################################################
#  Validate input parameters and sudo
###################################################################################################

if [[ $# -lt 2 ]]; then
    log "There are mandatory parameters missing. Use $(basename $0) --help for more information."
    log "Exiting..."
    exit 1
fi

if [[ "$(whoami)" != "root" ]]; then
    log "Please run this script as root. (e.g. sudo $0 $1 $2)"
    log "Exiting..."
	exit 1
fi

###################################################################################################
#  Download CentrifyDC packages depending on OS
###################################################################################################

os=$(cat /etc/os-release | grep "^NAME=" | cut -d '"' -f 2)
osVersion=$(cat /etc/os-release | grep "^VERSION_ID=" | cut -d '"' -f 2)
log "Found OS: $os $osVersion"

scriptUrl="http://download.calpine.com/devops/scripts/"
if [[ ! -z $3 ]]; then
    centrifyTar=$3
elif [[ "$os" == "Ubuntu" ]]; then
    centrifyTar="UbuntuCentrifyDC-Install.tar.gz"
elif [[ "$os" == "Red Hat Enterprise Linux Server" ]]; then
    centrifyTar="RhelCentrifyDC-Install.tar.gz"
else
    log "Could not find domain script for OS: $os $osVersion."
    log "Exiting..."
    exit 1
fi

cd /var/tmp

# delete tar file if it already exists
if [[ -f $centrifyTar ]]; then
    rm $centrifyTar
fi

wget "$scriptUrl$centrifyTar"

if [[ -f $centrifyTar ]]; then
    log "Downloaded $scriptUrl$centrifyTar to /var/tmp"
else
    log "Could not download $scriptUrl$centrifyTar"
    log "Exiting..."
    exit 1
fi

if [[ -d "/var/tmp/centrify" ]]; then
    rm -rf "/var/tmp/centrify"
fi


mkdir "/var/tmp/centrify"

tar -xzvf $centrifyTar -C /var/tmp/centrify
log "Uncompressed $centrifyTar to /var/tmp/centrify"
cd /var/tmp/centrify

###################################################################################################
#  REMOVE ALL option
###################################################################################################

if [[ "$2" = [Rr][Ee][Mm][Oo][Vv][Ee] ]]; then
    log "Remove all option selected..."
    # remove entries from /etc/security/access.conf
    sudo sed -i 's/+ : Linux_Admins : ALL//' /etc/security/access.conf
    sudo sed -i 's/+ : $(hostname -s)_Admins : ALL//' /etc/security/access.conf
    sudo sed -i 's/+ : cpnntlcl : ALL//' /etc/security/access.conf
    sudo sed -i 's/+ : svc_cark_recon_na : ALL//' /etc/security/access.conf
    sudo sed -i 's/- : ALL : ALL//' /etc/security/access.conf
    # remove all blank lines
    sed -i -e :a -e '/^\n*$/{$d;N;ba' -e '}' /etc/security/access.conf    
    log "Reverted any changes to /etc/security/access.conf."

    # remove sudoers file
    rm /etc/sudoers.d/Linux_Admins
    log "Removed /etc/sudoers.d/Linux_Admins file."
    
    adStatus=$(adinfo -m | grep -o connected 2> /dev/null)

    if [ "$(adinfo -m | grep -o connected 2> /dev/null)" != "connected" ]; then
       log "This system is not currently joined to the domain. There is nothing else to do."
       log "Exiting..."
       exit 0
    fi

    echo "./install-express.sh << EOF" > removeAll.sh
    echo "E" >>  removeAll.sh
    echo "Y" >>  removeAll.sh
    echo "N" >>  removeAll.sh
    echo "_svc_AzDomJoin" >>  removeAll.sh
    echo "$1" >>  removeAll.sh
    echo "EOF" >>  removeAll.sh

    chmod 755  removeAll.sh
    log "Running install-express.sh..."
    ./removeAll.sh
    rm removeAll.sh
    log "Removed system from the domain."
    log "Finished removing Centrify."
    exit 0
fi

###################################################################################################
#  Check if Centrifydc has already been installed.
###################################################################################################

if [[ $(adinfo -m 2>/dev/null) == "connected" ]]; then
    log "This machine is already joined to a domain. Running main Centrify script."

    if [[ $os = "Ubuntu" ]]; then
        apt-get update
    else
        yum update
    fi

    ./install-express.sh
    #cat centrifydc.conf > /etc/centrifydc/centrifydc.conf
    exit 0
fi

###################################################################################################
#  resolve.conf configuration changes
###################################################################################################

# Remove existing configuration
chattr -i /etc/resolv.conf
sed -i "s/nameserver*.*//" /etc/resolv.conf
sed -i "s/search*.*//" /etc/resolv.conf
sed -i '/^$/d' /etc/resolv.conf

# Insert the correct configuration
echo "nameserver 10.221.0.69" >> /etc/resolv.conf
echo "nameserver 10.221.0.70" >> /etc/resolv.conf
echo "search na.calpine.com"  >> /etc/resolv.conf
echo "search reddog.microsoft.com" >> /etc/resolv.conf
log "Replaced nameserver and search domains in /etc/resolv.conf file."

chmod 644 /etc/resolv.conf
chattr +i /etc/resolv.conf
log "Made /etc/resolv.conf immutable."

###################################################################################################
#  Configure access policies
###################################################################################################

# Create and populate Linux_Admins sudoers file
echo "%Linux_Admins  ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/Linux_Admins
echo "%$(hostname -s)_Admins  ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/Linux_Admins
echo "cpnntlcl  ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/Linux_Admins
echo "_svc_cark_recon_na  ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/Linux_Admins
chmod 440 /etc/sudoers.d/Linux_Admins
log "Created /etc/sudoers.d/Linux_Admins sudoers file."

# Force authentication based on access.conf
sudo sed -i '/pam_access/s/^# //g' /etc/pam.d/sshd
sudo sed -i '/pam_access/s/^# //g' /etc/pam.d/login
log "Uncommented pam_access in /etc/pam.d/sshd and /etc/pam.d/login files."

# Update access.conf file to allow authorized users
if ! grep -q "+ : Linux_Admins : ALL" /etc/security/access.conf; then
    echo "+ : Linux_Admins : ALL" >> /etc/security/access.conf
fi

if ! grep -q "+ : $(hostname -s)_Admins : ALL" /etc/security/access.conf; then
    echo "+ : $(hostname -s)_Admins : ALL" >> /etc/security/access.conf
fi

if ! grep -q "+ : cpnntlcl : ALL" /etc/security/access.conf; then
    echo "+ : cpnntlcl : ALL" >> /etc/security/access.conf
fi

if ! grep -q "+ : svc_cark_recon_na : ALL" /etc/security/access.conf; then
    echo "+ : svc_cark_recon_na : ALL" >> /etc/security/access.conf
fi

if ! grep -q "\- : ALL : ALL" /etc/security/access.conf; then
    echo "- : ALL : ALL" >> /etc/security/access.conf
fi

log "Added entries to /etc/security/access.conf file."

# Check or rcpbind prerequisite
isInstalled=
if [[ ! $(dpkg --get-selections | grep -i -o rpcbind) == "rpcbind" ]]; then    
    if [[ $os = "Ubuntu" ]]; then
        apt-get install rpcbind -y
    else
        yum install rpcbind -y
    fi
    echo "Installed rpcbind."
fi

###################################################################################################
#  Run Centrify install script
###################################################################################################

# Get hostname details
subscription=$(echo $2 | tr A-Z a-z)
log "Subscription = $subscription"
vmName=$(hostname -s | tr A-Z a-z)
log "VM Name = $vmName"
environmentLetter=$(hostname -s | head -c 3 | tail -c 1 | tr A-Z a-z)
log "Environment = $environmentLetter"

case $subscription in
    sub_calpinedevtest)
        ou="OU=LinuxServers,OU=Sub_CalpineDevTest,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_commercialdev)
        ou="OU=LinuxServers,OU=Sub_CommercialDev,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_commercialprod)
        ou="OU=LinuxServers,OU=Sub_CommercialProd,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_corporatedev)
        ou="OU=LinuxServers,OU=Sub_CorporateDev,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_corporateprod)
        ou="OU=LinuxServers,OU=Sub_CorporateProd,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_poweropsdev)
        ou="OU=LinuxServers,OU=Sub_PowerOpsDev,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_poweropsprod)
        ou="OU=LinuxServers,OU=Sub_PowerOpsProd,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_retaildev)
        ou="OU=LinuxServers,OU=Sub_RetailDev,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_retailprod)
        ou="OU=LinuxServers,OU=Sub_RetailProd,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_retaildmzdev)
        ou="OU=LinuxServers,OU=Sub_RetailDMZDev,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_retaildmzprod)
        ou="OU=LinuxServers,OU=Sub_RetailDMZProd,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    sub_sharedservices)
        ou="OU=LinuxServers,OU=Sub_SharedServices,OU=Primary,OU=Azure,OU=Servers,DC=na,DC=calpine,DC=com"
        ;;
    *)
        log "$subscription is not a supported subscription. Exiting..."
        exit 0
        ;;
esac

log "OU=$ou"

#Create file with answers to prompts from Centrify install script
echo "./install-express.sh << EOF" > executeInstall.sh

if [ "$(dpkg-query -s centrifydc 2>/dev/null | grep Status)" ==  "Status: install ok installed" ]; then
    echo "R" >> executeInstall.sh
    echo "R" >> executeInstall.sh
    echo "R" >> executeInstall.sh
    echo "R" >> executeInstall.sh
    echo "Y" >> executeInstall.sh
    echo "Y" >> executeInstall.sh
    echo "Y" >> executeInstall.sh
    echo "Y" >> executeInstall.sh
else 
    echo "X" >>  executeInstall.sh
fi
echo "Y" >> executeInstall.sh
echo "Y" >> executeInstall.sh
echo "Y" >> executeInstall.sh
echo "Y" >> executeInstall.sh
echo "N" >> executeInstall.sh
echo "Y" >>  executeInstall.sh
echo "na.calpine.com" >>  executeInstall.sh
echo "_svc_AzDomJoin" >>  executeInstall.sh
echo "$1" >>  executeInstall.sh
echo "$vmName" >> executeInstall.sh
echo "$ou" >>  executeInstall.sh
echo "pazpwdcna01.na.calpine.com" >>  executeInstall.sh
echo "N" >>  executeInstall.sh
echo  "Y" >>  executeInstall.sh
echo "EOF" >>  executeInstall.sh

chmod 755 executeInstall.sh
if [[ $os = "Ubuntu" ]]; then
    apt-get update
else
    yum update
fi
./executeInstall.sh
sudo rm executeInstall.sh
#cat centrifydc.conf > /etc/centrifydc/centrifydc.conf
log "Finished installing Centrify."
exit 0