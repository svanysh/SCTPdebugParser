/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.04.2013
 * Time: 21:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using SctpHostData;

namespace consoleTester
{
	class Program
	{
		
				
		public static void Main(string[] args)
		{
			Console.WriteLine("sctphost parser");
						
			/*StreamReader sr = File.OpenText("input.txt");					
			string input = sr.ReadToEnd();*/
			SctpHost host = new SctpHost("input.txt");
			Console.WriteLine("ENDPOINTS");
			foreach(var ep in host.Endpoints)
			{
				Console.WriteLine("[{0}] {1}", ep.Key, ep.Value);
			}

			Console.WriteLine("ASSOCIATIONS");
			foreach(var assoc in host.Associations)
			{
				Console.WriteLine("[{0}] {1}", assoc.Key, assoc.Value);
				Console.WriteLine("in={0}, out={1}",
				                  assoc.Value.counters.InDataChunks,
				                  assoc.Value.counters.OutDataChunks);
			}
			
			Console.WriteLine("SCTPI clients");
			foreach(var client in host.SCTPIclients)
			{
				Console.WriteLine("[{0}] pv{1}, PID=0x{2}",
				                  client.Value.ID,
				                  client.Value.PV,
				                  client.Value.PID);
			}
			
			Console.WriteLine("RPU={0}, CP={1}, BASE state={2}, HOST state={3}, board={4}",
			                  host.RpuId, host.CpId, host.BASEstate, host.HOSTstate, host.Board);

// TODO: Implement Functionality Here
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}