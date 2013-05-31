/*
 * Created by SharpDevelop.
 * User: svanysh
 * Date: 05/30/2013
 * Time: 15:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Text;

using SctpHostData;

namespace SctpDebugVisualizer.ViewModel
{
	/// <summary>
	/// Description of AssociationVM.
	/// </summary>
	public class AssociationVM: INotifyPropertyChanged
	{
		protected SctpAssociation assoc;
		
		public AssociationVM(SctpAssociation a)
		{
			assoc = a;
			RaisePropChange("ID");
			RaisePropChange("LocalPort");
			RaisePropChange("RemotePort");
			RaisePropChange("LocalIpAddress1");
			RaisePropChange("LocalIpAddress2");
			RaisePropChange("RemoteIpAddress1");
			RaisePropChange("RemoteIpAddress2");
			RaisePropChange("Counters");
			RaisePropChange("OutDataChunks");
			RaisePropChange("InDataChunks");
			RaisePropChange("OutOutOfOrderedChunks");
			RaisePropChange("InOutOfOrdered");
			RaisePropChange("RtxChunks");
			RaisePropChange("OutControlChunks");
			RaisePropChange("OutFragmentedUserMsges");
			RaisePropChange("InReassembledUserMsges");
			RaisePropChange("OutPacks");
			RaisePropChange("InPacks");
			RaisePropChange("Congestions");
			RaisePropChange("CongestionCeased");
			RaisePropChange("OutUserMsgDiscards");
			RaisePropChange("InChunksDropped");
			RaisePropChange("InSackWithGap");
			RaisePropChange("OutSackWithGap");
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
		
		public AssocCounters Counters{ get {return assoc.counters;}}
		
		public int OutDataChunks {get{return assoc.counters.OutDataChunks;}}
		public int InDataChunks {get{return assoc.counters.InDataChunks;}}
		public int OutOutOfOrderedChunks{get{return assoc.counters.OutOutOfOrderedChunks;}}
		public int InOutOfOrdered{get{return assoc.counters.InOutOfOrdered;}}
		public int RtxChunks{get{return assoc.counters.RtxChunks;}}
		public int OutControlChunks{get{return assoc.counters.OutControlChunks;}}
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
		
		#endregion
		
		
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
