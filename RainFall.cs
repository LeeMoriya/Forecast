using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;

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
        //Debug
        On.RoomSettings.LoadAmbientSounds += RoomSettings_LoadAmbientSounds;
    }

    private static void StoryGameSession_ctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, int saveStateNumber, RainWorldGame game)
    {
        orig.Invoke(self, saveStateNumber, game);
        Downpour.exposureControllers = new List<ExposureController>();
    }

    private static void RoomSettings_LoadAmbientSounds(On.RoomSettings.orig_LoadAmbientSounds orig, RoomSettings self, string[] s)
    {
        orig.Invoke(self, s);
        if (Downpour.debug)
        {
            for (int i = 0; i < s.Length; i++)
            {
                string[] array = Regex.Split(s[i], "><");
                if (array[0] == "OMNI")
                {
                    Debug.Log("SOUND: " + array[1]);
                }
            }
        }
    }

    private static void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
    {
        if (Downpour.snow)
        {
            return;
        }
        orig.Invoke(self, eu);
    }

    private static void RainCycle_RainHit(On.RainCycle.orig_RainHit orig, RainCycle self)
    {
        if (Downpour.snow)
        {
            return;
        }
        orig.Invoke(self);
    }

    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig.Invoke(self, abstractCreature, world);
        if (Downpour.snow && self.room.game.session is StoryGameSession)
        {
            Downpour.exposureControllers.Add(new ExposureController(self));
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
                        self.realizedRoom.AddObject(new Preciptator(self.realizedRoom, Downpour.snow));
                    }
                }
            }
        }
    }

    private static void RainWorld_LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
    {
        Futile.atlasManager.LoadAtlasFromTexture("snowpile", Downpour.snowPileTex);
        Futile.atlasManager.LoadAtlasFromTexture("logo", Downpour.logo);
        Futile.atlasManager.LoadAtlasFromTexture("logo2", Downpour.logo2);
        Futile.atlasManager.LoadAtlasFromTexture("blizzard", Downpour.blizzardTexture);
        Futile.atlasManager.LoadAtlasFromTexture("overlay1", Downpour.overlay1);
        Futile.atlasManager.LoadAtlasFromTexture("overlay2", Downpour.overlay2);
        orig.Invoke(self);
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
                room.AddObject(new Preciptator(room, Downpour.snow));
            }
            ceilingCount = 0;
        }
    }
    private static void Lightning_ctor(On.Lightning.orig_ctor orig, Lightning self, Room room, float intensity, bool bkgOnly)
    {
        orig.Invoke(self, room, intensity, bkgOnly);
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
        orig.Invoke(self, player);
        rainList.Clear();

        //Rain Chance
        if (UnityEngine.Random.Range(0, 100) < Downpour.rainChance)
        {
            noRainThisCycle = false;
        }
        else
        {
            noRainThisCycle = true;
        }
        //Rain Starting Intensity (Non-Dynamic)
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
                rainIntensity = 0.9f;
                Downpour.dynamic = false;
            }
        }
        //Rain Starting Intensity (Dynamic)
        else
        {
            Downpour.dynamic = true;
            if (!Downpour.snow)
            {
                rainIntensity = UnityEngine.Random.Range(-1.5f, 0.8f);
            }
            else
            {
                rainIntensity = UnityEngine.Random.Range(0.15f, 0.9f);
            }
        }
        //Rain Direction
        switch (Downpour.direction)
        {
            case 0:
                direction = UnityEngine.Random.Range(1, 4);
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
            if (Downpour.rainRegions.Contains(self.world.region.name))
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
        //Add lightning effect to rooms.
        if (!noRain)
        {
            //Add Rain Sound Object
            if (self.game != null)
            {
                self.AddObject(new RainSound(self));
            }
        }
    }
    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig.Invoke(self);
        if (Downpour.snow && self.world.rainCycle.RainDarkPalette > 0f)
        {
            //Update exposure
            if(Downpour.exposureControllers != null & Downpour.exposureControllers.Count > 0)
            {
                for (int i = 0; i < Downpour.exposureControllers.Count; i++)
                {
                    Downpour.exposureControllers[i].Update();
                }
            }
        }
        if (Downpour.debug)
        {
            //Decrease Intensity
            if (Input.GetKey(KeyCode.Alpha1))
            {
                rainIntensity -= 0.005f;
                if (rainIntensity < 0f)
                {
                    rainIntensity = 0f;
                }
                Debug.Log("Rain Intensity = " + rainIntensity);
            }
            //Increase Intensity
            if (Input.GetKey(KeyCode.Alpha2))
            {
                rainIntensity += 0.005f;
                if (rainIntensity > 1f)
                {
                    rainIntensity = 1f;
                }
                Debug.Log("Rain Intensity = " + rainIntensity);
            }
            //Show Room X and Y pos
            if (Input.GetKey(KeyCode.Alpha3))
            {
                if (self.BeingViewed)
                {
                    Debug.Log("---ROOM POSITION---");
                    Debug.Log("X POS: " + self.abstractRoom.mapPos.x.ToString());
                    Debug.Log("Y POS: " + self.abstractRoom.mapPos.y.ToString());
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (self.BeingViewed)
                {
                    Downpour.direction++;
                    if (Downpour.direction > 3)
                    {
                        Downpour.direction = 1;
                    }
                    Debug.Log("Direction: " + Downpour.direction);
                }
            }
            //Fast Forward Cycle Timer
            if (Input.GetKey(KeyCode.Alpha5))
            {
                self.world.rainCycle.timer += 20;
            }

        }
        //Rain intensity increases with cycle duration if in dynamic mode
        if (Downpour.dynamic && !noRainThisCycle && self.BeingViewed)
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
    }
}

