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
		protected SolidColorBrush filterBrush = new SolidColorBrush(Colors.GreenYellow);
		protected Brush defaultBrush;

		public MainWindow()
		{
			InitializeComponent();
			sctphost = new SctpHostVM();
			
			this.DataContext = sctphost;
			defaultBrush = AssocFilterLabel.Background;
		}
		
		void Load_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog().Value)
			{
				if (sctphost.Load(ofd.FileName))
					this.Title = "sctphost parser for "+ofd.FileName;
				else
					this.Title = "sctphost parser";
			}
			AssocFilterLabel.Background = defaultBrush;
			EndpointFilterLabel.Background = defaultBrush;
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
		
		void ClientButton_Click(object sender, RoutedEventArgs e)
		{
			int clId = Convert.ToInt32((sender as Button).Tag);
			sctphost.AssociationFilter = new AssociationFilter(AssocFilterType.ClientId, clId);
			sctphost.EndpointFilter = new EndpointFilter(EndpointFilterType.ClientId, clId);
			AssocFilterLabel.Background = filterBrush;
			EndpointFilterLabel.Background = filterBrush;
		}
		
		void M3onlyButton_Click(object sender, RoutedEventArgs e)
		{			
			sctphost.AssociationFilter = new AssociationFilter(AssocFilterType.M3);
			sctphost.EndpointFilter = new EndpointFilter(EndpointFilterType.M3);
			AssocFilterLabel.Background = filterBrush;
			EndpointFilterLabel.Background = filterBrush;
		}
		
		void EndpointButton_Click(object sender, RoutedEventArgs e)
		{
			int epId = Convert.ToInt32((sender as Button).Tag);
			sctphost.AssociationFilter = new AssociationFilter(AssocFilterType.EndpointId, epId);
			AssocFilterLabel.Background = filterBrush;			
		}
		
		void RemPortButton_Click(object sender, RoutedEventArgs e)
		{
			int rPort = Convert.ToInt32((sender as Button).Tag);
			sctphost.AssociationFilter = new AssociationFilter(AssocFilterType.RemotePort, rPort);
			AssocFilterLabel.Background = filterBrush;
		}
		
		void RemIpButton_Click(object sender, RoutedEventArgs e)
		{
			string rIp = (sender as Button).Tag as string;
			sctphost.AssociationFilter = new AssociationFilter(rIp);
			AssocFilterLabel.Background = filterBrush;
		}
		
		void ShowAllButton_Click(object sender, RoutedEventArgs e)
		{
			sctphost.AssociationFilter = new AssociationFilter(AssocFilterType.All);
			sctphost.EndpointFilter = new EndpointFilter(EndpointFilterType.All);
			AssocFilterLabel.Background = defaultBrush;
			EndpointFilterLabel.Background = defaultBrush;
		}
		
		
	}
}