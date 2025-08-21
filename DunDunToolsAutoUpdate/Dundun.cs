using DunAutoUpdateTool;

namespace DunDunToolsAutoUpdate;

class Dundun {
    private static readonly string _host = "210.22.86.114";
    private static readonly int _port = 22222;
    private static readonly string _username = "suanfa2";
    private static readonly string _password = "Sfkf2024@";
    public static string Three(int a) {
        string b = a.ToString();
        if (b.Length == 1)
            return $"00{b}";
        if (b.Length == 2)
            return $"0{b}";
        return b;
    }

    public static string Two(int a) {
        string b = a.ToString();
        if (b.Length == 1)
            return $"0{b}";
        return b;
    }

    public static SimpleFtpClient Ftp() {
        SimpleFtpClient client = new SimpleFtpClient(host: _host, port: _port, username: _username, password: _password);
        return client;
    }

    public static string FileName4Train(string rawFilename) {
        // 假设 rawFilename 是一个完整的文件路径
        // 按\\分割路径
        // D:\\FTP2\\2025-07-12\\18332-07-22-05\\sx-cs-z\\2025-07-12-07-23-06-393.jpg
        string[] parts = rawFilename.Split('\\');
        string part1, part2, part3; // 2025-07-12
        int hour = int.Parse(parts[3].Split('-')[1]);
        string amOrPm = "AM";
        if (hour >= 13 && hour <= 24)
            amOrPm = "PM";
        part1 = parts[^2].ToUpper();
        part2 = $"{parts[2]}-{amOrPm}";
        part3 = parts[^1];
        return $"{part1}+{part2}+{part3}";
    }
    
    

}
