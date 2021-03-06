﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ConnectModule;
using GameCommon.Model;
using GameScript.parser;
using GameCommon.StateMachine;
using GameScript.utility;

public class Connect_script_7pk: MonoBehaviour {

	private IConnect _Connector;

	public bool _local_test = true;
	//share item in game
	public UI_Text _ui_gameround;
	public UI_Buttonlist _coin;
	public UI_Text _bet_timer;
	public UI_Text _log;
	public UI_Enable _Coin_item;

	//open public card view
	public List<UI_Image> public_cardlist;
	public List<UI_Image> public_cardback;

	private List<UI_Image> temp_public_cardlist;
	private List<UI_Image> temp_public_cardback;

	//betview
	public List<UI_Text> bet_amount_list;
	public List<Button> _btnlist;
	public List<Image> hisotry_ball;

	//opencard
	public List<UI_Image> cardlist;
	public List<UI_Image> cardback;


	private List<UI_Image> temp_cardlist;
	private List<UI_Image> temp_cardback;

	//settle
	public List<UI_Text> _result_bet;
	public List<UI_Text> _result_settle;
	public List<Image> _result_title;

	//ui
	private List<Button> _coin_list;

	//delegate
	public avalibe _view_avalible;
	public avalibe _win_hint;
	public avalibe _tab_table;

	//TODO no need
	private StateMachine _state_m;

	//data 
	private Model _model = Model.Instance;
	private Model_bet _bet_model;

	public Connect_script_lobby _lobby_proxy;

	//using to avalible view
	private string _state;
	private Dictionary<string,int> state_view_mapping;

	private poker _poker;

	private int phase_msg_start_bet = 1;
	private int phase_msg_stop_bet = 2;
	private int phase_msg_no_credit = 4;

	private string _prebtn;
	private List<int> poker_open;
	private List<string> state_update;

	//poker
	private List<Sprite> _poker_sprite;

	private List<Sprite> _hisotry_sprite;


	//sim_relative
	private List<string> simpack;
	private int simpck_idx;
	private IParser _sim_parser;

	// Use this for initialization
	void Start () {
		poker_open = new List<int> ();
		state_update = new List<string> ();
		_poker_sprite = new List<Sprite> ();
		_hisotry_sprite = new List<Sprite> ();
		state_view_mapping = new Dictionary<string, int> ();

		temp_cardlist = cardlist;
		temp_cardback = cardback;
		temp_public_cardlist = public_cardlist;
		temp_public_cardback = public_cardback;


		state_view_mapping.Add("NewRoundState",1);
		state_view_mapping.Add("StartBetState",36);
		state_view_mapping.Add("EndBetState",40);
		state_view_mapping.Add("OpenState",40);
		state_view_mapping.Add("EndRoundState",16);

		_poker = new poker ();

		//poker load TODO remove to resmgr
		for(int i=1;i< 55 ;i++)
		{
			string id = "poker_"+String.Format("{0:D2}",i);
			Sprite s = (Sprite)Resources.Load<Sprite> ("share/poker/"+id);
			_poker_sprite.Add(s);
		}


		_hisotry_sprite.Add((Sprite)Resources.Load<Sprite> ("dk/history/player_ball"));
		_hisotry_sprite.Add((Sprite)Resources.Load<Sprite> ("dk/history/banker_ball"));
		_hisotry_sprite.Add((Sprite)Resources.Load<Sprite> ("dk/history/tie_ball"));
		_hisotry_sprite.Add((Sprite)Resources.Load<Sprite> ("dk/history/bigwin_ball"));
	}

