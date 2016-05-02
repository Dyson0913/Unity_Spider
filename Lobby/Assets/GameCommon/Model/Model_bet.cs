using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using GameCommon.Model;

namespace GameCommon.Model
{
	//single bet  
	// 1. add_bet()
	// 2. server return  ok
	// 3. bet_ok()

	//uncheck bet(one zone) 
	//1. add_bet_local() 
	//2. ready to send  comfirm_bet() 
	//3. server return  ok  
	//4. bet_ok()

	public class Model_bet 
	{
		private Model _model = Model.Instance;

		public Dictionary<string,string> zone_mapping;
		public Dictionary<string,int> coin_list;
		public Dictionary<string,int> zone_idx_mapping;

		public Dictionary<string,List<JObject>> zone_bet;
		public List<JObject> queue;

		//settle
		public List<string> settle { get; set; }
		public List<string> bet { get; set;}

		public List<JObject> one_zone_temp_bet;

		public Model_bet()
		{
			zone_mapping = new Dictionary<string, string> ();
			zone_idx_mapping = new Dictionary<string, int> ();
			coin_list = new Dictionary<string, int> ();
			queue = new List<JObject> ();

			zone_bet = new Dictionary<string, List<JObject>> ();
			one_zone_temp_bet = new List<JObject> ();


			settle = new List<string> ();
			bet = new List<string> ();

			define_bet_zone ();
		}

		public virtual void define_bet_zone (){}

		public void add_bet_local(string bet_zone_name)
		{
			int bet_amount = coin_list [_model.getValue ("coin_select")];
			string type = zone_mapping[bet_zone_name];
			JObject ob = new JObject
			{
				{"betType",type},
				{"bet_amount",bet_amount}
			};

			one_zone_temp_bet.Add (ob);
		}

		public JObject comfirm_bet()
		{
			JObject betob = sum_temp_bet ();

			//action_queue
			queue.Add (betob);			
			return betob;
		}

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

		public JObject sum_temp_bet()
		{
			int cnt = one_zone_temp_bet.Count;
			int total = 0;
			for (int i= 0; i< cnt; i++) {
				JObject single = one_zone_temp_bet [i];
				total += Int32.Parse ( single ["bet_amount"].ToString());
			}

			JObject one_remcode = one_zone_temp_bet [0];
			string type = one_remcode ["betType"].ToString ();
			JObject ob = new JObject
			{
				{"betType",type},
				{"bet_amount",total},
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

			one_zone_temp_bet.Clear ();
		}

		public List<int> get_zone_amount()
		{
			List<int> zone_amount = new List<int> ();

			int cnt = zone_mapping.Count;
			for (int i= 0; i<cnt; i++) {
				string zone_name = "bet_"+(i+1);
				string type = zone_mapping[zone_name];
				List<JObject> lis = zone_bet [type];
				
				int zone_total = 0;
				for (int k= 0; k< lis.Count; k++) {
					JObject single = lis[k];
					zone_total += Int32.Parse (single["total_bet_amount"].ToString());
				}
				zone_amount.Add(zone_total);
			}

			return zone_amount;
		}

		public string clean_bet()
		{
			one_zone_temp_bet.Clear ();
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
			float mytotal = 0;
			foreach (string value in my_bet) {
				mytotal += float.Parse( value);
			}
			
			my_bet.Add (mytotal.ToString ());
			bet = my_bet;//new List<string>(bet_amount.Split(','));
		}



	}
}
