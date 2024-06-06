using Clock.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clock.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Top = Properties.Settings.Default.Top;
			Left = Properties.Settings.Default.Left;

			Hide();
		}

		private void Window_LostFocus(object sender, RoutedEventArgs e)
		{
			ApplicationController.Instance.HideWindow();
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			ApplicationController.Instance.HideWindow();
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var relativePosition = Mouse.GetPosition(this);
			var absolutePosition = PointToScreen(relativePosition);

			var border = 0;
			var screenArea = Screen.PrimaryScreen.WorkingArea;

			if (absolutePosition.X < border)
				Left = 0;

			if (absolutePosition.Y < border)
				Top = 0;

			if (absolutePosition.X > screenArea.Width - Width - border)
				Left = screenArea.Width - Width;

			if (absolutePosition.Y > screenArea.Height - Height - border)
				Top = screenArea.Height - Height;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Properties.Settings.Default.Top = Top;
			Properties.Settings.Default.Left = Left;

			Properties.Settings.Default.Save();
		}
	}
}