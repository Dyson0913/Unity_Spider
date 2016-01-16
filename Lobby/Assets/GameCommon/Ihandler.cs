using System;
using UnityEngine;
using System.Collections;

using Facebook.Unity;

public class Ihandler {

	private string status = "Ready";
	private string lastResponse = string.Empty;

	//TODO  Func<bool> Func<TResult> ?
	public delegate void MyMethodDelegate(params object[] args);

	public MyMethodDelegate call_back;

	public virtual void delga(params object[] args)
	{

	}

	public Ihandler()
	{
		call_back = null;
	}

	protected string LastResponse
	{
		get
		{
			return this.lastResponse;
		}
		
		set
		{
			this.lastResponse = value;
		}
	}
	
	protected string Status
	{
		get
		{
			return this.status;
		}
		
		set
		{
			this.status = value;
		}
	}

	private void HandleResult(IResult result)
	{
		if (result == null)
		{
			this.LastResponse = "Null Response\n";
			LogView.AddLog(this.LastResponse);		
			return;
		}
		
		//this.LastResponseTexture = null;
		
		// Some platforms return the empty string instead of null.
		if (!string.IsNullOrEmpty(result.Error))
		{
			this.Status = "Error - Check log for details";
			this.LastResponse = "Error Response:\n" + result.Error;
			LogView.AddLog(result.Error);
		}
		else if (result.Cancelled)
		{
			this.Status = "Cancelled - Check log for details";
			this.LastResponse = "Cancelled Response:\n" + result.RawResult;
			LogView.AddLog(result.RawResult);
		}
		else if (!string.IsNullOrEmpty(result.RawResult))
		{
			this.Status = "Success - Check log for details";
			this.LastResponse = "Success Response:\n" + result.RawResult;
			this.result_handle(result);
			LogView.AddLog(result.RawResult);
		}
		else
		{
			this.LastResponse = "Empty Response\n";
			LogView.AddLog(this.LastResponse);
		}
	}

	public virtual void result_handle(IResult result)
	{
		
	}
}
