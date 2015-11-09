

using System.Collections.Generic;
using System;
//websocket
using WebSocketSharp;

//json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConnectModule
{
	public class websocketModule :IConnect
	{
		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static websocketModule Instance
		{
			get { return Nested.Instance; }
		}
		
		private class Nested
		{
			static Nested()
			{
			}
			internal static readonly websocketModule Instance = new websocketModule();
		}

		private WebSocket _ws;

		private websocketModule()
		{
				
		}

		public override void create()
		{
			_ws = new WebSocket ("ws://106.186.116.216:8001/gamesocket/token/c9f0f895fb98ab9159f51fd0297e236d");
			_ws.OnOpen += OnOpen;
			_ws.OnMessage += OnMessage;
			_ws.OnClose += OnClose; 
			_ws.OnError += OnError;
		}

		public override void connect()
		{
			_ws.Connect ();
		}

		private void OnOpen(object sender,EventArgs e)
		{

		}

		private void OnMessage(object sender,WebSocketSharp.MessageEventArgs e)
		{
			JObject jo = new JObject();
			jo = JsonConvert.DeserializeObject<JObject>(e.Data);
			if (jo.Property ("message_type").Value.ToString() == "MsgLogin") 
			{
				Dictionary<string,string> pack = new Dictionary<string,string>();
				pack.Add("message_type",jo.Property("message_type").Value.ToString());
				JObject jo2 = new JObject();
				jo2 = JsonConvert.DeserializeObject<JObject>(jo.Property("player_info").Value.ToString());
				pack.Add("player_name",jo2.Property("player_name").Value.ToString());
				pack.Add("player_credit",jo2.Property("player_credit").Value.ToString());
				packparse_over(new packArgs(pack));
			}
		}


		private void OnClose(object sender,EventArgs e)
		{
			
		}
		 
		private void OnError(object sender,EventArgs e)
		{
			
		}


	}
}