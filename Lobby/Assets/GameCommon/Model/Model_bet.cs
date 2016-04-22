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

		//settle
		public List<string> settle { get; set; }
		public List<string> bet { get; set;}

		public Model_bet()
		{
			zone_mapping = new Dictionary<string, string> ();
			zone_idx_mapping = new Dictionary<string, int> ();
			zone_displayname_mapping = new Dictionary<string, string> ();
			coin_list = new Dictionary<string, int> ();
			queue = new List<JObject> ();

			zone_bet = new Dictionary<string, List<JObject>> ();

			settle = new List<string> ();
			bet = new List<string> ();

			define_bet_zone ();
		}

		public virtual void define_bet_zone (){}

		public JObject add_bet(string bet_zone_name)
		{
			JObject betob = create_betOb (bet_zone_name);

			//action_queue
			queue.Add (betob);

			return betob;
		}

		public JObject create_betOb(string bet_zone_name)
		{
			int bet_amount = coin_list [_model.getValue ("coin_select")];
			int total = get_total(bet_zone_name) + bet_amount;
			string type = zone_mapping[bet_zone_name];
			JObject ob = new JObject
			{
				{"betType",type},
				{"bet_amount",bet_amount},
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
				//total += coin_list[single ["bet_amount"].ToString()];
				total += Int32.Parse ( single ["bet_amount"].ToString());
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
			settle.Clear ();
			bet.Clear ();
			return zone_bet.Count.ToString();

		}

		public int zone_idx(string type)
		{
			if (!zone_idx_mapping.ContainsKey (type))
				return -1;
			return zone_idx_mapping[type];
		}

		public void settle_amount(string settle_amount)
		{

			List<string> bet = new List<string>(settle_amount.Split(','));
			//TODO not in here
			bet.RemoveAt (bet.Count-1);
			bet.RemoveAt (bet.Count-1);
			float mytotal = 0;
			foreach (string value in bet) {
				mytotal += float.Parse( value);
			}

			bet.Add (mytotal.ToString ());
			settle = bet;//new List<string>(settle_amount.Split(','));
		}

		public void bet_amount(string bet_amount)
		{
			List<string> my_bet = new List<string>(bet_amount.Split(','));
			my_bet.RemoveAt(my_bet.Count-1);
			my_bet.RemoveAt(my_bet.Count-1);
			float mytotal = 0;
			foreach (string value in my_bet) {
				mytotal += float.Parse( value);
			}
			
			my_bet.Add (mytotal.ToString ());
			bet = my_bet;//new List<string>(bet_amount.Split(','));
		}

		//deprecate
		public string display_name(string type)
		{
			if (!zone_displayname_mapping.ContainsKey (type))
				return "";
			return zone_displayname_mapping [type];
		}

		//deprecate
		public int coin_value(string coinType)
		{
			return coin_list[coinType];
		}

	}
}
