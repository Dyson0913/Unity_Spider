using UnityEngine;
using System.Collections;
using System;


namespace ConnectModule
{
	public class IParser 
	{
		public IParser()
		{

		}

		public virtual packArgs paser(string data){ return null;}

		public event EventHandler<packArgs> MsgResponse;

		public void packparse_over(packArgs p)
		{
			if (MsgResponse != null) 
			{
				MsgResponse(this, p);
			}
		}
	}
}