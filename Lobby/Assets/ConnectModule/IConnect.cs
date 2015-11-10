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

		public virtual void create(string url){}
		public virtual void connect(){}
		public virtual void close(){}
		public IParser parser { get; set; }


		public event EventHandler<stringArgs> stateResponse;
		public void socket_state(stringArgs state)
		{
			if (stateResponse != null) 
			{
				stateResponse(this, state);
			}
		}

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