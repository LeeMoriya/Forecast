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
    public bool init = false;
    public Forecast()
    {
        
    }
    public void OnEnable()
    {
        if (!init)
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            init = true;
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig.Invoke(self);
        RainFall.Patch();
        RainPalette.Patch();
        new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.ScreenShake)).GetGetMethod(), (Func<Func<RainCycle, float>, RainCycle, float>)RainCycle_get_ScreenShake);
        new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.MicroScreenShake)).GetGetMethod(), (Func<Func<RainCycle, float>, RainCycle, float>)RainCycle_get_MicroScreenShake);
    }

    private float RainCycle_get_MicroScreenShake(Func<RainCycle, float> orig, RainCycle rainCycle)
    {
        if (snow && blizzard)
        {
            return 0f;
        }
        return orig.Invoke(rainCycle);
    }

    private float RainCycle_get_ScreenShake(Func<RainCycle, float> orig, RainCycle rainCycle)
    {
        if (snow && blizzard)
        {
            return 0f;
        }
        return orig.Invoke(rainCycle);
    }

    public static int palettecount = 0;
    public static bool paletteChange = true;
    public static bool lightning = true;
    public static bool strike = false;
    public static int strikeDamage = 0;
    public static bool dynamic = false;
    public static int intensity = 3;
    public static bool rainbow = false;
    public static bool configLoaded = false;
    public static bool debug = true;
    public static bool snow = true;
    public static bool bg = false;
    public static bool water = true;
    public static bool decals = true;
    public static bool dust = true;
    public static bool blizzard = true;
    public static bool effectColors = true;
    public static List<string> rainRegions = new List<string>()
    {
        "SI","SU","CC"
    };
    public static List<ExposureController> exposureControllers;
    public static int rainAmount = 50;
    public static int direction = 0;
    public static int windDirection = 0;
    public static int rainChance = 100;
    public static bool interiorRain = true;
    public static Texture2D snowExt1;
    public static Texture2D snowInt1;
}