using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFCommonLibrary.Helpers
{
	public abstract class BaseDelegateCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;
		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

		public bool IsExecuting { get; protected set; }

		protected Func<object, bool> canExecuteMethod { get; private set; }
		public BaseDelegateCommand(Func<object, bool> canExecuteMethod) => this.canExecuteMethod = canExecuteMethod;

		public bool CanExecute(object parameter) => !IsExecuting && (canExecuteMethod?.Invoke(parameter) ?? true);

		protected abstract void ActuallyExecute(object parameter);

		public void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				try
				{
					IsExecuting = true;
					ActuallyExecute(parameter);
				}
				finally
				{
					IsExecuting = false;
				}
			}

			RaiseCanExecuteChanged();
		}
	}

	public class DelegateCommand : BaseDelegateCommand
	{
		protected Action<object> executeMethod { get; private set; }
		public DelegateCommand(Action<object> executeMethod) : this(executeMethod, null) { }
		public DelegateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod) : base(canExecuteMethod)
			=> this.executeMethod = executeMethod ?? throw new ArgumentNullException(nameof(executeMethod));

		protected override void ActuallyExecute(object parameter) => executeMethod(parameter);
	}

	public interface IAsyncCommand : ICommand
	{
		Task ExecuteAsync(object parameter);
	}

	public class DelegateCommandAsyncPure : BaseDelegateCommand, IAsyncCommand
	{
		protected Func<object, Task> executeMethod { get; private set; }
		public DelegateCommandAsyncPure(Func<object, Task> executeMethod) : this(executeMethod, null) { }
		public DelegateCommandAsyncPure(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod) : base(canExecuteMethod)
			=> this.executeMethod = executeMethod ?? throw new ArgumentNullException(nameof(executeMethod));

		protected override async void ActuallyExecute(object parameter) => await ExecuteAsync(parameter);

		public async Task ExecuteAsync(object parameter) => await executeMethod(parameter);
	}

	public class DelegateCommandAsync : DelegateCommand
	{
		public DelegateCommandAsync(Action<object> executeMethod) : this(executeMethod, null) { }

		public DelegateCommandAsync(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
			: base(executeMethod, canExecuteMethod) { }

		protected override void ActuallyExecute(object parameter) => Task.Run(() => executeMethod(parameter));
	}
}
