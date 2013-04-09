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
			
			for (ushort i=1; i<10;i++)
			{
				ep.AddAssocaitioin(i, "192.168.0." + i.ToString());
			}
			
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