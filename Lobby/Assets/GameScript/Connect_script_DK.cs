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



public class Connect_script_DK: MonoBehaviour {

	private IConnect _Connector;


	//share item in game
	public UI_Text _ui_gameround;
	public UI_Buttonlist _coin;
	public UI_Text _bet_timer;
	public UI_Text _log;

	public List<UI_Text> bet_amount_list;
	public List<UI_Text> cardlist;
	public List<Button> _btnlist;
	public List<UI_Text> _result_bet_zone;
	public List<UI_Text> _result_bet_amount;
	public List<UI_Text> _result_settle_amount;

	//ui
	private List<Button> _coin_list;

	//delegate
	public avalibe _view_avalible;

	private StateMachine _state_m;

	//data 
	private Model _model = Model.Instance;
	private Model_bet _bet_model;

	private string _state;

	private List<string> playercard;
	private List<string> bankercard;
	private List<string> rivercard;

	private List<int> zone_bet;
	private string _prebtn;


	public Dictionary<string,string> state_mapping;


	//sim_relative
	private List<string> simpack;
	private int simpck_idx;
	private IParser _sim_parser;

	// Use this for initialization
//	void Start () {
//
//	}

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
		_state_m.state = "None";
		
		playercard = new List<string> ();
		bankercard = new List<string> ();
		rivercard = new List<string> ();
		zone_bet = new List<int> ();
		
		_bet_model = new DK_bet ();
		
		_model.putValue ("coin_select", "Coin_1");
		
		state_mapping = new Dictionary<string, string> ();
		state_mapping.Add ("NewRoundState", "新局");
		state_mapping.Add ("StartBetState", "請開始下注");
		state_mapping.Add ("EndBetState", "請停止下注");
		state_mapping.Add ("OpenState", "開牌中");
		state_mapping.Add ("EndRoundState", "結算中");

		foreach (Button bt in _btnlist) 
		{
			//work aroud
			string idx = bt.name;
			bt.onClick.AddListener(()=>betType(idx));	
		}

		//change color
		//ColorBlock co= coin_list[0].colors;
		//co.normalColor = Color.red;
		//coin_list[0].colors = co;
		//		foreach (Button coin in coin_list) 
		//		{
		//			//work aroud
		//			string coin_idx = coin.name;
		//			coin.onClick.AddListener(()=>coin_select(idx));	
		//		}
		for (int i=0; i< _coin_list.Count; i++) {
			//string coin_idx = _coin_list[i].name;
			string idx =i.ToString();
			_coin_list[i].onClick.AddListener(()=>coin_select(idx));
			
		}
		_prebtn = "";

		//TODO not handle
//		foreach (UI_Text bet in bet_amount_list) 
//		{
//			//work aroud
//			bet.textContent = "";
//		}