	public void init()
	{
		//get view item
		_log = GameObject.Find ("Log").GetComponent<UI_Text>();
		_ui_gameround.gameObject.SetActive(true); 
		round_code ("");
		_bet_timer.textContent = "";

		_coin_list = _coin._list;

		//data setting
		_state_m = new StateMachine ();
		_state_m._state = "None";

		_bet_model = new super7pk_bet ();

		foreach (Button bt in _btnlist) 
		{
			string idx = bt.name;
			bt.onClick.AddListener(()=>betType(idx));	
		}



		//change color
		//ColorBlock co= coin_list[0].colors;
		//co.normalColor = Color.red;
		//coin_list[0].colors = co;

		_model.putValue ("coin_select", "Coin_1");
		for (int i=0; i< _coin_list.Count; i++) {
			string idx = i.ToString();
			_coin_list[i].onClick.AddListener(()=>coin_select(idx));			
		}
		_prebtn = "";


		if( _local_test) pc_test ();
		else connect_to_server ();
	}

	public void pc_test()
	{
		simpack =  new List<string>();
		simpck_idx =0;
		//#if UNITY_EDITOR
		 //string test = File.ReadAllText ("pack_DK_normal.txt");
		// TextAsset tet =(TextAsset)Resources.LoadAssetAtPath ("Assets/Resources/" + "pack_DK_normal.txt", typeof(TextAsset));
		//#endif

		TextAsset tet = (TextAsset)Resources.Load<TextAsset> ("pack_7pk");

		JToken token = JToken.Parse(tet.text);
		//Debug.Log("DK json = "+ token);
		JArray jarr = token["packlist"] as JArray;

		for (int i=0; i< jarr.Count; i++) {
			simpack.Add(jarr[i].ToString());
		}

		_sim_parser =  new super7pk_parser ();
	}

	public void connect_to_server()
	{
		Debug.Log ("dk uuid = " + _model.getValue("uuid"));
		_Connector = new websocketModule();
		_Connector.parser = new super7pk_parser ();
		_Connector.create ("ws://www.mm9900.net:8401/gamesocket/token/"+_model.getValue("uuid"));
		_Connector.MsgResponse += OnMessage;
		_Connector.stateResponse += Onstate;
		_Connector.connect ();
	}

	public void leave()
	{
		foreach (Button bt in _btnlist) 
		{
			bt.onClick.RemoveAllListeners();
		}

		for (int i=0; i< _coin_list.Count; i++) {
			_coin_list[i].onClick.RemoveAllListeners();
			
		}


		_view_avalible.auto_set_avalible(0,avalibe.effect.just_open_close);

		_coin.gameObject.SetActive(false);
		log ("");
		round_code ("");
		timer_stop ();
		remove_poker_event ();

		poker_open.Clear ();
		state_update.Clear ();

		_poker.clean ();

		if (_Connector != null) {
			_Connector.close ();
			Debug.Log ("DK leave disconnect ");
		}

	}

	private void error_msg(string msg)
	{
		_lobby_proxy.error_msg (msg);
	}

	private void Onstate(object sender,stringArgs e)
	{
		Debug.Log("DK Onstate = "+ e.msg);
		if( e.msg != "open") error_msg (e.msg);
	}

	private void OnMessage(object sender,packArgs e)
	{
		pack_handel (e);
	}

