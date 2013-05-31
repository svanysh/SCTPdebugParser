using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Xml;

namespace SctpDebugVisualizer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public Dictionary<int, AssociationWindow> assocWindows = new Dictionary<int, AssociationWindow>();
	}
}