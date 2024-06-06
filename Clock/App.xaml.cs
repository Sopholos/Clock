using Clock.Controllers;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Clock
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static TaskbarIcon notifyIcon;

		[STAThread]
		public static void Main()
		{
			var application = new App();

			application.Init();
			application.Run();

			notifyIcon?.Dispose();
		}
		
		public void Init()
		{
			this.InitializeComponent();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			try
			{
				var ap = new ApplicationController();
				notifyIcon = (TaskbarIcon)this.FindResource("NotifyIcon");
				notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
				notifyIcon.DataContext = ap;
				notifyIcon.TrayMouseMove += ap.NotifyIcon_TrayMouseMove;				
			}
			catch
			{
				this.Shutdown();
				return;
			}
		}
	}
}