	public void pack_handel(packArgs e)
	{
		string st = e.pack ["message_type"];
		Debug.Log ("dk pack_handel message = " + st);


		if (st == "MsgBPInitialInfo") 
		{
			_state = e.pack["game_state"];
			Debug.Log ("dk _state = " + _state);
			_state_m._state = _state;
			view_enable();
			
			_model.putValue("game_id",e.pack["game_id"]);
			_model.putValue("game_type",e.pack["game_type"]);
			_model.putValue("game_round",e.pack["game_round"]);
			round_code(_model.getValue("game_round"));



			if( _state== "StartBetState" || _state =="NewRoundState")
			{
				_model.putValue("remain_time",e.pack["remain_time"]);

				_model.putValue("history_winner",e.pack["history_winner"]);

				string card = e.pack["extra_card_list"];
				string no_open = e.pack["river_card_list"];
				Debug.Log("all poker = "+ card +","+no_open);
				if( card !="")
				{
					if( no_open !="") _poker.set_all(poker_type.Player,card +","+no_open);
					else _poker.set_all(poker_type.Player,card);
				}
				//TODO MsgBPState also
				//_Coin_item.item_set ("into_game");
			}
		

			if( _state == "OpenState")
			{
				string card = e.pack["extra_card_list"];
				if( card !="")
				{
					_poker.set_all(poker_type.Player,card );
					if( _poker.get_count(poker_type.Player) == 1)
					{
						poker_open.Add(0);
					}
					if( _poker.get_count(poker_type.Player) ==2)
					{
						poker_open.Add(0);
						poker_open.Add(1);
					}
					if( _poker.get_count(poker_type.Player) ==3)
					{
						poker_open.Add(0);
						poker_open.Add(1);
						poker_open.Add(2);
					}
					if( _poker.get_count(poker_type.Player) ==4)
					{
						poker_open.Add(0);
						poker_open.Add(1);
						poker_open.Add(2);
						poker_open.Add(3);
					}
					if( _poker.get_count(poker_type.Player) ==5)
					{
						poker_open.Add(0);
						poker_open.Add(1);
						poker_open.Add(2);
						poker_open.Add(3);
						poker_open.Add(4);
					}
				}
				
				Debug.Log("pack all p= "+ e.pack["player_card_list"]);
				Debug.Log("pack all b= "+ e.pack["banker_card_list"]);
				Debug.Log("pack all r= "+ e.pack["river_card_list"]);
				Debug.Log("pack all e= "+ e.pack["extra_card_list"]);
			}

			state_update.Add (_state);
			
		}
		if (st == "MsgBPState") 
		{
			_state = e.pack["game_state"];
			Debug.Log ("dk _state = " + _state);
			_state_m._state = _state;


			_model.putValue("game_round",e.pack["game_round"]);
			round_code(_model.getValue("game_round"));

			if( _state =="NewRoundState")
			{
				//TODO hisotry recode

				//switch to public_card
				_view_avalible.auto_set_avalible(2,avalibe.effect.just_open_close);

				//can't put in  new_round_init(), becuase ....you know that
				Debug.Log ("poker clean");
				_poker.clean();
				cardlist = temp_public_cardlist;
				cardback = temp_public_cardback;
				reset_poker_item();
				remove_poker_event ();

			}
			else if( _state =="StartBetState")
			{
				_model.putValue("remain_time",e.pack["remain_time"]);
				//_Coin_item.item_set ("into_game");
				view_enable();
			}
			else if( _state == "EndBetState")
			{
				cardlist = temp_cardlist;
				cardback = temp_cardback;
				reset_poker_item();
				remove_poker_event ();

				view_enable();
			}
			else
			{
				view_enable();
			}



			state_update.Add (_state);

		}
		if (st == "MsgBPOpenCard") 
		{
			_state = e.pack["game_state"];
			Debug.Log ("dk _state = " + _state);
			_state_m._state = _state;

			Debug.Log("carty = "+e.pack["card_type"]);
			Debug.Log("card_list = "+e.pack["card_list"]);
			string cardtype = e.pack["card_type"];

			//public card open
			if( _state =="NewRoundState")
			{
				if( cardtype == "Extra")
				{
					string card = e.pack["card_list"];



					//open public using diff cardlist
					cardlist = temp_public_cardlist;
					cardback = temp_public_cardback;
					int pcount = _poker.get_count(poker_type.Player);
					Debug.Log("MsgBPOpenCard -> NewRoundState pcount = "+pcount);
					cardback [pcount].RotateCard (1,pcount);
					cardback[pcount].CardRotateComplete += card_open_done;

					_poker.set_poker(poker_type.Player,card);
				}
			}

			//laster two card
			if( _state == "OpenState")
			{
				if( cardtype == "River")
				{
					string card = e.pack["card_list"];


					cardlist = temp_cardlist;
					cardback = temp_cardback;
					int pcount = _poker.get_count(poker_type.Player);
					Debug.Log("pcount = "+pcount);
					cardback [pcount].RotateCard (1,pcount);
					cardback[pcount].CardRotateComplete += card_open_done;

					_poker.set_poker(poker_type.Player,card);
				}
			}


			state_update.Add (_state);
		}
		
		if (st == "MsgPlayerBet") 
		{
			if( e.pack["result"] == "0")
			{
				string s = _bet_model.bet_ok();
				
				Debug.Log("bet ok = "+ s);
				string btnname = _model.getValue ("last_btn");
				bet_amount_list [_bet_model.zone_idx (btnname)].textContent = _bet_model.get_total (btnname).ToString();
			}
		}
		if (st == "MsgBPEndRound") 
		{
			_state = e.pack["game_state"];

			_state_m._state = _state;
			view_enable();

			List<string> settle = new List<string>(e.pack["settle_amount"].ToString().Split(','));
			_bet_model.settle_amount(string.Join(",",settle.ToArray()));

			List<string>  bet= new List<string>(e.pack["bet_amount"].ToString().Split(','));
			_bet_model.bet_amount(string.Join(",",bet.ToArray()));


			state_update.Add (_state);


		}

	}

