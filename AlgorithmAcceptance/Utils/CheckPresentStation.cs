using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AlgorithmAcceptanceTool.Utils;
public class CheckPresentStation
{
    public static string LocalApi = "18.23";
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
        string path = @"D:\";
        for(int i = 0; i < _stations.Length; i++)
        {
            string station = _stations[i];
            string stationPath = Path.Join(path, station);
            if (Directory.Exists(stationPath))
            {
                if (station == "杭州")
                    return (station, $"{_apis[i]}.14");
                
                return (station,$"{_apis[i]}.11");
            }

            
        }
        return (null, null);
    }
}
