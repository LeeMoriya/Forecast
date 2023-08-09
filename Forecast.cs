using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine.Video;
using RWCustom;
using Menu;
using System.Security.Permissions;
using MonoMod.RuntimeDetour;
using BepInEx;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

[BepInPlugin("LeeMoriya.Forecast", "Forecast", "1.03")]
public class Forecast : BaseUnityPlugin
{
    public static string version = "1.03";
    public static bool init = false;
    public Forecast()
    {
        
    }

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig.Invoke(self);

        ForecastLog.ClearLog();
        if (!init)
        {
            WeatherHooks.Patch();
            //RainPalette.Patch();
            //new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.ScreenShake)).GetGetMethod(), (Func<Func<RainCycle, float>, RainCycle, float>)RainCycle_get_ScreenShake);
            //new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.MicroScreenShake)).GetGetMethod(), (Func<Func<RainCycle, float>, RainCycle, float>)RainCycle_get_MicroScreenShake);
            init = true;
        }

        Options = new ForecastConfig(this);
        MachineConnector.SetRegisteredOI("forecast", Options);
    }

    private float RainCycle_get_MicroScreenShake(Func<RainCycle, float> orig, RainCycle rainCycle)
    {
        if (ForecastConfig.weatherType.Value == 0 && ForecastConfig.endBlizzard.Value)
        {
            return 0f;
        }
        return orig.Invoke(rainCycle);
    }

    private float RainCycle_get_ScreenShake(Func<RainCycle, float> orig, RainCycle rainCycle)
    {
        if (ForecastConfig.weatherType.Value == 0 && ForecastConfig.endBlizzard.Value)
        {
            return 0f;
        }
        return orig.Invoke(rainCycle);
    }

    public static ForecastConfig Options;
    public static int palettecount = 0;
    public static bool paletteChange = true;
    public static bool rainbow = false;
    public static bool debug = true;
    public static bool decals = true;
    public static List<string> rainRegions = new List<string>();
    public static List<ExposureController> exposureControllers;
    public static int blizzardDirection;
    public static bool interiorRain = true;
    public static Texture2D snowExt1;
    public static Texture2D snowInt1;
}