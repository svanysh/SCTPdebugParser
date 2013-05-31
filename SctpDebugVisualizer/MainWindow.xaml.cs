/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 07.04.2013
 * Time: 20:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Win32;
using SctpHostData;
using SctpDebugVisualizer.ViewModel;

namespace SctpDebugVisualizer
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		SctpHostVM sctphost {get;set;}

		public MainWindow()
		{
			InitializeComponent();
			sctphost = new SctpHostVM();
			
			this.DataContext = sctphost;
		}
		
		void Load_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog().Value)
			{
				sctphost.Load(ofd.FileName);
			}			
		}
		
		public Dictionary<int, AssociationWindow> AssocWindows
		{
			get{return (Application.Current as App).assocWindows;}
		}
		
		void AssociationsDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if ((sender as DataGrid).SelectedItem == null) return;
			SctpAssociation assoc = ((sender as DataGrid).SelectedItem as SctpAssociation);
			AssociationWindow asWnd;
			if (AssocWindows.ContainsKey(assoc.ID))
				return;
			asWnd = new AssociationWindow(new AssociationVM(assoc));
			AssocWindows.Add(assoc.ID, asWnd);
			asWnd.Show();		
			
		}
	}
}