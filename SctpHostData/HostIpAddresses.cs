/*
 * Created by SharpDevelop.
 * User: Sergey
 * Date: 09.03.2014
 * Time: 17:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SctpHostData
{
	/// <summary>
	/// Description of HostIpAddresses.
	/// </summary>
	public class HostIpAddresses
	{		
		/// <summary>
		/// first IP address
		/// </summary>
		public string IpAddress1 {get; internal set;}
		
		/// <summary>
		/// second IP address
		/// </summary>
		public string IpAddress2 {get; internal set;}

		public HostIpAddresses()
		{
		}
	}
}
