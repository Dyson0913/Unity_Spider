using System;
using System.Collections;
using System.Collections.Generic;


namespace GameScript.utility
{
	public enum poker_type
	{
		Player =0,
		Banker =1,
		River =2
	}

	public class poker 
	{
		private List<string> playercard;
		private List<string> bankercard;
		private List<string> rivercard;

		public poker()
		{
			playercard = new List<string> ();
			bankercard = new List<string> ();
			rivercard = new List<string> ();
		}

		public void set_all(poker_type type,string card)
		{
			if (type == poker_type.Player) {
				playercard = new List<string>(card.Split(','));
			}
			if (type == poker_type.Banker) {
				bankercard = new List<string>(card.Split(','));
			}
			if (type == poker_type.River) {
				rivercard = new List<string>(card.Split(','));
			}
		}

		public void set_poker(poker_type type,string card)
		{
			if (type == poker_type.Player)  playercard.Add (card);
			if (type == poker_type.Banker)  bankercard.Add (card);
			if (type == poker_type.River)  rivercard.Add (card);
		}

		public int get_count(poker_type type)
		{
			if (type == poker_type.Player)  return playercard.Count;
			if (type == poker_type.Banker)  return bankercard.Count;
			if (type == poker_type.River)  return rivercard.Count;
			return 0;
		}

		public string get_poker(poker_type type)
		{
			if (type == poker_type.Player)  return playercard[playercard.Count-1];
			if (type == poker_type.Banker)  return bankercard[bankercard.Count-1];
			if (type == poker_type.River)  return rivercard[rivercard.Count-1];
			return "";
		}

		public List<int> get_pokser_res_idx(poker_type type)
		{
			List<string> poker = new List<string> ();
			if (type == poker_type.Player) poker = playercard;
			if (type == poker_type.Banker) poker = bankercard;
			if (type == poker_type.River) poker = rivercard;

			List<int> poker_idx_list = new List<int>();
			for( int i=0; i< poker.Count;i++)
			{
				int idx = this.pokerTrans (poker[i]);
				poker_idx_list.Add(idx);
			}

			return poker_idx_list;
		}


		public int pokerTrans(string poker_s)
		{			
			String point = poker_s.Substring(0, 1);
			String color = poker_s.Substring(1, 1);
			
			int myidx = 0;

			if ( color == "s") myidx = 0;
			if ( color == "h") myidx = 13;
			if ( color == "c") myidx = 26;
			if ( color == "d") myidx = 39;
			
			if ( point == "i") myidx += 9;
			else if ( point == "j") myidx += (10);
			else if ( point == "q") myidx += (11);
			else if ( point == "k") myidx += (12);
			else 	myidx +=  Int32.Parse(point) -1 ;
			
			return myidx;
		}

		public int get_Point(poker_type type)
		{
			List<string> poker = new List<string> ();
			if (type == poker_type.Player) poker = playercard;
			if (type == poker_type.Banker) poker = bankercard;
			if (type == poker_type.River) poker = rivercard;

			int n = poker.Count;
			if (n != 2)
				return -1;

			int total = 0;
			for (int i = 0; i < n; i++)
			{
				total += get_baccarat_point(poker[i]);
			}

			total %= 10;
			return total;
		}

		public int get_baccarat_point(string poker)
		{
			string point = poker.Substring (0, 1);
			if (point == "i" || point == "j" || point == "q" || point == "k")
				return 10;

			return  Int32.Parse (point);
		}

		public void clean()
		{
			playercard.Clear();
			bankercard.Clear();
			rivercard.Clear();
		}

	}
}
