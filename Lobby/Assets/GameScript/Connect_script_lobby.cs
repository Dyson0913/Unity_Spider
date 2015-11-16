using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using ConnectModule;
using GameCommon.Model;
using GameScript.parser;

public class Connect_script_lobby : MonoBehaviour {

	private IConnect _Connector;

	public UI_Text _name;
	public UI_Text _credit;
	public UI_Text _log;
	public avalibe _avalibelist;
	
	private Model _model = Model.Instance;

	// Use this for initialization
	void Start () {

		_Connector = new websocketModule();
		_Connector.parser = new lobby_parser ();
		_Connector.create ("ws://106.186.116.216:8001/gamesocket/token/c9f0f895fb98ab9159f51fd0297e236d");
		_Connector.MsgResponse += OnMessage;
		_Connector.stateResponse += Onstate;

		_Connector.connect ();
	}

	public void bet1(string btnname)
	{
		Debug.Log ("btnname = "+btnname);
	}

	private void Onstate(object sender,stringArgs e)
	{
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

	public void click1()
	{
		Debug.Log ("click1");	
		_log.textContent = "clieck1";
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

		//create DK link ,witch when connect ok
		//gameObject.AddComponent<AudioSource>();
		Application.LoadLevel("DK");
		//Invoke("DKcreate", 0.5f);
	}

	public void DKcreate()
	{
//		GameObject ob = new GameObject ();
//		ob.name = "DK_connector";
//		ob.AddComponent<Connect_script_DK>();
//		ob.GetComponent<Connect_script_DK>()._uuid = _uuid;
	}

	// Update is called once per frame
	void Update () {
	
		//TODO isolate to Independon script
		if (Input.GetKeyUp (KeyCode.Escape)) 
		{
			Application.Quit();
		}
	}

	void OnDestroy() {
		_Connector.close ();
		Debug.Log ("Lobby connect destroy");
	}
	
}
