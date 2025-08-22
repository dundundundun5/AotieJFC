// See https://aka.ms/new-console-template for more information

namespace JFCTestScripts;

public class Program
{
    public static void Main()
    {
        AnalyzeScore();
    }

    public static void AnalyzeScore()
    {
        string resultPath = @"D:\枫泾\scores";
        if (!Directory.Exists(resultPath))
            Directory.CreateDirectory(resultPath);
        string filePath = @"D:\warning.txt";
        string[] lines = File.ReadLines(filePath).ToArray();
        foreach (string line in lines)
        {
            try
            {
                Console.WriteLine(line);
                string[] parts = line.Split(' ');
                string imagePath = parts[3].Split("e:")[1];
                string type = parts[4].Split(":")[1];
                string defectScore = parts[5].Split(":")[1].Substring(2, 2);
                string configScore = parts[6].Split(":")[1].Substring(2, 2);
                string newName = $"枫泾_{type}_{defectScore}_{configScore}.jpg";
                File.Copy(imagePath, Path.Join(resultPath, newName), true);
                Console.WriteLine($"{imagePath} -> {newName} \u2713");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }
        Console.ReadKey();
    }
}