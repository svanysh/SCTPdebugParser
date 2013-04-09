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
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class SctpAssociation
	{
		#region Properties
		
		public SctpEndpoint LocalEndpoint
		{
			get; internal set;
		}
		
		public uint assocId {get;set;}

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
		
		#endregion Properties
				
		internal SctpAssociation(SctpEndpoint localEp, UInt16 rPort, String remIp1, String remIp2)
		{
			if (localEp == null)
				throw new NullReferenceException("null Enpoint parameter passed to constructor of Asscoiation");
			this.LocalEndpoint = localEp;
			this.RemotePort = rPort;
			this.RemoteIpAddress1 = remIp1;
			this.RemoteIpAddress2 = remIp2;
		}
		
		internal SctpAssociation(UInt16 lPort, String locIp1, String locIp2,
		                         UInt16 rPort, String remIp1, String remIp2)
		{
			this.LocalEndpoint  = new SctpEndpoint(lPort,locIp1, locIp2);
			this.RemotePort = rPort;
			this.RemoteIpAddress1 = remIp1;
			this.RemoteIpAddress2 = remIp2;
		}
		
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