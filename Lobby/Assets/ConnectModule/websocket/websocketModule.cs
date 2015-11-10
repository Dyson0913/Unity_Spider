

using System;
//websocket
using WebSocketSharp;


namespace ConnectModule
{
	public class websocketModule :IConnect
	{
		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
//		public static websocketModule Instance
//		{
//			get { return Nested.Instance; }
//		}
//		
//		private class Nested
//		{
//			static Nested()
//			{
//			}
//			internal static readonly websocketModule Instance = new websocketModule();
//		}

		private WebSocket _ws;

		public websocketModule()
		{
				
		}

		public override void create(string url)
		{
			_ws = new WebSocket (url);
			_ws.OnOpen += OnOpen;
			_ws.OnMessage += OnMessage;
			_ws.OnClose += OnClose; 
			_ws.OnError += OnError;
		}

		public override void connect()
		{
			_ws.Connect ();
		}

		public override void close()
		{
			_ws.Close ();
		}

		private void OnOpen(object sender,EventArgs e)
		{

		}

		private void OnMessage(object sender,WebSocketSharp.MessageEventArgs e)
		{
			packArgs pack = parser.paser(e.Data);
			packparse_over(pack);
		}


		private void OnClose(object sender,EventArgs e)
		{
			
		}
		 
		private void OnError(object sender,EventArgs e)
		{
			
		}





	}
}