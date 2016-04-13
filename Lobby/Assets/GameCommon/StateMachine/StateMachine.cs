using System;
using System.Collections.Generic;

namespace GameCommon.StateMachine
{

	public class StateMachine 
	{
		public string state { get; set; }
		private string _prestate;

		public StateMachine()
		{
			_prestate = "None";
		}

		public List<string> stateupdate(string state)
		{
			if (state == "None")
				return null;

			if (_prestate == state)
				return null;

			//bet,open settle
			List<string> avalible_list = new List<string> ();
			if (state == "NewRoundState") 
			{
				avalible_list.Add("1");
				avalible_list.Add("0");
				avalible_list.Add("0");
				avalible_list.Add("0");
			}
			if (state == "StartBetState") 
			{
				avalible_list.Add("0");
				avalible_list.Add("1");
				avalible_list.Add("0");
				avalible_list.Add("0");
			}
			if (state == "EndBetState") 
			{
				avalible_list.Add("0");
				avalible_list.Add("1");
				avalible_list.Add("0");
				avalible_list.Add("0");
			}
			if (state == "OpenState") 
			{
				avalible_list.Add("0");
				avalible_list.Add("1");
				avalible_list.Add("0");
				avalible_list.Add("0");
			}
			if (state == "EndRoundState") 
			{
				avalible_list.Add("0");
				avalible_list.Add("0");
				avalible_list.Add("1");
				avalible_list.Add("0");
			}

			_prestate = state;
			return avalible_list;
		}


	}
}
