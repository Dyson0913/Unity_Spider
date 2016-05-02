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
	public class super7pk_parser : IParser
	{
		public super7pk_parser()
		{
			
		}

		public override packArgs paser(string data)
		{
			Debug.Log ("data = "+data);
			JObject jo = new JObject();
			jo = JsonConvert.DeserializeObject<JObject>(data);
			Dictionary<string,string> pack = new Dictionary<string,string> ();
			string pack_type = jo.Property ("message_type").Value.ToString ();

			if (pack_type == "MsgBPInitialInfo") {
				string _state = jo.Property ("game_state").Value.ToString ();
				pack.Add ("message_type", pack_type);
				pack.Add ("game_state", jo.Property ("game_state").Value.ToString ());
				pack.Add ("game_round", jo.Property ("game_round").Value.ToString ());
				pack.Add ("game_id", jo.Property ("game_id").Value.ToString ());
				pack.Add ("game_type", jo.Property ("game_type").Value.ToString ());
				pack.Add ("remain_time", jo.Property ("remain_time").Value.ToString ());

				JObject p = new JObject ();
				p = JsonConvert.DeserializeObject<JObject> (jo.Property ("cards_info").Value.ToString ());


				pack.Add ("player_card_list", arr_parse_no_token(p.Property ("player_card_list").Value.ToString ()));
				pack.Add ("banker_card_list", arr_parse_no_token(p.Property ("banker_card_list").Value.ToString ()));
				pack.Add ("river_card_list", arr_parse_no_token(p.Property ("river_card_list").Value.ToString ()));
				pack.Add ("extra_card_list", arr_parse_no_token(p.Property ("extra_card_list").Value.ToString ()));

				//history
				//Debug.Log("DK json = "+ jo.Property ("record_list").Value.ToString ());
				if( _state == "StartBetState" || _state =="NewRoundState")
				{
					JToken token = JToken.Parse(data);
					Debug.Log("7pk json = "+ token);
					List<string> winner = new List<string>();
					JArray jarr = token["record_list"] as JArray;
					for (int i=0; i< jarr.Count; i++) {
						JToken recode_token = JToken.Parse(jarr[i].ToString());
						
						winner.Add(recode_token["winner"].ToString());										
					}
					
					pack.Add ("history_winner",string.Join(",",winner.ToArray()));
				}


			} else if (pack_type == "MsgBPState") {
				string _state = jo.Property ("game_state").Value.ToString ();
				pack.Add ("message_type", pack_type);
				pack.Add ("game_state", jo.Property ("game_state").Value.ToString ());
				pack.Add ("game_round", jo.Property ("game_round").Value.ToString ());
				pack.Add ("remain_time", jo.Property ("remain_time").Value.ToString ());

				history_parse(pack,data,_state);

			} else if (pack_type == "MsgBPOpenCard") {
				pack.Add ("message_type", pack_type);
				pack.Add ("game_state", jo.Property ("game_state").Value.ToString ());
				pack.Add ("game_round", jo.Property ("game_round").Value.ToString ());
				pack.Add ("card_type", jo.Property ("card_type").Value.ToString ());


				pack.Add ("card_list", arr_parse_no_token(jo.Property ("card_list").Value.ToString ()));


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
			else if (pack_type == "MsgPlayerBet") 
			{
//				if( jo.Property ("timestamp").Value.ToString () =="1111" || jo.Property ("result").Value.ToString () =="0")
//				{
//					return new packArgs(pack);
//				}
				pack.Add ("message_type", pack_type);
				pack.Add ("result", jo.Property ("result").Value.ToString ());
			}
			else 
			{
				pack.Add ("message_type", "check");
				pack.Add ("_all", jo.ToString ());
			}

			return new packArgs(pack);
		}

		//["jk","kc"] type 
		public string arr_parse_no_token(string str)
		{
			JArray ja = JArray.Parse(str);			
			List<string> pcard = new List<string>();
			foreach(string st in ja)
			{
				pcard.Add(st);
			}
			return string.Join(",",pcard.ToArray());
		}

		public void history_parse (Dictionary<string,string> pack,string data,string state)
		{
			if( state =="NewRoundState")
			{
				JToken token = JToken.Parse(data);
				Debug.Log("7pk json = "+ token);
				List<string> winner = new List<string>();
				JArray jarr = token["record_list"] as JArray;
				for (int i=0; i< jarr.Count; i++) {
					JToken recode_token = JToken.Parse(jarr[i].ToString());
					
					winner.Add(recode_token["winner"].ToString());
				}
				
				pack.Add ("history_winner",string.Join(",",winner.ToArray()));				
			}
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

//					if ( ch.Property (ls[k]).Value.ToString () == "BetBWBonusTwoPair" ||
//					     ch.Property (ls[k]).Value.ToString () == "BetBWBonusTripple")
//					{
//						continue;
//					}
					all[k].Add (ch.Property (ls[k]).Value.ToString ());
				}
			}

			for (int i =0; i< ls.Count; i++) {
				pack.Add (ls [i], String.Join (",", all [i].ToArray ()));
			}
		}
	}

}