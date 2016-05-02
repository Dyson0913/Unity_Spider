using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GameCommon.Model;

public class super7pk_bet : Model_bet
{
	private Model _model = Model.Instance;



	public super7pk_bet()
	{

	}

	public override void define_bet_zone ()
	{
		zone_mapping.Add ("bet_1", "BetS7PKNone");
		zone_mapping.Add ("bet_2", "BetS7PKOnePair");
		zone_mapping.Add ("bet_3", "BetS7PKTwoPair");
		zone_mapping.Add ("bet_4", "BetS7PKTripple");
		zone_mapping.Add ("bet_5", "BetS7PKStraight");
		zone_mapping.Add ("bet_6", "BetS7PKFlush");
		zone_mapping.Add ("bet_7", "BetS7PKFullHouse");
		zone_mapping.Add ("bet_8", "BetS7PKFourOfAKind");
		zone_mapping.Add ("bet_9", "BetS7PKStraightFlush");
		zone_mapping.Add ("bet_10", "BetS7PKFiveOfAKind");
		zone_mapping.Add ("bet_11", "BetS7PKRoyalFlush");
		zone_mapping.Add ("bet_12", "BetS7PKPureRoyalFlush");

		//self
		zone_idx_mapping.Add ("bet_1", 0);
		zone_idx_mapping.Add ("bet_2", 1);
		zone_idx_mapping.Add ("bet_3", 2);
		zone_idx_mapping.Add ("bet_4", 3);
		zone_idx_mapping.Add ("bet_5", 4);
		zone_idx_mapping.Add ("bet_6", 5);
		zone_idx_mapping.Add ("bet_7", 6);
		zone_idx_mapping.Add ("bet_8", 7);
		zone_idx_mapping.Add ("bet_9", 8);
		zone_idx_mapping.Add ("bet_10", 9);
		zone_idx_mapping.Add ("bet_11", 10);
		zone_idx_mapping.Add ("bet_12", 11);


		coin_list.Add ("Coin_0", 5);
		coin_list.Add ("Coin_1", 500);
		coin_list.Add ("Coin_2", 1000);
		coin_list.Add ("Coin_3", 5000);
		coin_list.Add ("Coin_4", 10000);

	

	}


		



}
