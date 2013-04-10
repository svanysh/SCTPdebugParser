/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.04.2013
 * Time: 21:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SctpHostData;

namespace consoleTester
{
	class Program
	{
		
		protected void ParseFile()
		{
			ParseHeader();
			ParseEndpoints();
			ParseAssociations();
		}
		
		void ParseAssociations()
		{
			throw new NotImplementedException();
		}
		
		void ParseEndpoints()
		{
			throw new NotImplementedException();
		}
		
		void ParseHeader()
		{
			throw new NotImplementedException();
		}
		
		public static void Main(string[] args)
		{
			Console.WriteLine("sctphost parser");
			
			SctpEndpoint ep = new SctpEndpoint(1000, "192.168.10.1", "192.168.10.2");
			/*
			for (ushort i=1; i<10;i++)
			{
				ep.AddAssocaitioin(i, "192.168.0." + i.ToString());
			}*/
			
			String input = "0001: |---------------- SCTP ASSOCIATION  28 -----------------|\n"+
"0001: |-------------------------------------------------------|"+
"0001: | localPort:             3905"+
"0001: | remotePort:            3905"+
"0001: | dscp:                  0"+
"0001: | ulpKey:                0x43e0f728"+
"0001: | switchbackThreshold:   1"+
"0001: | recentSuccessfulHBs:   0"+
"0001: | sctpProfileId:         0"+
"0001: |-----------------Path Information----------------------\n"+
"0001: | abcd::83a0:4310 - abcd::83a0:4315 | preferred and active primary | last used | active   (+) | pmr: 0 ";

			
			foreach(SctpAssociation assoc in ep.Associations)
			{
				Console.WriteLine(assoc);
			}
			// TODO: Implement Functionality Here
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}