

using System;
using System.Collections.Generic;

namespace ConnectModule
{
	public class stringArgs :EventArgs
	{
		public string msg { get; set; }
	
		public stringArgs(string pass_msg)
		{
			msg = pass_msg;
		}
	}	
}


