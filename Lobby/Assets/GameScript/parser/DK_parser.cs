using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ConnectModule;


using GameCommon.Model;

namespace GameScript.parser
{
	public class DK_parser : IParser
	{
		private Model _model = Model.Instance;

		public DK_parser()
		{
			
		}

		public override packArgs paser(string data)
		{
			Debug.Log ("data = "+data);
			Dictionary<string,string> pack = new Dictionary<string,string> ();
			JToken init = JToken.Parse(data);
			string pack_type = init["message_type"].ToString();

			_model.putValue ("dk_message_type",pack_type);
			_model.putValue ("dk_game_id",init["game_id"].ToString());
			_model.putValue ("dk_game_round",init["game_round"].ToString());
			_model.putValue ("dk_game_type",init["game_type"].ToString());

			if (pack_type == "MsgPlayerBet") 
			{
				//				if( jo.Property ("timestamp").Value.ToString () =="1111" || jo.Property ("result").Value.ToString () =="0")
				//				{
				//					return new packArgs(pack);
				//				}
				pack.Add ("result",init["result"].ToString());
				return new packArgs(pack);
			}

			_model.putValue ("dk_game_state",init["game_state"].ToString());

			//old 
			JObject jo = new JObject();
			jo = JsonConvert.DeserializeObject<JObject>(data);

			if (pack_type == "MsgBPInitialInfo") {
				string _state = init["game_state"].ToString();

				_model.putValue ("dk_remain_time",init["remain_time"].ToString());

				JToken cards = JToken.Parse(init["cards_info"].ToString());
				JArray p_card_jarr = cards["player_card_list"] as JArray;
				JArray b_card_jarr = cards["banker_card_list"] as JArray;
				JArray r_card_jarr = cards["river_card_list"] as JArray;
				JArray e_card_jarr = cards["extra_card_list"] as JArray;
				_model.putValue ("player_card_list", Jarr_parse_no_token(p_card_jarr));
				_model.putValue ("banker_card_list", Jarr_parse_no_token(b_card_jarr));
				_model.putValue ("river_card_list", Jarr_parse_no_token(r_card_jarr));
				_model.putValue ("extra_card_list", Jarr_parse_no_token(e_card_jarr));

				//history
				//Debug.Log("DK json = "+ jo.Property ("record_list").Value.ToString ());
				if( _state == "StartBetState" || _state =="NewRoundState")
				{
					JToken token = JToken.Parse(data);
					Debug.Log("DK json = "+ token);
					List<string> winner = new List<string>();
					List<string> point = new List<string>();
					List<string> player_pair = new List<string>();
					List<string> banker_pair = new List<string>();
					JArray jarr = token["record_list"] as JArray;
					for (int i=0; i< jarr.Count; i++) {
						JToken recode_token = JToken.Parse(jarr[i].ToString());
						
						winner.Add(recode_token["winner"].ToString());
						point.Add(recode_token["point"].ToString());
						player_pair.Add(recode_token["player_pair"].ToString());
						banker_pair.Add(recode_token["banker_pair"].ToString());
					}
					
					_model.putValue ("history_winner",string.Join(",",winner.ToArray()));
					_model.putValue ("history_point",string.Join(",",point.ToArray()));
					_model.putValue ("history_player_pair",string.Join(",",player_pair.ToArray()));
					_model.putValue ("history_banker_pair",string.Join(",",player_pair.ToArray()));
					
				}


			} else if (pack_type == "MsgBPState") {

				_model.putValue ("dk_remain_time",init["remain_time"].ToString());

				string _state = init["game_state"].ToString();
				history_parse(pack,data,_state);

			} else if (pack_type == "MsgBPOpenCard") {

				_model.putValue ("card_type",init["card_type"].ToString());
				_model.putValue ("card_list", arr_parse_no_token(init["card_list"].ToString ()));

				//"cards_bigwin_prob": [0.00017,0.011068,0.072029,0.815305,1.00866,2.15512]
				JToken token = JToken.Parse(data);
				JArray jarr = token["cards_bigwin_prob"] as JArray;
				_model.putValue("dk_prob", Jarr_parse_no_token(jarr));


			}
			else if (pack_type == "MsgBPEndRound") 
			{
				_model.putValue ("dk_remain_time",init["remain_time"].ToString());
				string _state = init["game_state"].ToString();
				List<string > poker = new List<string> ();
				poker.Add ("bet_type");
				poker.Add ("settle_amount");
				poker.Add ("odds");
				poker.Add ("win_state");
				poker.Add ("bet_amount");
				arr_parse (pack, jo.Property ("result_list").Value.ToString (), poker);
			}


			return new packArgs(pack);
		}

		public string Jarr_parse_no_token(JArray data)
		{
			if (data == null)
				return "";

			List<string> raw_data = new List<string>();
			for (int i=0; i< data.Count; i++) {
				raw_data.Add(data[i].ToString());
			}
			return string.Join (",", raw_data.ToArray ());
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
				Debug.Log("DK json = "+ token);
				List<string> winner = new List<string>();
				List<string> point = new List<string>();
				List<string> player_pair = new List<string>();
				List<string> banker_pair = new List<string>();
				JArray jarr = token["record_list"] as JArray;
				for (int i=0; i< jarr.Count; i++) {
					JToken recode_token = JToken.Parse(jarr[i].ToString());
					
					winner.Add(recode_token["winner"].ToString());
					point.Add(recode_token["point"].ToString());
					player_pair.Add(recode_token["player_pair"].ToString());
					banker_pair.Add(recode_token["banker_pair"].ToString());
				}
				
				pack.Add ("history_winner",string.Join(",",winner.ToArray()));
				pack.Add ("history_point",string.Join(",",point.ToArray()));
				pack.Add ("history_player_pair",string.Join(",",player_pair.ToArray()));
				pack.Add ("history_banker_pair",string.Join(",",player_pair.ToArray()));
				
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