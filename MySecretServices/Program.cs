﻿using System.ServiceProcess;

namespace MySecretServices
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new Service()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
