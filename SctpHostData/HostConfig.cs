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
		/// <summary>
		/// Common Parts version
		/// </summary>
		public string CPversion {get; internal set;}
		
		/// <summary>
		/// Managment module version
		/// </summary>
		public string MMversion {get; internal set;}
		
		/// <summary>
		/// FrontEnd module verison (empty for CBM board)
		/// </summary>
		public string FEIFversion {get; internal set;}
		
		/// <summary>
		/// BASE SCTP module version
		/// </summary>
		public string SCTPversion {get; internal set;}
		#endregion CC modules versions
		
		#region SCTP conf file
		/// <summary>
		/// version of SCTP configuratino file
		/// </summary>
		public string SCTPcfVersion {get;internal set;}
		
		/// <summary>
		/// Maximum number of SCTP association
		/// </summary>
		public int NumOfAssociations {get;internal set;}
		
		/// <summary>
		/// ICMP status (depends on NODE type)
		/// </summary>
		public int ICMPstatus{get; internal set;}				
		
		/// <summary>
		/// Minimum  port value (1 if port distribution is not used)
		/// </summary>
		public int PortRangeFrom{get; internal set;}
		
		/// <summary>
		/// Maximum port value (65535 if port distribution is not used)
		/// </summary>		
		public int PortRangeTo{get; internal set;}
				
		/// <summary>
		/// Maximum number of SCTP assocations to be realzed together (depends on CP internal queue size)
		/// </summary>
		public int AssocReleaseBurstSize{get; internal set;}
		
		/// <summary>
		/// Minimum RTO
		/// </summary>
		public int MinRTO{get; internal set;}
		
		/// <summary>
		/// Maximum RTO
		/// </summary>		
		public int MaxRTO{get; internal set;}
		
		/// <summary>
		/// Initial RTO
		/// </summary>
		public int InitialRTO{get; internal set;}
		
		/// <summary>
		/// RTO alpha index
		/// </summary>
		public int RTOalpha{get; internal set;}
		
		/// <summary>
		/// RTO beta index
		/// </summary>
		public int RTObeta{get; internal set;}
		
		/// <summary>
		/// Valid cookie life
		/// </summary>
		public int ValidCookieLife{get; internal set;}
		
		/// <summary>
		/// Allowed Increment Cookie Life
		/// </summary>
		public int AllowedIncrementCookieLife{get; internal set;}
		
		/// <summary>
		/// Path Selection
		/// </summary>
		public int PathSelection{get; internal set;}
		
		/// <summary>
		/// Assoc Max Rtx (AMR)
		/// </summary>
		public int AssocMaxRtx{get; internal set;}
		
		/// <summary>
		/// PathMaxRtx (PMR)
		/// </summary>
		public int PathMaxRtx{get; internal set;}
		
		/// <summary>
		/// Minimum Threshold for exponential switchback
		/// </summary>
		public int minThreshold{get; internal set;}
		
		/// <summary>
		/// Maximum Threshold for exponential switchback
		/// </summary>
		public int maxThreshold{get; internal set;}
		
		/// <summary>
		/// potentially failed Max RTX (PFMR)
		/// </summary>
		public int PFMR{get; internal set;}
		
		/// <summary>
		/// Maximum INIT (COOKIE ECHO) retransmission limit
		/// </summary>
		public int MaxInitRtx{get; internal set;}
		
		/// <summary>
		/// Maximum SHUTDWON retransmission limit
		/// </summary>
		public int MaxShutdownRtx{get; internal set;}
		
		/// <summary>
		/// HEARTBEAT Interval <seealso cref="InitialHeartbeatInterval"/>
		/// </summary>
		public int HeartbeatInterval{get; internal set;}
		
		/// <summary>
		/// Is HEARTBEAT mechanism enabled? <seealso cref="HeartbeatInterval"/>
		/// </summary>
		public bool HeartbeatStatus{get; internal set;}
		
		/// <summary>
		/// HEARTBEAT interval during path probing <seealso cref="HeartbeatInterval"/>
		/// </summary>
		public int InitialHeartbeatInterval{get; internal set;}
		
		/// <summary>
		/// Maximum number of incoming streams
		/// </summary>
		public int MIS{get; internal set;}
		
		/// <summary>
		/// Maximum number of outgoing streams
		/// </summary>
		public int MOS{get; internal set;}
		
		/// <summary>
		/// Size of M buffer
		/// </summary>
		public int Mbuffer{get; internal set;}
		
		/// <summary>
		/// N threshold - limit of <see cref="Mbuffer">M buffer</see> to send CongestionInd
		/// </summary>
		public int Nthreshold{get; internal set;}
		
		/// <summary>
		/// N percentage - percent of N threshold to send CongestionCeaseInd
		/// </summary>
		public int Npercentage{get; internal set;}
		
		/// <summary>
		/// Initial value of Advertise Reciver Window
		/// </summary>
		public int InitialARWND{get; internal set;}
		
		/// <summary>
		/// Path MTU - maimum size of SCTP packet
		/// </summary>
		public int PMTU{get; internal set;}
		
		/// <summary>
		/// Maximum burst
		/// </summary>
		public int MaxBurst{get; internal set;}
		
		/// <summary>
		/// Delay to wait more incoming DATA to send back SACK
		/// </summary>
		public int SACKtimer{get; internal set;}
		
		/// <summary>
		/// Is bundling enabled? <seealso cref="BundlingTimer"/>
		/// </summary>
		public bool BundlingStatus{get; internal set;}
		
		/// <summary>
		/// Bundling timer <seealso cref="BundlingStatus"/>
		/// </summary>
		public int BundlingTimer{get; internal set;}
		
		/// <summary>
		/// DSCP
		/// </summary>
		public int DSCP{get; internal set;}
		
		/// <summary>
		/// Zero windown supervision timer value.
		/// </summary>
		public int ZeroWndSupervisionTimer {get; internal set;}
		
		#endregion SCTP conf file
		
		#endregion Properties
		
		public HostConfig()
		{
		}
	}
}
