#!/usr/bin/env bash
# set -x
# trap read debug

if [[ "$1" = '--help' ]] || [[ "$1" = '-help' ]] || [[ "$1" = help ]] || [[ "$1" = '-h' ]] || [[ "$1" = '-H' ]] ; then
Yellow='\e[93m'
printf "${Yellow}"

cat << EOF
This script installs and configures security agents as well as other misc Linux provisioning steps and optionally runs the CentrifyDC install script
to join the VM to the Calpine domain.

Usage:
  To skip domain join, use the command: ./Linux-SetupScript.sh subscription redhatPortalPassword redhatSubscriptionPool
  To join to the domain as well, use the command: ./Linux-SetupScript.sh subscription redhatPortalPassword redhatSubscriptionPool adJoinPassword azureSubscriptionOUName

Description of options:
  subscription - The Azure subscription the VM is deployed to.
  redhatPortalPassword - The is the password for the redhat subscription manager.
  redhatSubscriptionPool - The redhat subscription pool to attach to.
  adJoinPassword - The password of the account with AD join privileges.
  azureSubscriptionOUName - The Azure subscription name OU name to join to the domain
EOF
Default='\033[0m'
printf "${Default}"
exit 0
fi

subscription=$1
timezone=$2
redhatPortalPassword=$3
redhatSubscriptionPool=$4
adJoinPassword=$5
azureSubscriptionOUName=$6

computerName=$(hostname)

logFile="/var/tmp/BuildLog_$computerName.log"
log() {
    formattedString="$(date)|Linux-SetupScript.sh|$1"
    echo "$formattedString" >> $logFile
    echo "$formattedString"
}

if [[ "$(whoami)" != "root" ]]; then
	log "Please run this script as root. (e.g. sudo $0)"
	exit 1
fi

ipAddress=$(hostname -I)
os=$(cat /etc/os-release | grep "^NAME=" | cut -d '"' -f 2)
osVersion=$(cat /etc/os-release | grep "^VERSION_ID=" | cut -d '"' -f 2)
log "Found OS: $os $osVersion"
log ""

cd /var/tmp

# Make [[ comparisons case insensitive
shopt -s nocasematch

###################################################################################################
#   Download and run AD domain script
###################################################################################################

if [[ -z "$adJoinPassword" ]]; then
    log "Domain join account password parameter is missing. Skipping domain join..."
elif [[ -z "$azureSubscriptionOUName" ]]; then
    log "Subscription OU name is missing. Skipping domain join..."
else 
    wget http://download.calpine.com/devops/scripts/InstallCentrify.sh
    log "Downloaded http://download.calpine.com/devops/scripts/InstallCentrify.sh to /var/tmp"
    chmod u+x /var/tmp/InstallCentrify.sh
    log "Running /var/tmp/InstallCentrify.sh"    
    /var/tmp/InstallCentrify.sh $adJoinPassword $azureSubscriptionOUName
fi

###################################################################################################
#   Download and install Splunk agent
###################################################################################################

if [[ -d "/opt/splunkforwarder/bin" ]]; then
    log "Splunk: Already installed. Skipping..."
    log ""
else
    log "Splunk: Downloading agent and configuration files..."
    wget http://download.calpine.com/DevOps/SecurityAgents/Splunk/Linux/splunkforwarder-linux-x86_64.tgz
    wget http://download.calpine.com/DevOps/SecurityAgents/Splunk/Linux/splunkforwarder-linux-deploymentclient.tar.gz
    
    log "Splunk: Installing and configuring agent..."
    tar xvzf splunkforwarder-linux-x86_64.tgz -C /opt
    tar xvzf splunkforwarder-linux-deploymentclient.tar.gz -C /opt/splunkforwarder/etc/apps
    
    #configure splunk user
    useradd splunk
    chown -R splunk:splunk /opt/splunkforwarder/
    
    #configure Splunk agent
    sudo -u splunk /opt/splunkforwarder/bin/splunk start --accept-license --answer-yes --no-prompt --gen-and-print-passwd
    /opt/splunkforwarder/bin/splunk enable boot-start -systemd-managed 0 -user splunk
    sudo -u splunk /opt/splunkforwarder/bin/splunk restart
    
    rm splunkforwarder-linux-x86_64.tgz
    rm splunkforwarder-linux-deploymentclient.tar.gz
    
    log "Splunk: Finished installing and configuring."
    log ""
