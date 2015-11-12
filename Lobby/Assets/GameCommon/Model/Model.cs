using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameCommon.Model
{


	public class Model 
	{
		public static Model Instance
		{
			get { return Nested.Instance; }
		}
		
		private class Nested
		{
			static Nested()
			{
			}
			internal static readonly Model Instance = new Model();
		}

		public Dictionary<string,string> _model;

		public Model()
		{
			_model = new Dictionary<string, string> ();
		}

		public bool putValue(string key,string value)
		{
			if(! _model.ContainsKey(key))
			{
				_model.Add(key,value);
				return true;
			}
			return false;

		}

		public string getValue(string key)
		{
			if (_model.ContainsKey (key))
				return _model [key];
			return "";
		}
	}
}