	private void log(string logmsg)
	{
		_log.textContent = logmsg;
	}

	private void round_code(string round)
	{
		_ui_gameround.textContent = round;
	}

	private void timer_start(string time_count)
	{
		_bet_timer.textContent = time_count;
		_bet_timer.countDown = true;
	}

	private void new_round_init()
	{
		//string s = "0,0";
		//_win_hint.set_avalible(new List<string>(s.Split(',')));
		//_win_hint.gameObject.SetActive (false);


	}
	

	private void view_enable()
	{
		int idx = state_view_mapping [_state];
		_view_avalible.auto_set_avalible(idx,avalibe.effect.just_open_close);
	}

	private void start_bet_init()
	{
		_coin.gameObject.SetActive(true);

		timer_start(_model.getValue("remain_time"));

		reset_bet_amount();
		_bet_model.clean_bet();

		_win_hint.gameObject.SetActive (false);



		_lobby_proxy.phase_msg (phase_msg_start_bet,avalibe.effect.fadeout);
	}

	public void fadeout(int idx,bool b)
	{
		Debug.Log ("DK fadeout ");
	}

	private void end_bet_init()
	{
		_coin.gameObject.SetActive(false);
		timer_stop ();


		//_poker.clean();

		//TODO private int phase_msg_no_credit = 2;
		_lobby_proxy.phase_msg (phase_msg_stop_bet,avalibe.effect.fadeout);

		//TODO open five
		Debug.Log ("open five");
		if( _poker.get_count(poker_type.Player) == 1)
		{
			poker_open.Add(0);
		}
		if( _poker.get_count(poker_type.Player) ==2)
		{
			poker_open.Add(0);
			poker_open.Add(1);
		}
		if( _poker.get_count(poker_type.Player) ==3)
		{
			poker_open.Add(0);
			poker_open.Add(1);
			poker_open.Add(2);
		}
		if( _poker.get_count(poker_type.Player) ==4)
		{
			poker_open.Add(0);
			poker_open.Add(1);
			poker_open.Add(2);
			poker_open.Add(3);
		}
		if( _poker.get_count(poker_type.Player) ==5)
		{
			poker_open.Add(0);
			poker_open.Add(1);
			poker_open.Add(2);
			poker_open.Add(3);
			poker_open.Add(4);
		}
	}

	private void open_card_init()
	{

	}
	
	private void end_round_init()
	{

		List<string> bet = _bet_model.bet;
		List<string> settle = _bet_model.settle;
		for(int i=0;i< _result_bet.Count;i++)
		{
			_result_bet[i].textContent = bet[i];
			_result_settle[i].textContent = settle[i];
		}

		//int p_point = _poker.get_Point (poker_type.Player);
		//string s = p_point.ToString()+","+b_point.ToString();
		//_win_hint.set_avalible(new List<string>(s.Split(',')));

		//Invoke("show_settle", 2f);

		//7pk poker 結算clear

	}

