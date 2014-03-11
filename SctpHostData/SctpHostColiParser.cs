/*
 * Created by SharpDevelop.
 * User: Sergey
 * Date: 06.03.2014
 * Time: 22:07
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace SctpHostData
{
	/// <summary>
		/// Parser of "sctphost -all" COLI command
		/// </summary>
	public class SctpHostColiParser
	{
		private SctpHostColiParser() { }
		
		
		static SctpHost _host;
		static string input = "";
		static protected Dictionary<int, SctpEndpoint> EPforAssoc = new Dictionary<int, SctpEndpoint>(512);

		/// <summary>
		/// Parse SctpHost from input file with name @ref fileName
		/// </summary>
		/// <param name="fileName">name of file to parse SctpHost</param>
		/// <returns>parsed SctpHost</returns>
		static public SctpHost Parse(String fileName)
		{
			if(fileName == "" || !File.Exists(fileName))
			{
				throw new Exception("incrrect path to file passed: "+fileName);
			}
			StreamReader sr = File.OpenText(fileName);			
			input = sr.ReadToEnd();
			
			_host = new SctpHost();
			
			if(!ParseHeader())
			{
				throw new Exception("could not parse header of sctphost COLI command");
			}
			if(!ParseConfig())
			{
				throw new Exception("could not parse configuration part of sctphost COLI command");
			}			
			if (!ParseExtClientsInfo())
			{
				throw new Exception("could not parse ext client info in sctphost COLI command");
			}
			if(!ParseEndpoints())
			{
				throw new Exception("could not parse endpoints in scpthost coli command from target file: "+ fileName);
			}
			if(!ParseAssociations())
			{
				throw new Exception("could not parse associations in scpthost coli command from target file: "+ fileName);
			}
			if (!ParseAssociationCounters())
			{
				throw new Exception("could not parse assoc counters in scpthost coli command from target file: "+ fileName);
			}
			
			return _host;		
		}
		
		protected const RegexOptions reOpt = RegexOptions.ExplicitCapture | RegexOptions.Singleline;
		
		#region Common Patterns
		/// <summary>
		/// pattern for IP address
		/// </summary>
		const string ipPat = @"(?:[\d|a-f]{4}::[\d|a-f]{4}:[\d|a-f]{4})|(?:\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})";
		
		#endregion
		
		#region Parser methods
		
		#region header
	    /// <example>
	    /// |----------------------- SCTP HOST ---------------------|
	    /// |RpuId:            3
	    /// |SctpInstId:       1
	    /// |Base State:       SCTP_STARTED | FEIF_STARTED
	    /// |Host State:       A|C|R|X|IA
	    /// |Non-M3UA clients: 1
	    /// |Alarm Timer:      NOT RUNNING
	    /// |Top 5 profiles:    2(21) 3(1) 1(0) 4(0) 5(0)
	    /// |HOST type:        CBM (CXC1731354)
	    /// </example>
		const string headerPat =
			@".*SCTP HOST.*\n.*RpuId:[ ]+(?<rpuId>\d+).*\n.*SctpInstId:[ ]+(?<cpId>[\-]?\d+).*\n"+
			@".*Base State:[ ]+(?<baseState>SCTP_\w+).*\n.*Host State:[ ]+(?<hostState>.*)\n"+
			@".*Non-M3UA clients:[ ]+(?<nExt>\d).*\n"/*+
			@".*\n.*\n(?:.*HOST type:[ ]+(?<board>\w{3}).*\n)?"*/;
        #endregion header
		static protected bool ParseHeader()
		{
			if (Regex.Matches(input, headerPat, reOpt).Count > 1)
				throw new Exception("there are more than one sctphost coli output in file");
			/*foreach (Match m in Regex.Matches(input, headerPat))
			{*/
			Match m = Regex.Match(input, headerPat, reOpt);
				try
				{
					_host.RpuId = Convert.ToInt32(m.Groups["rpuId"].Value);
					_host.CpId = Convert.ToInt32(m.Groups["cpId"].Value);
					_host.BASEstate = m.Groups["baseState"].Value;
					//_host.HOSTstate = m.Groups["hostState"].Value;					
					switch (m.Groups["board"].Value)
					{
						case "GPB": _host.Board = BoardType.GPB; break;
						case "CBM": _host.Board = BoardType.CBM;break;
						case "EPB": _host.Board = BoardType.EPB;break;
						default : _host.Board= BoardType.UNKNOWN; break;
					}					 
				}
				catch
				{
					return false;
				}
			//}
			return true;
		}

		#region BASE labels
		
		/// <example>
		/// BASE labels
 		/// CP:   CAA20129-CP-R18F_3_EC06
 		/// MM:   CAA20130-MM-R9L
 		/// FEIF: CAA901892-FE_HD-R7F_3
 		/// SCTP: CAA901548-SCTP-R10F_1_EC02
		const string baseLabel = 
			@"BASE labels.*\n"+
			@".*CP:\s+(?<CPv>CAA20129-CP-R[\w|_]+).*\n"+
			@"\s*MM:\s+(?<MMv>CAA20130-MM-R[\w|_]+).*\n"+
			@"(?:\s*FEIF:\s+(?<FEIFv>CAA901892-FE_HD-R[\w|_]+).*\n)?"+
			@".*SCTP:\s+(?<SCTPv>CAA901548-SCTP-R[\w|_]+).*\n";
 		
		#endregion
		#region config file regex
		/// SCTP configFile:
		/// # SCTP config
		/// CAA901548R10T	File Version Number
		///...
		/// </example>
		const string sctpCFpat = 
			@".*\n.*\nCAA901548R10(?<cfVer>[\w]+)\s+File Version Number.*\n"+
			@"(?<numOfAssocs>[0-9]+)\s+Number of Associations.*\n"+
			@".*\n"+ //Interval for lost user handling
			@".*\n"+ //Key Change Period
			@".*\n"+ //Number of local IP addresses
			@".*\n"+ //Size of outgoing IP buffer
			@".*\n"+ //Size of incoming IP buffer
			@".*\n"+ //CRC-Calculation Status
			@"(?<ICMPst>[0-9]+)\s+ICMP Status.*\n"+
			@".*\n"+ //Bind To Device Flag
			@".*\n"+ //ULM Buffer Size
			@".*\n"+ //Total Assured Memory
			@".*\n"+ //ULM SFI Part
			@"(?<PortFrom>[0-9]+)\s+Port Range From.*\n"+
			@"(?<PortTo>[0-9]+)\s+Port Range To.*\n"+
			@".*\n"+ //Reconfiguration settings
			@".*\n"+ //PMTUD Accuracy
			@".*\n"+ //PMTUD Interval
			@".*\n"+ //Wait next probe interval
			@".*\n"+ //Number of attempts to discover congested paths
			@".*\n"+ //Minimum IPv4 PMTU
			@".*\n"+ //Maximum IPv4 PMTU
			@".*\n"+ //Maximum IPv6 PMTU
			@".*\n"+ //Start Delay
			@".*\n"+ //Takeover Process Limit
			@".*\n"+ //Unloading Max Number of Associations
			@".*\n"+ //Unloading General Delay
			@".*\n"+ //Unloading Burst Delay
			@".*\n"+ //Would Block Case Treshold
			@".*\n"+ //FE ID
			@".*\n"+ //Associations release burst timer 
			@"(?<AsRelBurstSize>[0-9]+)\s+Associations release burst size.*\n"+
			@".*\n"+ //Individual Takeover Process Time Limit
			@".*\n"+ //SCTP over UDP
			@".*\n"+ //UDP Local Port
			@".*\n"+ //UDP Remote Port
			@".*\n"+ //Statistics 64bit Update Timer
			@".*\n"+ //DNS Host Name Resolving Interval
			@".*\n"+ //Interval For Lost Primitives Check
			@".*\n"+ //Multi-home Robustness
			@".*\n"+ //Exhaustive IP Path Statistic Calculation
			@".*\n"+ //Initial RWND Setting Timeout
			@".*\n"+ //Maximum Number of Endpoint Users
			@"(?:.*\n)?"+ //Management behaviour bitmask
			@"(?:.*\n)?"+ //Check IP Interfaces timer value
			@".*\n"+ //Number of Optional Config Groups
			
			@".*\n"+ //Configuration Group ID
			@"(?:.*\n)?(?<minRto>[0-9]+)\s+Minimum RTO.*\n"+
			@"(?<maxRto>[0-9]+)\s+Maximum RTO.*\n"+
			@"(?<initRto>[0-9]+)\s+Initial Retransmission time-out.*\n"+
			@"(?<rtoA>[0-9]+)\s+RTO Alpha.*\n"+
			@"(?<rtoB>[0-9]+)\s+RTO Beta.*\n"+
			@"(?<ValCOOKlife>[0-9]+)\s+Valid Cookie Life.*\n"+
			@"(?<IncCOOKlife>[0-9]+)\s+Allowed Increment Cookie Life.*\n"+
			@"(?<AMR>[0-9]+)\s+Assoc.Max.Rtx.*\n"+
			@"(?<PMR>[0-9]+)\s+Path.Max.Rtx.*\n"+
			@"(?<maxInitRtx>[0-9]+)\s+Maximum Initial Retransmissions.*\n"+
			@"(?<maxShDwnRtx>[0-9]+)\s+Maximum Shutdown Retransmissions.*\n"+
			@"(?<HBint>[0-9]+)\s+Heartbeat Interval in.*\n"+
			@".*\n"+ //Heartbeat Interval Reduce Rate
			@"(?<HBstatus>[0|1])\s+Heartbeat Status.*\n"+
			@".*\n"+ //	Hb.Max.Burst
			@"(?<InitHBint>[0-9]+)\s+Initial HB Interval.*\n"+
			@".*\n"+ //	Hb.Max.Burst
			@"(?<MIS>[0-9]+)\s+Maximum Incoming Streams.*\n"+
			@"(?<MOS>[0-9]+)\s+Maximum Outgoing Streams.*\n"+
			@"(?<M>[0-9]+)\s+M in.*\n"+
			@"(?<N>[0-9]+)\s+N in .*\n"+
			@"(?<Nperc>[0-9]+)\s+N Percentage.*\n"+
			@"(?<ARWND>[0-9]+)\s+Initial Rwnd.*\n"+
			@".*\n"+ //Arwnd Update Threshold
			@".*\n"+ //Maximum Number Of OOB Packets
			@".*\n"+ //Interval For OOB Packets 
			@"(?<MaxBurst>[0-9]+)\s+Maximum Burst.*\n"+
			@"(?<SACKt>[0-9]+)\s+SACK Timer.*\n"+
			@".*\n"+ //	SACK Frequency
			@"(?<BundlSt>[0|1])\s+Bundling Status.*\n"+
			@"(?<BundlT>[0-9]+)\s+Bundling Timer.*\n"+
			@"(?<PS>[0-9]+)\s+Path Selection.*\n"+
			@"(?<PMTU>[0-9]+)\s+PMTU in.*\n"+
			@".*\n"+ //IPv6 PMTU
			@".*\n"+ //ECN Capability
			@".*\n"+ //DNS support
			@".*\n"+ //Stream Statistic Flag
			@".*\n"+ //			Block Cross-paths Flag
			@"(?<minThr>[0-9]+)\s+Minimum Activate Threshold.*\n"+
			@"(?<maxThr>[0-9]+)\s+Maximum Activate Threshold.*\n"+
			@".*\n"+ //Activate Threshold Factor
			@"(?<PFMR>[0-9]+)\s+Primary path max rtx.*\n"+
			@".*\n"+ //Number of Attempts to Probe Unreachable IP Paths
			@".*\n"+ //Probing Unreachable IP Paths Interval
			@"(?<DSCP>[0-9]+)\s+DSCP.*\n"+
			@"(?<zwndST>[0-9]+)\s+Zero RWND Supervision Timer.*\n";
		#endregion config file regex
		static protected bool ParseConfig()
		{
			//TODO parse IP addresses
			Match m = Regex.Match(input, baseLabel, reOpt);
			try
			{
				_host.Configuration.CPversion = m.Groups["CPv"].Value;
				_host.Configuration.MMversion = m.Groups["MMv"].Value;
				_host.Configuration.FEIFversion = m.Groups["FEIFv"].Value;
				_host.Configuration.SCTPversion = m.Groups["SCTPv"].Value;
			}
			catch
			{
				return false;
			}
			m = Regex.Match(input, sctpCFpat, reOpt);
			try
			{
				_host.Configuration.SCTPcfVersion = m.Groups["cfVer"].Value;
				_host.Configuration.NumOfAssociations = Convert.ToInt32(m.Groups["numOfAssocs"].Value);
				_host.Configuration.ICMPstatus = Convert.ToInt32(m.Groups["ICMPst"].Value);
				_host.Configuration.PortRangeFrom = Convert.ToInt32(m.Groups["PortFrom"].Value);
				_host.Configuration.PortRangeTo = Convert.ToInt32(m.Groups["PortTo"].Value);
				_host.Configuration.AssocReleaseBurstSize = Convert.ToInt32(m.Groups["AsRelBurstSize"].Value);
				_host.Configuration.MinRTO = Convert.ToInt32(m.Groups["minRto"].Value);
				_host.Configuration.MaxRTO = Convert.ToInt32(m.Groups["maxRto"].Value);
				_host.Configuration.InitialRTO = Convert.ToInt32(m.Groups["initRto"].Value);
				_host.Configuration.RTOalpha = Convert.ToInt32(m.Groups["rtoA"].Value);
				_host.Configuration.RTObeta = Convert.ToInt32(m.Groups["rtoB"].Value);
				_host.Configuration.ValidCookieLife = Convert.ToInt32(m.Groups["ValCOOKlife"].Value);
				_host.Configuration.AllowedIncrementCookieLife = Convert.ToInt32(m.Groups["IncCOOKlife"].Value);
				_host.Configuration.AssocMaxRtx = Convert.ToInt32(m.Groups["AMR"].Value);
				_host.Configuration.PathMaxRtx = Convert.ToInt32(m.Groups["PMR"].Value);
				_host.Configuration.MaxInitRtx = Convert.ToInt32(m.Groups["maxInitRtx"].Value);
				_host.Configuration.MaxShutdownRtx = Convert.ToInt32(m.Groups["maxShDwnRtx"].Value);
				_host.Configuration.HeartbeatInterval = Convert.ToInt32(m.Groups["HBint"].Value);
				_host.Configuration.HeartbeatStatus = (m.Groups["HBstatus"].Value =="1")? true:false;
				_host.Configuration.InitialHeartbeatInterval = Convert.ToInt32(m.Groups["InitHBint"].Value);
				_host.Configuration.MIS = Convert.ToInt32(m.Groups["MIS"].Value);
				_host.Configuration.MOS = Convert.ToInt32(m.Groups["MOS"].Value);
				_host.Configuration.Mbuffer = Convert.ToInt32(m.Groups["M"].Value);
				_host.Configuration.Nthreshold = Convert.ToInt32(m.Groups["N"].Value);
				_host.Configuration.Npercentage = Convert.ToInt32(m.Groups["Nperc"].Value);
				_host.Configuration.InitialARWND = Convert.ToInt32(m.Groups["ARWND"].Value);
				_host.Configuration.MaxBurst = Convert.ToInt32(m.Groups["MaxBurst"].Value);
				_host.Configuration.SACKtimer = Convert.ToInt32(m.Groups["SACKt"].Value);
				_host.Configuration.BundlingStatus = (m.Groups["BundlSt"].Value=="1") ? true : false;
				_host.Configuration.BundlingTimer = Convert.ToInt32(m.Groups["BundlT"].Value);
				_host.Configuration.PathSelection = Convert.ToInt32(m.Groups["PS"].Value);
				_host.Configuration.PMTU = Convert.ToInt32(m.Groups["PMTU"].Value);
				_host.Configuration.minThreshold = Convert.ToInt32(m.Groups["minThr"].Value);
				_host.Configuration.maxThreshold = Convert.ToInt32(m.Groups["maxThr"].Value);
				_host.Configuration.PFMR = Convert.ToInt32(m.Groups["PFMR"].Value);
				_host.Configuration.DSCP = Convert.ToInt32(m.Groups["DSCP"].Value);
				_host.Configuration.ZeroWndSupervisionTimer = Convert.ToInt32(m.Groups["zwndST"].Value);				
			}
			catch
			{
				return false;
			}
			return true;
		}
		
		#region ext clients
		const string extInfoPat = 		
			@".*Client (?<id>\d) .pv(?<pv>\d).*0x(?<pid>\d+).*\n";
			
		#endregion ext clients
		static protected bool ParseExtClientsInfo()
		{
			foreach(Match m in Regex.Matches(input, extInfoPat, reOpt))
			{
				try
				{
					int id = Convert.ToInt32(m.Groups["id"].Value);
					_host.SCTPIclients.Add(id, new ExtClient(id, 
					                                   Convert.ToInt32(m.Groups["pv"].Value),
					                                   Int32.Parse(m.Groups["pid"].Value, NumberStyles.HexNumber)));
					                                   	
				}
				catch{continue;}
			}
			
			return true;
		}
		
		#region EP pattern
		/// <summary>
		/// pattern of enpdpoint in scpthost -ep -all section
		/// </summary>
		/// <example>
		/// |------------------- SCTP ENDPOINT   1 -----------------|
		/// |-------------------------------------------------------|
		/// | localPort:            5113
		/// | dscp:                 40
		/// | numOfLocalIp:         1
		/// | localIpAddress 1:     10.70.48.102
		/// | list of associations: 1 6
		/// | Client identity:      0
		/// </example>
		const string epPat =
			@"SCTP ENDPOINT[ ]+(?<epId>\d{1,3}).*\n.*\n.*localPort:[ ]+(?<port>\d{1,5}).*\n"+
			@".*dscp:[ ]+(?<dscp>\d+).*\n.*numOfLocalIp:[ ]+(?<nIP>\d).*\n"+
			".*localIpAddress 1:[ ]+ (?<ip1>"+ipPat+").*\n(?:.*localIpAddress 2:[ ]+ (?<ip2>"+ipPat+").*\n*)?"+
			@".*list of associations:[ ]+(?<assocList>\d+(?: \d+)*).*\n"+
			@".*Client identity:[ ]+(?<clId>\d).*\n";
		
		const string m3epPat =
			@"SCTP ENDPOINT[ ]+(?<epId>\d{1,3}).*\n.*\n.*localPort:[ ]+(?<port>\d{1,5}).*\n"+
			@".*dscp:[ ]+(?<dscp>\d+).*\n.*numOfLocalIp:[ ]+(?<nIP>\d).*\n"+
			".*localIpAddress 1:[ ]+ (?<ip1>"+ipPat+").*\n(?:.*localIpAddress 2:[ ]+ (?<ip2>"+ipPat+").*\n*)?"+
			@".*list of associations:[ ]+(?<assocList>\d+(?: \d+)*).*\n"+
			@".*M3UA endpoint.*\n";
		#endregion EP pattern
		static protected bool ParseEndpoints()
		{
			/* M3UA endpoints */
			foreach(Match m in Regex.Matches(input, m3epPat, reOpt))
			{
				try
				{
					int epId = Convert.ToInt32(m.Groups["epId"].Value);
					_host.Endpoints.Add(epId, new SctpEndpoint(epId,
				    	          	Convert.ToUInt16(m.Groups["port"].Value),
				    	          	m.Groups["ip1"].Value,
				    	          	m.Groups["ip2"].Value,
				    	          	Convert.ToInt32(m.Groups["dscp"].Value)));
					string [] assocList = m.Groups["assocList"].Value.Split(new char[]{' '},
					                                                        StringSplitOptions.RemoveEmptyEntries);
					foreach(string s in assocList)
						EPforAssoc.Add(Convert.ToInt32(s), _host.Endpoints[epId]);
				}
				catch
				{
					continue;
				}
			}
			/* SCTPI endpoints */
			foreach(Match m in Regex.Matches(input, epPat, reOpt))
			{
				try
				{
					int epId = Convert.ToInt32(m.Groups["epId"].Value);
					_host.Endpoints.Add(epId, new SctpEndpoint(epId,
				    	          	Convert.ToUInt16(m.Groups["port"].Value),
				    	          	m.Groups["ip1"].Value,
				    	          	m.Groups["ip2"].Value,
				    	          	Convert.ToInt32(m.Groups["dscp"].Value),
				    	          	Convert.ToInt32(m.Groups["clId"].Value)));
					string [] assocList = m.Groups["assocList"].Value.Split(new char[]{' '},
					                                                        StringSplitOptions.RemoveEmptyEntries);
					foreach(string s in assocList)
						EPforAssoc.Add(Convert.ToInt32(s), _host.Endpoints[epId]);
				}
				catch
				{
					continue;
				}
			}
			
			
			
			return true;
		}

		#region assoc pattern
		/// <summary>
		/// pattern of association in sctphost -assoc -all section
		/// </summary>
		/// <example>
		/// |---------------- SCTP ASSOCIATION   6 -----------------|
		/// |-------------------------------------------------------|
		/// | localPort:             4001
		/// | remotePort:            4001
		/// | dscp:                  0
		/// | ulpKey:                0x43e1c600
		/// | switchbackThreshold:   1
		/// | recentSuccessfulHBs:   0
		/// | sctpProfileId:         2
		/// |-----------------Path Information----------------------|
		/// | abcd::83a0:4310 - abcd::83a0:4311 | preferred and active primary | last used | active   (+) | pmr: 0 
		/// </example>
		const string assocPat =
			@"SCTP ASSOCIATION[ ]+(?<assocId>\d{1,3}).*\n.*\n"+
			@".*localPort:[ ]+(?<lPort>\d{1,5}).*\n.*remotePort:[ ]+(?<rPort>\d{1,5}).*\n" +
			@".*dscp:[ ]+(?<dscp>\d+).*\n.*ulpKey:[ ]+(?<ulpKey>\d+).*\n.*\n.*\n" +
			@".*sctpProfileId:[ ]+(?<profId>\d+).*\n" +
			".*Path Information.*\n" +
			".* (?<la1>"+ipPat+")" + " - (?<ra1>"+ipPat+").*\n"+
			"(?:.* (?<la2>"+ipPat+")" + " - (?<ra2>"+ipPat+").*\n)?"+
			"(?:.* (?<la3>"+ipPat+")" + " - (?<ra3>"+ipPat+").*\n)?"+
			"(?:.* (?<la4>"+ipPat+")" + " - (?<ra4>"+ipPat+").*\n)?"
			;
		#endregion assoc pattern		
		static protected bool ParseAssociations()
		{
			foreach(Match m in Regex.Matches(input, assocPat, reOpt))
			{
				try
				{
					List<IpPath> pathes = new List<IpPath>();
					pathes.Add(new IpPath(m.Groups["la1"].Value, m.Groups["ra1"].Value));
					if (m.Groups["la2"].Value != "")
					{
						pathes.Add(new IpPath(m.Groups["la2"].Value, m.Groups["ra2"].Value));
						if (m.Groups["la3"].Value != "")
						{
							pathes.Add(new IpPath(m.Groups["la3"].Value, m.Groups["ra3"].Value));
							if (m.Groups["la4"].Value != "")
							{
								pathes.Add(new IpPath(m.Groups["la4"].Value, m.Groups["ra4"].Value));
								
							}
						}
					}
						
					
					int aId = Convert.ToInt32(m.Groups["assocId"].Value);
					int ulpKey = Convert.ToInt32(m.Groups["ulpKey"].Value);
					int dscp = Convert.ToInt32(m.Groups["dscp"].Value);
					_host.Associations.Add(aId,
					                      new SctpAssociation(aId, EPforAssoc[aId],
					                                          Convert.ToUInt16(m.Groups["rPort"].Value),
					                                          pathes, ulpKey,dscp));
						                                               
				}
				catch
				{
					continue;					
				}
			}
			
			return true;
		}
		
		#region assoc stat
		/// <summary>
		/// pattern for sctphost -assoc -stat section
		/// </summary>
		/// <example>
		/// |-------------------- SCTP ASSOCIATION  6 --------------------|
		/// |--------------------------------------------------------------|
		/// |------------------ Statistic (assoc level) -------------------|
		/// | [ID  7]:            assocStatOutDataChunks, Count:          8|
		/// | [ID  8]:             assocStatInDataChunks, Count:          7|
		/// | [ID  9]:    assocStatOutOutOfOrderedChunks, Count:          0|
		/// | [ID 10]:           assocStatInOutOfOrdered, Count:          0|
		/// | [ID 12]:                assocStatRtxChunks, Count:          0|
		/// | [ID 13]:         assocStatOutControlChunks, Count:         18|
		/// | [ID 14]:          assocStatInControlChunks, Count:         10|
		/// | [ID 15]:   assocStatOutFragmentedUserMsges, Count:          0|
		/// | [ID 16]:   assocStatInReassembledUserMsges, Count:          0|
		/// | [ID 17]:                 assocStatOutPacks, Count:         21|
		/// | [ID 18]:                  assocStatInPacks, Count:         13|
		/// | [ID 19]:              assocStatCongestions, Count:          0|
		/// | [ID 20]:         assocStatCongestionCeased, Count:          0|
		/// | [ID 21]:       assocStatOutUserMsgDiscards, Count:          0|
		/// | [ID 22]:          assocStatInChunksDropped, Count:          0|
		/// | [ID 37]:            assocStatInSackWithGap, Count:          0|
		/// | [ID 38]:           assocStatOutSackWithGap, Count:          0|
		/// | [ID 41]:               assocStatHBtimeouts, Count:          2|
		/// | [ID 48]:   assocStatInControlChunkAbnormal, Count:          0|
		/// | [ID 49]:   assocStatInControlChunkDiscards, Count:          0|
		/// | [ID 50]:      assocStatInDataChunkAbnormal, Count:          0|
		/// | [ID 51]:    assocStatInDataChunkUnexpected, Count:          0|
		/// | [ID 55]:      assocStatInDataChunkDiscards, Count:          0|
		/// | [ID 57]:           assocStatOwnZeroWindows, Count:          0|
		/// | [ID 58]:        assocStatTimeOwnZeroWindow, Count:          0|
		/// | [ID 59]:          assocStatPeerZeroWindows, Count:          0|
		/// | [ID 60]:       assocStatTimePeerZeroWindow, Count:          0|
		/// | [ID 67]:            assocStatTimeCongested, Count:          0|
		/// </example>
		const string AssocStat = 
			@"SCTP ASSOCIATION[ ]+(?<assocId>\d{1,3}).*\n.*\n.*Statistic.*\n"+
			@".*ID  7.*Count:[ ]+(?<OutDC>\d+).*\n"+
			@".*ID  8.*Count:[ ]+(?<InDC>\d+).*\n"+
			@".*ID  9.*Count:[ ]+(?<OutOOO>\d+).*\n"+
			@".*ID 10.*Count:[ ]+(?<InOOO>\d+).*\n"+
			@".*ID 12.*Count:[ ]+(?<Rtx>\d+).*\n"+
			@".*ID 13.*Count:[ ]+(?<OutCC>\d+).*\n"+
			@".*ID 14.*Count:[ ]+(?<InCC>\d+).*\n"+
			@".*ID 15.*Count:[ ]+(?<OutFragUM>\d+).*\n"+
			@".*ID 16.*Count:[ ]+(?<InReasUM>\d+).*\n"+
			@".*ID 17.*Count:[ ]+(?<OutPacks>\d+).*\n"+
			@".*ID 18.*Count:[ ]+(?<InPacks>\d+).*\n"+
			@".*ID 19.*Count:[ ]+(?<Cong>\d+).*\n"+
			@".*ID 20.*Count:[ ]+(?<CongCease>\d+).*\n"+
			@".*ID 21.*Count:[ ]+(?<OutUMdisc>\d+).*\n"+
			@".*ID 22.*Count:[ ]+(?<InChunkDrop>\d+).*\n"+
			@"(?:.*ID 37.*Count:[ ]+(?<InSACKgap>\d+).*\n)?"+
			@"(?:.*ID 38.*Count:[ ]+(?<OutSACKgap>\d+).*\n)?"+
			@"(?:.*ID 41.*Count:[ ]+(?<HBto>\d+).*\n)?"+
			@"(?:.*ID 48.*Count:[ ]+(?<InCCabnorm>\d+).*\n)?"+
			@"(?:.*ID 49.*Count:[ ]+(?<InCCdisc>\d+).*\n)?"+
			@"(?:.*ID 50.*Count:[ ]+(?<InDCabnorm>\d+).*\n)?"+
			@".*ID 51.*Count:[ ]+(?<InDCunexp>\d+).*\n"+
			@".*ID 55.*Count:[ ]+(?<InDCdisc>\d+).*\n"+
			@"(?:.*ID 57.*Count:[ ]+(?<Own0win>\d+).*\n)?"+
			@"(?:.*ID 58.*Count:[ ]+(?<Own0winTime>\d+).*\n)?"+
			@"(?:.*ID 59.*Count:[ ]+(?<Peer0win>\d+).*\n)?"+
			@"(?:.*ID 60.*Count:[ ]+(?<Peer0winTime>\d+).*\n)?"+
			@"(?:.*ID 67.*Count:[ ]+(?<CongTime>\d+).*\n)?";
			
		#endregion assoc stat	
		#region assoc stat in C14.2
		
		/// <summary>
		/// pattern for sctphost -assoc -stat section in C14.2
		/// </summary>
		/// <example>
		/// |-------------------- SCTP ASSOCIATION   1 --------------------|
		/// |------------------------ Outgoing stat -----------------------|
		/// | [ID  7]:               assocStatSentChunks, Count:     723539|
		/// | [ID 12]:            assocStatRetransChunks, Count:          1|
		/// | [ID  9]:     assocStatOutOfOrderSentChunks, Count:          0|
		/// | [ID 21]:        assocStatSentChunksDropped, Count:          0|
		/// | [ID 13]:        assocStatSentControlChunks, Count:     684022|
		/// | [ID 38]:          assocStatSentSackWithGap, Count:          0|
		/// | [ID 41]:               assocStatHBtimeouts, Count:          0|
		/// | [ID 17]:             assocStatSentPackages, Count:     757071|
		/// | [ID 15]:        assocStatFragmentedUserMsg, Count:          0|
		/// | [ID 46]:             assocStatOutOctetsLow, Count:  103960032|
		/// | [ID 47]:            assocStatOutOctetsHigh, Count:          0|
		/// |------------------------ Incoming stat -----------------------|
		/// | [ID  8]:                assocStatRecChunks, Count:     723539|
		/// | [ID 10]:      assocStatOutOfOrderRecChunks, Count:          0|
		/// | [ID 50]:     assocStatRecDataChunkAbnormal, Count:          0|
		/// | [ID 51]:  assocStatRecDataChunksUnexpected, Count:          0|
		/// | [ID 55]:      assocStatRecOutOfFreeSpaceDC, Count:          0|
		/// | [ID 14]:    assocStatReceivedControlChunks, Count:     715110|
		/// | [ID 37]:           assocStatRecSackWithGap, Count:          3|
		/// | [ID 48]:  assocStatRecControlChunkAbnormal, Count:          0|
		/// | [ID 49]:  assocStatRecControlChunkDiscards, Count:          0|
		/// | [ID 22]:         assocStatRecChunksDropped, Count:          0|
		/// | [ID 18]:         assocStatReceivedPackages, Count:     724819|
		/// | [ID 16]:       assocStatReassembledUserMsg, Count:          0|
		/// | [ID 44]:              assocStatInOctetsLow, Count:  104070220|
		/// | [ID 45]:             assocStatInOctetsHigh, Count:          0|
		/// |------------------------ AssocStates -------------------------|
		/// | [ID 19]:                 assocStatCommStop, Count:          0|
		/// | [ID 20]:               assocStatCommResume, Count:          0|
		/// | [ID 67]:            assocStatTimeCongested, Count:          0|
		/// | [ID 57]:       assocStatTimesOwnZeroWindow, Count:          0|
		/// | [ID 58]:   assocStatTotalTimeOwnZeroWindow, Count:          0|
		/// | [ID 59]:      assocStatTimesPeerZeroWindow, Count:          0|
		/// | [ID 60]:  assocStatTotalTimePeerZeroWindow, Count:          0|
		/// |--------------------------------------------------------------|
		/// | Extended association ID:   50331649                          |
		/// </example>
		const string AssocStat14B = 
			@"SCTP ASSOCIATION[ ]+(?<assocId>\d+).*\n"+
			@".*Outgoing stat.*\n"+
			@".*ID  7.*Count:[ ]+(?<OutDC>\d+).*\n"+
			@".*ID 12.*Count:[ ]+(?<Rtx>\d+).*\n"+
			@".*ID  9.*Count:[ ]+(?<OutOOO>\d+).*\n"+
			@".*ID 21.*Count:[ ]+(?<OutUMdisc>\d+).*\n"+
			@".*ID 13.*Count:[ ]+(?<OutCC>\d+).*\n"+
			@".*ID 38.*Count:[ ]+(?<OutSACKgap>\d+).*\n"+
			@".*ID 41.*Count:[ ]+(?<HBto>\d+).*\n"+
			@".*ID 17.*Count:[ ]+(?<OutPacks>\d+).*\n"+
			@".*ID 15.*Count:[ ]+(?<OutFragUM>\d+).*\n"+
			@".*ID 46.*Count:[ ]+(?<OutOctL>\d+).*\n"+
			@".*ID 47.*Count:[ ]+(?<OutOctH>\d+).*\n"+			
			@".*\n" + //Incoming stat
			@".*ID  8.*Count:[ ]+(?<InDC>\d+).*\n"+
			@".*ID 10.*Count:[ ]+(?<InOOO>\d+).*\n"+
			@".*ID 50.*Count:[ ]+(?<InDCabnorm>\d+).*\n"+
			@".*ID 51.*Count:[ ]+(?<InDCunexp>\d+).*\n"+
			@".*ID 55.*Count:[ ]+(?<InDCdisc>\d+).*\n"+
			@".*ID 14.*Count:[ ]+(?<InCC>\d+).*\n"+
			@".*ID 37.*Count:[ ]+(?<InSACKgap>\d+).*\n"+
			@".*ID 48.*Count:[ ]+(?<InCCabnorm>\d+).*\n"+
			@".*ID 49.*Count:[ ]+(?<InCCdisc>\d+).*\n"+
			@".*ID 22.*Count:[ ]+(?<InChunkDrop>\d+).*\n"+
			@".*ID 18.*Count:[ ]+(?<InPacks>\d+).*\n"+
			@".*ID 16.*Count:[ ]+(?<InReasUM>\d+).*\n"+
			@".*ID 44.*Count:[ ]+(?<InOctL>\d+).*\n"+
			@".*ID 45.*Count:[ ]+(?<InOctH>\d+).*\n"+
			@".*\n" + //AssocStates
			@".*ID 19.*Count:[ ]+(?<Cong>\d+).*\n"+
			@".*ID 20.*Count:[ ]+(?<CongCease>\d+).*\n"+
			@".*ID 67.*Count:[ ]+(?<CongTime>\d+).*\n"+
			@".*ID 57.*Count:[ ]+(?<Own0win>\d+).*\n"+
			@".*ID 58.*Count:[ ]+(?<Own0winTime>\d+).*\n"+
			@".*ID 59.*Count:[ ]+(?<Peer0win>\d+).*\n"+
			@".*ID 60.*Count:[ ]+(?<Peer0winTime>\d+).*\n";			
		#endregion assoc stat in C14.2
		static protected bool ParseAssociationCounters()
		{
            bool isC14B = true;
			foreach(Match m in Regex.Matches(input, AssocStat, reOpt))
			{
				try
				{
					int aId = Convert.ToInt32(m.Groups["assocId"].Value);
					SctpAssociation assoc = _host.Associations[aId];
					assoc.counters.OutDataChunks=Convert.ToUInt32(m.Groups["OutDC"].Value);
					assoc.counters.InDataChunks=Convert.ToUInt32(m.Groups["InDC"].Value);
					assoc.counters.OutOutOfOrderedChunks=Convert.ToUInt32(m.Groups["OutOOO"].Value);
					assoc.counters.InOutOfOrdered=Convert.ToUInt32(m.Groups["InOOO"].Value);
					assoc.counters.RtxChunks=Convert.ToUInt32(m.Groups["Rtx"].Value);
					assoc.counters.OutControlChunks=Convert.ToUInt32(m.Groups["OutCC"].Value);
					assoc.counters.InControlChunks=Convert.ToUInt32(m.Groups["InCC"].Value);
					assoc.counters.OutFragmentedUserMsges=Convert.ToUInt32(m.Groups["OutFragUM"].Value);
					assoc.counters.InReassembledUserMsges=Convert.ToUInt32(m.Groups["InReasUM"].Value);
					assoc.counters.OutPacks=Convert.ToUInt32(m.Groups["OutPacks"].Value);
					assoc.counters.InPacks=Convert.ToUInt32(m.Groups["InPacks"].Value);
					assoc.counters.Congestions=Convert.ToUInt32(m.Groups["Cong"].Value);
					assoc.counters.CongestionCeased=Convert.ToUInt32(m.Groups["CongCease"].Value);
					assoc.counters.OutUserMsgDiscards=Convert.ToUInt32(m.Groups["OutUMdisc"].Value);
					assoc.counters.InChunksDropped=Convert.ToUInt32(m.Groups["InChunkDrop"].Value);
					
					assoc.counters.InDataChunkUnexpected=Convert.ToUInt32(m.Groups["InDCunexp"].Value);
					assoc.counters.InDataChunkDiscards=Convert.ToUInt32(m.Groups["InDCdisc"].Value);
					
					if(m.Groups["InSACKgap"].Value != "")
					{
						assoc.counters.InSackWithGap=Convert.ToUInt32(m.Groups["InSACKgap"].Value);
						assoc.counters.OutSackWithGap=Convert.ToUInt32(m.Groups["OutSACKgap"].Value);					
						assoc.counters.HBtimeouts=Convert.ToUInt32(m.Groups["HBto"].Value);
						assoc.counters.InControlChunksAbnormal=Convert.ToUInt32(m.Groups["InCCabnorm"].Value);
						assoc.counters.InControlChunkDiscards=Convert.ToUInt32(m.Groups["InCCdisc"].Value);
						assoc.counters.InDataChunkAbnormal=Convert.ToUInt32(m.Groups["InDCabnorm"].Value);
						assoc.counters.OwnZeroWindows=Convert.ToUInt32(m.Groups["Own0win"].Value);
						assoc.counters.TimeOwnZeroWindow=Convert.ToUInt32(m.Groups["Own0winTime"].Value);
						assoc.counters.PeerZeroWindows=Convert.ToUInt32(m.Groups["Peer0win"].Value);
						assoc.counters.TimePeerZeroWindow=Convert.ToUInt32(m.Groups["Peer0winTime"].Value);
						assoc.counters.TimeCongested=Convert.ToUInt32(m.Groups["CongTime"].Value);
					}
				    isC14B = false;
				}
				catch {continue;}
			}
			
            if (isC14B)
            	foreach(Match m in Regex.Matches(input, AssocStat14B, reOpt))
			{
				try
				{
					int aId = Convert.ToInt32(m.Groups["assocId"].Value);
					SctpAssociation assoc = _host.Associations[aId];
					assoc.counters.OutDataChunks=Convert.ToUInt32(m.Groups["OutDC"].Value);
					assoc.counters.InDataChunks=Convert.ToUInt32(m.Groups["InDC"].Value);
					assoc.counters.OutOutOfOrderedChunks=Convert.ToUInt32(m.Groups["OutOOO"].Value);
					assoc.counters.InOutOfOrdered=Convert.ToUInt32(m.Groups["InOOO"].Value);
					assoc.counters.RtxChunks=Convert.ToUInt32(m.Groups["Rtx"].Value);
					assoc.counters.OutControlChunks=Convert.ToUInt32(m.Groups["OutCC"].Value);
					assoc.counters.InControlChunks=Convert.ToUInt32(m.Groups["InCC"].Value);
					assoc.counters.OutFragmentedUserMsges=Convert.ToUInt32(m.Groups["OutFragUM"].Value);
					assoc.counters.InReassembledUserMsges=Convert.ToUInt32(m.Groups["InReasUM"].Value);
					assoc.counters.OutPacks=Convert.ToUInt32(m.Groups["OutPacks"].Value);
					assoc.counters.InPacks=Convert.ToUInt32(m.Groups["InPacks"].Value);
					assoc.counters.Congestions=Convert.ToUInt32(m.Groups["Cong"].Value);
					assoc.counters.CongestionCeased=Convert.ToUInt32(m.Groups["CongCease"].Value);
					assoc.counters.OutUserMsgDiscards=Convert.ToUInt32(m.Groups["OutUMdisc"].Value);
					assoc.counters.InChunksDropped=Convert.ToUInt32(m.Groups["InChunkDrop"].Value);
					
					assoc.counters.InDataChunkUnexpected=Convert.ToUInt32(m.Groups["InDCunexp"].Value);
					assoc.counters.InDataChunkDiscards=Convert.ToUInt32(m.Groups["InDCdisc"].Value);
					
					assoc.counters.InSackWithGap=Convert.ToUInt32(m.Groups["InSACKgap"].Value);
					assoc.counters.OutSackWithGap=Convert.ToUInt32(m.Groups["OutSACKgap"].Value);					
					assoc.counters.HBtimeouts=Convert.ToUInt32(m.Groups["HBto"].Value);
					assoc.counters.InControlChunksAbnormal=Convert.ToUInt32(m.Groups["InCCabnorm"].Value);
					assoc.counters.InControlChunkDiscards=Convert.ToUInt32(m.Groups["InCCdisc"].Value);
					assoc.counters.InDataChunkAbnormal=Convert.ToUInt32(m.Groups["InDCabnorm"].Value);
					assoc.counters.OwnZeroWindows=Convert.ToUInt32(m.Groups["Own0win"].Value);
					assoc.counters.TimeOwnZeroWindow=Convert.ToUInt32(m.Groups["Own0winTime"].Value);
					assoc.counters.PeerZeroWindows=Convert.ToUInt32(m.Groups["Peer0win"].Value);
					assoc.counters.TimePeerZeroWindow=Convert.ToUInt32(m.Groups["Peer0winTime"].Value);
					assoc.counters.TimeCongested=Convert.ToUInt32(m.Groups["CongTime"].Value);
					assoc.counters.OutOctets = Convert.ToUInt64(m.Groups["OutOctH"].Value) * UInt32.MaxValue
						+ Convert.ToUInt32(m.Groups["OutOctL"].Value);
					assoc.counters.InOctets = Convert.ToUInt64(m.Groups["InOctH"].Value) * UInt32.MaxValue
						+ Convert.ToUInt32(m.Groups["InOctL"].Value);
				}
				catch {continue;}
			}
			
			return true;
		}
		#endregion Parsers methods
	}
}
