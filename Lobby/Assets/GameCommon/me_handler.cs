using UnityEngine;
using System.Collections;

using Facebook.Unity;
using Facebook.MiniJSON;

public class me_handler : Ihandler {

	public override void result_handle(IResult result)
	{
		LogView.AddLog(result.RawResult);
		LogView.AddLog("me handler ok");

		IDictionary dict = Json.Deserialize(result.RawResult) as IDictionary;
		LogView.AddLog("me handler name = "+ dict["name"]);
		LogView.AddLog("me handler id = "+ dict["id"]);
		//next setp
		this.call_back (dict ["name"], dict ["name"]);
		//this.call_back ("ok");
		//this.call_back ("秋哥","的unity");
	}


}