	public void show_settle()
	{
		//wait to new switch view	
	}

	public void caculate_point()
	{
		int p_point = _poker.get_Point (poker_type.Player);
		int b_point = _poker.get_Point (poker_type.Banker);
		if (p_point != -1 || b_point != -1) {
			_win_hint.gameObject.SetActive (true);
		}
	}

	private void reset_bet_amount()
	{
	   foreach (UI_Text bet in bet_amount_list) 
	   {	 
		 bet.textContent = "";
		}
	}

	private void direct_turn_poker()
	{	
		List<int> poker_list;
		 poker_list = _poker.get_pokser_res_idx (poker_type.Player);
	
		for (int i=0; i< poker_list.Count; i++) {
			int id = poker_list[i];
			cardlist [i].GetComponent<Image> ().sprite = _poker_sprite [id];
		}


		for (int idx =0; idx< poker_open.Count; idx++) {
			int open_idx = poker_open[idx];
			cardlist[open_idx].rotate_to(0,360,0);
			cardback[open_idx].rotate_to(0,90,0);
			
		}
	}

	private void reset_poker_item()
	{
		for( int i=0;i<cardlist.Count;i++)
		{
			cardlist[i].rotate_to(0,270,0);
		}
		
		for( int i=0;i<cardback.Count;i++)
		{
			cardback[i].rotate_to(0,0,0);
		}
	}

	private void init_poker_event()
	{
		for( int i=0;i<cardlist.Count;i++)
		{
			cardlist[i].CardRotateComplete += card_open_done;
		}

		for( int i=0;i<cardback.Count;i++)
		{		
			cardback[i].CardRotateComplete += card_open_done;
		}
	}

	private void remove_poker_event()
	{

		for( int i=0;i<cardlist.Count;i++)
		{
			cardlist[i].CardRotateComplete -= card_open_done;
		}
		
		for( int i=0;i<cardback.Count;i++)
		{		
			cardback[i].CardRotateComplete -= card_open_done;
		}

	}


	private void timer_stop()
	{
		_bet_timer.stop_count ();		
	}

	public void betType(string btnname)
	{
		Debug.Log ("value = " +btnname);

		JObject bet=  _bet_model.add_bet (btnname);
		//Debug.Log ("ob = "+bet.ToString());

		_model.putValue ("last_btn", btnname);

		if (_local_test) {
			string s = _bet_model.bet_ok();
			Debug.Log("bet ok = "+ s);
			//coin update
			bet_amount_list [_bet_model.zone_idx (btnname)].textContent = _bet_model.get_total (btnname).ToString();
		}
		else {
			JObject ob = new JObject
			{
				{ "id",_model.getValue("uuid")},
				{ "timestamp",1111},
				{"message_type","MsgPlayerBet"},
				{"game_id",_model.getValue("game_id")},
				{"game_type",_model.getValue("game_type")},
				{"game_round",_model.getValue("game_round")},
				{"bet_type", bet["betType"]},
				//{"bet_amount",_bet_model.coin_value(bet["bet_amount"].ToString())},
				{"bet_amount",bet["bet_amount"]},
				{"total_bet_amount",bet["total_bet_amount"]}
			};
			Debug.Log ("ob = "+ob.ToString());
			_Connector.send_to_Server(ob.ToString());
		}

	}

	public void coin_select(string idx)
	{
		string btnname = "Coin_"+idx;
		Debug.Log ("btnnamed "+ idx);
		//color (idx);
		//TODO select effect

		_model.putValue("coin_select",btnname);
		Debug.Log ("coin_select "+ _model.getValue("coin_select"));


		if (_prebtn == btnname) {
			Debug.Log ("coin same return");
			return;
		}

		_prebtn = btnname;
		Debug.Log ("_prebtn ="+ _prebtn);

	}

	void OnDestroy() {
		if (_Connector != null) {
			_Connector.close ();
		}
		Debug.Log ("DK connect destroy");
	}

