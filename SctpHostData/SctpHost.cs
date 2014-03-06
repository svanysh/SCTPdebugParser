/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 13.04.2013
 * Time: 14:40
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using System.Collections.Generic;

namespace SctpHostData
{
	/// <summary>
	/// Type of board where SCTP host is loaded
	/// </summary>
	public enum BoardType
	{
		UNKNOWN,
		GPB,
		CBM,
		EPB
	}
	
	/// <summary>
	/// Description of SctpHost.
	/// </summary>
	public class SctpHost
	{
		#region Properties		
		public Dictionary<int, SctpEndpoint> Endpoints {get; internal set;}
		public Dictionary<int, SctpAssociation> Associations {get;set;}
		public Dictionary<int, ExtClient> SCTPIclients{get;set;}
		
		public HostConfig Configuration {get;set;}
		
		public int RpuId {get; internal set;}
		public int CpId {get; internal set;}
		public string BASEstate {get; internal set;}
		public string HOSTstate {get; internal set;}
		public BoardType Board {get; internal set;}
		#endregion Properties
		
		public SctpHost()
		{			
			Endpoints = new Dictionary<int, SctpEndpoint>(128);
			Associations = new Dictionary<int, SctpAssociation>(512);
			SCTPIclients = new Dictionary<int, ExtClient>(9);
			//SCTPIclients.Add(Int32.MaxValue, new ExtClient(Int32.MaxValue,0,0));
			Configuration = new HostConfig();
		}

		#region Fields		
		
		#endregion		
	}
}
