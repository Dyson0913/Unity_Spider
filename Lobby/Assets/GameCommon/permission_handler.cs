using UnityEngine;
using System.Collections;

using Facebook.Unity;

public class permission_handler : Ihandler {

	private me_handler _me_handler;

	public override void result_handle(IResult result)
	{
		LogView.AddLog(result.RawResult);
		LogView.AddLog("permission ok login");

		//next step
		_me_handler = new me_handler ();
		_me_handler.call_back = this.call_back;
		FB.API("/me", HttpMethod.GET, _me_handler.result_handle);

	}


}
