using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using Menu;

public class RainFall
{
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
    public static RainEffect.RainDrop[] snowDrops = new RainEffect.RainDrop[2000];

    public static void Patch()
    {
        On.Room.Update += Room_Update;
        On.Room.Loaded += Room_Loaded;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.Lightning.ctor += Lightning_ctor;
        On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
        On.AbstractRoom.Abstractize += AbstractRoom_Abstractize;
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
        if (!Downpour.dynamic)
        {
            if (Downpour.intensity == 1)
            {
                rainIntensity = 0.31f;
                Downpour.dynamic = false;
            }
            if (Downpour.intensity == 2)
            {
                rainIntensity = 0.6f;
                Downpour.dynamic = false;
            }
            if (Downpour.intensity == 3)
            {
                rainIntensity = 1f;
                Downpour.dynamic = false;
            }
        }
        else
        {
            Downpour.dynamic = true;
            rainIntensity = UnityEngine.Random.Range(0.3f, 1f);
        }
        startingIntensity = rainIntensity;
        Debug.Log("Current rain intensity: " + rainIntensity);
    }
    private static void Lightning_ctor(On.Lightning.orig_ctor orig, Lightning self, Room room, float intensity, bool bkgOnly)
    {
        self.room = room;
        self.intensity = intensity;
        self.bkgOnly = bkgOnly;
        self.lightningSources = new Lightning.LightningSource[2];
        for (int i = 0; i < 2; i++)
        {
            self.lightningSources[i] = new Lightning.LightningSource(self, i == 1);
        }
        self.bkgGradient = new Color[2];
        if (!room.game.IsArenaSession && (room.world.region.name == "UW" || room.world.region.name == "TR"))
        {
            self.bkgGradient[0] = new Color(0.19607843f, 0.23529412f, 0.78431374f);
            self.bkgGradient[1] = new Color(0.21176471f, 1f, 0.22352941f);
        }
        else
        {
            self.bkgGradient[0] = room.game.cameras[0].currentPalette.skyColor;
            self.bkgGradient[1] = Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), rainIntensity);
        }
    }
    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        //Starting rain intensity determined at the start of the cycle
        orig.Invoke(self, player);
        rainList.Clear();
        rainIntensity = 0;
        startingIntensity = 0;
        if (Downpour.configLoaded == false)
        {
            Downpour.rainRegions = new List<string>();
            Downpour.rainRegions.Add("CC");
            Downpour.rainRegions.Add("DS");
            Downpour.rainRegions.Add("HI");
            Downpour.rainRegions.Add("GW");
            Downpour.rainRegions.Add("SI");
            Downpour.rainRegions.Add("SU");
            Downpour.rainRegions.Add("SH");
            Downpour.rainRegions.Add("SL");
            Downpour.rainRegions.Add("LF");
        }
        if (UnityEngine.Random.Range(0, 100) < Downpour.rainChance)
        {
            noRainThisCycle = false;
        }
        else
        {
            noRainThisCycle = true;
        }
        if (!Downpour.dynamic)
        {
            if (Downpour.intensity == 1)
            {
                rainIntensity = 0.31f;
                Downpour.dynamic = false;
            }
            if (Downpour.intensity == 2)
            {
                rainIntensity = 0.6f;
                Downpour.dynamic = false;
            }
            if (Downpour.intensity == 3)
            {
                rainIntensity = 1f;
                Downpour.dynamic = false;
            }
        }
        else
        {
            Downpour.dynamic = true;
            switch (self.saveState.deathPersistentSaveData.karma)
            {
                case 0:
                    rainIntensity = UnityEngine.Random.Range(-0.3f, 0.7f);
                    break;
                case 1:
                    rainIntensity = UnityEngine.Random.Range(-0.4f, 0.7f);
                    break;
                case 2:
                    rainIntensity = UnityEngine.Random.Range(-0.5f, 0.7f);
                    break;
            }
            if (self.saveState.deathPersistentSaveData.karma > 2)
            {
                rainIntensity = UnityEngine.Random.Range(-2f, 0.7f);
            }
        }
        switch (Downpour.direction)
        {
            case 0:
                direction = UnityEngine.Random.Range(1, 3);
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
        if (self.game != null && !self.game.IsArenaSession)
        {
            if (Downpour.configLoaded == false)
            {
                if (self.world.region.name == "UW" || self.world.region.name == "SB" || self.world.region.name == "SS")
                {
                    noRain = true;
                }
                else
                {
                    noRain = false;
                }
            }
            else
            {
                if (Downpour.rainRegions.Contains(self.world.region.name))
                {
                    noRain = false;
                }
                else
                {
                    noRain = true;
                }
            }
        }
        else
        {
            noRain = false;
        }
        //Add lightning effect to rooms.
        if (!noRain)
        {
            if (self.game != null && !self.abstractRoom.shelter && Downpour.lightning && self.roomRain != null)
            {
                if (self.roomRain.dangerType == RoomRain.DangerType.Rain && startingIntensity > 0.5f && self.lightning == null)
                {
                    self.lightning = new Lightning(self, 1f, false);
                    self.lightning.bkgOnly = true;
                    self.AddObject(self.lightning);
                }
                if (self.roomRain.dangerType == RoomRain.DangerType.FloodAndRain && startingIntensity > 0.5f && self.lightning == null)
                {
                    self.lightning = new Lightning(self, 1f, false);
                    self.lightning.bkgOnly = true;
                    self.AddObject(self.lightning);
                }
            }
            if (self.game != null)
            {
                self.AddObject(new RainSound(self));
            }
            if (self != null && self.roomRain != null && self.world.rainCycle.TimeUntilRain > 0 && !noRain)
            {
                if (!self.abstractRoom.shelter && self.roomRain.dangerType != RoomRain.DangerType.Flood && rainList.Contains(self.abstractRoom.name) == false)
                {
                    for (int r = 0; r < self.TileWidth; r++)
                    {
                        if (self.Tiles[r, self.TileHeight - 1].Solid)
                        {
                            ceilingCount++;
                        }
                    }
                    if (ceilingCount < (self.Width * 0.95) && !noRain)
                    {
                        self.AddObject(new Preciptator(self, Downpour.snow, ceilingCount));
                        rainList.Add(self.abstractRoom.name);
                    }
                    ceilingCount = 0;
                }
            }
        }
    }
    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig.Invoke(self);
        if (Downpour.debug)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                rainIntensity -= 0.005f;
                if (rainIntensity < 0f)
                {
                    rainIntensity = 0f;
                }
                Debug.Log("Rain Intensity = " + rainIntensity);
            }
            if (Input.GetKey(KeyCode.Alpha2))
            {
                rainIntensity += 0.005f;
                if (rainIntensity > 1f)
                {
                    rainIntensity = 1f;
                }
                Debug.Log("Rain Intensity = " + rainIntensity);
            }
            if (Input.GetKey(KeyCode.Alpha3))
            {
                Debug.Log("Rain Intensity = " + rainIntensity);
            }
            if (Input.GetKey(KeyCode.Alpha4))
            {
                Debug.Log("Rain Disabled: " + noRainThisCycle);
            }
            if (Input.GetKey(KeyCode.Alpha5))
            {
                self.world.rainCycle.timer += 20;
            }

        }
        //Rain intensity increases with cycle duration if in dynamic mode
        if (Downpour.dynamic && !noRainThisCycle)
        {
            if (self.world.rainCycle.RainDarkPalette <= 0)
            {
                rainIntensity = Mathf.Lerp(startingIntensity, 1f, self.world.rainCycle.CycleProgression);
            }
            else
            {
                rainIntensity = Mathf.Lerp(0.95f, 0f, self.world.rainCycle.RainDarkPalette);
            }
        }
        //Flooding tests
        if(self.waterObject != null && rainIntensity > 0.7f)
        {
            //flooding = true;
            //self.waterObject.fWaterLevel = self.waterObject.originalWaterLevel + floodLevel;
        }
        else
        {
            flooding = false;
        }
    }
}

