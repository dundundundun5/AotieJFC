using ScottPlot;
using System.Linq;
using System.IO;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;

// 从模型升级测试.md中提取的数据
string[] labels = ["JGQ", "HX", "RG", "ZL", "ZJ", "BT", "P", "PB", "ZW", "M", "YW", "SL", "Z", "None"];
double[] oldModel = [538, 736, 928, 694, 1798, 1865, 1490, 1693, 1445, 333, 668, 242, 322, 3015];
double[] newModel = [1071, 747, 929, 1481, 1811, 1720, 1836, 1704, 1452, 334, 731, 251, 323, 3673];
double[] totalSamples = [1076, 749, 933, 1485, 1818, 1998, 1836, 1704, 1452, 335, 731, 251, 323, 3952];
double[] oldAcc = [];

string[] groupNames = ["旧", ""];
double[][] modelByGroup = [ oldModel, newModel ];
ScottPlot.Plot myPlot = new();

// Create a histogram from a collection of values
double[][] heightsByGroup = { SampleData.MaleHeights(), SampleData.FemaleHeights() };
Color[] groupColors = { Colors.Blue, Colors.Red };

for (int i = 0; i < 2; i++)
{
    double[] heights = heightsByGroup[i];
    var hist = ScottPlot.Statistics.Histogram.WithBinSize(1, heights);

    // Display the histogram as a bar plot
    var barPlot = myPlot.Add.Bars(hist.Bins, hist.GetProbability());

    // Customize the style of each bar
    foreach (var bar in barPlot.Bars)
    {
        bar.Size = hist.FirstBinSize;
        bar.LineWidth = 0;
        bar.FillStyle.AntiAlias = false;
        bar.FillColor = groupColors[i].WithAlpha(.2);
    }

    // Plot the probability curve on top the histogram
    ScottPlot.Statistics.ProbabilityDensity pd = new(heights);
    double[] xs = Generate.Range(heights.Min(), heights.Max(), 1);
    double scale = 1.0 / hist.Bins.Select(x => pd.GetY(x)).Sum();
    double[] ys = pd.GetYs(xs, scale);

    var curve = myPlot.Add.ScatterLine(xs, ys);
    curve.LineWidth = 2;
    curve.LineColor = groupColors[i];
    curve.LinePattern = LinePattern.DenselyDashed;
    curve.LegendText = groupNames[i];
}

// Customize plot style
myPlot.Legend.Alignment = Alignment.UpperRight;
myPlot.Axes.Margins(bottom: 0);
myPlot.YLabel("Probability (%)");
myPlot.XLabel("Height (cm)");
myPlot.HideGrid();

myPlot.SavePng("demo.png", 400, 300);
