using System;
using UnityEngine;
using System.Collections;
using WebSocketSharp;


public class wstest : MonoBehaviour 
{

		public static void Main (string[] args)
		{
			Debug.Log ("aaa");
		using (var ws = new WebSocket ("ws://106.186.116.216:8001/gamesocket/token/e4da3b7fbbce2345d7772b0674a318d5")) {
				ws.OnMessage += (sender, e) =>{
					Console.WriteLine ("Laputa says: " + e.Data);
				};
				ws.OnClose += (sender, e) => {
					Console.WriteLine ("ws close" +e.Code);
				};
				ws.OnError += (sender, e) => {
					Console.Write ("ws error" + e.Exception);
				};

				//ws.Connect ();
				ws.ConnectAsync();
				//ws.Send ("BALUS");
			}
		}
}