		pc_test ();
		//connect_to_server ();
	}

	public void pc_test()
	{
		simpack =  new List<string>();
		simpck_idx =0;
		string test = File.ReadAllText ("pack_DK_normal.txt");
		JToken token = JToken.Parse(test);
		//Debug.Log("DK json = "+ token);
		JArray jarr = token["packlist"] as JArray;

		for (int i=0; i< jarr.Count; i++) {
			simpack.Add(jarr[i].ToString());
		}

		_sim_parser =  new DK_parser ();
	}

	public void connect_to_server()
	{
		_Connector = new websocketModule();
		_Connector.parser = new DK_parser ();
		_Connector.create ("ws://www.mm9900.net:8001/gamesocket/token/"+_model.getValue("uuid"));
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
		Debug.Log("DK st = "+ st);
		if (st == "MsgBPInitialInfo") 
		{
			_state = e.pack["game_state"];
			_log.textContent = state_str(_state);
			List<string> openlist = _state_m.stateupdate(_state);
			_view_avalible.set_avalible(openlist);
			
			_model.putValue("game_id",e.pack["game_id"]);
			_model.putValue("game_type",e.pack["game_type"]);
			_model.putValue("game_round",e.pack["game_round"]);
			round_code(_model.getValue("game_round"));
			if( openlist[0] =="1")
			{
				//timer
				Debug.Log("remain = "+e.pack["remain_time"]);
				timer_start(e.pack["remain_time"]);
			}
			
			if( openlist[1] =="1")
			{
				
				string card = e.pack["player_card_list"];
				if( card !="")
				{

					playercard = new List<string>(card.Split(','));
					if( playercard.Count == 1)cardlist[0].textContent = playercard[0];
					if( playercard.Count ==2)
					{
						cardlist[0].textContent = playercard[0];
						cardlist[1].textContent = playercard[1];
					}
				}
				
				card = e.pack["banker_card_list"];
				if( card !="")
				{
					bankercard = new List<string>(card.Split(','));
					if( bankercard.Count ==1)cardlist[2].textContent = bankercard[0];				
					if( bankercard.Count ==2)
					{
						cardlist[2].textContent = bankercard[0];
						cardlist[3].textContent = bankercard[1];
					}
				}
				
				card = e.pack["river_card_list"];
				if( card !="")
				{
					rivercard = new List<string>(card.Split(','));
					Debug.Log("ini r card= "+ rivercard.Count );
					if( rivercard.Count ==1)cardlist[4].textContent = rivercard[0];				
					if( rivercard.Count ==2)
					{
						cardlist[4].textContent = rivercard[0];
						cardlist[5].textContent = rivercard[1];
					}
				}
				
				Debug.Log("pack all p= "+ e.pack["player_card_list"]);
				Debug.Log("pack all b= "+ e.pack["banker_card_list"]);
				Debug.Log("pack all r= "+ e.pack["river_card_list"]);
				Debug.Log("pack all e= "+ e.pack["extra_card_list"]);
			}
			
			
		}
		if (st == "MsgBPState") 
		{
			_state = e.pack["game_state"];
			_log.textContent = state_str(_state);
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
			}
			if( _state =="StartBetState")
			{
				timer_start(e.pack["remain_time"]);
				Debug.Log("clen bet ="+ _bet_model.clean_bet());
				
				playercard.Clear();
				bankercard.Clear();
				rivercard.Clear();
				
				for( int i=0;i<bet_amount_list.Count;i++)
				{
					bet_amount_list[i].textContent = "";
				}
				
				for( int i=0;i<cardlist.Count;i++)
				{
					cardlist[i].textContent = "";
				}
				
			}
			
		}
		if (st == "MsgBPOpenCard") 
		{
			_state = e.pack["game_state"];
			_log.textContent = state_str(_state);
			
			List<string> openlist = _state_m.stateupdate(_state);
			if( openlist !=null) _view_avalible.set_avalible(openlist);
			
			Debug.Log("carty = "+e.pack["card_type"]);
			Debug.Log("card_list = "+e.pack["card_list"]);
			string cardtype = e.pack["card_type"];
			if( cardtype == "Player")
			{
				Debug.Log("playercard len =" + playercard.Count );
				if( playercard.Count == 0)cardlist[0].textContent = e.pack["card_list"];
				if( playercard.Count == 1)cardlist[1].textContent = e.pack["card_list"];
				playercard.Add(e.pack["card_list"]);
			}
			
			if( cardtype == "Banker")
			{
				Debug.Log("bankercard len =" + bankercard.Count );
				if( bankercard.Count == 0)cardlist[2].textContent = e.pack["card_list"];
				if( bankercard.Count == 1)cardlist[3].textContent = e.pack["card_list"];
				bankercard.Add(e.pack["card_list"]);
			}
			
			if( cardtype == "River")
			{
				Debug.Log("rivercard len =" + rivercard.Count );
				if( rivercard.Count == 0)cardlist[4].textContent = e.pack["card_list"];
				if( rivercard.Count == 1)cardlist[5].textContent = e.pack["card_list"];
				rivercard.Add(e.pack["card_list"]);
			}
			
		}
		
		if (st == "MsgPlayerBet") 
		{
			if( e.pack["result"] == "0")
			{
				string s = _bet_model.bet_ok();
				
				Debug.Log("bet ok = "+ s);
				//string btnname = _model.getValue ("last_btn");
				//bet_amount_list [_bet_model.zone_idx (btnname)].textContent = _bet_model.get_total (btnname).ToString();
			}
		}
		if (st == "MsgBPEndRound") 
		{
			_state = e.pack["game_state"];
			_log.textContent = state_str(_state);
			Debug.Log("carty bet_type= "+e.pack["bet_type"]);
			Debug.Log("carty settle_amount= "+e.pack["settle_amount"]);
			Debug.Log("carty odds= "+e.pack["odds"]);
			Debug.Log("carty win_state= "+e.pack["win_state"]);
			Debug.Log("carty bet_amount= "+e.pack["bet_amount"]);
			List<string> openlist = _state_m.stateupdate(_state);
			_view_avalible.set_avalible(openlist);
			
//			List<string> name  = new List<string>(e.pack["bet_type"].ToString().Split(','));
//			List<string> settle  = new List<string>(e.pack["settle_amount"].ToString().Split(','));
//			List<string> bet_amount  = new List<string>(e.pack["bet_amount"].ToString().Split(','));
//			for(int i=0;i< _result_bet_zone.Count;i++)
//			{
//				_result_bet_zone[i].textContent = _bet_model.display_name(name[i]);
//				_result_bet_amount[i].textContent = bet_amount[i];
//				_result_settle_amount[i].textContent = settle[i];
//			}
		}
		if (st == "check") 
		{
			Debug.Log("pack all = "+ e.pack["_all"]);
		}
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

	public void betType(string btnname)
	{
		Debug.Log ("value = " +btnname);
		return;
		JObject bet=  _bet_model.add_bet (btnname);
		//Debug.Log ("ob = "+bet.ToString());
		_model.putValue ("last_btn", btnname);

		//test
		//string s = _bet_model.bet_ok();
		//Debug.Log("bet ok = "+ s);
		//coin update
		//bet_amount_list [_bet_model.zone_idx (btnname)].textContent = _bet_model.get_total (btnname).ToString();
		
		JObject ob = new JObject
		{
			{ "id",_model.getValue("uuid")},
			{ "timestamp",1111},
			{"message_type","MsgPlayerBet"},
			{"game_id",_model.getValue("game_id")},
			{"game_type",_model.getValue("game_type")},
			{"game_round",_model.getValue("game_round")},
			{"bet_type", bet["betType"]},
			{"bet_amount",_bet_model.coin_value(bet["bet_amount"].ToString())},
			{"total_bet_amount",bet["total_bet_amount"]}
		};
		Debug.Log ("ob = "+ob.ToString());

		_Connector.send_to_Server(ob.ToString());
	}

	public void coin_select(string idx)
	{
		string btnname = "Coin_"+ (Int32.Parse(idx)+1);
		Debug.Log ("btnnamed "+ btnname);
		//return;
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

		Debug.Log ("coin same to red "+ btnname);

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
		//color (idx);

	}

	public void color(string idx)
	{
//		for (int i=0; i< coin_list.Count; i++) {
//			ColorBlock co = coin_list[i].colors;
//			
//			if( i == (Int32.Parse(idx)))
//			{
//				Debug.Log("i = "+ idx);
//				co.normalColor = Color.red;
//				co.highlightedColor = Color.red;
//			}
//			else
//			{
//				co.normalColor = Color.white;
//				co.highlightedColor = Color.white;
//			}
//			coin_list[i].colors = co;
//		}
	}

	// Update is called once per frame
	void Update () {
	

	}

	void OnDestroy() {
		if (_Connector != null) {
			_Connector.close ();
		}
		Debug.Log ("DK connect destroy");
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
