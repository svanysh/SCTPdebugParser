/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.04.2013
 * Time: 21:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

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
			
			// TODO: Implement Functionality Here
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}