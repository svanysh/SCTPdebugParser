/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.04.2013
 * Time: 20:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace SctpHostData
{
	public struct IpPath
	{
		public string localAddress;
		public string remoteAddress;
		public IpPath(string l, string r)
		{
			localAddress = l;
			remoteAddress = r;
		}
	}

	
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class SctpAssociation
	{
		#region Properties
		
		public int ID {get;set;}

		#region Extended Propterties
		public UInt16 LocalPort
		{
			get
			{
				return LocalEndpoint.Port;
			}
			internal set
			{
				LocalEndpoint.Port = value;				
			}
		}

		public String LocalIpAddress1
		{
			get
			{
				return LocalEndpoint.IP1;	
			}
			internal set
			{
				LocalEndpoint.IP1 = value;
			}
		}
		
		public String LocalIpAddress2
		{
			get
			{
				return LocalEndpoint.IP2;
			}
			internal set
			{
				LocalEndpoint.IP2 = value;
			}
		}
	
		#endregion Extended Propterties
		
		public UInt16 RemotePort
		{
			get;
			internal set;
		}

		public String RemoteIpAddress1
		{
			get;
			internal set;
		}
		
		public String RemoteIpAddress2
		{
			get;
			internal set;
		}
	
		public SctpEndpoint LocalEndpoint
		{
			get; internal set;
		}
				
		public List<IpPath> Pathes { get; internal set;}
		
		public int ULPkey {get; internal set;}
		
		public int DSCP {get; internal set;}
		
		public AssocCounters counters;
		
		#endregion Properties
				
		internal SctpAssociation(
			int assocId, 
			SctpEndpoint localEp,
			UInt16 rPort,
			List<IpPath> pathes, 
			int ulpKey,
			int dscp)
		{
			if (localEp == null)
				throw new NullReferenceException("null Enpoint parameter passed to constructor of Asscoiation");
			if (pathes == null || pathes.Count < 1)
				throw new NullReferenceException("empty pathes list passed to contructor of Association");
			this.ID =assocId;
			this.LocalEndpoint = localEp;
			this.RemotePort = rPort;
			this.Pathes = pathes;
			this.RemoteIpAddress1 = pathes[0].remoteAddress;
			if (pathes.Count > 1 && pathes[1].remoteAddress != this.RemoteIpAddress1)
				this.RemoteIpAddress2 = pathes[1].remoteAddress;
			else if (pathes.Count > 3 && pathes[2].remoteAddress != this.RemoteIpAddress1)
				this.RemoteIpAddress2 = pathes[2].remoteAddress;
			this.ULPkey = ulpKey;
			this.DSCP = dscp;
				
			localEp.Associations.Add(this);
		}
		
/*		internal SctpAssociation(UInt16 lPort, String locIp1, String locIp2,
		                         UInt16 rPort, String remIp1, String remIp2)
		{
			this.LocalEndpoint  = new SctpEndpoint(lPort,locIp1, locIp2);
			this.RemotePort = rPort;
			this.RemoteIpAddress1 = remIp1;
			this.RemoteIpAddress2 = remIp2;
		} */
		
		public override string ToString()
		{
			StringBuilder assocStr = new StringBuilder(LocalEndpoint+" - " + RemoteIpAddress1, 210);
			if (RemoteIpAddress2 != "")
			{
				assocStr.AppendFormat("; {0}", RemoteIpAddress2);
			}
			assocStr.AppendFormat(" :{0}", RemotePort);
			return assocStr.ToString();
		}

	}
}