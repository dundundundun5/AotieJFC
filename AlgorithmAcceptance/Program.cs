using System;
using System.Windows.Forms;

namespace AlgorithmAcceptanceTool
{
    class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		
		[STAThread]
		public static void Main()
		{
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Main());
			// Application.Run(new Segment());
			//Application.Run(new OCR);
			// Application.Run(new RiskDetect());

		}
	}
}
