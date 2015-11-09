

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

		public override void close()
		{
			_ws.Close ();
		}

		private void OnOpen(object sender,EventArgs e)
		{

		}

		private void OnMessage(object sender,WebSocketSharp.MessageEventArgs e)
		{
			JObject jo = new JObject();
			jo = JsonConvert.DeserializeObject<JObject>(e.Data);
			Dictionary<string,string> pack = new Dictionary<string,string> ();
			if (jo.Property ("message_type").Value.ToString () == "MsgLogin") {

				pack.Add ("message_type", jo.Property ("message_type").Value.ToString ());
				JObject jo2 = new JObject ();
				jo2 = JsonConvert.DeserializeObject<JObject> (jo.Property ("player_info").Value.ToString ());
				pack.Add ("player_name", jo2.Property ("player_name").Value.ToString ());
				pack.Add ("player_credit", jo2.Property ("player_credit").Value.ToString ());

				//game_list
				JArray jo3 = new JArray ();
				jo3 = JsonConvert.DeserializeObject<JArray> (jo.Property ("game_list").Value.ToString ());

				List<string> web = new List<string>();
				List<string> game_description = new List<string>();
				List<string> game_online = new List<string>();
				List<string> game_type = new List<string>();
				List<string> game_id = new List<string>();
				List<string> game_avaliable = new List<string>();
				for(int i =0;i< jo3.Count;i++)
				{
					JObject ch = (JObject)jo3[i];
					web.Add(ch.Property("game_website").Value.ToString());
					game_description.Add(ch.Property("game_description").Value.ToString());
					game_online.Add(ch.Property("game_online").Value.ToString());
					game_type.Add(ch.Property("game_type").Value.ToString());
					game_id.Add(ch.Property("game_id").Value.ToString());
					game_avaliable.Add(ch.Property("game_avaliable").Value.ToString());
				}
				pack.Add("web",String.Join(",",web.ToArray()));
				pack.Add("game_online",String.Join(",",game_description.ToArray()));
				pack.Add("game_type",String.Join(",",game_type.ToArray()));
				pack.Add("game_id",String.Join(",",game_id.ToArray()));
				pack.Add("game_avaliable",String.Join(",",game_avaliable.ToArray()));

				packparse_over (new packArgs (pack));
			} else if (jo.Property ("message_type").Value.ToString () == "MsgKeepLive") 
			{
				//TODO 
			}
			else 
			{
				pack.Add ("message_type", "check");
				pack.Add ("_all", jo.ToString ());
				packparse_over (new packArgs (pack));
			}

//			if (jo.Property ("message_type").Value.ToString() == "MsgPlayerCreditUpdate") 
		}


		private void OnClose(object sender,EventArgs e)
		{
			
		}
		 
		private void OnError(object sender,EventArgs e)
		{
			
		}


		


	}
}