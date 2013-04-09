/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.04.2013
 * Time: 20:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.ObjectModel;

namespace SctpHostData
{
	/// <summary>
	/// Description of Endpoint.
	/// </summary>
	public class Endpoint
	{
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
		
		public ObservableCollection<SctpAssociation> Associations
		{
			get; internal set;			
		}
		
		public Endpoint(UInt16 port, String ip1, String ip2)
		{
			this.Port = port;
			this.IP1 = ip1;
			this.IP2 = ip2;
		}
		
		
		
		public override string ToString()
		{
			return IP1+";"+IP2+ Port.ToString();
		}

		
	}
}
