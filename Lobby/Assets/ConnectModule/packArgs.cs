

using System;
using System.Collections.Generic;

namespace ConnectModule
{
	public class packArgs :EventArgs
	{
		public Dictionary<string,string> pack { get; set; }
	
		public packArgs(Dictionary<string,string> passpack)
		{
			pack = passpack;
		}
	}	
}


