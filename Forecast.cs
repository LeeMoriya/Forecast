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

[BepInPlugin("LeeMoriya.Forecast", "Forecast", "1.2.4")]
public class ForecastMod : BaseUnityPlugin
{
    public static bool init = false;
    public static string versionNum = "1.2.4";

    public static string Translate(string text)
    {
        // Short-string files store each entry on one physical line.
        string key = text.Replace("\r\n", "\n").Replace("\n", "<LINE>");
        string translated = Custom.rainWorld?.inGameTranslator?.Translate(key);
        return string.IsNullOrEmpty(translated) || translated == "!NO TRANSLATION!"
            ? text
            : translated.Replace("<LINE>", "\n");
    }

    public ForecastMod()
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
            LoadSprites();

            AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/forecastshaders"));
            self.Shaders.Add("LeeMoriya.FrostVignette", FShader.CreateShader("LeeMoriya.FrostVignette", assetBundle.LoadAsset<Shader>("Assets/Shaders/FrostVignette.shader")));

            WeatherHooks.Patch();
            //RainPalette.Patch();
            new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.ScreenShake)).GetGetMethod(), (Func<Func<RainCycle, float>, RainCycle, float>)RainCycle_get_ScreenShake);
            new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.MicroScreenShake)).GetGetMethod(), (Func<Func<RainCycle, float>, RainCycle, float>)RainCycle_get_MicroScreenShake);
            init = true;
        }

        Options = new ForecastConfig(this);
        MachineConnector.SetRegisteredOI("leemoriya.forecast", Options);
    }

    private void LoadSprites()
    {
        Futile.atlasManager.LoadAtlas("sprites\\weatherSprites");

        ForecastMod.snowExt = new Texture2D(0, 0, TextureFormat.ARGB32, false);
        if (File.Exists(AssetManager.ResolveFilePath("sprites\\snowExt.png")))
        {
            ForecastLog.Log("FORECAST: Loaded snowExt.png");
        }
        ForecastMod.snowExt.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowExt.png")));
        ForecastMod.snowExt.filterMode = FilterMode.Point;

        ForecastMod.snowInt = new Texture2D(0, 0, TextureFormat.ARGB32, false);
        if (File.Exists(AssetManager.ResolveFilePath("sprites\\snowInt.png")))
        {
            ForecastLog.Log("FORECAST: Loaded snowInt.png");
        }
        ForecastMod.snowInt.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowInt.png")));
        ForecastMod.snowInt.filterMode = FilterMode.Point;

        if (!Futile.atlasManager.DoesContainAtlas("bg_rain"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\bg_rain.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("bg_rain", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("snowpile"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowDecal.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("snowpile", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("logo"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\rainLogo.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("logo", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("logo2"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowLogo.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("logo", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("blizzard"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\blizzTex.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("blizzard", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("overlay1"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\overlay1.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("overlay1", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("overlay2"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\overlay2.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("overlay2", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("rainDirLight"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\rainDirLight.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("rainDirLight", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("rainDirHeavy"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\rainDirHeavy.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("rainDirHeavy", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("rainMidLight"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\rainMidLight.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("rainMidLight", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("rainMidHeavy"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\rainMidHeavy.png")));
            texture.filterMode = FilterMode.Point;
            Futile.atlasManager.LoadAtlasFromTexture("rainMidHeavy", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("frostOverlay"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\frostOverlay.png")));
            texture.filterMode = FilterMode.Bilinear;
            Futile.atlasManager.LoadAtlasFromTexture("frostOverlay", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("fogOverlay1"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\fogOverlay1.png")));
            texture.filterMode = FilterMode.Bilinear;
            Futile.atlasManager.LoadAtlasFromTexture("fogOverlay1", texture, false);
        }
        if (!Futile.atlasManager.DoesContainAtlas("fogOverlay2"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\fogOverlay2.png")));
            texture.filterMode = FilterMode.Bilinear;
            Futile.atlasManager.LoadAtlasFromTexture("fogOverlay2", texture, false);
        }
    }

    private float RainCycle_get_MicroScreenShake(Func<RainCycle, float> orig, RainCycle rainCycle)
    {
        WeatherController.WeatherSettings settings = null;

        var players = rainCycle?.world?.game?.Players;
        if (players != null)
        {
            foreach (var player in players)
            {
                var room = player?.Room?.realizedRoom;
                if (room == null) continue;

                if (room.BeingViewed == true && WeatherHooks.roomSettings.TryGetValue(room, out var ws))
                {
                    settings = ws;
                }
            }
        }
        if (ForecastConfig.classicSnow.Value && settings != null && settings.weatherType == 2)
        {
            return 0f;
        }
        return orig.Invoke(rainCycle);
    }

    private float RainCycle_get_ScreenShake(Func<RainCycle, float> orig, RainCycle rainCycle)
    {
        WeatherController.WeatherSettings settings = null;

        var players = rainCycle?.world?.game?.Players;
        if (players != null)
        {
            foreach (var player in players)
            {
                var room = player?.Room?.realizedRoom;
                if (room == null) continue;

                if (room.BeingViewed == true && WeatherHooks.roomSettings.TryGetValue(room, out var ws))
                {
                    settings = ws;
                }
            }
        }
        if (ForecastConfig.classicSnow.Value && settings != null && settings.weatherType == 2)
        {
            return 0f;
        }
        return orig.Invoke(rainCycle);
    }


    public static ForecastConfig Options;
    public static int palettecount = 0;
    public static bool paletteChange = true;
    public static bool rainbow = false;
    public static bool decals = true;
    public static List<ExposureController> exposureControllers;
    public static int blizzardDirection;
    public static bool interiorRain = true;
    public static Texture2D snowExt;
    public static Texture2D snowInt;
}