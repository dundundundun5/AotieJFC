using System;
using System.Collections.Generic;
using AlgoritmAcceptanceToolAvalonia.Models;

namespace AlgoritmAcceptanceToolAvalonia.Converters;

public class TaskNameConverter
{
    public static readonly string Load = "LOAD";
    public static readonly string Body = "BODY";
    public static readonly string Left = "LEFT";
    public static readonly string Crop = "CROP";
    public static List<string> FromEnum(EnumTaskName taskName)
    {
        List<string> res = [];
        if (taskName is EnumTaskName.车身)
        {
            res.Add(Load);
        }
        else if (taskName is EnumTaskName.走行)
        {
            res.Add(Body);
        }
        else if (taskName is EnumTaskName.标志灯)
        {
            res.Add(Load);
            res.Add(Left);
            res.Add(Load);
            res.Add(Load);
        }

        return res;
    }
}