using System;
using System.Collections.Generic;
using AlgorithmAcceptanceToolAvalonia.Models;
using AlgorithmAcceptanceToolAvalonia.Models.Enums;

namespace AlgorithmAcceptanceToolAvalonia.Converters;

public class TaskNameConverter
{
    public static readonly string Load = "LOAD";
    public static readonly string Body = "BODY";
    public static readonly string Left = "LEFT";
    public static readonly string Crop = "CROP";
    public static string FromEnum(EnumTaskName taskName)
    {
        if (taskName is EnumTaskName.车身)
        {
            return Load;
        }
        else if (taskName is EnumTaskName.走行)
        {
            return Body;
        }
        else if (taskName is EnumTaskName.标志灯)
        {
            return Load;
        }

        return Load;
    }
}