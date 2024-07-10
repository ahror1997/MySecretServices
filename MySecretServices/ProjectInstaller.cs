﻿using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace MySecretServices
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		private ServiceProcessInstaller serviceProcessInstaller;
		private ServiceInstaller serviceInstaller;

		public ProjectInstaller()
		{
			serviceProcessInstaller = new ServiceProcessInstaller();
			serviceInstaller = new ServiceInstaller();

			// Configure the Service Account
			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

			// Configure the Service Installer
			serviceInstaller.ServiceName = "My Super Secret Service";
			serviceInstaller.Description = "A Windows Service that includes secret services. v01";
			serviceInstaller.StartType = ServiceStartMode.Automatic;

			Installers.Add(serviceProcessInstaller);
			Installers.Add(serviceInstaller);

			//InitializeComponent();
		}
	}
}
