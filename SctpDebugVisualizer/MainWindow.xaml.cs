/*
 * Created by SharpDevelop.
 * User: svanysh
 * Date: 19.03.2014
 * Time: 10:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SctpDebugVisualizer.ViewModel;

using Microsoft.Win32;
using SctpHostData;

namespace SctpDebugVisualizer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
		
		protected IEnumerable<string> SplitOnSctpHostColi(string input)
		{
			string []splitters = {"sctphost -all", "|-- sctphost -traces : --|"};
			return
				from sctphostcoli in input.Split(splitters, StringSplitOptions.RemoveEmptyEntries)
				where sctphostcoli.Contains("SCTP HOST")
				select sctphostcoli;
		}
		
		void Load_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog().Value) {
				foreach(string coli in SplitOnSctpHostColi(File.ReadAllText(ofd.FileName))) {
					try {
						SctpHostVM sctphost = new SctpHostVM();
						sctphost.Load(coli);
						SctpHostWindow sctpHostWnd = new SctpHostWindow(sctphost);
						sctpHostWnd.Show();
					}
					catch
					{
						continue;
					}
				}
			}
		}
		
	}
}