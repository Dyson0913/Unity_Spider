using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GameCommon.Model;

public class PA_bet : Model_bet
{
	private Model _model = Model.Instance;



	public PA_bet()
	{

	}

	public override void define_bet_zone ()
	{
		zone_mapping.Add ("bet_1", "BetPAEvil");
		zone_mapping.Add ("bet_2", "BetPAAngel");
		zone_mapping.Add ("bet_3", "BetPABigEvil");
		zone_mapping.Add ("bet_4", "BetPABigAngel");
		zone_mapping.Add ("bet_5", "BetPAUnbeatenEvil");
		zone_mapping.Add ("bet_6", "BetPAPerfectAngel");
		zone_mapping.Add ("bet_7", "BetPATiePoint");
		zone_mapping.Add ("bet_8", "BetPABothNone");

		//self
		zone_idx_mapping.Add ("bet_1", 0);
		zone_idx_mapping.Add ("bet_2", 1);
		zone_idx_mapping.Add ("bet_3", 2);
		zone_idx_mapping.Add ("bet_4", 3);
		zone_idx_mapping.Add ("bet_5", 4);
		zone_idx_mapping.Add ("bet_6", 5);
		zone_idx_mapping.Add ("bet_7", 6);
		zone_idx_mapping.Add ("bet_8", 7);

		coin_list.Add ("Coin_0", 5);
		coin_list.Add ("Coin_1", 500);
		coin_list.Add ("Coin_2", 1000);
		coin_list.Add ("Coin_3", 5000);
		coin_list.Add ("Coin_4", 10000);
	}


		



}
