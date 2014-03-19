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
using System.Windows;
using System.Linq;

using SctpHostData;

namespace SctpDebugVisualizer.ViewModel
{
	
	/// <summary>
	/// Description of SctpHostVM.
	/// </summary>
	public class SctpHostVM: INotifyPropertyChanged
	{
		protected SctpHost sctphost;
		protected AssociationFilter assocFilter = null;
		protected EndpointFilter epFilter = null;
		
		public SctpHostVM()
		{	
			Endpoints = new ObservableCollection<SctpEndpoint>();
		}
		
		
		public bool Load(string inputStr)
		{
			try
			{
				sctphost = SctpHostColiParser.Parse(inputStr);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Parsing error");
				return false;
			}
			EndpointFilter = new EndpointFilter(EndpointFilterType.All);
			AssociationFilter = new AssociationFilter(AssocFilterType.All);
			Clients = new ObservableCollection<ExtClient>(sctphost.SCTPIclients.Values);
			Config = new ConfigVM(sctphost.Configuration);
			RaisePropChange("Endpoints");			
			RaisePropChange("Clients");
			RaisePropChange("RpuId");
			RaisePropChange("CpId");
			RaisePropChange("BASEstate");
			RaisePropChange("HOSTstate");
			RaisePropChange("Board");			
			RaisePropChange("Configuration");
			RaisePropChange("Config");
			RaisePropChange("Ip1");
			RaisePropChange("Ip2");
			return true;
		}
		
		#region Properties
		public ObservableCollection<SctpEndpoint> Endpoints {get; set;}
		public ObservableCollection<SctpAssociation> Associations { get; set;}
		public ObservableCollection<ExtClient> Clients {get;set;}
				
		public int RpuId {get {return sctphost.RpuId;}}			
		public int CpId {get{return sctphost.CpId;}}
		public string BASEstate {get{return sctphost.BASEstate;}}
		public string HOSTstate {get{return sctphost.HOSTstate;}}
		public BoardType Board {get{return sctphost.Board;}}
		
		public string Ip1 {get {return sctphost.Configuration.IpAddresses.IpAddress1;}}
		public string Ip2 {get {return sctphost.Configuration.IpAddresses.IpAddress2;}}
		
		public HostConfig Configuration {get {return sctphost.Configuration;}}
		public ConfigVM Config { get;set;}
		
		public EndpointFilter EndpointFilter
		{
			get { return epFilter;}
			set 
			{
				if (sctphost == null) return;
				epFilter = value;
				if (epFilter == null || epFilter.FilterType == EndpointFilterType.All)
				{
					Endpoints = new ObservableCollection<SctpEndpoint>(sctphost.Endpoints.Values);
				}
				else switch(epFilter.FilterType)
				{
					case EndpointFilterType.ClientId:
						Endpoints = new ObservableCollection<SctpEndpoint>(
							from e in sctphost.Endpoints.Values
							where e.ClientId == epFilter.ID
							select e);
						break;
						
					case EndpointFilterType.M3:
						Endpoints = new ObservableCollection<SctpEndpoint>(
							from e in sctphost.Endpoints.Values
							where e.ClientId == null
							select e);
						break;
				}
				RaisePropChange("Endpoints");
				RaisePropChange("EndpointFilter");
			}
		}
		
		public AssociationFilter AssociationFilter
		{
			get { return assocFilter;}
			set 
			{
				if (sctphost == null) return;
				assocFilter = value;
				if (assocFilter == null || assocFilter.FilterType == AssocFilterType.All)
				{
					Associations = new ObservableCollection<SctpAssociation>(sctphost.Associations.Values);					
				}
				else switch(assocFilter.FilterType)
				{
					case AssocFilterType.ClientId:
						Associations = new ObservableCollection<SctpAssociation>(
							from a in sctphost.Associations.Values
							where a.LocalEndpoint.ClientId == assocFilter.ID
							select a);
						break;
						
					case AssocFilterType.M3:
						Associations = new ObservableCollection<SctpAssociation>(
							from a in sctphost.Associations.Values
							where a.LocalEndpoint.ClientId == null
							select a);
						break;
						
					case AssocFilterType.EndpointId:
						Associations = new ObservableCollection<SctpAssociation>(
							from a in sctphost.Associations.Values
							where a.LocalEndpoint.ID == assocFilter.ID
							select a);
						break;
						
					case AssocFilterType.RemotePort:
						Associations = new ObservableCollection<SctpAssociation>(
							from a in sctphost.Associations.Values
							where a.RemotePort == assocFilter.ID
							select a);
						break;
						
					case AssocFilterType.RemoteIpAddress:
						Associations = new ObservableCollection<SctpAssociation>(
							from a in sctphost.Associations.Values
							where a.RemoteIpAddress1 == assocFilter.IP1
								|| a.RemoteIpAddress2 == assocFilter.IP1
							select a);
						break;
						
					case AssocFilterType.RemoteIpAddresses:
						Associations = new ObservableCollection<SctpAssociation>(
							from a in sctphost.Associations.Values
							where (a.RemoteIpAddress1 == assocFilter.IP1
							       && a.RemoteIpAddress2 == assocFilter.IP2)
								|| (a.RemoteIpAddress1 == assocFilter.IP2
							       && a.RemoteIpAddress2 == assocFilter.IP1)
							select a);
						break;
				}
				RaisePropChange("Associations");
				RaisePropChange("AssociationFilter");
			}
		}
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
