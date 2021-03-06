﻿/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.04.2013
 * Time: 20:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace SctpHostData
{
	/// <summary>
	/// Description of Endpoint.
	/// </summary>
	public class SctpEndpoint
	{
        #region Properties
        public int ID {get;set;}
        
		public UInt16 Port
		{ 
			get;
			set;
		}
		
		public String IP1  
		{ 
			get;
			set;
		}
		
		public String IP2
		{
			get;
			set;
		}
		
		public int DSCP{get; set;}
		
		public int? ClientId {get; set;}
		
		public ObservableCollection<SctpAssociation> Associations
		{
			get; internal set;			
		}
		#endregion Properties
				
		public SctpEndpoint(int id, UInt16 port, String ip1, String ip2, int dscp=0, int? clientId=null)
		{
			this.ID = id;
			this.Port = port;
			this.IP1 = ip1;
			this.IP2 = ip2;
			this.DSCP = dscp;
			this.ClientId = clientId;
			
			this.Associations = new ObservableCollection<SctpAssociation>();
		}
		
		/*public SctpAssociation AddAssocaitioin(int assocId, UInt16 remPort, String remIp1, String remIp2="")
		{
			SctpAssociation newAssoc = new SctpAssociation(assocId, this, remPort, remIp1, remIp2);
			this.Associations.Add(newAssoc);
			return newAssoc;
		}*/
		
		public override string ToString()
		{
			StringBuilder epStr = new StringBuilder(IP1, 100);
			if (IP2 != "")
			{
				epStr.AppendFormat("; {0}", IP2);
			}
			epStr.AppendFormat(" :{0}", Port);
			return epStr.ToString();
		}

		
	}
}
