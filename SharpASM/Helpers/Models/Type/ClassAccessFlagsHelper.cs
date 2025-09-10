using System;
using System.Collections.Generic;
using SharpASM.Models.Type;

namespace SharpASM.Helpers.Models.Type;

public static class ClassAccessFlagsHelper
{
    public static List<ClassAccessFlags> GetFlagsList(ushort value)
    {
        var result = new List<ClassAccessFlags>();
        var flags = (ClassAccessFlags)value;

        foreach (ClassAccessFlags flag in Enum.GetValues(typeof(ClassAccessFlags)))
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
