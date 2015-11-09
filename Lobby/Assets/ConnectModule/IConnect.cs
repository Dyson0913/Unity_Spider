using UnityEngine;
using System.Collections;
using System;


namespace ConnectModule
{
	public class IConnect 
	{
		public IConnect()
		{

		}

		public virtual void create(){}
		public virtual void connect(){}
		public virtual void close(){}

		public event EventHandler<packArgs> MsgResponse;

		public void packparse_over(packArgs p)
		{
			if (MsgResponse != null) 
			{
				MsgResponse(this, p);
			}
		}

		public void send_to_Server(){}




	}
}