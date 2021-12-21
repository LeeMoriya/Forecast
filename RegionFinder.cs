using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class RegionFinder
{
    public static List<string> Generate()
    {
        List<string> regionList = new List<string>();
        StringBuilder sb = new StringBuilder();
        sb.Append("DOWNPOUR - Vanilla Regions: ");

        //Vanilla Regions
        if (File.Exists(Custom.RootFolderDirectory() + "/World/Regions/regions.txt"))
        {
            string[] vanillaRegions = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");
            for (int i = 0; i < vanillaRegions.Length; i++)
            {
                if (!regionList.Contains(vanillaRegions[i]))
                {
                    regionList.Add(vanillaRegions[i]);
                    sb.Append(vanillaRegions[i] + ", ");
                }
            }
        }
        else
        {
            Debug.Log("Vanilla regions.txt does not exist, how did you even get this far?");
        }

        //Print Vanilla Regions
        Debug.Log(sb.ToString());
        sb = new StringBuilder();
        sb.Append("DOWNPOUR - Custom Regions: ");

        //Custom Regions
        if (Directory.Exists(string.Concat(new object[]
        {
            Custom.RootFolderDirectory(),
            "Mods",
            Path.DirectorySeparatorChar,
            "CustomResources"
            })))
        {
            foreach (string dir in Directory.GetDirectories(Custom.RootFolderDirectory() + "Mods" + Path.DirectorySeparatorChar + "CustomResources"))
            {
                //Region Folder
                if (Directory.Exists(dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions"))
                {
                    //Check whether CRS version is using regionInfo or packInfo
                    string jsonType = "";
                    if (Directory.GetFiles(dir).Contains(dir + Path.DirectorySeparatorChar + "packInfo.json"))
                    {
                        jsonType = "packInfo.json";
                    }
                    else if (Directory.GetFiles(dir).Contains(dir + Path.DirectorySeparatorChar + "regionInfo.json"))
                    {
                        jsonType = "regionInfo.json";
                    }
                    if (jsonType != "")
                    {
                        string[] crsRegion = new string[]
                        {
                            string.Empty
                        };
                        crsRegion = File.ReadAllLines(dir + Path.DirectorySeparatorChar + jsonType);
                        for (int i = 0; i < crsRegion.Length; i++)
                        {
                            if (crsRegion[i].Contains("activated"))
                            {
                                string[] crsLine = Regex.Split(crsRegion[i], ": ");
                                if (crsLine.Length > 0)
                                {
                                    if (crsLine[1].StartsWith("true"))
                                    {
                                        foreach(string reg in Directory.GetDirectories(dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions"))
                                        {
                                            string name = Path.GetFileName(reg);
                                            if (!regionList.Contains(name))
                                            {
                                                regionList.Add(name);
                                                sb.Append(name + ", ");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("CustomResources path was not found");
        }

        //Print Custom Regions
        Debug.Log(sb.ToString());

        return regionList;
    }
}

