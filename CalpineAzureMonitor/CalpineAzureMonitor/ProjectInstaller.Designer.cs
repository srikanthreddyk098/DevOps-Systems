namespace CalpineAzureMonitor
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CalpineAzureMonitorProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.CalpineAzureMonitorInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // CalpineAzureMonitorProcessInstaller
            // 
            this.CalpineAzureMonitorProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.CalpineAzureMonitorProcessInstaller.Password = null;
            this.CalpineAzureMonitorProcessInstaller.Username = null;
            // 
            // CalpineAzureMonitorInstaller
            // 
            this.CalpineAzureMonitorInstaller.Description = "Custom monitoring solution for Calpine Azure resources.";
            this.CalpineAzureMonitorInstaller.DisplayName = "Calpine Azure Monitor";
            this.CalpineAzureMonitorInstaller.ServiceName = "CalpineAzureMonitor";
            this.CalpineAzureMonitorInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.CalpineAzureMonitorProcessInstaller,
            this.CalpineAzureMonitorInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller CalpineAzureMonitorProcessInstaller;
        private System.ServiceProcess.ServiceInstaller CalpineAzureMonitorInstaller;
    }
}