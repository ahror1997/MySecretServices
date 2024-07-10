using System.Diagnostics;
using System.ServiceProcess;
using System.Web.Http.SelfHost;
using System.Web.Http;

namespace MySecretServices
{
	public partial class Service : ServiceBase
	{
		private HttpSelfHostServer server;

		public Service()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			string baseAddress = "http://localhost:8123/";

			var config = new HttpSelfHostConfiguration(baseAddress);

			// Enable attribute routing
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "API Default",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			// Register all controller types in the current assembly
			//config.Services.Replace(typeof(IHttpControllerTypeResolver), new DefaultHttpControllerTypeResolver());

			server = new HttpSelfHostServer(config);
			server.OpenAsync().Wait();

			EventLog.WriteEntry("Web API self-host server started at " + baseAddress, EventLogEntryType.Information);
		}

		protected override void OnStop()
		{
			server.CloseAsync().Wait();
			server.Dispose();
			EventLog.WriteEntry("Web API self-host server stopped", EventLogEntryType.Information);
		}
	}
}
