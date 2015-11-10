using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ConnectModule;

namespace GameScript.parser
{
	public class lobby_parser : IParser
	{
		public lobby_parser()
		{
			
		}

		public override packArgs paser(string data)
		{
			JObject jo = new JObject();
			jo = JsonConvert.DeserializeObject<JObject>(data);
			Dictionary<string,string> pack = new Dictionary<string,string> ();
			string pack_type = jo.Property ("message_type").Value.ToString ();

			if (pack_type == "MsgLogin")
			{
				pack.Add ("message_type", pack_type);
				JObject jo2 = new JObject ();
				jo2 = JsonConvert.DeserializeObject<JObject> (jo.Property ("player_info").Value.ToString ());
				pack.Add ("player_name", jo2.Property ("player_name").Value.ToString ());
				pack.Add ("player_credit", jo2.Property ("player_credit").Value.ToString ());
				pack.Add ("player_uuid", jo2.Property ("player_uuid").Value.ToString ());
				
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

			} else if (pack_type == "MsgKeepLive") 
			{
				pack.Add ("message_type", pack_type);
			}
			else 
			{
				pack.Add ("message_type", "check");
				pack.Add ("_all", jo.ToString ());
			}

			return new packArgs(pack);
			//			if (jo.Property ("message_type").Value.ToString() == "MsgPlayerCreditUpdate") 
		}
	}

}