fi

###################################################################################################
#   Download and install Tanium agent
###################################################################################################

if [[ -d "/opt/Tanium/TaniumClient" ]]; then
    echo "Tanium: Already installed. Skipping..."
    echo ""
else
    if [[ "$os" == "Ubuntu" && "$osVersion" =~ "14." ]]; then
        agentFilename="taniumclient-ubuntu14_amd64.deb"
    elif [[ "$os" == "Ubuntu" && "$osVersion" =~ "16." ]]; then
        agentFilename="taniumclient-ubuntu16_amd64.deb"
    elif [[ "$os" == "Ubuntu" && "$osVersion" =~ "18." ]]; then
        agentFilename="taniumclient-ubuntu16_amd64.deb"
    elif [[ "$os" == "Red Hat Enterprise Linux Server" && "$osVersion" =~ "7." ]]; then
        agentFilename="taniumclient-rhe7_x86_64.rpm"
    elif [[ "$os" == "Red Hat Enterprise Linux Server" && "$osVersion" =~ "6." ]]; then
        agentFilename="taniumclient-rhe6_x86_64.rpm"
    elif [[ "$os" == "SLES" && "$osVersion" =~ "12." ]]; then
        agentFilename="taniumclient-sle12_x86_64.rpm"
    elif [[ "$os" == "SLES" && "$osVersion" =~ "11." ]]; then
        agentFilename="taniumclient-sle11_x86_64.rpm"
    else
        agentFilename=""
    fi
    
    if [[ "$agentFilename" == "" ]]; then
        echo "Tanium: A TaniumClient agent is not available for the OS: $os $osVersion."
    else
        echo "Tanium: Downloading agent and public key..."
        wget  "http://download.calpine.com/DevOps/SecurityAgents/Tanium/Linux/$agentFilename"
        wget  "http://download.calpine.com/DevOps/SecurityAgents/Tanium/tanium.pub"
        
        echo "Tanium: Installing and configuring agent..."
        
        if [[ "$os" == "Red Hat Enterprise Linux Server" ]]; then
            yum localinstall "$agentFilename" -y
        elif [[ "$os" == "SLES" ]]; then
            zypper install "$agentFilename" 
        else
            dpkg -i "$agentFilename"
        fi
        
        mv tanium.pub /opt/Tanium/TaniumClient/
        
        #configuring Tanium agent
        /opt/Tanium/TaniumClient/TaniumClient config set ServerNameList zone.calpine.com
        /opt/Tanium/TaniumClient/TaniumClient config set LogVerbosityLevel 1
        service taniumclient restart
        
        log "Tanium: Finished installing and configuring."
        log ""
        
        rm $agentFilename
    fi
fi

###################################################################################################
#   Download and install Eracent agent
###################################################################################################

if [[ -d "/var/Eracent" ]]; then
    echo "Eracent: Already installed. Skipping..."
    echo ""
else
    if [[ "$os" == "Ubuntu" ]]; then
        agentFilename="eracent-linux_EPA_EUA-.x86_64.deb"
    elif [[ "$os" == "Red Hat Enterprise Linux Server" ]]; then
        agentFilename="eracent-linux_EPA_EUA-x86_64.rpm"
    elif [[ "$os" == "SLES" ]]; then
        agentFilename="eracent-linux_EPA_EUA-x86_64.rpm"
    else
        agentFilename=""
    fi
    
    if [[ "$agentFilename" == "" ]]; then
        echo "Eracent: An Eracent agent is not available for the OS: $os $osVersion."
    else
        echo "Eracent: Downloading agent..."
        wget  "http://download.calpine.com/DevOps/SecurityAgents/Eracent/Linux/$agentFilename"
        
        echo "Eracent: Installing and configuring agent..."
        
        if [[ "$os" == "Red Hat Enterprise Linux Server" ]]; then
            yum localinstall "$agentFilename" -y
        elif [[ "$os" == "SLES" ]]; then
            zypper install "$agentFilename"             
        else
            dpkg -i "$agentFilename"
        fi
        
        log "Eracent: Finished installing and configuring."
        log ""
        
        rm $agentFilename
    fi
fi

###################################################################################################
#   Download and install SentinelOne agent
###################################################################################################

if [[ -f "/usr/bin/sentinelctl" ]]; then
    echo "SentinelOne: Already installed. Skipping..."
    echo ""
