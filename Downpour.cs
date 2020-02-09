using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using OptionalUI;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using RWCustom;
using Menu;

public class RainScript : MonoBehaviour
{
    public static Downpour mod;
    public void Initialize()
    {
        RainFall.Patch();
        RainPalette.Patch();
    }
    public void Update()
    {

    }
}
public class Downpour : PartialityMod
{
    public Downpour()
    {
        this.ModID = "Downpour";
        this.Version = "v0.4";
        this.author = "LeeMoriya";
    }

    public static RainScript script;
    public static bool paletteChange = true;
    public static bool lightning = true;
    public static bool dynamic = true;
    public static int intensity = 0;
    public static bool rainbow = false;
    public static bool configLoaded = false;
    public static bool debug = true;
    public static bool snow = false;
    public static bool bg = true;
    public static List<string> rainRegions = new List<string>();
    public static int rainAmount;

    public override void OnEnable()
    {
        base.OnEnable();
        RainScript.mod = this;
        GameObject go = new GameObject();
        script = go.AddComponent<RainScript>();
        script.Initialize();
    }
    public static OptionInterface LoadOI()
    {
        return new DownpourConfig();
    }
}