	public void special_paytable()
	{
		//特牌型賠率
		_tab_table.auto_set_avalible(1,avalibe.effect.just_open_close);
	}

	public void main_paytable()
	{
		//主注賠率
		_tab_table.auto_set_avalible(2,avalibe.effect.just_open_close);
	}

	public void history_table()
	{
		if (_state == "OpenState")
			return;

		//歷史
		_tab_table.auto_set_avalible(4,avalibe.effect.just_open_close);

		string s = _model.getValue ("history_winner");
		List<string> win_type = new List<string> (s.Split (','));

		for (int i=0; i< win_type.Count; i++) {
			//Sprite sp = null;
			int idx = 0;
			if( win_type[i] == "BetBWPlayer")  idx =0;
			else if( win_type[i] == "BetBWBanker") idx = 2;
			else if( win_type[i] == "None") idx = 4;
			else
			{
				string win_str ="";
				if( win_type[i] == "WSBWStraight") win_str = "STR";
				if( win_type[i] == "WSBWFlush") win_str = "FLU";
				if( win_type[i] == "WSBWFullHouse") win_str = "FUH";
				if( win_type[i] == "WSBWFourOfAKind") win_str = "4K";
				if( win_type[i] == "WSBWStraightFlush") win_str = "STF";
				if( win_type[i] == "WSBWRoyalFlush") win_str = "RTF";
				idx =2;
			}
			//avalibe myball = _hisotry_avalibe[i];
			//Debug.Log("ball = "+ myball);
			//ball.auto_set_avalible(idx,avalibe.effect.just_open_close);
		}


	}

	public void close_all_table()
	{
		//
		_tab_table.auto_set_avalible(0,avalibe.effect.just_open_close);
	}

	public void single_test()
	{
//		List<int> poker_list =_poker.get_pokser_res_idx (poker_type.Player);
//		for (int i=0; i< poker_list.Count; i++) {
//			int id = poker_list[i];
//			cardlist [i].GetComponent<Image> ().sprite = _poker_sprite [id]; //Your sprite
//			cardlist [i].RotateCard (2,0);
//			cardlist[i].CardRotateComplete += dispayer_card_open_done;
//		}


	}

	void Update () {

		if (poker_open.Count != 0) {
			
			direct_turn_poker();
			
			poker_open.Clear();
			//caculate_point();
		}

		if (state_update.Count != 0) {
			string state = state_update[0];
			Debug.Log("state_update = "+ state);
			if( state == "NewRoundState") new_round_init();
			if( state == "StartBetState")start_bet_init();
			if( state == "EndBetState")end_bet_init();
			if( state == "OpenState") open_card_init();
			if( state == "EndRoundState") end_round_init();


			state_update.Clear();
		}

	}

	private void card_open_done(object sender,stringArgs e)
	{
		cardback[Int32.Parse(e.msg)].CardRotateComplete -= card_open_done;

		//change poker
		int idx = Int32.Parse (e.msg);
		int poker_idx = 0;
		string poker ="";

		//單張處理
		poker = _poker.get_poker (poker_type.Player);
		poker_idx = _poker.pokerTrans (poker);

		cardlist [idx].GetComponent<Image> ().sprite = _poker_sprite [poker_idx]; //Your sprite
		cardlist [idx].RotateCard (2,idx);
		cardlist[idx].CardRotateComplete += dispayer_card_open_done;

	}

	private void dispayer_card_open_done(object sender,stringArgs e)
	{
		cardback[Int32.Parse(e.msg)].CardRotateComplete -= dispayer_card_open_done;
		caculate_point();
	}

	public void sim_pack()
	{
		Debug.Log ("sim pack ="+simpack[simpck_idx]);
		packArgs pack = _sim_parser.paser(simpack[simpck_idx]);
		if (pack.pack.Count == 0) {
			Debug.Log ("sim pack drop=");
		} else {
			pack_handel (pack);
		}
		simpck_idx = (simpck_idx + 1) % simpack.Count;

	}
	
}
