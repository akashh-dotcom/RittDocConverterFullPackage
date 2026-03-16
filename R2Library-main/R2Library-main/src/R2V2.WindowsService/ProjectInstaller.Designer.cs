using System.ComponentModel;
using System.ServiceProcess;

namespace R2V2.WindowsService
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
			this.R2v2WindowsService = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessInstaller1
			// 
			this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalService;
			this.serviceProcessInstaller1.Password = null;
			this.serviceProcessInstaller1.Username = null;
			// 
			// R2v2WindowsService
			// 
			this.R2v2WindowsService.Description = "R2 Library Version 2 - Windows Server (Email, Logs, Orders, Etc.)";
			this.R2v2WindowsService.DisplayName = "R2v2 Windows Service";
			this.R2v2WindowsService.ServiceName = "R2v2 Windows Service";
			this.R2v2WindowsService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.R2v2WindowsService});

		}

		#endregion

		private ServiceProcessInstaller serviceProcessInstaller1;
		private ServiceInstaller R2v2WindowsService;
	}
}