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

public class Connect_script_DK: MonoBehaviour {

	private IConnect _Connector;

	public bool _local_test = true;
	//share item in game
	public UI_Text _ui_gameround;
	public UI_Buttonlist _coin;
	public UI_Text _bet_timer;
	public UI_Text _log;
	public UI_Enable _Coin_item;

	//betview
	public List<UI_Text> bet_amount_list;
	public List<Button> _btnlist;

	//opencard
	public List<UI_Image> cardlist;
	public List<UI_Image> cardback;

	//settle
	public List<UI_Text> _result_bet;
	public List<UI_Text> _result_settle;

	//ui
	private List<Button> _coin_list;

	//delegate
	public avalibe _view_avalible;
	public avalibe _win_hint;

	private StateMachine _state_m;

	//data 
	private Model _model = Model.Instance;
	private Model_bet _bet_model;

	private string _state;

	private poker _poker;

	private string _prebtn;
	private List<int> poker_open;
	private List<string> state_update;

	public Dictionary<string,string> state_mapping;


	//sim_relative
	private List<string> simpack;
	private int simpck_idx;
	private IParser _sim_parser;

	// Use this for initialization
	void Start () {
		poker_open = new List<int> ();
		state_update = new List<string> ();
	}

	public void init()
	{
		//get view item
		_log = GameObject.Find ("Log").GetComponent<UI_Text>();
		_ui_gameround.gameObject.SetActive(true); // = GameObject.Find ("game_round").GetComponent<UI_Text>();
		round_code ("");
		_bet_timer.textContent = "";

		_coin_list = _coin._list;

		_view_avalible.gameObject.SetActive (true);// = GameObject.Find ("view_avalible_DK").GetComponent<avalibe>();

		//data setting
		_state_m = new StateMachine ();
		_state_m._state = "None";

		_poker = new poker ();

		_bet_model = new DK_bet ();

		
		state_mapping = new Dictionary<string, string> ();
		state_mapping.Add ("NewRoundState", "新局");
		state_mapping.Add ("StartBetState", "請開始下注");
		state_mapping.Add ("EndBetState", "請停止下注");
		state_mapping.Add ("OpenState", "開牌中");
		state_mapping.Add ("EndRoundState", "結算中");

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

		//init_poker_event ();

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

		List<string> openlist = _state_m.stateupdate("NewRoundState");
		_view_avalible.set_avalible(openlist);

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

	public string state_str(string state)
	{
		if (!state_mapping.ContainsKey (state))
			return "";
		return state_mapping [state];
	}


	private void Onstate(object sender,stringArgs e)
	{
		Debug.Log("DK Onstate = "+ e.msg);
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
			log(state_str(_state));
			List<string> openlist = _state_m.stateupdate(_state);
			_view_avalible.set_avalible(openlist);
			
			_model.putValue("game_id",e.pack["game_id"]);
			_model.putValue("game_type",e.pack["game_type"]);
			_model.putValue("game_round",e.pack["game_round"]);
			round_code(_model.getValue("game_round"));

			if( _state =="StartBetState")
			{
				timer_start(e.pack["remain_time"]);
				Debug.Log("clen bet ="+ _bet_model.clean_bet());
				//_Coin_item.item_set ("into_game");
				state_update.Add (_state);
			}
			if( _state == "EndBetState")
			{
				//timer
				Debug.Log("remain = "+e.pack["remain_time"]);
				timer_start(e.pack["remain_time"]);
			}
			if( _state == "OpenState")
			{
				
				string card = e.pack["player_card_list"];
				if( card !="")
				{
					_poker.set_all(poker_type.Player,card);
					//playercard = new List<string>(card.Split(','));
					if( _poker.get_count(poker_type.Player) == 1)
					{
						poker_open.Add(0);
					}
					if( _poker.get_count(poker_type.Player) ==2)
					{
						poker_open.Add(0);
						poker_open.Add(1);
					}
				}
				
				card = e.pack["banker_card_list"];
				if( card !="")
				{
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
				}
				
				card = e.pack["river_card_list"];
				if( card !="")
				{
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
				}
				
				Debug.Log("pack all p= "+ e.pack["player_card_list"]);
				Debug.Log("pack all b= "+ e.pack["banker_card_list"]);
				Debug.Log("pack all r= "+ e.pack["river_card_list"]);
				//Debug.Log("pack all e= "+ e.pack["extra_card_list"]);
			}
			
			
		}
		if (st == "MsgBPState") 
		{
			_state = e.pack["game_state"];
			log(state_str(_state));
			List<string> openlist = _state_m.stateupdate(_state);
			if( openlist !=null)
			{
				_view_avalible.set_avalible(openlist);
			}
			
			_model.putValue("game_round",e.pack["game_round"]);
			round_code(_model.getValue("game_round"));
			if( _state =="NewRoundState")
			{
				//TODO hisotry recode
				Debug.Log("NewRoundState=");
				state_update.Add (_state);
			}
			if( _state =="StartBetState")
			{
				timer_start(e.pack["remain_time"]);
				Debug.Log("clen bet ="+ _bet_model.clean_bet());
				//_Coin_item.item_set ("into_game");
				state_update.Add (_state);

			}
			if( _state =="EndBetState")
			{
				_poker.clean();

				state_update.Add (_state);

				//why add will filp poker
				//init_poker_event ();
			}
			if( _state == "OpenState")
			{
				state_update.Add (_state);
			}
			if( _state == "EndRoundState")
			{
				state_update.Add (_state);
			}
			
		}
		if (st == "MsgBPOpenCard") 
		{
			_state = e.pack["game_state"];
			log(state_str(_state));
			
			//List<string> openlist = _state_m.stateupdate(_state);
			//if( openlist !=null) _view_avalible.set_avalible(openlist);
			
			Debug.Log("carty = "+e.pack["card_type"]);
			Debug.Log("card_list = "+e.pack["card_list"]);
			string cardtype = e.pack["card_type"];
			if( cardtype == "Player")
			{
				int pcount = _poker.get_count(poker_type.Player);
				cardback [pcount].RotateCard (1,pcount);
				cardback[pcount].CardRotateComplete += card_open_done;

				_poker.set_poker(poker_type.Player,e.pack["card_list"]);
				//playercard.Add(e.pack["card_list"]);
			}
			
			if( cardtype == "Banker")
			{
				int bcount = _poker.get_count(poker_type.Banker);
				cardback [bcount+2].RotateCard (1,bcount+2);
				cardback[bcount+2].CardRotateComplete += card_open_done;
				_poker.set_poker(poker_type.Banker,e.pack["card_list"]);
			}
			
			if( cardtype == "River")
			{	
				int rcount = _poker.get_count(poker_type.River);
				cardback [rcount+4].RotateCard (1,rcount+4);
				cardback[rcount+4].CardRotateComplete += card_open_done;
				_poker.set_poker(poker_type.River,e.pack["card_list"]);
			}
			
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
			log(state_str(_state));
			Debug.Log(" bet_type= "+e.pack["bet_type"]);
			Debug.Log(" settle_amount= "+e.pack["settle_amount"]);
			Debug.Log(" odds= "+e.pack["odds"]);
			Debug.Log(" win_state= "+e.pack["win_state"]);
			Debug.Log(" bet_amount= "+e.pack["bet_amount"]);


			List<string> settle  = new List<string>(e.pack["settle_amount"].ToString().Split(','));
			List<string> bet_amount  = new List<string>(e.pack["bet_amount"].ToString().Split(','));
			_model.putValue("settle_amount",settle);
			_model.putValue("bet_amount",bet_amount);

			state_update.Add (_state);

		}
		if (st == "check") 
		{
			Debug.Log("pack all = "+ e.pack["_all"]);
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

	private void start_bet_init()
	{
		_coin.gameObject.SetActive(true);
		reset_bet_amount();

		_win_hint.gameObject.SetActive (false);
	}

	private void end_bet_init()
	{
		_coin.gameObject.SetActive(false);
		timer_stop ();
		reset_poker_item();
		remove_poker_event ();
	}

	private void end_round_init()
	{
		_win_hint.gameObject.SetActive (true);


		Invoke("show_settle", 2f);
	}

	public void show_settle()
	{
		//wait to new switch view
		List<string> openlist = _state_m.stateupdate(_state_m._state);
		_view_avalible.set_avalible(openlist);

		for(int i=0;i< _result_bet.Count;i++)
		{
			_result_bet[i].textContent = bet_amount[i];
			_result_settle[i].textContent = settle[i];
		}

	}

	private void reset_bet_amount()
	{
	   foreach (UI_Text bet in bet_amount_list) 
	   {	 
		 bet.textContent = "";
		}
	}

	private void direct_turn_poker(int idx)
	{
		cardlist[idx].rotate_to(0,360,0);
		cardback[idx].rotate_to(0,90,0);
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

		//why not working
//		Button bt  = GameObject.Find (btnname).GetComponent<Button>();
//		ColorBlock co= bt.colors;
//		co.normalColor = Color.red;
//		bt.colors = co;
//		int id = Int32.Parse (btnname);
//		ColorBlock co= coin_list[id].colors;
//		co.normalColor = Color.red;
//		coin_list[id].colors = co;



//		if ( (_prebtn != btnname) && (_prebtn !="")) {
////			int id2 = Int32.Parse (_prebtn);
////			ColorBlock co2= coin_list[id].colors;
////			co2.normalColor = Color.white;
////			coin_list[id2].colors = co2;
//
//			Button bt2  = GameObject.Find (_prebtn).GetComponent<Button>();
//			ColorBlock co2= bt2.colors;
//			co2.normalColor = Color.red;
//			bt2.colors = co2;
//
//			Debug.Log ("coin same to while ="+ _prebtn);
//		}

		_prebtn = btnname;
		Debug.Log ("_prebtn ="+ _prebtn);
		//color (idx);

	}
	

	public void color(string idx)
	{
		for (int i=0; i< _coin_list.Count; i++) {
			ColorBlock co = _coin_list[i].colors;
			
			if( i == (Int32.Parse(idx)))
			{
				Debug.Log("i = "+ idx);
				co.normalColor = Color.red;
				co.highlightedColor = Color.red;
			}
			else
			{
				co.normalColor = Color.white;
				co.highlightedColor = Color.white;
			}
			_coin_list[i].colors = co;
		}
	}
	
	void OnDestroy() {
		if (_Connector != null) {
			_Connector.close ();
		}
		Debug.Log ("DK connect destroy");
	}

	public void single_test()
	{
		//cardback [0].RotateCard (1);
	}

	void Update () {

		if (poker_open.Count != 0) {
			for( int i=0;i< poker_open.Count;i++)
			{
				Debug.Log("poker_open"+poker_open[i]);
				direct_turn_poker(poker_open[i]);
			}
			poker_open.Clear();
		}

		if (state_update.Count != 0) {
			string state = state_update[0];
			if( state == "StartBetState")start_bet_init();
			if( state == "EndBetState")end_bet_init();
			if( state == "EndRoundState") end_round_init();
			state_update.Clear();
		}

	}

	private void card_open_done(object sender,stringArgs e)
	{
		cardback[Int32.Parse(e.msg)].CardRotateComplete -= card_open_done;
		cardlist [Int32.Parse(e.msg)].RotateCard (2,Int32.Parse(e.msg));
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
