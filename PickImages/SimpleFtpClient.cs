using System.Net;
using System.Text;

namespace PickImagesTool; 
class SimpleFtpClient {
    private readonly string _host;
    private readonly int _port;
    private readonly NetworkCredential _credentials;
    public SimpleFtpClient(string host, int port, string username, string password) {
        _host = host.StartsWith("ftp://") ? host : $"ftp://{host}";
        _port = port;
        _credentials = new NetworkCredential(username, password);
    }

    public bool UploadFile(string localFilePath, string remoteFilePath) {
        var uri = new UriBuilder(_host) { Port = _port, Path = remoteFilePath }.Uri;
        using var client = new WebClient { Credentials = _credentials };
        try {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            client.Encoding = Encoding.GetEncoding("GBK");
            client.UploadFile(uri, WebRequestMethods.Ftp.UploadFile, localFilePath);
            //Console.WriteLine($"文件 {localFilePath} 上传到 {remoteFilePath} 成功");
            return true;
        }
        catch (WebException ex) {
            //Console.WriteLine($"上传文件失败: {ex.Message}");
            return false;
        }
    }

    public bool DownloadFile(string remoteFilePath, string localFilePath) {
        var uri = new UriBuilder(_host) { Port = _port, Path = remoteFilePath }.Uri;
        using var client = new WebClient { Credentials = _credentials };
        try {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            client.Encoding = Encoding.GetEncoding("GBK"); // 设置编码为GBK
            client.DownloadFile(uri, localFilePath);
            //Console.WriteLine($"文件 {remoteFilePath} 下载到 {localFilePath} 成功");
            return true;
        }
        catch (WebException ex) {
            //Console.WriteLine($"下载文件失败: {ex.Message}");
            return false;
        }
    }
}
