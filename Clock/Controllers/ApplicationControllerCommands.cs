using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WPFCommonLibrary.Helpers;

namespace Clock.Controllers
{
	public partial class ApplicationController
	{
		DelegateCommand _CloseApplicationCommand;
		public DelegateCommand CloseApplicationCommand => Command(ref _CloseApplicationCommand, (p) => CloseApplication());

		DelegateCommand _ShowWindowCommand;
		public DelegateCommand ShowWindowCommand => Command(ref _ShowWindowCommand, (p) => ShowWindow(), c => true);


		protected DelegateCommand Command(ref DelegateCommand _command, Action<object> executeMethod, Func<object, bool> canExecuteMethod = null)
		{
			if (_command == null)
			{
				_command = new DelegateCommand
				(
					executeMethod: (p) => executeMethod(p),
					canExecuteMethod: canExecuteMethod
				);
			}

			return _command;
		}

		private void CloseApplication()
		{
			App.Current.MainWindow.Close();
		}

		private async void PureShowWindow()
		{
			await App.Current.InvokeAsync(() =>
			{
				var window = App.Current.MainWindow;
				if (!window.IsVisible)
					window.Show();

				if (window.WindowState == WindowState.Minimized)
					window.WindowState = WindowState.Normal;

				window.Activate();
				window.Topmost = false;
				window.Topmost = true;
				window.Focus();
			});
		}

		private async void PureHideWindow()
		{
			await App.Current.InvokeAsync(() => App.Current.MainWindow.Hide());
		}

		bool forcedToShow = false;


		private void ShowWindow()
		{
			forcedToShow = true;
			PureShowWindow();
		}

		public void HideWindow()
		{
			forcedToShow = false;
			PureHideWindow();
		}

		DateTime lastMove = DateTime.UtcNow.AddDays(-1);

		object locker = new object();

		bool m_bTrackMouse;
		System.Drawing.Point m_ptMouse;
		System.Drawing.Point ptMouse;
		public void NotifyIcon_TrayMouseMove(object sender, RoutedEventArgs e)
		{
			lock (locker)
			{
				m_ptMouse = System.Windows.Forms.Cursor.Position;
				if (!m_bTrackMouse)
				{
					OnMouseHover();
					m_bTrackMouse = true;
				}
			}
		}

		private void OnMouseHover()
		{
			PureShowWindow();
		}

		void InitTrack()
		{
			var thr = new Thread(() => TrackMouse());
			thr.IsBackground = true;
			thr.Start();
		}

		void TrackMouse()
		{
			while (true)
			{
				Thread.Sleep(200);

				if (m_bTrackMouse)
				{
					ptMouse = System.Windows.Forms.Cursor.Position;

					if (ptMouse.X != m_ptMouse.X || ptMouse.Y != m_ptMouse.Y)
					{
						m_bTrackMouse = false;
						OnMouseLeave();
					}
				}
			}
		}

		private void OnMouseLeave()
		{
			if (!forcedToShow)
				PureHideWindow();
		}
	}

	public static partial class Extenders
	{
		public async static Task InvokeAsync(this Application app, Action action, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			if (app?.Dispatcher?.CheckAccess() == false)
			{
				Exception thrown = null;

				var finished = false;
				await app.Dispatcher.BeginInvoke(
					priority,
					(Action)delegate
					{
						try
						{
							action();
						}
						catch (Exception ex)
						{
							thrown = ex;
						}
						finally
						{
							finished = true;
						}
					}
				);

				if (thrown != null)
					throw new ApplicationException("Exception inside delegate", thrown);

				if (!finished)
					throw new ApplicationException($"Invoke logic failed");
			}
			else
				action();
		}

		public async static Task<T> InvokeAsync<T>(this Application app, Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			if (app?.Dispatcher?.CheckAccess() == false)
			{
				Exception thrown = null;

				T result = default;

				var finished = false;
				await app.Dispatcher.BeginInvoke(
					priority,
					(Action)delegate
					{
						try
						{
							result = func();
						}
						catch (Exception ex)
						{
							thrown = ex;
						}
						finally
						{
							finished = true;
						}
					}
				);

				if (thrown != null)
					throw new ApplicationException("Exception inside delegate", thrown);

				if (!finished)
					throw new ApplicationException($"Invoke logic failed");
				else
					return result;
			}
			else
				return func();
		}
	}
}