else
    if [[ "$os" == "Ubuntu" && "$osVersion" != "12."* ]]; then
        log "SentinelOne: Downloading agent..."
        wget http://download.calpine.com/devops/SecurityAgents/SentinelOne/SentinelOne_ubuntu.deb
        log "SentinelOne: Installing agent..."
        dpkg -i SentinelOne_ubuntu.deb
        sentinelctl management token set eyJ1cmwiOiAiaHR0cHM6Ly91c2VhMS0wMDguc2VudGluZWxvbmUubmV0IiwgInNpdGVfa2V5IjogImFlNDkxNDhhMzJhMGY5MDIifQ==
        sentinelctl control start
        rm SentinelOne_ubuntu.deb
        log "SentinelOne: Finished installing."
        log ""
    elif [[ "$os" == "Red Hat Enterprise Linux Server" ]]; then
        log "SentinelOne: Downloading agent..."
        wget http://download.calpine.com/devops/SecurityAgents/SentinelOne/SentinelOne_redhat.rpm
        log "SentinelOne: Installing agent..."
        yum -y install SentinelOne_redhat.rpm
        sentinelctl management token set eyJ1cmwiOiAiaHR0cHM6Ly91c2VhMS0wMDguc2VudGluZWxvbmUubmV0IiwgInNpdGVfa2V5IjogImFlNDkxNDhhMzJhMGY5MDIifQ==
        sentinelctl control start
        rm SentinelOne_redhat.rpm
        log "SentinelOne: Finished installing."
        log ""
    else
        log "Antimalware: An antimalware agent has not been configured for the following os: $os $osVersion"
        log ""
    fi
fi

###################################################################################################
#   Download and install Rapid7 agent
###################################################################################################

if [[ -d "/opt/rapid7/ir_agent" ]]; then
    log "Rapid7: Already installed. Skipping..."
    log ""
else
    log "Rapid7: Downloading agent..."
    wget http://download.calpine.com/DevOps/SecurityAgents/Rapid7/Linux/rapid7-linux.sh
    
    log "Rapid7: Installing agent..."
    chmod u+x rapid7-linux.sh
    ./rapid7-linux.sh install_start --token us:30fc5a4b-de8e-4b6e-b46a-3a979b7781e8
    
    rm rapid7-linux.sh
    
    log "Rapid7: Finished installing."
    log ""
fi

###################################################################################################
#Add to hosts files
###################################################################################################

if [[ $(cat /etc/hosts | grep $computerName -c) = 0 ]];
then 
    echo "" >> /etc/hosts
    echo "$ipAddress $computerName" >> /etc/hosts
    log "Hostname: Added $ipAddress $computerName to /etc/hosts."
    log ""
else
    log "Hostname: $computerName already added to /etc/hosts. Skipping..."
    log ""
fi

if [[ $(cat /etc/hostname | grep $computerName -c) = 0 ]];
then 
    echo "$computerName" >> /etc/hostname
    log "Hostname: Added $computerName to /etc/hostname."
    log ""
else
    log "Hostname: $computerName already added to /etc/hosts. Skipping..."
    log ""
fi

###################################################################################################
#Set the time zone
###################################################################################################

if [[ "$timezone" = "PST" ]]; then
    timedatectl set-timezone America/Los_Angeles
elif [[ "$timezone" = "EST" ]]; then
    timedatectl set-timezone America/New_York
else
    timedatectl set-timezone America/Chicago
fi
systemctl restart systemd-timedated.service
timedatectl

###################################################################################################
#Configure redhat portal subscription
###################################################################################################

