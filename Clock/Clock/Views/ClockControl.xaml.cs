using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

// source article https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/walkthrough-hosting-a-wpf-clock-in-win32?view=netframeworkdesktop-4.8
namespace Clock.Views
{
	/// <summary>
	/// Interaction logic for ClockControl.xaml
	/// </summary>
	public partial class ClockControl : UserControl
	{
		private DispatcherTimer _dayTimer;

		public ClockControl()
		{
			InitializeComponent();

			Loaded += Clock_Loaded;			
			
			SystemEvents.TimeChanged += SystemEvents_TimeChanged;
		}
		
		private void SystemEvents_TimeChanged(object sender, EventArgs e)
		{
			RefreshClock();
		}

		private void Clock_Loaded(object sender, RoutedEventArgs e)
		{
			// then set up a timer to fire at the start of tomorrow, so that we can update
			// the datacontext
			_dayTimer = new DispatcherTimer { Interval = new TimeSpan(1, 0, 0, 0) - DateTime.Now.TimeOfDay };
			_dayTimer.Tick += OnDayChange;
			_dayTimer.Start();

			RefreshClock();
		}

		private void RefreshClock()
		{
			// set the datacontext to be today's date
			var now = DateTime.Now;
			DataContext = now.Day.ToString("00");

			// finally, seek the timeline, which assumes a beginning at midnight, to the appropriate
			// offset
			var sb = (Storyboard)PodClock.FindResource("sb");
			sb.Begin(PodClock, HandoffBehavior.SnapshotAndReplace, true);
			sb.Seek(PodClock, now.TimeOfDay, TimeSeekOrigin.BeginTime);
		}

		private void OnDayChange(object sender, EventArgs e)
		{
			RefreshClock();

			_dayTimer.Interval = new TimeSpan(1, 0, 0, 0);
		}
	}
}
