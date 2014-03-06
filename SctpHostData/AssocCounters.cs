/*
 * Created by SharpDevelop.
 * User: svanysh
 * Date: 05/30/2013
 * Time: 13:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SctpHostData
{


	/// <summary>
	/// Description of AssocCounters.
	/// </summary>
	public struct AssocCounters
	{
		public UInt32 OutDataChunks;
		public UInt32 InDataChunks;
		public UInt32 OutOutOfOrderedChunks;
		public UInt32 InOutOfOrdered;
		public UInt32 RtxChunks;
		public UInt32 OutControlChunks;
		public UInt32 InControlChunks;
		public UInt32 OutFragmentedUserMsges;
		public UInt32 InReassembledUserMsges;
		public UInt32 OutPacks;
		public UInt32 InPacks;
		public UInt32 Congestions;
		public UInt32 CongestionCeased;
		public UInt32 OutUserMsgDiscards;
		public UInt32 InChunksDropped;
		public UInt32 InSackWithGap;
		public UInt32 OutSackWithGap;
		public UInt32 HBtimeouts;
		public UInt32 InControlChunksAbnormal;
		public UInt32 InControlChunkDiscards;
		public UInt32 InDataChunkAbnormal;
		public UInt32 InDataChunkUnexpected;
		public UInt32 InDataChunkDiscards;
		public UInt32 OwnZeroWindows;
		public UInt32 TimeOwnZeroWindow;
		public UInt32 PeerZeroWindows;
		public UInt32 TimePeerZeroWindow;
		public UInt32 TimeCongested;
		public UInt64 InOctets;
		public UInt64 OutOctets;
	}
}