if [[ "$os" = "Red Hat Enterprise Linux Server" ]]; then
    if [[ -z "$redhatPortalPassword" ]]; then
        log "Redhat subscription manager password parameter is missing. Skipping subscription register..."
    else
        ##Register in Redhat portal
        subscription-manager register --username calpine_devops --password $redhatPortalPassword
        subscription-manager attach --pool=$redhatSubscriptionPool >> $logFile

        ##RHEL Repos:
        subscription-manager repos --enable=rhel-7-server-optional-rpms
        subscription-manager repos --enable=rhel-7-server-rpms
        subscription-manager repos --enable=rhel-server-rhscl-7-rpms
        yum install -y yum-utils
        yum-config-manager --enable rhel-7-server-devtools-rpms

        if [[ "$computerName" =~ "db" || "$computerName" =~ "ora" ]]; then
            ##Update:-
            yum install -y update

            ##Xauth Installation:-
            yum install -y xauth

            ##Packages installation:-
            yum install -y xorg-x11-xkb-utils-7.7-14.el7.x86_64
            yum install -y xorg-x11-proto-devel-2018.4-1.el7.noarch
            yum install -y xorg-x11-server-Xorg-1.20.1-5.1.el7.x86_64
            yum install -y xorg-x11-drv-void-1.4.1-2.el7.1.x86_64
            yum install -y xorg-x11-drv-void-1.4.1-2.el7.1.x86_64
            yum install -y xorg-x11-apps
            yum install -y net-tools
            yum install -y sysstat
            yum install -y smartmontools
            yum install -y compat-libstdc++-33-3.2.3-72.el7.x86_64
            yum install -y ksh-20120801-139.el7.x86_64
            yum install -y compat-libstdc++-33-3.23
            yum install -y compat-libcap1-1.10
            yum install -y libstdc++-devel-4.8.2
            yum install -y gcc-4.8.2,gcc-4.8.2
            yum install -y gcc-c++-4.8.2
            yum install -y ksh
            yum install -y glibc-devel-2.17
            yum install -y libaio-devel-0.3.109
            yum install -y devtoolset-7
            yum install -y gcc-c++-4.8.5-36.el7_6.1.x86_64
            yum install -y xorg-x11-utils
            yum install -y xorg-x11-server-common-1.20.1-5.2.el7_6.x86_64
            yum install -y xorg-x11-server-Xorg-1.20.1-5.1.el7.x86_64
            yum install -y xorg-x11-xauth-1.0.9-1.el7.x86_64
            yum install -y xorg-x11-xkb-utils-7.7-14.el7.x86_64
            yum install -y xorg-x11-drv-void-1.4.1-2.el7.1.x86_64
            yum install -y xorg-x11-utils-7.5-23.el7.x86_64
            yum install -y libstdc++-devel-4.8.5-36.el7.x86_64 --skip-broken
            yum install -y gcc-c++-4.8.5-36.el7.x86_64 --skip-broken
        fi
    fi
fi

###################################################################################################
#Get service status
###################################################################################################

body+="<b>Computer</b>: $computerName<br/>"
body+="<b>Operating System</b>: $os $osVersion<br/>"
body+="<b>IP Address</b>: $ipAddress<br/>"
body+="<br/><b>Agent Services</b>:<br/>"

for i in splunk taniumclient EracentEUAService EracentEPAService centrifydc ir_agent;
do
    body+="&nbsp;&nbsp;&nbsp;&nbsp;Service '$i' has status of: $(service $i status)<br/>"
done

body+="&nbsp;&nbsp;&nbsp;&nbsp;Service SentinelOne has status of: $(sentinelctl control status)<br/>"

bodyEncoded="$(echo -n $body | iconv -f UTF8 -t UTF16LE | base64)"

###################################################################################################
#Start rapid7 scan
###################################################################################################
log "Starting rapid7 scan..."

encodedLogFile="$(cat $logFile | base64)"
attachmentTemplate='{"FileName": "%s", "Attachment": "%s"}'
attachments=$(printf "$attachmentTemplate" "BuildLog_$computerName.txt" "$encodedLogFile")
attachmentsEncoded="$(echo -n $attachments | iconv -f UTF8 -t UTF16LE | base64)"

bodyTemplate='{"Subscription": "%s", "ComputerName": "%s", "IpAddress": "%s", "EmailBody": "%s", EmailAttachments: "%s"}'
postBody=$(printf "$bodyTemplate" "$subscription" "$computerName" "$ipAddress" "$bodyEncoded" "$attachmentsEncoded")
#log "$(printf "$bodyTemplate" "$computerName" "$ipAddress" "$bodyEncoded" "$postAttachment")"

rapid7FunctionUrl="https://devopspowershellfunctions.app.calpine.com/api/Rapid7Scan?code=Q3XK1Hrz8tffySegoGAPEMUoC96dS48nChkVGXeutvTBIQVqbP9TJw=="
curl -d "$postBody" -H "Content-Type: application/json" -X POST "$rapid7FunctionUrl" -k

###################################################################################################

log "Finished Linux-SetupScript.sh"