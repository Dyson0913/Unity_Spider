using System;
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
using ProgressBar;

public class Connect_script_DK: MonoBehaviour {

	private IConnect _Connector;

	public bool _local_test = true;
	//share item in game
	public UI_Text _ui_gameround;
	public UI_Buttonlist _coin;
	public UI_Text _log;
	public UI_Enable _Coin_item;
	public GameObject _digi_timer_settle;

	public Sprite _sp_table_normal;
	public Sprite _sp_table_press;

	public Sprite _main_table_normal;
	public Sprite _main_table_press;

	public GameObject _top_bar;

	//betview
	public List<UI_Text> bet_amount_list;
	public List<Button> _btnlist;
	public List<Image> hisotry_ball;
	public List<ProgressBarBehaviour>  _prob_bar;
	public Sprite _red_prob_sprite;
	public Sprite _green_prob_sprite;
	public GameObject _mi_pokerboard;
	public GameObject _digi_timer_bet;

	//opencard
	public List<UI_Image> cardlist;
	public List<UI_Image> cardback;
	public UI_Text _player_point;
	public UI_Text _banker_point;

	//settle
	public List<UI_Text> _result_bet;
	public List<UI_Text> _result_settle;

	//ui
	private List<Button> _coin_list;

	//delegate
	public avalibe _view_avalible;
	public avalibe _win_hint;
	public avalibe _tab_table;
	public avalibe _paytable_avalible;

	//TODO no need
	private StateMachine _state_m;

	//data 
	private Model _model = Model.Instance;
	private Model_bet _bet_model;
	private int _last_high_prob_idx;

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

	private List<Sprite> _mobile_poker_sprite;
	private List<Sprite> _digit_num;

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
		_mobile_poker_sprite = new List<Sprite> ();
		_digit_num = new List<Sprite> ();

		_hisotry_sprite = new List<Sprite> ();
		state_view_mapping = new Dictionary<string, int> ();

		state_view_mapping.Add("NewRoundState",1);
		state_view_mapping.Add("StartBetState",18);
		state_view_mapping.Add("EndBetState",20);
		state_view_mapping.Add("OpenState",20);
		state_view_mapping.Add("EndRoundState",8);

		_poker = new poker ();

		//poker load TODO remove to resmgr
		for(int i=1;i< 55 ;i++)
		{
			string id = "poker_"+String.Format("{0:D2}",i);
			Sprite s = (Sprite)Resources.Load<Sprite> ("share/poker/"+id);
			_poker_sprite.Add(s);
		}


		for(int i=1;i< 55 ;i++)
		{
			string id = "m_poker_"+String.Format("{0:D2}",i);
			Sprite s = (Sprite)Resources.Load<Sprite> ("share/mobile_poker/"+id);
			_mobile_poker_sprite.Add(s);
		}

		//number
		for(int i=0;i< 10 ;i++)
		{
			string id = String.Format("{0:D1}",i);
			Sprite s = (Sprite)Resources.Load<Sprite> ("share/digit_num/"+id);
			_digit_num.Add(s);
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

		_coin_list = _coin._list;

		//data setting
		_state_m = new StateMachine ();
		_state_m._state = "None";

		_bet_model = new DK_bet ();

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

		TextAsset tet = (TextAsset)Resources.Load<TextAsset> ("pack_DK_normal");

		JToken token = JToken.Parse(tet.text);
		//Debug.Log("DK json = "+ token);
		JArray jarr = token["packlist"] as JArray;

		for (int i=0; i< jarr.Count; i++) {
			simpack.Add(jarr[i].ToString());
		}

		_sim_parser =  new DK_parser ();
	}

	public static string DataPathToAssetPath(string path)
	{
	     if (Application.platform == RuntimePlatform.WindowsEditor)
	         return path.Substring(path.IndexOf("Assets\\"));
	     else
	         return path.Substring(path.IndexOf("Assets/"));
	}

	public static string DataPathToResourcesPath(string path)
	{
		
		if (Application.platform == RuntimePlatform.WindowsEditor){
			// Environment\Line_Up\Up\Flower_Category1\Flower@sprite.png
			string result =  path.Substring(path.IndexOf("text\\"));
			
			// Environment/Line_Up/Up/Flower_Category1/Flower@sprite.png
			string result1 =  result.Replace("\\","/");
			
			// Environment/Line_Up/Up/Flower_Category1/Flower@sprite
			//int index = result1.LastIndexOf('.');
			//string result2 = result1.Substring(0, index);
			
			return result1;
			
		}
		
		else
			return path.Substring(path.IndexOf("text/"));
	}
	
