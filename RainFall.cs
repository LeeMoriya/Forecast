﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
    public static List<Vector2> spawnPositions = new List<Vector2>();

    public static void Patch()
    {
        On.Room.Update += Room_Update;
        On.Room.Loaded += Room_Loaded;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.Lightning.ctor += Lightning_ctor;
        On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
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
        if (!room.game.IsArenaSession && room.world.region.name == "UW")
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
                    rainIntensity = UnityEngine.Random.Range(0.6f, 0.7f);
                    break;
                case 1:
                    rainIntensity = UnityEngine.Random.Range(0.45f, 0.7f);
                    break;
                case 2:
                    rainIntensity = UnityEngine.Random.Range(0.3f, 0.7f);
                    break;
            }
            if (self.saveState.deathPersistentSaveData.karma > 2)
            {
                rainIntensity = UnityEngine.Random.Range(0f, 0.7f);
            }
        }
        startingIntensity = rainIntensity;
        Debug.Log("Current rain intensity: " + rainIntensity);
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
                if (self.roomRain.dangerType == RoomRain.DangerType.Rain && rainIntensity > 0.5f)
                {
                    self.lightning = new Lightning(self, 1f, false);
                    self.AddObject(self.lightning);
                }
                if (self.roomRain.dangerType == RoomRain.DangerType.FloodAndRain && rainIntensity > 0.5f)
                {
                    self.lightning = new Lightning(self, 1f, false);
                    self.AddObject(self.lightning);
                }
            }
            if (self.game != null)
            {
                self.AddObject(new RainSound(self));
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
                if(rainIntensity < 0f)
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
        }
        if (Downpour.rainAmount == 0)
        {
            rainAmount = Mathf.Lerp(0, 60, rainIntensity);
        }
        else
        {
            rainAmount = Mathf.Lerp(0, Downpour.rainAmount, rainIntensity);
        }
        Player player = (self.game.Players.Count <= 0) ? null : (self.game.Players[0].realizedCreature as Player);
        //Rain intensity increases with cycle duration if in dynamic mode
        if (startingIntensity > 0.3f && Downpour.dynamic)
        {
            rainIntensity = Mathf.Lerp(startingIntensity, 1f, self.world.rainCycle.CycleProgression);
        }
        if (self != null && self.roomRain != null && self.world.rainCycle.TimeUntilRain > 0 && !noRain)
        {
            if (!self.abstractRoom.shelter && self.roomRain.dangerType != RoomRain.DangerType.Flood)
            {
                //Count the top row of tiles in a room, if a certain percentage are air, add the room to list of rooms that can contain rain.
                for (int r = 0; r < self.TileWidth; r++)
                {
                    if (self.Tiles[r, self.TileHeight - 1].Solid)
                    {
                        spawnPositions.Add(RWCustom.Custom.IntVector2ToVector2(new RWCustom.IntVector2(self.Tiles[r, self.TileHeight - 1].X, self.Tiles[r, self.TileHeight - 1].Y)));
                        ceilingCount++;
                    }
                }
                if (ceilingCount < (self.Width * 0.90) && !noRain)
                {
                    rainList.Add(self.abstractRoom.name);
                }
                ceilingCount = 0;
            }
            //If the room is present in the list of rooms that can contain rain, spawn rainfall
            if (rainList.Contains(self.abstractRoom.name) && rainIntensity > 0.3f)
            {
                //Rainfall follows player pos
                if (player != null && player.inShortcut == false)
                {
                    if (!Downpour.snow)
                    {
                        for (int m = 0; m < (int)rainAmount; m++)
                        {
                            self.AddObject(new RainDrop(new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1400f, player.mainBodyChunk.pos.x + 1400f), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-1f, 1f), -20f), Color.Lerp(self.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.12f), 10, 10, self, false, rainIntensity));
                        }
                    }
                    else
                    {
                        for (int m = 0; m < (int)rainAmount * 0.6f; m++)
                        {
                            self.AddObject(new RainDrop(new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1400f, player.mainBodyChunk.pos.x + 1400f), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-1f, 1f), -20f), Color.Lerp(self.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.12f), 10, 10, self, true, rainIntensity));
                        }
                    }
                }
                //Rainfall randomly placed
                else
                {
                    if (!Downpour.snow)
                    {
                        for (int m = 0; m < (int)rainAmount; m++)
                        {
                            self.AddObject(new RainDrop(new Vector2(UnityEngine.Random.Range(self.RoomRect.left - 100f, self.RoomRect.right + 100f), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -20f), Color.Lerp(self.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.12f), 10, 10, self, false, rainIntensity));
                        }
                    }
                    else
                    {
                        for (int m = 0; m < (int)rainAmount * 0.5f; m++)
                        {
                            self.AddObject(new RainDrop(new Vector2(UnityEngine.Random.Range(self.RoomRect.left - 100f, self.RoomRect.right + 100f), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -20f), Color.Lerp(self.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.12f), 10, 10, self, true, rainIntensity));
                        }
                    }
                }
            }
        }
        //Stop rainfall at the end of a cycle so bullet rain is visible
        else if (self.world.rainCycle.TimeUntilRain < 0)
        {
            rainIntensity = 0f;
        }
    }
}

