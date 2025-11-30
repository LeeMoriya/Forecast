using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.CompilerServices;
using MoreSlugcats;

public class WeatherHooks
{
    public static Dictionary<Room, WeatherController.WeatherSettings> roomSettings = new Dictionary<Room, WeatherController.WeatherSettings>();
    public static DebugWeatherUI debugUI;

    public static List<RoomRain.DangerType> invalidDangerTypes;

    static WeatherHooks()
    {
        invalidDangerTypes = new List<RoomRain.DangerType>()
        {
            RoomRain.DangerType.AerieBlizzard,
            RoomRain.DangerType.Flood,
            RoomRain.DangerType.None
        };

        if (ModManager.MSC || ModManager.Watcher)
        {
            invalidDangerTypes.Add(DLCSharedEnums.RoomRainDangerType.Blizzard);
        }
    }

    public static float rainIntensity; //OLD - used for classic snow

    public static void Patch()
    {
        On.Room.Update += Room_Update;
        //On.Room.Loaded += Room_Loaded;
        On.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.Lightning.ctor += Lightning_ctor;
        On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
        On.ArenaGameSession.ctor += ArenaGameSession_ctor;
        On.Player.ctor += Player_ctor;
        On.StoryGameSession.ctor += StoryGameSession_ctor;
        On.RainWorld.Update += RainWorld_Update;
        On.RoomRain.Update += RoomRain_Update;
        On.AbstractRoom.Abstractize += AbstractRoom_Abstractize; //Remove settings
        On.WinState.CycleCompleted += WinState_CycleCompleted;
        On.RoomRain.DrawSprites += RoomRain_DrawSprites;
    }

    private static void ArenaGameSession_ctor(On.ArenaGameSession.orig_ctor orig, ArenaGameSession self, RainWorldGame game)
    {
        orig.Invoke(self, game);
        if (ForecastConfig.classicSnow.Value)
        {
            ForecastMod.exposureControllers = new List<ExposureController>();
        }
    }

    private static void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
    {
        if (roomSettings.ContainsKey(self.room))
        {
            if (roomSettings[self.room].weatherType == 2 && ForecastConfig.classicSnow.Value)
            {
                return;
            }
        }
        orig.Invoke(self, eu);
    }

    private static void RoomRain_DrawSprites(On.RoomRain.orig_DrawSprites orig, RoomRain self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        //Fix to stop freeze when deleting RoomRain at cycle start if weather is Blizzard
        if ((ModManager.MSC || ModManager.Watcher) && self.room != null && self.room.roomSettings != null && self.room.roomSettings.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);

    }

    private static void WinState_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
    {
        orig.Invoke(self, game);

        if (WeatherForecast.regionWeatherForecasts != null)
        {
            //Progress weather
            foreach (KeyValuePair<string, List<WeatherForecast.Weather.WeatherType>> pair in WeatherForecast.regionWeatherForecasts)
            {
                pair.Value[0] = pair.Value[1];
                pair.Value[1] = pair.Value[2];
                pair.Value[2] = WeatherForecast.NextWeather(pair.Key, pair.Value[1]);
            }
            WeatherData.Save();
        }
    }

    private static void AbstractRoom_Abstractize(On.AbstractRoom.orig_Abstractize orig, AbstractRoom self)
    {
        if (roomSettings.ContainsKey(self.realizedRoom))
        {
            roomSettings.Remove(self.realizedRoom);
        }
        orig.Invoke(self);
    }

    private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        orig.Invoke(self);
        if (ForecastConfig.debugMode.Value)
        {
            if (self.processManager.currentMainLoop is RainWorldGame)
            {
                if (debugUI == null)
                {
                    debugUI = new DebugWeatherUI();
                }
                else
                {
                    debugUI.Update();
                }
            }
            else if (debugUI != null)
            {
                debugUI.RemoveSprites();
                debugUI = null;
            }
        }
    }

    private static void StoryGameSession_ctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name saveStateNumber, RainWorldGame game)
    {
        try
        {
            orig.Invoke(self, saveStateNumber, game);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        //Load save data first if there isn't any, run the next two methods
        WeatherData.Load();

        if (WeatherForecast.regionWeatherForecasts == null || WeatherForecast.regionWeatherForecasts.Keys.Count == 0)
        {
            WeatherForecast.InitialWeather();
        }

        WeatherForecast.ClearAll();
        roomSettings.Clear();

        if (ForecastConfig.debugMode.Value && ForecastMod.exposureControllers != null)
        {
            for (int i = 0; i < ForecastMod.exposureControllers.Count; i++)
            {
                ForecastMod.exposureControllers[i].RemoveDebugLabels();
            }
        }
        if (ForecastConfig.classicSnow.Value)
        {
            ForecastMod.exposureControllers = new List<ExposureController>();
        }
    }

    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig.Invoke(self, abstractCreature, world);
        if (ForecastConfig.classicSnow.Value && self.room.game.session is StoryGameSession)
        {
            ForecastMod.exposureControllers.Add(new ExposureController(self));
        }
    }

    private static void AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        orig.Invoke(self, world, game);
        //Add Weather Object
        if (self.realizedRoom != null && self.realizedRoom.roomRain != null)
        {
            //if (!self.shelter && !self.gate && self.realizedRoom.roomSettings.RainIntensity > 0f && !invalidDangerTypes.Contains(self.realizedRoom.roomRain.dangerType))

            if (!self.shelter && self.realizedRoom.roomSettings.RainIntensity > 0f)
            {
                if (!roomSettings.ContainsKey(self.realizedRoom))
                {
                    if (ModManager.MSC && game.manager.artificerDreamNumber != -1) { return; }
                    self.realizedRoom.AddObject(new WeatherController(self.realizedRoom));

                    if (roomSettings.ContainsKey(self.realizedRoom) && roomSettings[self.realizedRoom].weatherType == 2 && ForecastConfig.classicSnow.Value)
                    {
                        if (self.realizedRoom.game != null && !self.realizedRoom.abstractRoom.shelter)
                        {
                            ForecastLog.Log("Adding WeatherSounds");
                            self.realizedRoom.AddObject(new WeatherSounds(self.realizedRoom));
                        }
                    }
                }
            }
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
                if (!roomSettings.ContainsKey(room))
                {
                    room.AddObject(new WeatherController(room));
                }
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
        if (roomSettings.ContainsKey(self) && roomSettings[self].weatherType == 2 && ForecastConfig.classicSnow.Value)
        {
            if (self.game != null && !self.abstractRoom.shelter)
            {
                ForecastLog.Log("Adding WeatherSounds");
                self.AddObject(new WeatherSounds(self));
            }
        }
    }
    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig.Invoke(self);
        if (self.BeingViewed && roomSettings.ContainsKey(self) && roomSettings[self].weatherType == 2 && ForecastConfig.classicSnow.Value && self.world.rainCycle.RainDarkPalette > 0f)
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
            if (Input.GetKey(KeyCode.Alpha4))
            {
                self.world.rainCycle.timer += 25;
            }
            if (ModManager.MSC || ModManager.Watcher)
            {
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4))
                {
                    //Toggle AerieBlizzard and normal Blizzard
                    if (self.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard)
                    {
                        self.roomSettings.DangerType = DLCSharedEnums.RoomRainDangerType.Blizzard;
                    }
                    else if (self.roomSettings.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard)
                    {
                        self.roomSettings.DangerType = RoomRain.DangerType.AerieBlizzard;
                    }
                }
            }
        }
    }
}