	public void connect_to_server()
	{
		Debug.Log ("dk uuid = " + _model.getValue("uuid"));
		_Connector = new websocketModule();
		_Connector.parser = new DK_parser ();
		_Connector.create ("ws://www.mm9900.net:8201/gamesocket/token/"+_model.getValue("uuid"));
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

		remove_poker_event ();

		poker_open.Clear ();
		state_update.Clear ();

		_poker.clean ();

		if (_Connector != null) {
			_Connector.close ();
			Debug.Log ("DK leave disconnect ");
		}

		_model.putValue ("mi_kind", "");
	}

	private void error_msg(string msg)
	{
		_lobby_proxy.error_msg (msg);
	}

	private void Onstate(object sender,stringArgs e)
	{
		Debug.Log("DK Onstate = "+ e.msg);
		if (e.msg == "open" || e.msg == "1005") {
			;
		}
		else error_msg (e.msg);

	}

	private void OnMessage(object sender,packArgs e)
	{
		pack_handel (e);
	}

	public void pack_handel(packArgs e)
	{
		//string st = e.pack ["message_type"];
		string st = _model.getValue ("dk_message_type");
		_state = _model.getValue ("dk_game_state");

		if (st == "MsgBPInitialInfo") 
		{
			Debug.Log ("dk _state = " + _state);
			_state_m._state = _state;
			view_enable();

			round_code(_model.getValue("dk_game_round"));		

			string card = _model.getValue ("player_card_list");			
			_poker.set_all(poker_type.Player,card);
			if( _poker.get_count(poker_type.Player) == 1)
			{
				poker_open.Add(0);
			}
			if( _poker.get_count(poker_type.Player) ==2)
			{
				poker_open.Add(0);
				poker_open.Add(1);
			}
			
			card = _model.getValue ("banker_card_list");

			_poker.set_all(poker_type.Banker,card);
			if( _poker.get_count(poker_type.Banker) ==1)
			{
				poker_open.Add(2);
			}
			if( _poker.get_count(poker_type.Banker) ==2)
			{
				poker_open.Add(2);
				poker_open.Add(3);
			}
			
			card = _model.getValue ("river_card_list");				
			_poker.set_all(poker_type.River,card);
			if( _poker.get_count(poker_type.River)==1)
			{
				poker_open.Add(4);
			}
			if( _poker.get_count(poker_type.River) ==2)
			{
				poker_open.Add(4);
				poker_open.Add(5);
			}

			
			Debug.Log("pack all p= "+ _model.getValue ("player_card_list"));
			Debug.Log("pack all b= "+ _model.getValue ("banker_card_list"));
			Debug.Log("pack all r= "+ _model.getValue ("river_card_list"));

			state_update.Add (_state);
			
		}
		if (st == "MsgBPState") 
		{
			_state_m._state = _state;
			view_enable();

			round_code(_model.getValue("dk_game_round"));

			//_Coin_item.item_set ("into_game");

			state_update.Add (_state);

		}
		if (st == "MsgBPOpenCard") 
		{		
			_state_m._state = _state;
         
			string cardtype = _model.getValue ("card_type");
			string card_list = _model.getValue ("card_list");
			Debug.Log("carty = "+cardtype);
			Debug.Log("card_list = "+card_list);

			if( cardtype == "Player")
			{
				int pcount = _poker.get_count(poker_type.Player);

				//mi poker
				if( pcount == 1 )
				{
					_model.putValue("mi_kind",poker_type.Player.ToString());
				}
				else
				{
					cardback [pcount].RotateCard (1,pcount);
					cardback[pcount].CardRotateComplete += card_open_done;
				}
				_poker.set_poker(poker_type.Player,card_list);
			}
			
			if( cardtype == "Banker")
			{
				int bcount = _poker.get_count(poker_type.Banker);

				if( bcount == 1 )
				{
					_model.putValue("mi_kind",poker_type.Banker.ToString());
				}
				else
				{
					cardback [bcount+2].RotateCard (1,bcount+2);
					cardback[bcount+2].CardRotateComplete += card_open_done;
				}

				_poker.set_poker(poker_type.Banker,card_list);
			}
			
			if( cardtype == "River")
			{	
				int rcount = _poker.get_count(poker_type.River);

				string s = _model.getValue ("dk_prob");		
				List<string> prob = new List<string> (s.Split (','));
				List<float> raw_data = new List<float> ();
				int len = prob.Count;
				float total = 0;
				for( int i=0;i< len;i++)
				{
					total += float.Parse(prob[i]);
				}

				if( rcount == 1 && total !=0.0f)
				{
					_model.putValue("mi_kind",poker_type.River.ToString());
				}
				else
				{
					cardback [rcount+4].RotateCard (1,rcount+4);
					cardback[rcount+4].CardRotateComplete += card_open_done;
				}
				_poker.set_poker(poker_type.River,card_list);
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

				//TODO
				//flush_comfirm_bet();
			}
		}
		if (st == "MsgBPEndRound") 
		{
			//_state = e.pack["game_state"];

			_state_m._state = _state;
			view_enable();

			Debug.Log(" bet_type= "+e.pack["bet_type"]);
			Debug.Log(" settle_amount= "+e.pack["settle_amount"]);
			Debug.Log(" odds= "+e.pack["odds"]);
			Debug.Log(" win_state= "+e.pack["win_state"]);
			Debug.Log(" bet_amount= "+e.pack["bet_amount"]);

			List<string> settle = new List<string>(e.pack["settle_amount"].ToString().Split(','));
			settle.RemoveAt (settle.Count-1);
			settle.RemoveAt (settle.Count-1);

			_bet_model.settle_amount(string.Join(",",settle.ToArray()));

			List<string>  bet= new List<string>(e.pack["bet_amount"].ToString().Split(','));
			bet.RemoveAt (bet.Count-1);
			bet.RemoveAt (bet.Count-1);
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


	private void new_round_init()
	{
		string s = "0,0";
		_win_hint.set_avalible(new List<string>(s.Split(',')));
		_win_hint.gameObject.SetActive (false);

		_player_point.textContent = "";
		_banker_point.textContent = "";

		_digi_timer_settle.gameObject.SetActive (false);
		_top_bar.gameObject.SetActive (true);
	}
	

	private void view_enable()
	{
		int idx = state_view_mapping [_state];
		_view_avalible.auto_set_avalible(idx,avalibe.effect.just_open_close);
	}

	private void start_bet_init()
	{

		_coin.gameObject.SetActive(true);

		reset_bet_amount();
		_bet_model.clean_bet();

		_win_hint.gameObject.SetActive (false);

		Invoke("start_bet_count", 1f);

		_lobby_proxy.phase_msg (phase_msg_start_bet,avalibe.effect.fadeout);
	}

	public void fadeout(int idx,bool b)
	{
		Debug.Log ("DK fadeout ");
	}

	private void end_bet_init()
	{
		_coin.gameObject.SetActive(false);

		reset_poker_item();
		remove_poker_event ();
		_poker.clean();

		_last_high_prob_idx =-1;
		_model.putValue ("mi_kind", "");

		//TODO private int phase_msg_no_credit = 2;
		_lobby_proxy.phase_msg (phase_msg_stop_bet,avalibe.effect.fadeout);
	}

	private void open_card_init()
	{
		Debug.Log ("open_card_init");
		mi_poker();
	}

	private void mi_poker()
	{
		string mi = _model.getValue ("mi_kind");
		Debug.Log ("mi = " + mi);
		if (mi == "")
			return;

		_mi_pokerboard.gameObject.SetActive (true);

		int poker_idx = 0;
		string poker ="";
		if (mi =="Player") poker = _poker.get_poker (poker_type.Player);
		if (mi =="Banker") poker = _poker.get_poker (poker_type.Banker);
		if (mi =="River") poker = _poker.get_poker (poker_type.River);
		poker_idx = _poker.pokerTrans (poker);
		
		_mi_pokerboard.transform.FindChild ("poker").GetComponent<Image> ().sprite = _poker_sprite [poker_idx]; //Your sprite
		TweenSequence se = _mi_pokerboard.transform.FindChild ("poker").GetComponent<TweenSequence> ();
		se.BeginSequence ();
		TweenTransforms trans = _mi_pokerboard.transform.FindChild ("poker").GetComponent<TweenTransforms> ();
		trans.TweenCompleted += tween_ok;
	}

	private void tween_ok()
	{
		Debug.Log ("tween_ok = ");
		Invoke ("hide", 1.0f);
	}

	private void hide()
	{
		_mi_pokerboard.gameObject.SetActive (false);

		string mi = _model.getValue ("mi_kind");
		Debug.Log ("mi = " + mi);
		if (mi == "")
			return;
		
		//_mi_pokerboard.gameObject.SetActive (true);
		
		int poker_idx = 0;
		string poker ="";
		int open_idx = 0;
		if (mi == "Player") {
			poker = _poker.get_poker (poker_type.Player);
			open_idx =1;
			caculate_point();
		}
		if (mi == "Banker") {
			poker = _poker.get_poker (poker_type.Banker);
			open_idx = 3;
			caculate_point();
		}
		if (mi == "River") {
			poker = _poker.get_poker (poker_type.River);
			open_idx =5;
		}
		poker_idx = _poker.pokerTrans (poker);

		cardlist [open_idx].GetComponent<Image> ().sprite = _mobile_poker_sprite [poker_idx];

		cardlist[open_idx].rotate_to(0,360,0);
		cardback[open_idx].rotate_to(0,90,0);
		


	}

	private void end_round_init()
	{

		List<string> bet = _bet_model.bet;
		List<string> settle = _bet_model.settle;
		for(int i=0;i< _result_bet.Count;i++)
		{
			_result_bet[i].textContent = this.account_num( bet[i] );
			_result_settle[i].textContent = this.account_num( settle[i] );
		}

		int p_point = _poker.get_Point (poker_type.Player);
		int b_point = _poker.get_Point (poker_type.Banker);
		string s = p_point.ToString()+","+b_point.ToString();
		_win_hint.set_avalible(new List<string>(s.Split(',')));


		//Invoke("show_settle", 2f);

		_top_bar.gameObject.SetActive (false);

		_digi_timer_settle.gameObject.SetActive (true);
		Invoke("start_count", 1f);

	}

	public string account_num(string or_num)
	{
		int digit = Int32.Parse (or_num);
		if (digit == 0)
			return "0";

		List<int> digit_list = new List<int> ();
		int num = 0;
		while (digit >=10) {
			num = digit % 10;
			digit_list.Add(num);
			digit /=10;
		}
		digit_list.Add (digit);

		string st = "";
		for (int i=0; i< digit_list.Count; i++) {
			if( i>0 && i %3 ==0) st = "," + st;
			st = digit_list[i].ToString() + st;
		}

		return st;
	}

	public void show_settle()
	{
		//wait to new switch view	

	}

	private void start_bet_count()
	{
		string time = string.Format("{0:D2}",Int32.Parse(_model.getValue ("dk_remain_time")));
		this.count (_digi_timer_bet, time,"start_bet_count");
	}

	private void start_count()
	{
		string time = string.Format("{0:D2}",Int32.Parse(_model.getValue ("dk_remain_time")));
		this.count (_digi_timer_settle, time,"start_count");
	}

	private void count(GameObject time_ob, string time,string count_name)
	{
		Debug.Log("time = "+time);
			
		int ten = Int32.Parse( time.Substring (0, 1));
		int one = Int32.Parse( time.Substring (1, 1));

		time_ob.transform.FindChild ("digit_1").GetComponent<Image> ().sprite = _digit_num [one];
		time_ob.transform.FindChild ("digit_2").GetComponent<Image> ().sprite = _digit_num [ten];
		time_ob.gameObject.SetActive (true);

		var reset_time = _model.getValue ("dk_remain_time");
		int reset = Int32.Parse(reset_time) -1;
		if (reset ==-1) 
		{
			CancelInvoke ();
			//time_ob.gameObject.SetActive (false);
			return;
		}

		_model.putValue ("dk_remain_time",reset.ToString());
		Invoke(count_name, 1f);

	}

	public void caculate_point()
	{
		int p_point = _poker.get_Point (poker_type.Player);
		int b_point = _poker.get_Point (poker_type.Banker);
		if (p_point != -1 || b_point != -1) {
			_win_hint.gameObject.SetActive (true);
			if( p_point !=-1) _player_point.textContent = p_point.ToString();
			if( b_point !=-1) _banker_point.textContent = b_point.ToString();
		}
	}

	private void reset_bet_amount()
	{
	   foreach (UI_Text bet in bet_amount_list) 
	   {	 
		 bet.textContent = "";
		}

		//TODO reset coin_stack
	}

	private void direct_turn_poker()
	{	
		List<int> poker_list;
		 poker_list = _poker.get_pokser_res_idx (poker_type.Player);
	
		for (int i=0; i< poker_list.Count; i++) {
			int id = poker_list[i];
			cardlist [i].GetComponent<Image> ().sprite = _mobile_poker_sprite [id];
		}

		poker_list.Clear ();
		poker_list = _poker.get_pokser_res_idx (poker_type.Banker);
		for (int i=0; i< poker_list.Count; i++) {
			int id = poker_list[i];
			cardlist [i+2].GetComponent<Image> ().sprite = _mobile_poker_sprite [id];
		}

		poker_list.Clear ();
		poker_list = _poker.get_pokser_res_idx (poker_type.River);
		for (int i=0; i< poker_list.Count; i++) {
			int id = poker_list[i];
			cardlist [i+4].GetComponent<Image> ().sprite = _mobile_poker_sprite [id];
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
	

	public void betType(string btnname)
	{
		Debug.Log ("value = " +btnname);

		//uncheck
		//_bet_model.add_bet_local (btnname);
		//flush_uncheck_bet ();
		//return;

		//single
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
				{"game_id",_model.getValue("dk_game_id")},
				{"game_type",_model.getValue("dk_game_type")},
				{"game_round",_model.getValue("dk_game_round")},
				{"bet_type", bet["betType"]},
				{"bet_amount",bet["bet_amount"]},
				{"total_bet_amount",bet["total_bet_amount"]}
			};
			//Debug.Log ("ob = "+ob.ToString());
			_Connector.send_to_Server(ob.ToString());
		}

	}

	//TODO time up call
	private void comfirm_bet()
	{
		JObject bet =  _bet_model.comfirm_bet ();
		JObject ob = new JObject
		{
			{ "id",_model.getValue("uuid")},
			{ "timestamp",1111},
			{"message_type","MsgPlayerBet"},
			{"game_id",_model.getValue("dk_game_id")},
			{"game_type",_model.getValue("dk_game_type")},
			{"game_round",_model.getValue("dk_game_round")},
			{"bet_type", bet["betType"]},
			{"bet_amount",bet["bet_amount"]},
			{"total_bet_amount",bet["total_bet_amount"]}
		};
		_Connector.send_to_Server(ob.ToString());
	}

	private void flush_uncheck_bet()
	{

	}

	private void flush_comfirm_bet()
	{
//		List<int> zone_amount = _bet_model.get_zone_amount ();
//
//		for(int i=0;i< zone_amount.Count;i++)
//		{
//			bet_amount_list [i].textContent = zone_amount[i];
//		}

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

	public void main_paytable()
	{
		//主注賠率
		_tab_table.auto_set_avalible(1,avalibe.effect.just_open_close);

		//default
		GameObject paytable = _tab_table.transform.FindChild ("main").gameObject;
		paytable.transform.FindChild ("sp_table_btn").GetComponent<Image> ().sprite = _sp_table_press;
		paytable.transform.FindChild ("main_table_btn").GetComponent<Image> ().sprite = _main_table_normal;

	}

	public void history_table()
	{
		if (_state == "OpenState")
			return;

		//歷史
		_tab_table.auto_set_avalible(2,avalibe.effect.just_open_close);

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

		//_model.getValue ("history_point");
		//_model.getValue ("history_player_pair");
		//_model.getValue ("history_banker_pair");
	}

	public void close_all_table()
	{
		_tab_table.auto_set_avalible(0,avalibe.effect.just_open_close);
	}

	public void sp_table_show()
	{
		_paytable_avalible.auto_set_avalible(1,avalibe.effect.just_open_close);
		GameObject paytable = _tab_table.transform.FindChild ("main").gameObject;
		paytable.transform.FindChild ("sp_table_btn").GetComponent<Image> ().sprite = _sp_table_press;
		paytable.transform.FindChild ("main_table_btn").GetComponent<Image> ().sprite = _main_table_normal;
	}

	public void main_table_show()
	{
		_paytable_avalible.auto_set_avalible(2,avalibe.effect.just_open_close);
		GameObject paytable = _tab_table.transform.FindChild ("main").gameObject;
		paytable.transform.FindChild ("main_table_btn").GetComponent<Image> ().sprite = _main_table_press;
		paytable.transform.FindChild ("sp_table_btn").GetComponent<Image> ().sprite = _sp_table_normal;
	}

	public void single_test()
	{

	}

	void Update () {

		if (poker_open.Count != 0) {
			
			direct_turn_poker();
			
			poker_open.Clear();
			caculate_point();
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
		if (idx == 0 || idx == 1) poker = _poker.get_poker (poker_type.Player);
		if (idx == 2 || idx == 3) poker = _poker.get_poker (poker_type.Banker);
		if (idx == 4 || idx == 5) poker = _poker.get_poker (poker_type.River);
		poker_idx = _poker.pokerTrans (poker);

		cardlist [idx].GetComponent<Image> ().sprite = _mobile_poker_sprite [poker_idx]; //Your sprite
		cardlist [idx].RotateCard (2,idx);
		cardlist[idx].CardRotateComplete += dispayer_card_open_done;

	}

	private void dispayer_card_open_done(object sender,stringArgs e)
	{
		cardback[Int32.Parse(e.msg)].CardRotateComplete -= dispayer_card_open_done;



		prob_update ();
	}

	private void prob_update()
	{
		sin_ki_formula ();

		string s = _model.getValue ("dk_prob");
		//Debug.Log ("s = " + s);
		List<string> prob = new List<string> (s.Split (','));
		List<string> sort_porb = prob.GetRange (0, prob.Count);
		int idx = -1;
		for( int i=0;i< prob.Count;i++)
		{
			_prob_bar[i].Value = float.Parse(prob[i]);
		}

		//big in front -x
		prob.Sort ((x,y) => { return -x.CompareTo(y);});

		if( float.Parse(prob[0]) !=0.0) idx = sort_porb.IndexOf (prob [0].ToString ());
		if (idx != -1) {
			if( _last_high_prob_idx !=-1) _prob_bar [_last_high_prob_idx].transform.FindChild ("Filler").GetComponent<Image> ().sprite = _green_prob_sprite;
			_prob_bar [idx].transform.FindChild ("Filler").GetComponent<Image> ().sprite = _red_prob_sprite;
		}

		_last_high_prob_idx = idx;
	}

	private void sin_ki_formula()
	{
		string s = _model.getValue ("dk_prob");		
		List<string> prob = new List<string> (s.Split (','));
		List<float> raw_data = new List<float> ();
		int len = prob.Count;
		for( int i=0;i< len;i++)
		{
			raw_data.Add(float.Parse(prob[i]) * 10000.0f);
		}

		for (int i = 0; i < len; i++)
		{				
			raw_data[i] = Mathf.Sqrt(raw_data[i]) *10 ;
		}

		float total= 0;
		List<string> new_prob = new List<string> ();
		for (int i = 0; i < len; i++)
		{


			raw_data[i] = Mathf.Sqrt(raw_data[i] ) * 10 ;
			total += raw_data[i];
			new_prob.Add(raw_data[i].ToString());

		}

		if (total == 0) {
			_model.putValue("dk_prob",string.Join(",",new_prob.ToArray()));
			return;
		}

		new_prob.Clear ();
		for (int i= 0; i < len; i++)
		{
			raw_data[i]  = raw_data[i]  / total ;
			new_prob.Add(raw_data[i].ToString());

		}

		//one kind match
		for (int i= 0; i < len; i++)
		{
			if ( raw_data[i] == 1 && total == 1000) 
			{
				_model.putValue("dk_prob",string.Join(",",new_prob.ToArray()));
				return ;
			}
		}

		new_prob.Clear ();
		for (int  i = 0; i < len; i++)
		{				
			if ( raw_data [i] != 0) 
			{
				raw_data[i] = Mathf.Min( 0.9f,  raw_data[i] + 0.3f );
				new_prob.Add(raw_data[i].ToString());
			}
			else
			{
				new_prob.Add("0");
			}
		}

		//unity is 0~100,not 0~1 ,so X100
		new_prob.Clear ();
		for (int i= 0; i < len; i++)
		{
			raw_data[i]  = raw_data[i] *100 ;
			new_prob.Add(raw_data[i].ToString());
			
		}

		_model.putValue("dk_prob",string.Join(",",new_prob.ToArray()));
	}

	public void sim_pack()
	{
		Debug.Log ("sim pack ="+simpack[simpck_idx]);
		packArgs pack = _sim_parser.paser(simpack[simpck_idx]);
		
		pack_handel (pack);
//		if (pack.pack.Count == 0) {
//			Debug.Log ("sim pack drop=");
//		} else {
//
//		}
		simpck_idx = (simpck_idx + 1) % simpack.Count;

	}
	
}
