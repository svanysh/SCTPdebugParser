/*
 * Created by SharpDevelop.
 * User: svanysh
 * Date: 05/30/2013
 * Time: 14:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SctpHostData
{
	/// <summary>
	/// Description of ExtClient.
	/// </summary>
	public class ExtClient
	{
		public int ID {get; internal set;}
		public int PV{get; internal set;}
		public int PID{get; internal set;}
		
		public ExtClient(int id, int pv, int pid)
		{
			ID = id;
			PV = pv;
			PID = pid;
		}
	}
}
