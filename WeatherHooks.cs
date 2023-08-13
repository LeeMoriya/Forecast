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
    public static WeatherForecast weatherForecast;
    public static DebugWeatherUI debugUI;

    public static List<RoomRain.DangerType> invalidDangerTypes = new List<RoomRain.DangerType>() //Invalid dangerTypes, don't spawn a WeatherController if the room uses one of these
    {
        RoomRain.DangerType.AerieBlizzard,
        RoomRain.DangerType.Flood,
        RoomRain.DangerType.None,
        MoreSlugcats.MoreSlugcatsEnums.RoomRainDangerType.Blizzard
    };

    public static float rainIntensity; //OLD - used for classic snow

    public static void Patch()
    {
        On.Room.Update += Room_Update;
        On.Room.Loaded += Room_Loaded;
        On.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.Lightning.ctor += Lightning_ctor;
        On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
        On.RainWorld.LoadResources += RainWorld_LoadResources;
        On.Player.ctor += Player_ctor;
        On.RainCycle.RainHit += RainCycle_RainHit;
        On.RoomRain.Update += RoomRain_Update;
        On.StoryGameSession.ctor += StoryGameSession_ctor;
        On.RainWorld.Update += RainWorld_Update;
    }

    private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        orig.Invoke(self);
        if (ForecastConfig.debugMode.Value)
        {
            if(self.processManager.currentMainLoop is RainWorldGame)
            {
                if(debugUI == null)
                {
                    debugUI = new DebugWeatherUI();
                }
                else
                {
                    debugUI.Update();
                }
            }
            else
            {
                debugUI.RemoveSprites();
                debugUI = null;
            }
        }
    }

    private static void StoryGameSession_ctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name saveStateNumber, RainWorldGame game)
    {
        orig.Invoke(self, saveStateNumber, game);

        if (weatherForecast == null)
        {
            weatherForecast = new WeatherForecast();
            ForecastLog.Log($"FORECAST: Creating WeatherForecast object");
        }
        else
        {
            weatherForecast.PrepareCycle(); 
            ForecastLog.Log($"FORECAST: Preparing next forecast");
        }

        if (ForecastConfig.debugMode.Value && ForecastMod.exposureControllers != null)
        {
            for (int i = 0; i < ForecastMod.exposureControllers.Count; i++)
            {
                ForecastMod.exposureControllers[i].RemoveDebugLabels();
            }
        }
        if (ForecastConfig.weatherType.Value == 1 && ForecastConfig.endBlizzard.Value)
        {
            ForecastMod.exposureControllers = new List<ExposureController>();
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
            ForecastMod.exposureControllers.Add(new ExposureController(self));
        }
    }

    private static void AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        orig.Invoke(self, world, game);
        //Add Weather Object
        int ceilingCount;
        if (self.realizedRoom != null && self.realizedRoom.roomRain != null)
        {
            if (!self.shelter && !self.gate && !invalidDangerTypes.Contains(self.realizedRoom.roomRain.dangerType))
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
                    for (int i = 0; i < self.realizedRoom.updateList.Count; i++)
                    {
                        if (self.realizedRoom.updateList[i] is WeatherController)
                        {
                            return;
                        }
                    }
                    self.realizedRoom.AddObject(new WeatherController(self.realizedRoom));
                }
            }
        }
    }

    private static void RainWorld_LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
    {
        orig.Invoke(self);
        ForecastMod.snowExt1 = new Texture2D(0, 0);
        if (File.Exists(AssetManager.ResolveFilePath("sprites\\snowExt1.png")))
        {
            ForecastLog.Log("FORECAST: Loaded snowExt1.png");
        }
        ForecastMod.snowExt1.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowExt1.png")));
        ForecastMod.snowExt1.filterMode = FilterMode.Point;

        ForecastMod.snowInt1 = new Texture2D(0, 0);
        if (File.Exists(AssetManager.ResolveFilePath("sprites\\snowInt1.png")))
        {
            ForecastLog.Log("FORECAST: Loaded snowInt1.png");
        }
        ForecastMod.snowInt1.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\snowInt1.png")));
        ForecastMod.snowInt1.filterMode = FilterMode.Point;

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
    }

    private static void ArenaGameSession_SpawnPlayers(On.ArenaGameSession.orig_SpawnPlayers orig, ArenaGameSession self, Room room, List<int> suggestedDens)
    {
        orig.Invoke(self, room, suggestedDens);
        int ceilingCount = 0;
        if (self != null && room.roomRain != null)
        {
            for (int r = 0; r < room.TileWidth; r++)
            {
                if (room.Tiles[r, room.TileHeight - 1].Solid)
                {
                    ceilingCount++;
                }
            }
            if (ceilingCount < (room.Width * 0.95))
            {
                room.AddObject(new WeatherController(room));
            }
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
        //Rain Direction
        int direction = 2;
        switch (ForecastConfig.windDirection.Value)
        {
            case 0:
                direction = UnityEngine.Random.Range(1, 4);
                ForecastMod.blizzardDirection = direction;
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
        if (direction == 2)
        {
            if (UnityEngine.Random.value >= 0.5f)
            {
                leftOrRight = 3;
            }
            else
            {
                leftOrRight = 1;
            }
        }
        ForecastMod.blizzardDirection = leftOrRight;
    }
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig.Invoke(self);
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
            if (ForecastMod.exposureControllers != null & ForecastMod.exposureControllers.Count > 0)
            {
                for (int i = 0; i < ForecastMod.exposureControllers.Count; i++)
                {
                    ForecastMod.exposureControllers[i].Update();
                }
            }
        }
        if (ForecastConfig.debugMode.Value && self.BeingViewed)
        {
            //Fast Forward Cycle Timer
            if (Input.GetKey(KeyCode.Alpha5))
            {
                self.world.rainCycle.timer += 25;
            }
        }
    }
}

