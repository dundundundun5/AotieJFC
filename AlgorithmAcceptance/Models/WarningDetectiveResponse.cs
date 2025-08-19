using System.Collections.Generic;

namespace AlgorithmAcceptanceTool.Models
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

	
	public class Point
	{
		public decimal X { get; set; }

		public decimal Y { get; set; }
	}
}
