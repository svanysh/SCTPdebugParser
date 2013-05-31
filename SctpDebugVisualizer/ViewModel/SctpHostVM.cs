/*
 * Created by SharpDevelop.
 * User: svanysh
 * Date: 05/29/2013
 * Time: 12:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using SctpHostData;

namespace SctpDebugVisualizer.ViewModel
{
	/// <summary>
	/// Description of SctpHostVM.
	/// </summary>
	public class SctpHostVM: INotifyPropertyChanged
	{
		protected SctpHost sctphost;
		
		public SctpHostVM()
		{	
			Endpoints = new ObservableCollection<SctpEndpoint>();
		}
		
		public bool Load(string fileName)
		{
			try
			{
			sctphost = new SctpHost(fileName);
			}
			catch
			{
				return false;
			}
			Endpoints = new ObservableCollection<SctpEndpoint>(sctphost.Endpoints.Values);
			Associations = new ObservableCollection<SctpAssociation>(sctphost.Associations.Values);
			RaisePropChange("Endpoints");
			RaisePropChange("Associations");
			RaisePropChange("RpuId");
			RaisePropChange("CpId");
			RaisePropChange("BASEstate");
			RaisePropChange("HOSTstate");
			RaisePropChange("Board");			
			return true;
		}
		
		#region Properties
		public ObservableCollection<SctpEndpoint> Endpoints {get; set;}
		public ObservableCollection<SctpAssociation> Associations { get; set;}
				
		public int RpuId {get {return sctphost.RpuId;}}			
		public int CpId {get{return sctphost.CpId;}}
		public string BASEstate {get{return sctphost.BASEstate;}}
		public string HOSTstate {get{return sctphost.HOSTstate;}}
		public BoardType Board {get{return sctphost.Board;}}
		#endregion Properties
		
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropChange(string propName)
		{
			if(PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}
		#endregion INotifyPropertyChanged
	}
}
