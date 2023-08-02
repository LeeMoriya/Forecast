using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

public static class ForecastLog
{
    public static void ClearLog()
    {
        File.WriteAllText(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "ForecastLog.txt", "[FORECAST LOGGER] - " + DateTime.Now.ToString());
    }

    public static void Log(string text)
    {
        if (RainWorld.ShowLogs)
        {
            string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "ForecastLog.txt";
            if (!File.Exists(path))
            {
                ClearLog();
            }
            File.AppendAllText(path, "\n" + text);
        }
    }

    public static void LogOnce(string text)
    {
        if (RainWorld.ShowLogs)
        {
            string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "ForecastLog.txt";
            if (!File.Exists(path))
            {
                ClearLog();
            }
            if (!onceText.Contains(text))
            {
                File.AppendAllText(path, "\n" + text);
                onceText.Add(text);
            }
        }
    }

    public static List<string> onceText = new List<string>();
}
