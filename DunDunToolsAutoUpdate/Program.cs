// See https://aka.ms/new-console-template for more information
using DunDunToolsAutoUpdate;
using System.IO.Compression;

Console.WriteLine("Hello, World!");


string[] dunDunTools = ["SegmentationTestTool.zip", "PickImagesTool.zip", "AlgorithmAcceptanceTool.zip"];
string remotePath = "/软件工具";
string localPath = @"D:\";

var ftp = Dundun.Ftp();

foreach (string dunTool in dunDunTools) {
    string localFilePath = Path.Join(localPath, dunTool);
    string remoteFilePath = Path.Join (remotePath, dunTool);
    try {
        ftp.DownloadFile(remoteFilePath, localFilePath);
        Console.WriteLine($"{remoteFilePath} -> {localPath} \u2713");
    } catch {
        Console.WriteLine($"{remoteFilePath} -> {localPath} \u2717");
    }
    try {
        ZipFile.ExtractToDirectory(localFilePath, localPath, overwriteFiles: true);
        Console.WriteLine($"{localFilePath} 解压成功 \u2713");
    } catch {
        Console.WriteLine($"{localFilePath} 解压失败 \u2717");
    }
    File.Delete(localFilePath);
    

}
int wait = 2;
for (int i = 0; i < wait; i++) {

    Console.Write($"{wait - i} 后自动关闭\n");
    Thread.Sleep(1000);
}
