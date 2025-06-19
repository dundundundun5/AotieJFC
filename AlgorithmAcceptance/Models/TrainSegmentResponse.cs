using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmAcceptance.Models
{
	public class TrainSegmentResponse
	{
		public int Code { get; set; }

		public string Message { get; set; }

		public TrainSegmentData Data { get; set; }



	}

	public class TrainSegmentData
	{
		public float CenterX { get; set; }

		public List<SegmentData> DefectList { get; set; } = new List<SegmentData>();

		public int ImageWidth { get; set; }

		public int ImageHeight { get; set; }
	}

	public class SegmentData
	{
		public SegmentPoint BottomRight { get; set; } = new SegmentPoint();

		public SegmentPoint TopLeft { get; set; } = new SegmentPoint();

		public double DefectScore { get; set; }

		public string DefectType { get; set; } = string.Empty;
	}

	public class SegmentPoint
	{
		public decimal X { get; set; }

		public decimal Y { get; set; }
	}
}
