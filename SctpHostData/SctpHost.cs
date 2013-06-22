/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 13.04.2013
 * Time: 14:40
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
		
		public SctpHost(String fileName)
		{
			if(fileName == "" || !File.Exists(fileName))
			{
				throw new Exception("incrrect path to file passed: "+fileName);
			}
			Endpoints = new Dictionary<int, SctpEndpoint>(128);
			Associations = new Dictionary<int, SctpAssociation>(512);
			SCTPIclients = new Dictionary<int, ExtClient>(8);
			Configuration = new HostConfig();
			
			StreamReader sr = File.OpenText(fileName);
			input = sr.ReadToEnd();
			
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
			
		}

		#region Fields		
		/// <summary>
		/// входная строка для парсинга
		/// </summary>
		protected string input;
		
		protected Dictionary<int, SctpEndpoint> EPforAssoc = new Dictionary<int, SctpEndpoint>(512);

		#endregion
		
		#region Patterns
		/// <summary>
		/// pattern for IP address
		/// </summary>
		const string ipPat = @"(?:[\d|a-f]{4}::[\d|a-f]{4}:[\d|a-f]{4})|(?:\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})";
		
		#region header
	    ///<summary>Pattern of header of sctphost command</summary>
	    /// <example>
	    /// |----------------------- SCTP HOST ---------------------|
	    /// |RpuId:            3
	    /// |SctpInstId:       1
	    /// |Base State:       SCTP_STARTED
	    /// |Host State:       A|C|R|X|IA
	    /// |Non-M3UA clients: 1
	    /// |Alarm Timer:      NOT RUNNING
	    /// |Top 5 profiles:    2(21) 3(1) 1(0) 4(0) 5(0)
	    /// |HOST type:        CBM (CXC1731354)
	    /// </example>
		const string headerPat =
			@".*SCTP HOST.*\n.*RpuId:[ ]+(?<rpuId>\d+).*\n.*SctpInstId:[ ]+(?<cpId>\d+).*\n"+
			@".*Base State:[ ]+(?<baseState>SCTP_\w+).*\n.*Host State:[ ]+(?<hostState>.*)\n"+
			@".*Non-M3UA clients:[ ]+(?<nExt>\d).*\n"/*+
			@".*\n.*\n(?:.*HOST type:[ ]+(?<board>\w{3}).*\n)?"*/;
        #endregion header	
		
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
			
		#region ext clients
		const string extInfoPat = 		
			@".*Client (?<id>\d) .pv(?<pv>\d).*0x(?<pid>\d+).*\n";
			
		#endregion ext clients
		
		#region BASE SCTP config
		
		/// <summary>
		/// Pattern for BASE configuration file
		/// </summary>
		/// <example>
		/// BASE labels
 		/// CP:   CAA20129-CP-R18F_3_EC06
 		/// MM:   CAA20130-MM-R9L
 		/// FEIF: CAA901892-FE_HD-R7F_3
 		/// SCTP: CAA901548-SCTP-R10F_1_EC02
		/// SCTP configFile:
		/// # SCTP config
		/// CAA901548R10T	File Version Number
		///...
		/// </example>
		const string sctpCFpat = 
			@"BASE labels.*\n.*CP:\s+(?<CPv>CAA20129-CP-R[\w|_]+).*\n"+
			@"\s*MM:\s+(?<MMv>CAA20130-MM-R[\w|_]+).*\n"+
			@"(?:\s*FEIF:\s+(?<FEIFv>CAA901892-FE_HD-R[\w|_]+).*\n)?"+
			@".*SCTP:\s+(?<SCTPv>CAA901548-SCTP-R[\w|_]+).*\n"+
			@".*\n.*\nCAA901548R(?<cfVer>[\w]+)\s+File Version Number.*\n"+
			@"(?<numOfAssocs>[0-9]+)\s+Number of Associations.*\n.*\n.*\n.*\n.*\n.*\n.*\n"+
			@"(?<ICMPst>[0-9]+)\s+ICMP Status.*\n.*\n.*\n.*\n.*\n"+
			@"(?<PortFrom>[0-9]+)\s+Port Range From.*\n"+
			@"(?<PortTo>[0-9]+)\s+Port Range To.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n"+
			@"(?<AsRelBurstSize>[0-9]+)\s+Associations release burst size.*\n.*\n.*\n.*\n.*\n"+
			@"(?<UpdTimer64stat>[0-9]+)\s+Statistics 64bit Update Timer.*\n.*\n.*\n.*\n.*\n.*\n.*\n.*\n"+
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
			@"(?<HBint>[0-9]+)\s+Heartbeat Interval in.*\n.*\n"+
			@"(?<HBstatus>[0|1])\s+Heartbeat Status.*\n.*\n"+
			@"(?<InitHBint>[0-9]+)\s+Initial HB Interval.*\n.*\n"+
			@"(?<MIS>[0-9]+)\s+Maximum Incoming Streams.*\n"+
			@"(?<MOS>[0-9]+)\s+Maximum Outgoing Streams.*\n"+
			@"(?<M>[0-9]+)\s+M in.*\n"+
			@"(?<N>[0-9]+)\s+N in .*\n"+
			@"(?<Nperc>[0-9]+)\s+N Percentage.*\n"+
			@"(?<ARWND>[0-9]+)\s+Initial Rwnd.*\n.*\n.*\n.*\n"+
			@"(?<MaxBurst>[0-9]+)\s+Maximum Burst.*\n"+
			@"(?<SACKt>[0-9]+)\s+SACK Timer.*\n.*\n"+
			@"(?<BundlSt>[0|1])\s+Bundling Status.*\n"+
			@"(?<BundlT>[0-9]+)\s+Bundling Timer.*\n"+
			@"(?<PS>[0-9]+)\s+Path Selection.*\n"+
			@"(?<PMTU>[0-9]+)\s+PMTU in.*\n.*\n.*\n.*\n.*\n.*\n"+
			@"(?<minThr>[0-9]+)\s+Minimum Activate Threshold.*\n"+
			@"(?<maxThr>[0-9]+)\s+Maximum Activate Threshold.*\n.*\n"+
			@"(?<PFMR>[0-9]+)\s+Primary path max rtx.*\n.*\n.*\n"+
			@"(?<DSCP>[0-9]+)\s+DSCP.*\n";
		
		#endregion
		
		#endregion
		
		#region Parser methods
		protected bool ParseHeader()
		{
			if (Regex.Matches(input, headerPat).Count > 1)
				throw new Exception("there are mote than one sctphost coli output in file");
			/*foreach (Match m in Regex.Matches(input, headerPat))
			{*/
			Match m = Regex.Match(input, headerPat);
				try
				{
					this.RpuId = Convert.ToInt32(m.Groups["rpuId"].Value);
					this.CpId = Convert.ToInt32(m.Groups["cpId"].Value);
					this.BASEstate = m.Groups["baseState"].Value;
					//this.HOSTstate = m.Groups["hostState"].Value;					
					switch (m.Groups["board"].Value)
					{
						case "GPB": this.Board = BoardType.GPB; break;
						case "CBM": this.Board = BoardType.CBM;break;
						case "EPB": this.Board = BoardType.EPB;break;
						default : this.Board= BoardType.UNKNOWN; break;
					}					 
				}
				catch
				{
					return false;
				}
			//}
			return true;
		}
		
		protected bool ParseConfig()
		{
			Match m = Regex.Match(input, sctpCFpat);
			try
			{
				Configuration.CPversion = m.Groups["CPv"].Value;
				Configuration.MMversion = m.Groups["MMv"].Value;
				Configuration.FEIFversion = m.Groups["FEIFv"].Value;
				Configuration.SCTPversion = m.Groups["SCTPv"].Value;
				
				Configuration.SCTPcfVersion = m.Groups["cfVer"].Value;
				Configuration.NumOfAssociations = Convert.ToInt32(m.Groups["numOfAssocs"].Value);
				Configuration.ICMPstatus = Convert.ToInt32(m.Groups["ICMPst"].Value);
				Configuration.PortRangeFrom = Convert.ToInt32(m.Groups["PortFrom"].Value);
				Configuration.PortRangeTo = Convert.ToInt32(m.Groups["PortTo"].Value);
				Configuration.AssocReleaseBurstSize = Convert.ToInt32(m.Groups["AsRelBurstSize"].Value);
				Configuration.Stat64bitUpdateTimer = Convert.ToInt32(m.Groups["UpdTimer64stat"].Value);
				Configuration.MinRTO = Convert.ToInt32(m.Groups["minRto"].Value);
				Configuration.MaxRTO = Convert.ToInt32(m.Groups["maxRto"].Value);
				Configuration.InitialRTO = Convert.ToInt32(m.Groups["initRto"].Value);
				Configuration.RTOalpha = Convert.ToInt32(m.Groups["rtoA"].Value);
				Configuration.RTObeta = Convert.ToInt32(m.Groups["rtoB"].Value);
				Configuration.ValidCookieLife = Convert.ToInt32(m.Groups["ValCOOKlife"].Value);
				Configuration.AllowedIncrementCookieLife = Convert.ToInt32(m.Groups["IncCOOKlife"].Value);
				Configuration.AssocMaxRtx = Convert.ToInt32(m.Groups["AMR"].Value);
				Configuration.PathMaxRtx = Convert.ToInt32(m.Groups["PMR"].Value);
				Configuration.MaxInitRtx = Convert.ToInt32(m.Groups["maxInitRtx"].Value);
				Configuration.MaxShutdownRtx = Convert.ToInt32(m.Groups["maxShDwnRtx"].Value);
				Configuration.HeartbeatInterval = Convert.ToInt32(m.Groups["HBint"].Value);
				Configuration.HeartbeatStatus = (m.Groups["HBstatus"].Value =="1")? true:false;
				Configuration.InitialHeartbeatInterval = Convert.ToInt32(m.Groups["InitHBint"].Value);
				Configuration.MIS = Convert.ToInt32(m.Groups["MIS"].Value);
				Configuration.MOS = Convert.ToInt32(m.Groups["MOS"].Value);
				Configuration.Mbuffer = Convert.ToInt32(m.Groups["M"].Value);
				Configuration.Nthreshold = Convert.ToInt32(m.Groups["N"].Value);
				Configuration.Npercentage = Convert.ToInt32(m.Groups["Nperc"].Value);
				Configuration.InitialARWND = Convert.ToInt32(m.Groups["ARWND"].Value);
				Configuration.MaxBurst = Convert.ToInt32(m.Groups["MaxBurst"].Value);
				Configuration.SACKtimer = Convert.ToInt32(m.Groups["SACKt"].Value);
				Configuration.BundlingStatus = (m.Groups["BundlSt"].Value=="1") ? true : false;
				Configuration.BundlingTimer = Convert.ToInt32(m.Groups["BundlT"].Value);
				Configuration.PathSelection = Convert.ToInt32(m.Groups["PS"].Value);
				Configuration.PMTU = Convert.ToInt32(m.Groups["PMTU"].Value);
				Configuration.minThreshold = Convert.ToInt32(m.Groups["minThr"].Value);
				Configuration.maxThreshold = Convert.ToInt32(m.Groups["maxThr"].Value);
				Configuration.PFMR = Convert.ToInt32(m.Groups["PFMR"].Value);
				Configuration.DSCP = Convert.ToInt32(m.Groups["DSCP"].Value);
				
			}
			catch
			{
				return false;
			}
			return true;
		}
		
		protected bool ParseExtClientsInfo()
		{
			foreach(Match m in Regex.Matches(input, extInfoPat))
			{
				try{
					int id = Convert.ToInt32(m.Groups["id"].Value);
					SCTPIclients.Add(id, new ExtClient(id, 
					                                   Convert.ToInt32(m.Groups["pv"].Value),
					                                   Int32.Parse(m.Groups["pid"].Value, NumberStyles.HexNumber)));
					                                   	
				}
				catch{continue;}
			}
			
			return true;
		}
		
		protected bool ParseEndpoints()
		{
			/* M3UA endpoints */
			foreach(Match m in Regex.Matches(input, m3epPat))
			{
				try
				{
					int epId = Convert.ToInt32(m.Groups["epId"].Value);
					Endpoints.Add(epId, new SctpEndpoint(epId,
				    	          	Convert.ToUInt16(m.Groups["port"].Value),
				    	          	m.Groups["ip1"].Value,
				    	          	m.Groups["ip2"].Value,
				    	          	Convert.ToInt32(m.Groups["dscp"].Value)));
					string [] assocList = m.Groups["assocList"].Value.Split(new char[]{' '},
					                                                        StringSplitOptions.RemoveEmptyEntries);
					foreach(string s in assocList)
						EPforAssoc.Add(Convert.ToInt32(s), Endpoints[epId]);
				}
				catch
				{
					continue;
				}
			}
			/* SCTPI endpoints */
			foreach(Match m in Regex.Matches(input, epPat))
			{
				try
				{
					int epId = Convert.ToInt32(m.Groups["epId"].Value);
					Endpoints.Add(epId, new SctpEndpoint(epId,
				    	          	Convert.ToUInt16(m.Groups["port"].Value),
				    	          	m.Groups["ip1"].Value,
				    	          	m.Groups["ip2"].Value,
				    	          	Convert.ToInt32(m.Groups["dscp"].Value),
				    	          	Convert.ToInt32(m.Groups["clId"].Value)));
					string [] assocList = m.Groups["assocList"].Value.Split(new char[]{' '},
					                                                        StringSplitOptions.RemoveEmptyEntries);
					foreach(string s in assocList)
						EPforAssoc.Add(Convert.ToInt32(s), Endpoints[epId]);
				}
				catch
				{
					continue;
				}
			}
			
			
			
			return true;
		}
		
		protected bool ParseAssociations()
		{
			foreach(Match m in Regex.Matches(input, assocPat))
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
					this.Associations.Add(aId,
					                      new SctpAssociation(aId, EPforAssoc[aId],
					                                          Convert.ToUInt16(m.Groups["rPort"].Value),
					                                          pathes));
						                                               
				}
				catch
				{
					continue;					
				}
			}
			
			return true;
		}
		
		protected bool ParseAssociationCounters()
		{
			foreach(Match m in Regex.Matches(input, AssocStat))
			{
				try
				{
					int aId = Convert.ToInt32(m.Groups["assocId"].Value);
					SctpAssociation assoc = Associations[aId];
					assoc.counters.OutDataChunks=Convert.ToInt32(m.Groups["OutDC"].Value);
					assoc.counters.InDataChunks=Convert.ToInt32(m.Groups["InDC"].Value);
					assoc.counters.OutOutOfOrderedChunks=Convert.ToInt32(m.Groups["OutOOO"].Value);
					assoc.counters.InOutOfOrdered=Convert.ToInt32(m.Groups["InOOO"].Value);
					assoc.counters.RtxChunks=Convert.ToInt32(m.Groups["Rtx"].Value);
					assoc.counters.OutControlChunks=Convert.ToInt32(m.Groups["OutCC"].Value);
					assoc.counters.InControlChunks=Convert.ToInt32(m.Groups["InCC"].Value);
					assoc.counters.OutFragmentedUserMsges=Convert.ToInt32(m.Groups["OutFragUM"].Value);
					assoc.counters.InReassembledUserMsges=Convert.ToInt32(m.Groups["InReasUM"].Value);
					assoc.counters.OutPacks=Convert.ToInt32(m.Groups["OutPacks"].Value);
					assoc.counters.InPacks=Convert.ToInt32(m.Groups["InPacks"].Value);
					assoc.counters.Congestions=Convert.ToInt32(m.Groups["Cong"].Value);
					assoc.counters.CongestionCeased=Convert.ToInt32(m.Groups["CongCease"].Value);
					assoc.counters.OutUserMsgDiscards=Convert.ToInt32(m.Groups["OutUMdisc"].Value);
					assoc.counters.InChunksDropped=Convert.ToInt32(m.Groups["InChunkDrop"].Value);
					
					assoc.counters.InDataChunkUnexpected=Convert.ToInt32(m.Groups["InDCunexp"].Value);
					assoc.counters.InDataChunkDiscards=Convert.ToInt32(m.Groups["InDCdisc"].Value);
					
					if(m.Groups["InSACKgap"].Value != "")
					{
						assoc.counters.InSackWithGap=Convert.ToInt32(m.Groups["InSACKgap"].Value);
						assoc.counters.OutSackWithGap=Convert.ToInt32(m.Groups["OutSACKgap"].Value);					
						assoc.counters.HBtimeouts=Convert.ToInt32(m.Groups["HBto"].Value);
						assoc.counters.InControlChunksAbnormal=Convert.ToInt32(m.Groups["InCCabnorm"].Value);
						assoc.counters.InControlChunkDiscards=Convert.ToInt32(m.Groups["InCCdisc"].Value);
						assoc.counters.InDataChunkAbnormal=Convert.ToInt32(m.Groups["InDCabnorm"].Value);
						assoc.counters.OwnZeroWindows=Convert.ToInt32(m.Groups["Own0win"].Value);
						assoc.counters.TimeOwnZeroWindow=Convert.ToInt32(m.Groups["Own0winTime"].Value);
						assoc.counters.PeerZeroWindows=Convert.ToInt32(m.Groups["Peer0win"].Value);
						assoc.counters.TimePeerZeroWindow=Convert.ToInt32(m.Groups["Peer0winTime"].Value);
						assoc.counters.TimeCongested=Convert.ToInt32(m.Groups["CongTime"].Value);
					}
				}
				catch {continue;}
			}
			return true;
		}
		#endregion Parsers methods
	}
}
