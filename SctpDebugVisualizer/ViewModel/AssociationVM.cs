/*
 * Created by SharpDevelop.
 * User: svanysh
 * Date: 05/30/2013
 * Time: 15:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;

using SctpHostData;

namespace SctpDebugVisualizer.ViewModel
{
	public class CounterVM
	{
		public string CounterName {get;set;}
		public int CounterValue {get;set;}
		public string Description {get;set;}
		
		public CounterVM(string name, int val, string description="")
		{
			this.CounterName = name;
			this.CounterValue = val;
			this.Description = description;
		}
	}
	
	/// <summary>
	/// Description of AssociationVM.
	/// </summary>
	public class AssociationVM: INotifyPropertyChanged
	{
		protected SctpAssociation assoc;
		protected ObservableCollection<CounterVM> counters;
		protected bool hide0Counters = false;
		
		public AssociationVM(SctpAssociation a)
		{
			assoc = a;
			this.IsZeroCountersShown = false;
			GenerateCountersList();
			
			RaisePropChange("ID");
			RaisePropChange("LocalPort");
			RaisePropChange("RemotePort");
			RaisePropChange("LocalIpAddress1");
			RaisePropChange("LocalIpAddress2");
			RaisePropChange("RemoteIpAddress1");
			RaisePropChange("RemoteIpAddress2");
			RaisePropChange("Counters");			
		}
		
		#region Properties
		
		public int ID {get{return assoc.ID;}}
		
		public int LocalPort {get {return assoc.LocalPort;}}		
		public String LocalIpAddress1 {get{return assoc.LocalIpAddress1;}}
		public String LocalIpAddress2 {get{return assoc.LocalIpAddress2;}}
		
		public int RemotePort {get {return assoc.RemotePort;}}		
		public String RemoteIpAddress1 {get{return assoc.RemoteIpAddress1;}}
		public String RemoteIpAddress2 {get{return assoc.RemoteIpAddress2;}}
		
		public String AssocName
		{
			get
			{
				StringBuilder sb = new StringBuilder(assoc.LocalIpAddress1);
				if (assoc.LocalIpAddress2 != "")
					sb.Append(";").Append(assoc.LocalIpAddress2);
				sb.Append(":").Append(assoc.LocalPort).Append("-");
				sb.Append(assoc.RemoteIpAddress1);
				if (assoc.RemoteIpAddress2!= "")
					sb.Append(";").Append(assoc.RemoteIpAddress2);
				sb.Append(":").Append(assoc.RemotePort);
				return sb.ToString();
			}
		}
		
		public ObservableCollection<CounterVM> Counters
		{
			get
			{
				if (IsZeroCountersShown)
					return counters;
				return new ObservableCollection<CounterVM>(
					from counter in counters
					where counter.CounterValue != 0
					select counter);
			}
		}
		
		public bool IsZeroCountersShown 
		{
			get{return hide0Counters ;}
			set
			{
				hide0Counters = value; 
				RaisePropChange("IsZeroCountersShown");
				RaisePropChange("Counters");
			}
		}
		
		/*
		public int OutDataChunks {get{return assoc.counters.OutDataChunks;}}
		public int InDataChunks {get{return assoc.counters.InDataChunks;}}
		public int OutOutOfOrderedChunks{get{return assoc.counters.OutOutOfOrderedChunks;}}
		public int InOutOfOrdered{get{return assoc.counters.InOutOfOrdered;}}
		public int RtxChunks{get{return assoc.counters.RtxChunks;}}
		public int OutControlChunks{get{return assoc.counters.OutControlChunks;}}
		public int InControlChunks {get{return assoc.counters.InControlChunks;}}
		public int OutFragmentedUserMsges{get{return assoc.counters.OutFragmentedUserMsges;}}
		public int InReassembledUserMsges{get{return assoc.counters.InReassembledUserMsges;}}
		public int OutPacks{get{return assoc.counters.OutPacks;}}
		public int InPacks{get{return assoc.counters.InPacks;}}
		public int Congestions{get{return assoc.counters.Congestions;}}
		public int CongestionCeased{get{return assoc.counters.CongestionCeased;}}
		public int OutUserMsgDiscards{get{return assoc.counters.OutUserMsgDiscards;}}
		public int InChunksDropped{get{return assoc.counters.InChunksDropped;}}
		public int InSackWithGap{get{return assoc.counters.InSackWithGap;}}
		public int OutSackWithGap{get{return assoc.counters.OutSackWithGap;}}
		public int HBtimeouts {get{return assoc.counters.HBtimeouts;}}
		public int InControlChunksAbnormal{get{return assoc.counters.InControlChunksAbnormal;}}
		public int InControlChunkDiscards{get{return assoc.counters.InControlChunkDiscards;}}
		public int InDataChunkAbnormal{get{return assoc.counters.InDataChunkAbnormal;}}
		public int InDataChunkUnexpected{get{return assoc.counters.InDataChunkUnexpected;}}
		public int InDataChunkDiscards{get{return assoc.counters.InDataChunkDiscards;}}
		public int OwnZeroWindows{get{return assoc.counters.OwnZeroWindows;}}
		public int TimeOwnZeroWindow{get{return assoc.counters.TimeOwnZeroWindow;}}
		public int PeerZeroWindows{get{return assoc.counters.PeerZeroWindows;}}
		public int TimePeerZeroWindow{get{return assoc.counters.TimePeerZeroWindow;}}
		public int TimeCongested{get{return assoc.counters.TimeCongested;}}
		*/
		#endregion
		
		protected void GenerateCountersList()
		{
			counters = new ObservableCollection<CounterVM>();
			counters.Add(new CounterVM("OutDataChunks", assoc.counters.OutDataChunks, ""));
			counters.Add(new CounterVM("InDataChunks",  assoc.counters.InDataChunks, ""));
			counters.Add(new CounterVM("OutOutOfOrderedChunks", assoc.counters.OutOutOfOrderedChunks, ""));
			counters.Add(new CounterVM("InOutOfOrdered", assoc.counters.InOutOfOrdered, ""));
			counters.Add(new CounterVM("RtxChunks", assoc.counters.RtxChunks, ""));
			counters.Add(new CounterVM("OutControlChunks", assoc.counters.OutControlChunks, ""));
			counters.Add(new CounterVM("InControlChunks",  assoc.counters.InControlChunks, ""));
			counters.Add(new CounterVM("OutFragmentedUserMsges", assoc.counters.OutFragmentedUserMsges, ""));
			counters.Add(new CounterVM("InReassembledUserMsges", assoc.counters.InReassembledUserMsges, ""));
			counters.Add(new CounterVM("OutPacks", assoc.counters.OutPacks, ""));
			counters.Add(new CounterVM("InPacks", assoc.counters.InPacks, ""));
			counters.Add(new CounterVM("Congestions", assoc.counters.Congestions, ""));
			counters.Add(new CounterVM("CongestionCeased", assoc.counters.CongestionCeased, ""));
			counters.Add(new CounterVM("OutUserMsgDiscards", assoc.counters.OutUserMsgDiscards, ""));
			counters.Add(new CounterVM("InChunksDropped", assoc.counters.InChunksDropped, ""));
			counters.Add(new CounterVM("InSackWithGap", assoc.counters.InSackWithGap, ""));
			counters.Add(new CounterVM("OutSackWithGap", assoc.counters.OutSackWithGap, ""));
			counters.Add(new CounterVM("HBtimeouts",  assoc.counters.HBtimeouts, ""));
			counters.Add(new CounterVM("InControlChunksAbnormal", assoc.counters.InControlChunksAbnormal, ""));
			counters.Add(new CounterVM("InControlChunkDiscards", assoc.counters.InControlChunkDiscards, ""));
			counters.Add(new CounterVM("InDataChunkAbnormal", assoc.counters.InDataChunkAbnormal, ""));
			counters.Add(new CounterVM("InDataChunkUnexpected", assoc.counters.InDataChunkUnexpected, ""));
			counters.Add(new CounterVM("InDataChunkDiscards", assoc.counters.InDataChunkDiscards, ""));
			counters.Add(new CounterVM("OwnZeroWindows", assoc.counters.OwnZeroWindows, ""));
			counters.Add(new CounterVM("TimeOwnZeroWindow", assoc.counters.TimeOwnZeroWindow, ""));
			counters.Add(new CounterVM("PeerZeroWindows", assoc.counters.PeerZeroWindows, ""));
			counters.Add(new CounterVM("TimePeerZeroWindow", assoc.counters.TimePeerZeroWindow, ""));
			counters.Add(new CounterVM("TimeCongested", assoc.counters.TimeCongested, ""));
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
