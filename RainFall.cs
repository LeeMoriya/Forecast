using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RainFall
{
    public static void Patch()
    {
        On.Room.Update += Room_Update;
        On.Room.Loaded += Room_Loaded;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
    }

    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);
        rainIntensity = UnityEngine.Random.Range(0f, 1f);
        Debug.Log("Current rain intensity: " + rainIntensity);
    }

    public static List<RainDrop> raindrops = new List<RainDrop>();
    //0 - Light rain, no Lightning
    //1 - Medium rain, no Lightning
    //2 - Heavy rain, Lightning
    public static float rainIntensity;


    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig.Invoke(self);
        if (self.game != null && !self.abstractRoom.shelter)
        {
            if (self.roomRain.dangerType == RoomRain.DangerType.Rain && rainIntensity > 0.5f)
            {
                self.lightning = new Lightning(self, 1f, false);
                self.AddObject(self.lightning);
            }
            if (self.roomRain.dangerType == RoomRain.DangerType.Flood && rainIntensity > 0.5f)
            {
                self.lightning = new Lightning(self, 1f, true);
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
        if (Input.GetKey(KeyCode.F11))
        {
            Debug.Log(raindrops.Count.ToString());
        }
        if (self != null && self.game.session is StoryGameSession && self.fullyLoaded && self.roomRain != null)
        {
            if (self.roomRain.dangerType == RoomRain.DangerType.Rain && rainIntensity > 0.7f)
            {
                if (raindrops.Count < 2000)
                {
                    for (int m = 0; m < 25; m++)
                    {
                        raindrops.Add(new RainDrop(new Vector2(0, 0), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -3f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self));
                        self.AddObject(raindrops[raindrops.Count - 1]);
                    }
                }
                else
                {
                    raindrops.RemoveRange(0, 2000);
                }
            }
            else if(self.roomRain.dangerType == RoomRain.DangerType.Rain && rainIntensity < 0.7f && rainIntensity > 0.5f)
            {
                if (raindrops.Count < 1200)
                {
                    for (int m = 0; m < 18; m++)
                    {
                        raindrops.Add(new RainDrop(new Vector2(0, 0), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -3f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self));
                        self.AddObject(raindrops[raindrops.Count - 1]);
                    }
                }
                else
                {
                    raindrops.RemoveRange(0, 1200);
                }
            }
            else if(self.roomRain.dangerType == RoomRain.DangerType.Rain && rainIntensity < 0.5f && rainIntensity > 0.3f)
            {
                if (raindrops.Count < 600)
                {
                    for (int m = 0; m < 6; m++)
                    {
                        raindrops.Add(new RainDrop(new Vector2(0, 0), new Vector2(UnityEngine.Random.Range(-3f, -0.2f), -3f), self.game.cameras[0].currentPalette.skyColor, 10, 10, self));
                        self.AddObject(raindrops[raindrops.Count - 1]);
                    }
                }
                else
                {
                    raindrops.RemoveRange(0, 600);
                }
            }
        }
    }
}
