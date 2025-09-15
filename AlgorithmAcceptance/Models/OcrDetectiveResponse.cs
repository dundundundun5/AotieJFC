
using System.Collections.Generic;

namespace AlgorithmAcceptanceTool.Models
{
	public class OcrDetectiveResponse
	{
		public int Code { get; set; }

		public string Message { get; set; } = string.Empty;

		public OcrData Data { get; set; }


	}

	public class OcrData
	{
		public int ImageWidth { get; set; }

		public int ImageHeight { get; set; }
		public List<SegmentDefectData> DefectList { get; set; } = new List<SegmentDefectData>();


	}
}
