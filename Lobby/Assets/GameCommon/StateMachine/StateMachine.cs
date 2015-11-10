using System;
using System.Collections.Generic;

namespace GameCommon.StateMachine
{

	public class StateMachine 
	{
		public string state { get; set; }


		public StateMachine()
		{
			
		}

		public List<string> stateupdate(string state)
		{
			if (state == "None")
				return null;
			//bet,open settle
			List<string> avalible_list = new List<string> ();
			if (state == "NewRoundState") 
			{
				avalible_list.Add("1");
				avalible_list.Add("0");
				avalible_list.Add("0");
			}
			if (state == "EndBetState") 
			{
				avalible_list.Add("0");
				avalible_list.Add("1");
				avalible_list.Add("0");
			}
			if (state == "OpenState") 
			{
				avalible_list.Add("0");
				avalible_list.Add("1");
				avalible_list.Add("0");
			}
			if (state == "EndRoundState") 
			{
				avalible_list.Add("0");
				avalible_list.Add("0");
				avalible_list.Add("1");
			}

			return avalible_list;
		}


	}
}
