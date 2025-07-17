using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmAcceptance.Models
{
	public class SegmentDefectData
	{
		public Point BottomRight { get; set; } = new Point();

		public Point TopLeft { get; set; } = new Point();

		public double DefectScore { get; set; }

		public string DefectType { get; set; } = string.Empty;

		public string DefectContent { get; set; } = string.Empty; 
	}
}
