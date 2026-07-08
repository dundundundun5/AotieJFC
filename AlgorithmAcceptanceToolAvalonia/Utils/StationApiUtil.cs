using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace AlgorithmAcceptanceToolAvalonia.Utils;

public class StationApiUtil
{
    private static string[] _stations = [
        "伍明",
        "凤台",
        "包庄",
        "大许",
        "宁波",
        "建国",
        "新塘边",
        "杨集",
        "杭州",
        "枫泾",
        "泗安",
        "淮北北",
        "湾沚",
        "炮车",
        "虞城",
        "西寺坡",
        "誓节渡",
        "李庄",
        "姚李庙",
        "杨楼",
        "梓树庄",
        "烔炀河",
        "东孝",
        "白龙桥",
        "汤溪",
        "后溪街",
        "牌头"
    ];
    private static string[] _apis = [
        "120",
        "73",
        "114",
        "71",
        "122",
        "113",
        "142",
        "116",
        "122", //杭州改成122.14即可
        "111",
        "96",
        "123",
        "117",
        "115",
        "143",
        "121",
        "112",
        "124",
        "119",
        "125",
        "118",
        "72",
        "92",
        "93",
        "94",
        "95",
        "91"
    ];

    
    public static (string,string) PresentStationAlgorithmApi()
    {
        string ip = GetLocalIPAddress();
        for(int i = 0; i < _apis.Length; i++)
        {
            string station = _stations[i];
            if (ip.Contains($".{_apis[i]}."))
            {
                if (_stations[i].Equals("杭州"))
                    return (_stations[i], $"{_apis[i]}.14");
                return (_stations[i], $"{_apis[i]}.11");
            }
                


        }
        return ("", "");
    }
    
 

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            // 只取IPv4地址，忽略回环地址（127.0.0.1）
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                return ip.ToString();
            }
        }

        return "";
    }
}