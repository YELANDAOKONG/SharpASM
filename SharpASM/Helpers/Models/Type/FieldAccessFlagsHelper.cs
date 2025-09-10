using System;
using System.Collections.Generic;
using SharpASM.Models.Type;

namespace SharpASM.Helpers.Models.Type;

public static class FieldAccessFlagsHelper
{
    public static List<FieldAccessFlags> GetFlagsList(ushort value)
    {
        var result = new List<FieldAccessFlags>();
        var flags = (FieldAccessFlags)value;

        foreach (FieldAccessFlags flag in Enum.GetValues(typeof(FieldAccessFlags)))
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
