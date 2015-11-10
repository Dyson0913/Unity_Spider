using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ConnectModule;

namespace ConnectModule
{
	public class DK_parser : IParser
	{
		public DK_parser()
		{
			
		}

		public override packArgs paser(string data)
		{
			JObject jo = new JObject();
			jo = JsonConvert.DeserializeObject<JObject>(data);
			Dictionary<string,string> pack = new Dictionary<string,string> ();
			string pack_type = jo.Property ("message_type").Value.ToString ();

			if (pack_type == "MsgBPInitialInfo") {
				pack.Add ("message_type", pack_type);
				pack.Add ("game_state", jo.Property ("game_state").Value.ToString ());
				pack.Add ("game_round", jo.Property ("game_round").Value.ToString ());
				pack.Add ("game_id", jo.Property ("game_id").Value.ToString ());

				JObject p = new JObject ();
				p = JsonConvert.DeserializeObject<JObject> (jo.Property ("cards_info").Value.ToString ());
				pack.Add ("player_card_list", p.Property ("player_card_list").Value.ToString ());
				pack.Add ("banker_card_list", p.Property ("banker_card_list").Value.ToString ());
				pack.Add ("river_card_list", p.Property ("river_card_list").Value.ToString ());
				pack.Add ("extra_card_list", p.Property ("extra_card_list").Value.ToString ());


//				
//				pack.Add ("player_card_list", jo2.Property ("player_card_list").Value.ToString ());
//				pack.Add ("river_card_list", jo2.Property ("river_card_list").Value.ToString ());

				//game_list
//				List<string > poker = new List<string> ();
//				poker.Add ("banker_card_list");
//				poker.Add ("player_card_list");
//				poker.Add ("river_card_list");
//				poker.Add ("extra_card_list");
//				arr_parse (pack, jo.Property ("cards_info").Value.ToString (), poker);

			} else if (pack_type == "MsgBPState") {
				pack.Add ("message_type", pack_type);
				pack.Add ("game_state", jo.Property ("game_state").Value.ToString ());
			} else if (pack_type == "MsgBPOpenCard") {
				pack.Add ("message_type", pack_type);
				pack.Add ("game_state", jo.Property ("game_state").Value.ToString ());
				pack.Add ("card_type", jo.Property ("card_type").Value.ToString ());

				//List<string > poker = new List<string> ();
				//poker.Add ("card_list");
				//arr_parse (pack, jo.Property ("card_list").Value.ToString (), poker);

			}
			else if (pack_type == "MsgBPEndRound") 
			{
				pack.Add ("message_type", pack_type);
				pack.Add ("game_state", jo.Property ("game_state").Value.ToString ());
				List<string > poker = new List<string> ();
				poker.Add ("bet_type");
				poker.Add ("settle_amount");
				poker.Add ("odds");
				poker.Add ("win_state");
				poker.Add ("bet_amount");
				arr_parse (pack, jo.Property ("result_list").Value.ToString (), poker);
			}
			else 
			{
				pack.Add ("message_type", "check");
				pack.Add ("_all", jo.ToString ());
			}

			return new packArgs(pack);
			//			if (jo.Property ("message_type").Value.ToString() == "MsgPlayerCreditUpdate") 
		}

		public void arr_parse(Dictionary<string,string> pack,string arrlist,List<string> ls)
		{
			JArray arr = new JArray ();
			arr = JsonConvert.DeserializeObject<JArray> (arrlist);


			List<List<string>> all = new List<List<string>> ();

			for (int n=0; n < ls.Count; n++) 
			{
				List<string> dynamic_list = new List<string>();
				all.Add(dynamic_list);
			}

			for (int i =0; i< arr.Count; i++) 
			{
				JObject ch = (JObject)arr [i];


				for (int k=0; k< ls.Count; k++) 
				{
					string s = ls[k];
					all[k].Add (ch.Property (ls[k]).Value.ToString ());
				}
			}

			for (int i =0; i< ls.Count; i++) {
				pack.Add (ls [i], String.Join (",", all [i].ToArray ()));
			}
		}
	}

}