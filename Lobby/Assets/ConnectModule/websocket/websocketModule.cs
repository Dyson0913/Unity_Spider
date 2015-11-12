

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

		public override void send_to_Server(string jsonString)
		{
			_ws.Send (jsonString);	
		}

		private void OnOpen(object sender,EventArgs e)
		{
			stringArgs s = new stringArgs ("open");
			socket_state (s);
		}

		private void OnMessage(object sender,WebSocketSharp.MessageEventArgs e)
		{
			packArgs pack = parser.paser(e.Data);
			packparse_over(pack);
		}


		private void OnClose(object sender,WebSocketSharp.CloseEventArgs e)
		{
			stringArgs pack = new stringArgs(e.Code.ToString());
			socket_state(pack);
		}
		 
		private void OnError(object sender,WebSocketSharp.ErrorEventArgs e)
		{
			stringArgs s = new stringArgs(e.Message);
			socket_state (s);
		}





	}
}