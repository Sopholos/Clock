using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tomilov.Common.CommonLibrary;

namespace Clock.Controllers
{
	public partial class ApplicationController : ModernEntity
	{
		public static ApplicationController Instance;

		public ApplicationController AppController => this;

		public ApplicationController() : this(isTesting: false) { }

		object instanceLock = new object();
		public ApplicationController(bool isTesting)
		{
			if (!isTesting)
			{
				lock (instanceLock)
				{
					if (Instance != null)
						throw new Exception("Single instance violation");
					Instance = this;
				}
			}

			InitTrack();

			//if (!isTesting)
			//	Start();
		}
	}
}
