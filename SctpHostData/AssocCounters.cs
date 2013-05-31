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
		public int OutDataChunks;
		public int InDataChunks;
		public int OutOutOfOrderedChunks;
		public int InOutOfOrdered;
		public int RtxChunks;
		public int OutControlChunks;
		public int OutFragmentedUserMsges;
		public int InReassembledUserMsges;
		public int OutPacks;
		public int InPacks;
		public int Congestions;
		public int CongestionCeased;
		public int OutUserMsgDiscards;
		public int InChunksDropped;
		public int InSackWithGap;
		public int OutSackWithGap;
	}
}
