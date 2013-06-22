/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 06/22/2013
 * Time: 13:35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SctpDebugVisualizer.ViewModel
{

	public enum AssocFilterType
	{
		All,
		ClientId,
		M3,
		EndpointId,
		RemotePort,
		RemoteIpAddress,
		RemoteIpAddresses
	}
	
	public class AssociationFilter
	{
		public AssocFilterType FilterType {get;set;}		
		public int ID {get;set;}
		public string IP1 {get;set;}
		public string IP2 {get;set;}
		
		public AssociationFilter(AssocFilterType type, int id=0)
		{
			FilterType = type;
			ID = id;
		}
		
		public AssociationFilter(string ip)
		{
			FilterType = AssocFilterType.RemoteIpAddress;
			IP1 = ip;
		}
		
		public AssociationFilter(string ip1, string ip2)
		{
			FilterType = AssocFilterType.RemoteIpAddresses;
			IP1 = ip1;
			IP2 = ip2;
		}
		
		public override string ToString()
		{
			switch(FilterType)
			{				
				case AssocFilterType.ClientId: return "clientId="+ID.ToString();
				case AssocFilterType.M3: return "M3";
				case AssocFilterType.EndpointId: return "epID="+ID.ToString();
				case AssocFilterType.RemotePort: return "rPort="+ID.ToString();
				case AssocFilterType.All: return "All";
				case AssocFilterType.RemoteIpAddress:return "ip="+IP1;
				case AssocFilterType.RemoteIpAddresses:return "ip="+IP1+"&ip="+IP2;
				default: return "Unknown";
			}
		}

	}
	
	public enum EndpointFilterType
	{
		All,
		ClientId,
		M3
	}
	
	public class EndpointFilter
	{
		public EndpointFilterType FilterType {get;set;}		
		public int ID {get;set;}
		public string IP1 {get;set;}
		public string IP2 {get;set;}
		
		public EndpointFilter(EndpointFilterType type, int id=0)
		{
			FilterType = type;
			ID = id;
		}
				
		public override string ToString()
		{
			switch(FilterType)
			{	
				case EndpointFilterType.All: return "All";					
				case EndpointFilterType.ClientId: return "clientId="+ID.ToString();
				case EndpointFilterType.M3: return "M3";
				default: return "Unknown";
			}
		}

	}
}
