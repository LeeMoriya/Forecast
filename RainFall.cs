using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RainFall
{

    public static List<RainDrop> raindrops = new List<RainDrop>();
    public static float rainIntensity;
    public static bool dynamic;
    public static bool debug = true;

    public static void Patch()
    {
        On.Room.Update += Room_Update;
        On.Room.Loaded += Room_Loaded;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.Player.Update += Player_Update;
        On.RoomRain.Update += RoomRain_Update;
    }

    private static void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
    {
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
        self.deathRainSound.Volume = Mathf.Pow(Mathf.InverseLerp(0.35f, 0.75f, self.intensity), 0.8f);
        self.deathRainSound.Update();
        self.rumbleSound.Volume = self.globalRain.RumbleSound * self.room.roomSettings.RumbleIntensity;
        self.rumbleSound.Update();
        self.distantDeathRainSound.Volume = Mathf.InverseLerp(1400f, 0f, (float)self.room.world.rainCycle.TimeUntilRain) * self.room.roomSettings.RainIntensity;
        self.distantDeathRainSound.Update();
        if (self.dangerType != RoomRain.DangerType.Rain)
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


    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig.Invoke(self, eu);
        if (self.room.abstractRoom.firstTimeRealized)
        {

        }
    }

    public static int intensity = 0;
    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);
        if (intensity == 1)
        {
            rainIntensity = 0.31f;
        }
        if (intensity == 2)
        {
            rainIntensity = 0.6f;
        }
        if (intensity == 3)
        {
            rainIntensity = 1f;
        }
        else
        {
            switch (self.saveState.deathPersistentSaveData.karma)
            {
                case 0:
                    rainIntensity = UnityEngine.Random.Range(0.7f, 1f);
                    break;
                case 1:
                    rainIntensity = UnityEngine.Random.Range(0.5f, 1f);
                    break;
                case 2:
                    rainIntensity = UnityEngine.Random.Range(0.3f, 1f);
                    break;
            }
            if (self.saveState.deathPersistentSaveData.karma > 2)
            {
                rainIntensity = UnityEngine.Random.Range(0f, 1f);
            }
        }
        Debug.Log("Current rain intensity: " + rainIntensity);
    }
    public static bool lightning = true;
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig.Invoke(self);
        if (self.game != null && !self.abstractRoom.shelter && lightning)
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
        float rainAmount = Mathf.Lerp(0, 60, rainIntensity);
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
            if (self.world.region.name != "UW" || self.world.region.name != "SS" || self.world.region.name != "SB")
            {
                if (self.roomRain.dangerType == RoomRain.DangerType.Rain || self.roomRain.dangerType == RoomRain.DangerType.FloodAndRain && rainIntensity > 0.3f)
                {
                    if (player != null && player.inShortcut == false)
                    {
                        if (raindrops.Count < 2000)
                        {
                            for (int m = 0; m < (int)rainAmount; m++)
                            {
                                raindrops.Add(new RainDrop(new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1400f, player.mainBodyChunk.pos.x + 1400f), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -3f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self));
                                self.AddObject(raindrops[raindrops.Count - 1]);
                            }
                        }
                        else
                        {
                            raindrops.RemoveRange(0, 2000);
                        }
                    }
                    else
                    {
                        if (raindrops.Count < 2000)
                        {
                            for (int m = 0; m < (int)rainAmount; m++)
                            {
                                raindrops.Add(new RainDrop(new Vector2(UnityEngine.Random.Range(self.RoomRect.left, self.RoomRect.right), self.RoomRect.top + 200f), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -3f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self));
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
        else if(self.world.rainCycle.TimeUntilRain < 0)
        {
            rainIntensity = 0f;
        }
    }
}

