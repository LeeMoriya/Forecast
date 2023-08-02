using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.CompilerServices;

public class WeatherHooks
{
    public static ConditionalWeakTable<Room, WeatherController.WeatherSettings> roomSettings = new ConditionalWeakTable<Room, WeatherController.WeatherSettings>();

    public static float rainIntensity;
    public static List<string> roomRainList;
    public static float startingIntensity;
    public static float rainAmount = 0;
    public static int ceilingCount = 0;
    public static Vector2 lastPlayerPos = new Vector2();
    public static bool noRain = false;
    public static List<string> rainList = new List<string>();
    public static int direction;
    public static bool noRainThisCycle = false;
    public static float floodLevel = 0;
    public static bool flooding = false;

    public static void Patch()
    {
        On.Room.Update += Room_Update;
        On.Room.Loaded += Room_Loaded;
        On.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.Lightning.ctor += Lightning_ctor;
        On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
        On.AbstractRoom.Abstractize += AbstractRoom_Abstractize;
        On.RainWorld.LoadResources += RainWorld_LoadResources;
        On.Player.ctor += Player_ctor;
        On.RainCycle.RainHit += RainCycle_RainHit;
        On.RoomRain.Update += RoomRain_Update;
        On.StoryGameSession.ctor += StoryGameSession_ctor;
    }

