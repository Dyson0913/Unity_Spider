using UnityEngine;
using System.Collections;

using Facebook.Unity;

public class permission_handler : Ihandler {

	private me_handler _me_handler;
	private Friends_handler _Friends_handler;
	//private 

	public override void result_handle(IResult result)
	{
		LogView.AddLog(result.RawResult);
		LogView.AddLog("permission ok login");

		//next step
		//_me_handler = new me_handler ();
		//_me_handler.call_back = this.call_back;
		//FB.API("/me", HttpMethod.GET, _me_handler.result_handle);

		//next step
		_Friends_handler = new Friends_handler ();
		_Friends_handler.call_back = this.call_back;
		FB.API("/me/friends", HttpMethod.GET, _Friends_handler.result_handle);

		//or /me?fields=id,name

	}


}
