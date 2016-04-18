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

		public int get_Point(poker_type type)
		{
			List<string> poker = new List<string> ();
			if (type == poker_type.Player) poker = playercard;
			if (type == poker_type.Banker) poker = bankercard;
			if (type == poker_type.River) poker = rivercard;

			int n = poker.Count;
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
