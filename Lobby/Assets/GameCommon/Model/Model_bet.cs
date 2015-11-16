using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using GameCommon.Model;

namespace GameCommon.Model
{

	public class Model_bet 
	{
//		public static Model Instance
//		{
//			get { return Nested.Instance; }
//		}
//		
//		private class Nested
//		{
//			static Nested()
//			{
//			}
//			internal static readonly Model Instance = new Model();
//		}

		private Model _model = Model.Instance;

		public Dictionary<string,string> zone_mapping;
		public Dictionary<string,int> coin_list;
		public Dictionary<string,int> zone_idx_mapping;
		public Dictionary<string,string> zone_displayname_mapping;

		public Dictionary<string,List<JObject>> zone_bet;
		public List<JObject> queue;

		public Model_bet()
		{
			zone_mapping = new Dictionary<string, string> ();
			zone_idx_mapping = new Dictionary<string, int> ();
			zone_displayname_mapping = new Dictionary<string, string> ();
			coin_list = new Dictionary<string, int> ();
			queue = new List<JObject> ();

			zone_bet = new Dictionary<string, List<JObject>> ();

			define_bet_zone ();
		}

		public virtual void define_bet_zone (){}

		public JObject add_bet(string betname)
		{
			JObject betob = create_betOb (betname);

			//action_queue
			queue.Add (betob);

			return betob;
		}

		public JObject create_betOb(string key)
		{
			int total = get_total(key) + coin_list [_model.getValue ("coin_select")];
			string type = zone_mapping[key];
			JObject ob = new JObject
			{
				{"betType",type},
				{"bet_amount",_model.getValue("coin_select")},
				{"total_bet_amount",total}
			};
			return ob;
		}

		public int get_total(string type)
		{
		   string key = zone_mapping[type];
		   if (!zone_bet.ContainsKey (key)) return 0;

		   List<JObject> betlist = zone_bet[key];
		   int total = 0;
		   for (int i= 0; i< betlist.Count; i++) {
				JObject single = betlist [i];
				total += coin_list[single ["bet_amount"].ToString()];
			}
		   return total;
		}

		public string bet_ok()
		{
			JObject bet = queue [0];
			queue.RemoveAt (0);
			string type = bet["betType"].ToString();


			if (!zone_bet.ContainsKey (type)) {

				List<JObject> j = new List<JObject>();
				j.Add(bet);
				zone_bet.Add (type, j);
			} else {
				List<JObject> betlist = zone_bet [type];
				betlist.Add(bet);
			}

			List<JObject> lis = zone_bet [type];

			string log = "";
			for (int i= 0; i< lis.Count; i++) {
				JObject single = lis[i];
				log += single["betType"].ToString() + " " +  single["bet_amount"].ToString () + " ";
			}
			return log;

		}

		public string clean_bet()
		{
			zone_bet.Clear ();
			return zone_bet.Count.ToString();

		}

		public int zone_idx(string type)
		{
			if (!zone_idx_mapping.ContainsKey (type))
				return -1;
			return zone_idx_mapping[type];
		}

		public string display_name(string type)
		{
			if (!zone_displayname_mapping.ContainsKey (type))
				return "";
			return zone_displayname_mapping [type];
		}

		public int coin_value(string coinType)
		{
			return coin_list[coinType];
		}

	}
}
