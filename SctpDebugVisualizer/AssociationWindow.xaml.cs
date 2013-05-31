/*
 * Created by SharpDevelop.
 * User: svanysh
 * Date: 05/30/2013
 * Time: 15:20
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
using SctpDebugVisualizer.ViewModel;

namespace SctpDebugVisualizer
{
	/// <summary>
	/// Interaction logic for AssociationWindow.xaml
	/// </summary>
	public partial class AssociationWindow : Window
	{
		
		
		public AssociationWindow(AssociationVM assocVM)
		{
			InitializeComponent();
			this.DataContext = assocVM;
		}
		
		void Window_Closed(object sender, EventArgs e)
		{
			(Application.Current as App).assocWindows.Remove((this.DataContext as AssociationVM).ID);
		}
	
	}
}