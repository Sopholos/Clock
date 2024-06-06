using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;

[assembly: CLSCompliant(true)]

namespace tomilov.Common.CommonLibrary
{
	// Class marked as compliant.
	[CLSCompliant(true)]
    [Serializable]
    public abstract class BaseClass
	{        
        public delegate void ExceptionEventHandler(object sender, Exception ex);
        [field: NonSerialized]
        public event ExceptionEventHandler ExceptionThrowed;
        
        public delegate void ExceptionExEventHandler(object sender, Exception ex, object Data);
        [field: NonSerialized]
        public event ExceptionExEventHandler ExceptionExThrowed;

		protected virtual void OnException(Exception ex)
		{
			OnException(this, ex);
		}

		protected virtual void OnExceptionEx(Exception ex, object Data)
		{
			OnExceptionEx(this, ex, Data);
		}

		protected virtual void OnException(object sender, Exception ex)
		{
			if (ExceptionThrowed != null)
				ExceptionThrowed(sender, ex);
			else
				throw ex;
		}

		protected virtual void OnExceptionEx(object sender, Exception ex, object Data)
		{
			if (ExceptionExThrowed != null)
				ExceptionExThrowed(sender, ex, Data);
			else
				throw ex;
		}
	}

    [CLSCompliant(true)]
    [Serializable]
    public abstract class LightEntity : BaseClass, INotifyPropertyChanged, IDataErrorInfo
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                try
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
                    PropertyChanged(this, e);
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            }
        }

        protected virtual void ReportPropertyChanged(string property)
        {
            OnPropertyChanged(property);
        }

        private Dictionary<string, string> validationErrors = new Dictionary<string, string>();
        protected virtual void AddError(string columnName, string msg)
        {
            if (!validationErrors.ContainsKey(columnName))
                validationErrors.Add(columnName, msg);
            else
                validationErrors[columnName] = msg;

            ReportPropertyChanged(nameof(HasErrors));
            ReportPropertyChanged(nameof(Error));
        }

        protected virtual void RemoveError(string columnName)
        {
            if (validationErrors.ContainsKey(columnName))
                validationErrors.Remove(columnName);

            ReportPropertyChanged(nameof(HasErrors));
            ReportPropertyChanged(nameof(Error));
        }

		protected virtual bool SetValue<T>(T value, ref T _field, params string[] propertyNames)
			=> SetValue(value, ref _field, true, propertyNames);

		protected virtual bool SetValue<T>(T value, ref T _field, bool checkEquality, params string[] propertyNames)
		{
			if (checkEquality)
			{
				if (_field?.Equals(value) ?? value == null)
					return true;
			}
			
			_field = value;
			foreach (var propertyName in propertyNames)
				ReportPropertyChanged(propertyName);
			return false;
		}

		public virtual bool HasErrors
        {
            get { return (validationErrors.Count > 0); }
        }

        [XmlIgnore]
        public string Error
        {
            get
            {
                if (validationErrors.Count > 0)
                {
                    return $"{this.GetType().Name} data is invalid.";
                }
                else
                {
                    return null;
                }
            }
        }
        
        public string this[string columnName]
        {
            get
            {
                if (validationErrors.ContainsKey(columnName))
                    return validationErrors[columnName];
                else
                    return null;
            }
        }
    }

    [CLSCompliant(true)]
    [Serializable]
    public abstract class ModernEntity : LightEntity, INotifyPropertyChanging
	{
        [field: NonSerialized]
        public event PropertyChangingEventHandler PropertyChanging;

		protected virtual void OnPropertyChanging(string property)
		{
			if (PropertyChanging != null)
			{
				try
				{
					PropertyChangingEventArgs e = new PropertyChangingEventArgs(property);
					PropertyChanging(this, e);
				}				
				catch (Exception ex)
				{
					OnException(ex);
				}
			}
		}

		protected virtual void ReportPropertyChanging(string property)
		{
			OnPropertyChanging(property);
		}

		protected override bool SetValue<T>(T value, ref T _field, bool checkEquality, params string[] propertyNames)
		{
			if (checkEquality)
			{
				if (_field?.Equals(value) ?? value == null)
					return true;
			}

			foreach (var propertyName in propertyNames)
				ReportPropertyChanging(propertyName);
			_field = value;
			foreach (var propertyName in propertyNames)
				ReportPropertyChanged(propertyName);
			return false;
		}
	}
	
	public interface IRunnable
	{
		bool Start();
		void Stop();
		void Pause();
		void Continue();

		bool Starting();
		void Stopping();
		void Pausing();
		void Continuing();		
	}

	public enum RunningStates : int
	{
		Initializing = 0,
		Starting = 100,
		Running = 200,
		Pausing = 250,
		Paused = 260,
		Stopping = 300,
		Stopped = 400,
	}

	[CLSCompliant(true)]
    [Serializable]
    public abstract class Runnable : ModernEntity, IRunnable
	{		
		private RunningStates runningState = RunningStates.Initializing;

		public RunningStates State
		{
			get
			{
				return runningState;
			}
			set
			{
				if (this.runningState == value)
					return;
				ReportPropertyChanging("State");
				runningState = value;
				ReportPropertyChanged("State");
				OnStateChanged();
			}
		}

		public event EventHandler StateChanged;

		protected virtual void OnStateChanged()
		{		
			if (StateChanged != null)
				StateChanged(this, EventArgs.Empty);
		}		
		
		public bool MustContinueRunning { get { return GetMustContinueRunning(); } }
		public bool IsPaused { get { return GetIsPaused(); } }
		public bool IsStopped { get { return GetIsStopped(); } }

		public event EventHandler Started;
		public event EventHandler Stopped;
		public event EventHandler Paused;
		public event EventHandler Continued;		

		public abstract bool Starting();
		public abstract void Stopping();
		public abstract void Pausing();
		public abstract void Continuing();		

		public virtual bool Start()
		{
			if (this.IsPaused)
			{
				Continue();
				return true;
			}
			if (this.IsStopped || this.State == RunningStates.Initializing)
			{

				bool result;
				State = RunningStates.Starting;
				result = Starting();
				return result;
			}
			else
				return false;
		}

		public virtual void Stop()
		{
			if (IsStopped || this.State == RunningStates.Initializing)
				return;

			State = RunningStates.Stopping;
			Stopping();
		}

		public virtual void Pause()
		{
			if (IsPaused)
				return;
			
			if (!MustContinueRunning)
				return;

			State = RunningStates.Pausing;
			Pausing();
		}

		public virtual void Continue()
		{
			State = RunningStates.Running;
			Continuing();
		}
									
		protected virtual bool GetMustContinueRunning()
		{
			WaitInPausedState();

			bool result;
			switch (State)
			{
				case RunningStates.Initializing:
				case RunningStates.Starting:
				case RunningStates.Running:
				case RunningStates.Pausing:
				case RunningStates.Paused:
					{						
						result = true;
						break;
					}
				case RunningStates.Stopping:
				case RunningStates.Stopped:
				default:
					{
						result = false;
						break;
					}				
			}

			return result;
		}

		protected virtual bool GetIsPaused()
		{
			bool result;
			switch (State)
			{				
				case RunningStates.Pausing:
				case RunningStates.Paused:
				{
					result = true;
					break;
				}
				default:
				{
					result = false;
					break;
				}				
			}

			return result;
		}

		protected virtual bool GetIsStopped()
		{
			bool result;
			switch (State)
			{				
				case RunningStates.Stopped:
				case RunningStates.Stopping:
				{
					result = true;
					break;
				}
				default:
				{
					result = false;
					break;
				}				
			}

			return result;
		}		

		protected virtual void WaitInPausedState()
		{
			if (IsPaused)
			{
				OnPaused();

				while (IsPaused)				
					Thread.Sleep(500);				

				if (MustContinueRunning)
					OnContinued();
			}
		}

		protected virtual void OnStarted()
		{
			State = RunningStates.Running;
			if (Started != null)
				Started(this, EventArgs.Empty);
		}

		protected virtual void OnStopped()
		{
			State = RunningStates.Stopped;
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);
		}

		protected virtual void OnPaused()
		{
			State = RunningStates.Paused;
			if (Paused != null)
				Paused(this, EventArgs.Empty);
		}

		protected virtual void OnContinued()
		{
			State = RunningStates.Running;
			if (Continued != null)
				Continued(this, EventArgs.Empty);
		}
	}
}
