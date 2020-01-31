using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RainFall
{
    //Used to limit the number of raindrops that can be present in a room
    public static List<RainDrop> raindrops = new List<RainDrop>();
    public static float rainIntensity;
    public static bool debug = true;
    public static List<string> roomRainList;
    public static float startingIntensity;
    public static float rainAmount = 0;

    public static void Patch()
    {
        On.Room.Update += Room_Update;
        On.Room.Loaded += Room_Loaded;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.RoomRain.Update += RoomRain_Update;
    }
    private static void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
    {
        //RoomRain hook has tweaks to rain sound volumes so its audible during cycles when rain intensity is high enough
        ((Action<bool>)(Activator.CreateInstance(typeof(Action<bool>), self, typeof(UpdatableAndDeletable).GetMethod("Update").MethodHandle.GetFunctionPointer())))(eu);
        if (self.dangerType == RoomRain.DangerType.Rain || self.dangerType == RoomRain.DangerType.FloodAndRain)
        {
            self.intensity = Mathf.Lerp(self.intensity, self.globalRain.Intensity, 0.2f);
        }
        self.intensity = Mathf.Min(self.intensity, self.room.roomSettings.RainIntensity);
        self.visibilitySetter = 0;
        if (self.intensity == 0f && self.lastIntensity > 0f)
        {
            self.visibilitySetter = -1;
        }
        else if (self.intensity > 0f && self.lastIntensity == 0f)
        {
            self.visibilitySetter = 1;
        }
        self.lastIntensity = self.intensity;
        if (self.globalRain.AnyPushAround)
        {
            self.ThrowAroundObjects();
        }
        if (self.bulletDrips.Count < (int)((float)self.room.TileWidth * self.globalRain.bulletRainDensity * self.room.roomSettings.RainIntensity))
        {
            self.bulletDrips.Add(new BulletDrip(self));
            self.room.AddObject(self.bulletDrips[self.bulletDrips.Count - 1]);
        }
        else if (self.bulletDrips.Count > (int)((float)self.room.TileWidth * self.globalRain.bulletRainDensity * self.room.roomSettings.RainIntensity))
        {
            self.bulletDrips[0].Destroy();
            self.bulletDrips.RemoveAt(0);
        }
        if (self.globalRain.flood > 0f)
        {
            if (self.room.waterObject != null)
            {
                self.room.waterObject.fWaterLevel = Mathf.Lerp(self.room.waterObject.fWaterLevel, self.FloodLevel, 0.2f);
                self.room.waterObject.GeneralUpsetSurface(Mathf.InverseLerp(0f, 0.5f, self.globalRain.Intensity) * 4f);
            }
            else if (self.room.roomSettings.DangerType == RoomRain.DangerType.Flood || self.room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain)
            {
                self.room.AddWater();
            }
        }
        if (self.dangerType != RoomRain.DangerType.Flood)
        {
            if (rainIntensity > 0.3f)
            {
                self.normalRainSound.Volume = 1.2f;
            }
            else
            {
                self.normalRainSound.Volume = ((self.intensity <= 0f) ? 0f : (0.1f + 0.9f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0.001f, 0.7f, self.intensity) * 3.14159274f)), 1.5f)));
            }
            self.normalRainSound.Update();
            if (rainIntensity > 0.7f)
            {
                self.heavyRainSound.Volume = 0.3f;
            }
            else
            {
                self.heavyRainSound.Volume = Mathf.Pow(Mathf.InverseLerp(0.12f, 0.5f, self.intensity), 0.85f) * Mathf.Pow(1f - self.deathRainSound.Volume, 0.3f);
            }
            self.heavyRainSound.Update();
        }
        else
        {
            if (rainIntensity > 0.3f)
            {
                self.floodingSound.Volume = 1f;
                self.floodingSound.Update();
            }
        }
        self.deathRainSound.Volume = Mathf.Pow(Mathf.InverseLerp(0.35f, 0.75f, self.intensity), 0.8f);
        self.deathRainSound.Update();
        self.rumbleSound.Volume = self.globalRain.RumbleSound * self.room.roomSettings.RumbleIntensity;
        self.rumbleSound.Update();
        self.distantDeathRainSound.Volume = Mathf.InverseLerp(1400f, 0f, (float)self.room.world.rainCycle.TimeUntilRain) * self.room.roomSettings.RainIntensity;
        self.distantDeathRainSound.Update();
        if (self.dangerType != RoomRain.DangerType.Rain && rainIntensity < 0.3f)
        {
            self.floodingSound.Volume = Mathf.InverseLerp(0.01f, 0.5f, self.globalRain.floodSpeed);
            self.floodingSound.Update();
        }
        if (self.room.game.cameras[0].room == self.room)
        {
            self.SCREENSHAKESOUND.Volume = self.room.game.cameras[0].ScreenShake * (1f - self.rumbleSound.Volume);
        }
        else
        {
            self.SCREENSHAKESOUND.Volume = 0f;
        }
        self.SCREENSHAKESOUND.Update();
    }
    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        //Starting rain intensity determined at the start of the cycle
        orig.Invoke(self, player);
        roomRainList = new List<string>();
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
        //Add lightning effect to rooms.
        if (self.game != null && !self.abstractRoom.shelter && Downpour.lightning)
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
    }
    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig.Invoke(self);
        rainAmount = Mathf.Lerp(0, 60, rainIntensity);
        Player player = (self.game.Players.Count <= 0) ? null : (self.game.Players[0].realizedCreature as Player);
        if (debug)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                rainIntensity -= 0.001f;
                Debug.Log(rainIntensity);
            }
            if (Input.GetKey(KeyCode.Alpha2))
            {
                rainIntensity += 0.001f;
                Debug.Log(rainIntensity);
            }
            if (Input.GetKey(KeyCode.Alpha3))
            {
                Debug.Log(rainIntensity.ToString() + " & " + rainAmount.ToString());
            }
        }
        if (self != null && self.game.session is StoryGameSession && self.fullyLoaded && self.roomRain != null && self.world.rainCycle.TimeUntilRain > 0)
        {
            //No rainfall in UW (above clouds), SS (interior) or SB (all underground)
            if (self.world.region.name != "UW" || self.world.region.name != "SS" || self.world.region.name != "SB")
            {
                if (self.roomRain.dangerType == RoomRain.DangerType.Rain || self.roomRain.dangerType == RoomRain.DangerType.FloodAndRain && rainIntensity > 0.3f)
                {
                    //Rain intensity increases with cycle duration if in dynamic mode
                    if (startingIntensity > 0.3f && Downpour.dynamic)
                    {
                        rainIntensity = Mathf.Lerp(startingIntensity, 1f, self.world.rainCycle.CycleProgression);
                    }
                    //Add rain that is mid-fall when entering a room to mask the start of the rainfall
                    if (self != null && self.BeingViewed && self.abstractRoom.shelter == false && roomRainList.Contains(self.abstractRoom.name) == false)
                    {
                        for (int m = 0; m < (int)rainAmount * 7; m++)
                        {
                            self.AddObject(new RainDrop(new Vector2(UnityEngine.Random.Range(self.RoomRect.left, self.RoomRect.right), UnityEngine.Random.Range(self.RoomRect.bottom+100f, self.RoomRect.top+100f)), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -10f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self, true));
                            roomRainList.Add(self.abstractRoom.name);
                        }
                    }
                    if (!self.BeingViewed && roomRainList.Contains(self.abstractRoom.name))
                    {
                        roomRainList.Remove(self.abstractRoom.name);
                    }
                    //Rainfall follows player pos
                    if (player != null && player.inShortcut == false)
                    {
                        if (raindrops.Count < 2000)
                        {
                            for (int m = 0; m < (int)rainAmount; m++)
                            {
                                raindrops.Add(new RainDrop(new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1400f, player.mainBodyChunk.pos.x + 1400f), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -3f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self, false));
                                self.AddObject(raindrops[raindrops.Count - 1]);
                            }
                        }
                        else
                        {
                            raindrops.RemoveRange(0, 2000);
                        }
                    }
                    //rainfall randomly placed along room length when player pos is unavailable
                    else
                    {
                        if (raindrops.Count < 2000)
                        {
                            for (int m = 0; m < (int)rainAmount; m++)
                            {
                                raindrops.Add(new RainDrop(new Vector2(UnityEngine.Random.Range(self.RoomRect.left, self.RoomRect.right), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -3f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self, false));
                                self.AddObject(raindrops[raindrops.Count - 1]);
                            }
                        }
                        else
                        {
                            raindrops.RemoveRange(0, 2000);
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