    private static void StoryGameSession_ctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name saveStateNumber, RainWorldGame game)
    {
        orig.Invoke(self, saveStateNumber, game);
        if(Forecast.debug && Forecast.exposureControllers != null)
        {
            for (int i = 0; i < Forecast.exposureControllers.Count; i++)
            {
                Forecast.exposureControllers[i].RemoveDebugLabels();
            }
        }
        if (ForecastConfig.weatherType.Value == 1 && ForecastConfig.endBlizzard.Value)
        {
            Forecast.exposureControllers = new List<ExposureController>();
        }
    }

    private static void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
    {
        if (ForecastConfig.weatherType.Value == 1 && ForecastConfig.endBlizzard.Value)
        {
            return;
        }
        orig.Invoke(self, eu);
    }

    private static void RainCycle_RainHit(On.RainCycle.orig_RainHit orig, RainCycle self)
    {
        if (ForecastConfig.weatherType.Value == 1 && ForecastConfig.endBlizzard.Value)
        {
            return;
        }
        orig.Invoke(self);
    }

    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig.Invoke(self, abstractCreature, world);
        if (ForecastConfig.weatherType.Value == 1 && ForecastConfig.endBlizzard.Value && self.room.game.session is StoryGameSession)
        {
            Forecast.exposureControllers.Add(new ExposureController(self));
        }
    }

    private static void AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        orig.Invoke(self, world, game);
        if (!noRain)
        {
            //Add Weather Object
            if (self.realizedRoom != null && self.realizedRoom.roomRain != null)
            {
                if (!self.shelter && !self.gate && self.realizedRoom.roomRain.dangerType != RoomRain.DangerType.Flood && !rainList.Contains(self.name))
                {
                    ceilingCount = 0;
                    for (int r = 0; r < self.realizedRoom.TileWidth; r++)
                    {
                        if (self.realizedRoom.Tiles[r, self.realizedRoom.TileHeight - 1].Solid)
                        {
                            ceilingCount++;
                        }
                    }
                    if (ceilingCount < (self.realizedRoom.Width * 0.95))
                    {
                        rainList.Add(self.name);
                        self.realizedRoom.AddObject(new WeatherController(self.realizedRoom, ForecastConfig.weatherType.Value));
                    }
                }
            }
        }
    }

    private static void RainWorld_LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
    {
        orig.Invoke(self);
        Forecast.snowExt1 = new Texture2D(0, 0);
        if (File.Exists(AssetManager.ResolveFilePath("sprites\\snowExt1.png")))
        {
            ForecastLog.Log("FORECAST: Loaded snowExt1.png");
        }
        Forecast.snowExt1.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowExt1.png")));
        Forecast.snowExt1.filterMode = FilterMode.Point;

        Forecast.snowInt1 = new Texture2D(0, 0);
        if (File.Exists(AssetManager.ResolveFilePath("sprites\\snowInt1.png")))
        {
            ForecastLog.Log("FORECAST: Loaded snowInt1.png");
        }
        Forecast.snowInt1.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowInt1.png")));
        Forecast.snowInt1.filterMode = FilterMode.Point;

        byte[] rainbytes = File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\rainButton.png"));
        Texture2D raintexture = new Texture2D(0, 0);
        raintexture.filterMode = FilterMode.Point;
        raintexture.LoadImage(rainbytes);
        Futile.atlasManager.LoadAtlasFromTexture("rainbutton", raintexture, false);

        byte[] snowbytes = File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowButton.png"));
        Texture2D snowtexture = new Texture2D(0, 0);
        snowtexture.filterMode = FilterMode.Point;
        snowtexture.LoadImage(snowbytes);
        Futile.atlasManager.LoadAtlasFromTexture("snowbutton", snowtexture, false);

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
    }

    private static void AbstractRoom_Abstractize(On.AbstractRoom.orig_Abstractize orig, AbstractRoom self)
    {
        orig.Invoke(self);
        if (self != null && self.realizedRoom == null && rainList.Contains(self.name))
        {
            rainList.Remove(self.name);
        }
    }

    private static void ArenaGameSession_SpawnPlayers(On.ArenaGameSession.orig_SpawnPlayers orig, ArenaGameSession self, Room room, List<int> suggestedDens)
    {
        orig.Invoke(self, room, suggestedDens);
        if (ForecastConfig.weatherIntensity.Value > 0)
        {
            if (ForecastConfig.weatherIntensity.Value == 1)
            {
                rainIntensity = 0.31f;
            }
            if (ForecastConfig.weatherIntensity.Value == 2)
            {
                rainIntensity = 0.6f;
            }
            if (ForecastConfig.weatherIntensity.Value == 3)
            {
                rainIntensity = 1f;
            }
        }
        else
        {
            rainIntensity = UnityEngine.Random.Range(0.3f, 1f);
        }
        startingIntensity = rainIntensity;
        ForecastLog.Log("Current rain intensity: " + rainIntensity);
        if (self != null && room.roomRain != null)
        {
            for (int r = 0; r < room.TileWidth; r++)
            {
                if (room.Tiles[r, room.TileHeight - 1].Solid)
                {
                    ceilingCount++;
                }
            }
            if (ceilingCount < (room.Width * 0.95) && !noRain)
            {
                room.AddObject(new WeatherController(room, ForecastConfig.weatherType.Value));
            }
            ceilingCount = 0;
        }
    }
    private static void Lightning_ctor(On.Lightning.orig_ctor orig, Lightning self, Room room, float intensity, bool bkgOnly)
    {
        orig.Invoke(self, room, intensity, bkgOnly);
        //self.bkgGradient = new Color[2];
        //if (!room.game.IsArenaSession && (room.world.region.name == "UW" || room.world.region.name == "TR"))
        //{
        //    self.bkgGradient[0] = new Color(0.19607843f, 0.23529412f, 0.78431374f);
        //    self.bkgGradient[1] = new Color(0.21176471f, 1f, 0.22352941f);
        //}
        //else
        //{
        //    self.bkgGradient[0] = room.game.cameras[0].currentPalette.skyColor;
        //    self.bkgGradient[1] = Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), rainIntensity);
        //}
    }
    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);
        rainList.Clear();

        //Rain Chance
        if (UnityEngine.Random.Range(0, 100) < ForecastConfig.weatherChance.Value)
        {
            noRainThisCycle = false;
        }
        else
        {
            noRainThisCycle = true;
        }
        //Rain Starting Intensity (Non-Dynamic)
        if (ForecastConfig.weatherIntensity.Value > 0)
        {
            if (ForecastConfig.weatherIntensity.Value == 1)
            {
                rainIntensity = 0.31f;
            }
            if (ForecastConfig.weatherIntensity.Value == 2)
            {
                rainIntensity = 0.6f;
            }
            if (ForecastConfig.weatherIntensity.Value == 3)
            {
                rainIntensity = 0.9f;
            }
        }
        //Rain Starting Intensity (Dynamic)
        else
        {
            if (ForecastConfig.weatherType.Value != 0)
            {
                rainIntensity = UnityEngine.Random.Range(-1.5f, 0.8f);
            }
            else
            {
                rainIntensity = UnityEngine.Random.Range(0.15f, 0.9f);
            }
        }
        //Rain Direction
        switch (ForecastConfig.windDirection.Value)
        {
            case 0:
                direction = UnityEngine.Random.Range(1, 4);
                Forecast.blizzardDirection = direction;
                break;
            case 1:
                direction = 1;
                break;
            case 2:
                direction = 2;
                break;
            case 3:
                direction = 3;
                break;
        }
        int leftOrRight = direction;
        if(direction == 2)
        {
            if(UnityEngine.Random.value >= 0.5f)
            {
                leftOrRight = 3;
            }
            else
            {
                leftOrRight = 1;
            }
        }
        Forecast.blizzardDirection = leftOrRight;
        //Apply Rain Intensity
        if (!noRainThisCycle)
        {
            startingIntensity = rainIntensity;
        }
        else
        {
            startingIntensity = 0;
            rainIntensity = 0;
        }
    }
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig.Invoke(self);
        //Enable Weather Per-Region based on Config
        if (self.game != null && !self.game.IsArenaSession)
        {
            if (Forecast.rainRegions.Contains(self.world.region.name))
            {
                noRain = false;
            }
            else
            {
                noRain = true;
            }
        }
        else
        {
            noRain = false;
        }
        if (ForecastConfig.weatherType.Value == 1 && ForecastConfig.endBlizzard.Value)
        {
            if (self.game != null && !self.abstractRoom.shelter)
            {
                self.AddObject(new WeatherSounds(self));
            }
        }
    }
    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig.Invoke(self);
        if (ForecastConfig.weatherType.Value == 1 && ForecastConfig.endBlizzard.Value && self.world.rainCycle.RainDarkPalette > 0f)
        {
            //Update exposure
            if(Forecast.exposureControllers != null & Forecast.exposureControllers.Count > 0)
            {
                for (int i = 0; i < Forecast.exposureControllers.Count; i++)
                {
                    Forecast.exposureControllers[i].Update();
                }
            }
        }
        if (Forecast.debug && self.BeingViewed)
        {
            //Decrease Intensity
            if (Input.GetKey(KeyCode.Alpha1))
            {
                rainIntensity -= 0.005f;
                if (rainIntensity < 0f)
                {
                    rainIntensity = 0f;
                }
                ForecastLog.Log("Rain Intensity = " + rainIntensity);
            }
            //Increase Intensity
            if (Input.GetKey(KeyCode.Alpha2))
            {
                rainIntensity += 0.005f;
                if (rainIntensity > 1f)
                {
                    rainIntensity = 1f;
                }
                ForecastLog.Log("Rain Intensity = " + rainIntensity);
            }
            //Show Room X and Y pos
            if (Input.GetKey(KeyCode.Alpha3))
            {
                if (self.BeingViewed)
                {
                    ForecastLog.Log("---ROOM POSITION---");
                    ForecastLog.Log("X POS: " + self.abstractRoom.mapPos.x.ToString());
                    ForecastLog.Log("Y POS: " + self.abstractRoom.mapPos.y.ToString());
                }
            }
            //Fast Forward Cycle Timer
            if (Input.GetKey(KeyCode.Alpha5))
            {
                self.world.rainCycle.timer += 25;
            }

        }
        ////Rain intensity increases with cycle duration if in dynamic mode
        //if (ForecastConfig.weatherIntensity.Value == 0 && !noRainThisCycle && self.BeingViewed)
        //{
        //    if (self.world.rainCycle.RainDarkPalette <= 0)
        //    {
        //        rainIntensity = Mathf.Lerp(startingIntensity, 1f, self.world.rainCycle.CycleProgression);
        //    }
        //    else
        //    {
        //        rainIntensity = Mathf.Lerp(0.95f, 0f, self.world.rainCycle.RainDarkPalette);
        //    }
        //}
    }
}

