using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Facebook.Unity;
using Facebook.MiniJSON;

//json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Friends_handler : Ihandler {

	public override void result_handle(IResult result)
	{
		LogView.AddLog(result.RawResult);
		LogView.AddLog("Friends handler ok");


//		{
//			"data": [
//			    {
//				"name": "Zheng-Wei Lin",
//				"id": "100000167846430"
//			    },
//			    {
//				"name": "陳昌徹",
//				"id": "100000219986343"
//			    },
//			    {
//				"name": "鐘義翔",
//				"id": "100000227531660"
//			    }
//			    ],
//			"paging": {
//				"next": "https://graph.facebook.com/v2.5/10152787845443271/friends?limit=25&offset=25&format=json&access_token=CAACEdEose0cBAEw1ki9j7ZBp6jvyUOnmbjDXvAxpXqtSz7cJIxePZBKMoYcpvy4gAlCqLd1LYqDiIN1nD3IlSl4KQLUVtxAxVaQu7cZCr2cAowmBsHbJydE0iGvwTN3inmtAva4P9yilCoBkSLZC5bMIsuxu5Ttbzg06zrFWwGKhpDBqp4GO5JGUqf2ZAZB0nHP6aXfsNJ8MIfD9JFqMJ2&__after_id=enc_AdA9jySi8YDX8rMZCbSAEB6Rdplo4dxH95OCiDYSfses6XZASWOIzTt8UBenFsWN2HX4lyITnb8O1yY9TWZC8efsxk4"
//			},
//			"summary": {
//				"total_count": 121
//			}
//		}

		JObject jo = new JObject();
		jo = JsonConvert.DeserializeObject<JObject>(result.RawResult);
		Dictionary<string,object> pack = new Dictionary<string,object> ();

		JObject jo2 = new JObject ();
		jo2 = JsonConvert.DeserializeObject<JObject> (jo.Property ("summary").Value.ToString ());
		string total_count = jo2.Property ("total_count").Value.ToString ();
		LogView.AddLog("Friends handler dict= "+ total_count);

		JArray jo3 = new JArray ();
		jo3 = JsonConvert.DeserializeObject<JArray> (jo.Property ("data").Value.ToString ());
	
		List<string> name = new List<string>();
		List<string> id = new List<string>();
		for (int i =0; i< jo3.Count; i++) 
		{
			JObject ch = (JObject)jo3[i];
			name.Add(ch.Property("name").Value.ToString());
			id.Add(ch.Property("id").Value.ToString());
		}

		LogView.AddLog("Friends handler name= "+ String.Join(",",name.ToArray()));
		LogView.AddLog("Friends handler id= "+ String.Join(",",id.ToArray()));


		//next setp
		//this.call_back (dict ["name"], dict ["name"]);
	}


}
