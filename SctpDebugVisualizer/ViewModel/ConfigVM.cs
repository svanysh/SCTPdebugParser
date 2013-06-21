/*
 * Created by SharpDevelop.
 * User: Serg
 * Date: 15.06.2013
 * Time: 22:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using SctpHostData;

namespace SctpDebugVisualizer.ViewModel
{
	public class ConfigParam
	{
		public string ParamName {get;set;}
		public string ParamValue {get;set;}
		public string ParamDescription {get;set;}
		
		public ConfigParam(string name, int val, string description = "")
		{
			ParamName = name;
			ParamValue = val.ToString();
			ParamDescription = description;
		}
		
		public ConfigParam(string name, string val, string description = "")
		{
			ParamName = name;
			ParamValue = val;
			ParamDescription = description;
		}
		
		public ConfigParam(string name, bool val, string description = "")
		{
			ParamName = name;
			ParamValue = val ? "ON" : "OFF";
			ParamDescription = description;
		}
	}
	
	/// <summary>
	/// Description of ConfigVM.
	/// </summary>
	public class ConfigVM: INotifyPropertyChanged
	{
		protected HostConfig _hostConfig;
		
		public ObservableCollection<ConfigParam> SCTPparams {get;set;}
		
		public ConfigVM(HostConfig hostConf)
		{
			SCTPparams = new ObservableCollection<ConfigParam>();
			_hostConfig = hostConf;
			
			SCTPparams.Add(new ConfigParam("Config file version", hostConf.SCTPcfVersion));
			SCTPparams.Add(new ConfigParam("Number of associations", hostConf.NumOfAssociations));
			SCTPparams.Add(new ConfigParam("ICMP status", hostConf.ICMPstatus));
			if (hostConf.PortRangeFrom != 1 || hostConf.PortRangeTo != 65535)
				SCTPparams.Add(new ConfigParam("Port range", 
				                              hostConf.PortRangeFrom+"-"+hostConf.PortRangeTo));
			if (hostConf.MinRTO != 100)
				SCTPparams.Add(new ConfigParam("Minimum RTO", hostConf.MinRTO));
			if (hostConf.MaxRTO != 400)
				SCTPparams.Add(new ConfigParam("Maximum RTO", hostConf.MaxRTO));
			if (hostConf.InitialRTO != 200)
				SCTPparams.Add(new ConfigParam("Initial RTO", hostConf.InitialRTO));
			if (hostConf.RTOalpha != 3)
				SCTPparams.Add(new ConfigParam("RTO alpha", hostConf.RTOalpha));
			if (hostConf.RTObeta != 2)
				SCTPparams.Add(new ConfigParam("RTO beta", hostConf.RTObeta));
			if (hostConf.ValidCookieLife != 60000)
				SCTPparams.Add(new ConfigParam("Valid COKIE life",
				                              hostConf.ValidCookieLife));
			if (hostConf.AllowedIncrementCookieLife != 30000)
				SCTPparams.Add(new ConfigParam("Allowed increment COOKIE life", 
				                              hostConf.AllowedIncrementCookieLife));
			
			SCTPparams.Add(new ConfigParam("Path Selection", hostConf.PathSelection));
			if (hostConf.AssocMaxRtx != 8)
				SCTPparams.Add(new ConfigParam("AssocMaxRtx", hostConf.AssocMaxRtx));
			if (hostConf.PathMaxRtx != 4)
				SCTPparams.Add(new ConfigParam("PathMaxRtx", hostConf.PathMaxRtx));
			if (hostConf.minThreshold != 1)
				SCTPparams.Add(new ConfigParam("Min Threshold", hostConf.minThreshold));
			if (hostConf.maxThreshold != 65535)
				SCTPparams.Add(new ConfigParam("Max Threshold", hostConf.maxThreshold));
			if (hostConf.PFMR != 4)
				SCTPparams.Add(new ConfigParam("Potentially failed max rtx", hostConf.PFMR));
			if (hostConf.MaxInitRtx != 20)
				SCTPparams.Add(new ConfigParam("MaxInitRtx", hostConf.MaxInitRtx));
			if (hostConf.MaxShutdownRtx != 8)
				SCTPparams.Add(new ConfigParam("MaxShutdownRtx", hostConf.MaxShutdownRtx));
			if (hostConf.HeartbeatInterval != 30000)
				SCTPparams.Add(new ConfigParam("Heartbeat Interval", hostConf.HeartbeatInterval));
			if (!hostConf.HeartbeatStatus)
				SCTPparams.Add(new ConfigParam("Heartbeat Status", hostConf.HeartbeatStatus));
			if (hostConf.InitialHeartbeatInterval != 5000)
				SCTPparams.Add(new ConfigParam("Initial Heartbeat Interval", hostConf.InitialHeartbeatInterval));
			if (hostConf.MIS != 17)
				SCTPparams.Add(new ConfigParam("MIS", hostConf.MIS));
			if (hostConf.MOS != 17)
				SCTPparams.Add(new ConfigParam("MOS", hostConf.MOS));
			
			SCTPparams.Add(new ConfigParam("M buffer", hostConf.Mbuffer));
			SCTPparams.Add(new ConfigParam("N threshold", hostConf.Nthreshold));
			SCTPparams.Add(new ConfigParam("N percentage", hostConf.Npercentage));
			SCTPparams.Add(new ConfigParam("Initial Rwnd", hostConf.InitialARWND));
			if (hostConf.PMTU != 1480)
				SCTPparams.Add(new ConfigParam("PMTU", hostConf.PMTU));
			if (hostConf.MaxBurst != 4)
				SCTPparams.Add(new ConfigParam("MaxBurst", hostConf.MaxBurst));
			if (hostConf.SACKtimer != 40)
				SCTPparams.Add(new ConfigParam("SACK timer", hostConf.SACKtimer));
			if (!hostConf.BundlingStatus)
				SCTPparams.Add(new ConfigParam("Bundling Status", hostConf.BundlingStatus));
			if (hostConf.BundlingTimer != 10)
			SCTPparams.Add(new ConfigParam("Bundling Timer", hostConf.BundlingTimer));
			SCTPparams.Add(new ConfigParam("DSCP", hostConf.DSCP));			
		}
		
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
