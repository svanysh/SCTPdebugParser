/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.06.2013
 * Time: 22:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SctpHostData
{
	/// <summary>
	/// Description of HostConfig.
	/// </summary>
	public class HostConfig
	{
		#region Properties
		
		#region CC modules versions
		public string CPversion {get;set;}
		public string MMversion {get;set;}
		public string FEIFversion {get;set;}
		public string SCTPversion {get;set;}
		#endregion CC modules versions
		
		#region SCTP conf file
		public string SCTPcfVersion {get;set;}
		public int NumOfAssociations{get;set;}
		public int ICMPstatus{get;set;}
		public int PortRangeFrom{get;set;}
		public int PortRangeTo{get;set;}
		public int AssocReleaseBurstSize{get;set;}
		public int Stat64bitUpdateTimer{get;set;}
		
		public int MinRTO{get;set;}
		public int MaxRTO{get;set;}
		public int InitialRTO{get;set;}
		public int RTOalpha{get;set;}
		public int RTObeta{get;set;}
		public int ValidCookieLife{get;set;}
		public int AllowedIncrementCookieLife{get;set;}
		
		public int PathSelection{get;set;}
		public int AssocMaxRtx{get;set;}
		public int PathMaxRtx{get;set;}
		public int minThreshold{get;set;}
		public int maxThreshold{get;set;}
		public int PFMR{get;set;}
		public int MaxInitRtx{get;set;}
		public int MaxShutdownRtx{get;set;}
		
		public int HeartbeatInterval{get;set;}
		public bool HeartbeatStatus{get;set;}
		public int InitialHeartbeatInterval{get;set;}
		
		public int MIS{get;set;}
		public int MOS{get;set;}
		
		public int Mbuffer{get;set;}
		public int Nthreshold{get;set;}
		public int Npercentage{get;set;}
		public int InitialARWND{get;set;}
		public int PMTU{get;set;}
		
		public int MaxBurst{get;set;}
		
		public int SACKtimer{get;set;}
		public bool BundlingStatus{get;set;}
		public int BundlingTimer{get;set;}
		
		public int DSCP{get;set;}
		
		#endregion SCTP conf file
		
		#endregion Properties
		
		public HostConfig()
		{
		}
	}
}
