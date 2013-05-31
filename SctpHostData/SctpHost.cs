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
		public Dictionary<int, SctpEndpoint> Endpoints {get; internal set;}
		public Dictionary<int, SctpAssociation> Associations {get;set;}
		public Dictionary<int, ExtClient> SCTPIclients{get;set;}
		
		public int RpuId {get; internal set;}
		public int CpId {get; internal set;}
		public string BASEstate {get; internal set;}
		public string HOSTstate {get; internal set;}
		public BoardType Board {get; internal set;}
	
		public SctpHost(String fileName)
		{
			if(fileName == "" || !File.Exists(fileName))
			{
				throw new Exception("incrrect path to file passed: "+fileName);
			}
			Endpoints = new Dictionary<int, SctpEndpoint>(128);
			Associations = new Dictionary<int, SctpAssociation>(512);
			SCTPIclients = new Dictionary<int, ExtClient>(8);
			StreamReader sr = File.OpenText(fileName);
			input = sr.ReadToEnd();
			
			if(!ParseHeader())
			{
				throw new Exception("could not parse header of sctphost COLI command");
			}
			if (!ParseExtClientsInfo())
			{
				throw new Exception("could not parse ext client info in sctphost COLI command");
			}
			if(!ParseEnpoints())
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
					//continue;
				}
			//}
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
		
		protected bool ParseEnpoints()
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
				}
				catch {continue;}
			}
			return true;
		}
		#endregion Parsers methods
	}
}
