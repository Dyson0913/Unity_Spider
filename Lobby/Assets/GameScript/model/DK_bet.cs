using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GameCommon.Model;

public class DK_bet : Model_bet
{
	private Model _model = Model.Instance;



	public DK_bet()
	{

	}

	public override void define_bet_zone ()
	{
		zone_mapping.Add ("bet_1", "BetBWPlayer");
		zone_mapping.Add ("bet_2", "BetBWBanker");
		zone_mapping.Add ("bet_3", "BetBWTiePoint");
		zone_mapping.Add ("bet_4", "BetBWBankerPair");
		zone_mapping.Add ("bet_5", "BetBWPlayerPair");
		zone_mapping.Add ("bet_6", "BetBWSpecial");

		//self
		zone_idx_mapping.Add ("bet_1", 0);
		zone_idx_mapping.Add ("bet_2", 1);
		zone_idx_mapping.Add ("bet_3", 2);
		zone_idx_mapping.Add ("bet_4", 3);
		zone_idx_mapping.Add ("bet_5", 4);
		zone_idx_mapping.Add ("bet_6", 5);

		coin_list.Add ("Coin_1", 100);
		coin_list.Add ("Coin_2", 500);
		coin_list.Add ("Coin_3", 1000);
		coin_list.Add ("Coin_4", 5000);
		coin_list.Add ("Coin_5", 10000);

		zone_displayname_mapping.Add ("BetBWPlayer", "閒家");
		zone_displayname_mapping.Add ("BetBWBanker", "莊家");
		zone_displayname_mapping.Add ("BetBWTiePoint", "合");
		zone_displayname_mapping.Add ("BetBWBankerPair", "莊對");
		zone_displayname_mapping.Add ("BetBWPlayerPair", "閒對");
		zone_displayname_mapping.Add ("BetBWSpecial", "特殊牌型");

	}


		



}
