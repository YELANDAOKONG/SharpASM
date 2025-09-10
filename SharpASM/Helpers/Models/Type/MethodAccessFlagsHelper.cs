using System;
using System.Collections.Generic;
using SharpASM.Models.Type;

namespace SharpASM.Helpers.Models.Type;

public static class MethodAccessFlagsHelper
{
    public static List<MethodAccessFlags> GetFlagsList(ushort value)
    {
        var result = new List<MethodAccessFlags>();
        var flags = (MethodAccessFlags)value;

        foreach (MethodAccessFlags flag in Enum.GetValues(typeof(MethodAccessFlags)))
        {
            if (flag != 0 && flags.HasFlag(flag))
            {
                result.Add(flag);
            }
        }

        return result;
    }

    public static string GetFlagsString(ushort value)
    {
        var list = GetFlagsList(value);
        return string.Join(" | ", list);
    }
}
