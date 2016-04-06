using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using Facebook.Unity;
using Facebook.MiniJSON;

using ConnectModule;
using GameCommon.Model;
using GameScript.parser;
using ProgressBar;



public class Connect_script_lobby : MonoBehaviour {

	private IConnect _Connector;

	public UI_Text _name;
	public UI_Text _credit;
	public UI_Text _log;
	public avalibe _avalibelist;
	public ProgressRadialBehaviour _progressbar;

	private permission_handler _permission_handler;
	
	private Model _model = Model.Instance;
	
	// Use this for initialization
	void Start () {

		this.login ();
	}



	private void login()
	{
		_Connector = new websocketModule();
		_Connector.parser = new lobby_parser ();
		_Connector.create ("ws://106.186.116.216:8001/gamesocket/token/c9f0f895fb98ab9159f51fd0297e236d");
		_Connector.MsgResponse += OnMessage;
		_Connector.stateResponse += Onstate;
		
		_Connector.connect ();
	}


	private void Onstate(object sender,stringArgs e)
	{
		_progressbar.Value = 100;
		Debug.Log("Lobby Onstate = "+ e.msg);
	}

	private void OnMessage(object sender,packArgs e)
	{
		//Debug.Log ("Lobby message = " + e.pack ["message_type"]);
		string state = e.pack ["message_type"];
		if (state == "MsgLogin") {
			_name.textContent = e.pack ["player_name"];
			_credit.textContent = e.pack ["player_credit"];
			_model.putValue ("uuid", e.pack ["player_uuid"]);
			string s = e.pack ["game_avaliable"];
			_avalibelist.set_avalible (new List<string> (s.Split (',')));

		} else if (state == "MsgKeepLive") {

		} 
		else if (state == "MsgPlayerCreditUpdate") 
		{
			_credit.textContent = e.pack ["player_credit"];
		}
		if (e.pack ["message_type"] == "check") 
		{
			Debug.Log("pack all = "+ e.pack["_all"]);
		}
	}

	public void Log(string log)
	{
		_log.textContent =log;
	}

	public void click1()
	{
		Debug.Log ("click1");	

	}

	public void click2()
	{
		Debug.Log ("click2");	
		_log.textContent = "clieck2";
	}

	public void click3()
	{
		Debug.Log ("click3");	
		_log.textContent = "clieck3";
		string s = "0,0,0,0";
		_avalibelist.set_avalible(new List<string>(s.Split(',')));

		Application.LoadLevel("DK");
		//Invoke("DKcreate", 0.5f);
	}

	public void loading_down()
	{
		Debug.Log ("loading_down");
		_progressbar.gameObject.SetActive (false);
	}

	public void click_Fb_login()
	{
		FB.Init (this.OnInitComplete, this.OnHideUnity);

	}

	private void OnInitComplete()
	{
		string logMessage = string.Format(
			"OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'",
			FB.IsLoggedIn,
			FB.IsInitialized);
		LogView.AddLog(logMessage);

		//登入流程
		//if (!FB.IsLoggedIn) {
			this.CallFBLogin ();
		//}
	}

	private void CallFBLogin()
	{
		_permission_handler = new permission_handler ();
		_permission_handler.call_back = this.Fb_data_ok;
		FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, _permission_handler.result_handle);
	}

	private void Fb_data_ok (params object[] args)
	{
		LogView.AddLog("Fb_data_ok");
		_name.textContent = args[0].ToString();

		this.login();
	}

	private void OnHideUnity(bool isGameShown)
	{
		//this.Status = "Success - Check logk for details";
		//this.LastResponse = string.Format("Success Response: OnHideUnity Called {0}\n", isGameShown);
		//LogView.AddLog("Is game shown: " + isGameShown);
	}



	// Update is called once per frame
	void Update () {
	
		//TODO isolate to Independon script
		if (Input.GetKeyUp (KeyCode.Escape)) 
		{
			Application.Quit();
		}

		if (!_progressbar.isDone) {
			_progressbar.Value = _progressbar.Value + 1;
			Debug.Log ("new value: " + _progressbar.Value);
		} 
	}

	void OnDestroy() {
		_Connector.close ();
		Debug.Log ("Lobby connect destroy");
	}
	
}
