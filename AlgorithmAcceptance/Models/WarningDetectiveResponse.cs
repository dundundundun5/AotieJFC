using System.Collections.Generic;

namespace AlgorithmAcceptance.Models
{
    public class WarningDetectiveResponse
    {
		public int Code { get; set; }

		public string Message { get; set; }

		public WarningData Data { get; set; }
	}

	public class WarningData
	{
		public List<SegmentDefectData> DefectList { get; set; } = new List<SegmentDefectData>();

		public int ImageWidth { get; set; }

		public int ImageHeight { get; set; }
	}

	public class SegmentDefectData {
		public Point BottomRight { get; set; } = new Point();

		public Point TopLeft { get; set; } = new Point();

		public double DefectScore { get; set; }

		public string DefectType { get; set; } = string.Empty;
	}

	public class Point
	{
		public decimal X { get; set; }

		public decimal Y { get; set; }
	}
}
