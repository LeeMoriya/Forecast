using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

public class WeatherController : UpdatableAndDeletable
{
    public bool disabled;

    public int rainDrops;
    public int snowFlakes;

    public float rainAmount;
    public int rainLimit;

    public int ceilingCount;

    public List<Vector2> skyreach;
    public Vector2 camPos;
    public List<Vector2> camSkyreach;
    public List<IntVector2> ceilingTiles;
    public List<IntVector2> groundTiles;
    public List<Vector2> surfaceTiles;

    public Blizzard blizzard;
    public WeatherSettings settings;

    public Texture2D origPaletteTexture;
    public Texture2D origFadePalA;
    public Texture2D origFadePalB;

    public bool exportTexture = false;

    public bool interior = false;

    public Color strikeColor = new Color(0f, 1f, 0f);
    public float lightningCounter;

    public WeatherController(Room weatherRoom)
    {
        this.room = weatherRoom;
        ForecastLog.Log($"WeatherController added to {room.abstractRoom.name}");
        if (room.game.IsStorySession)
        {
            settings = new WeatherSettings(ForecastConfig.regionSettings[room.world.region.name] == 1 ? "GLOBAL" : room.world.region.name, room.abstractRoom.name, this);
        }
        else
        {
            settings = new WeatherSettings("GLOBAL", room.abstractRoom.name, this);
        }
        WeatherHooks.roomSettings.Add(room, settings);

        Setup();
    }

    public void Setup()
    {
        skyreach = new List<Vector2>();
        camSkyreach = new List<Vector2>();
        ceilingTiles = new List<IntVector2>();
        groundTiles = new List<IntVector2>();
        surfaceTiles = new List<Vector2>();
        rainDrops = 0;
        snowFlakes = 0;
        ceilingCount = 0;

        if (!room.abstractRoom.gate)
        {
            for (int r = 0; r < room.TileWidth; r++)
            {
                if (room.Tiles[r, room.TileHeight - 1].Solid)
                {
                    ceilingCount++;
                }
            }
            //Gather list of open ceiling tiles for spawning particles
            if (ceilingCount < (room.Width * 0.95f))
            {
                for (int i = 0; i < room.TileWidth; i++)
                {
                    //Add every open air tile at the top of the room to a list
                    int j = room.TileHeight - 1;
                    if (room.GetTile(i, j).Terrain != Room.Tile.TerrainType.Solid && room.GetTile(i, j - 1).Terrain != Room.Tile.TerrainType.Solid && j > room.defaultWaterLevel)
                    {
                        ceilingTiles.Add(new IntVector2(i, j));
                        //Check each tile below this one until it hits something solid
                        for (int t = j - 1; t > 0; t--)
                        {
                            //If this tile is solid or is below the water level, add it to the list
                            Room.Tile tile = room.GetTile(i, t);
                            if (tile.Terrain == Room.Tile.TerrainType.Solid || t < room.defaultWaterLevel)
                            {
                                groundTiles.Add(new IntVector2(i, t));
                                surfaceTiles.Add(room.MiddleOfTile(i, t));
                                break;
                            }
                            else if (tile.Terrain == Room.Tile.TerrainType.Slope || tile.Terrain == Room.Tile.TerrainType.Floor)
                            {
                                surfaceTiles.Add(room.MiddleOfTile(i, t));
                            }
                            //If there are no solid tiles below this one, add a position for the bottom of the room
                            if (t == 0)
                            {
                                groundTiles.Add(new IntVector2(i, 0));
                                break;
                            }
                        }
                    }
                }
                foreach (Room.Tile tile in room.Tiles)
                {
                    if ((tile.Solid && room.GetTile(tile.X, tile.Y + 1).Terrain == Room.Tile.TerrainType.Air && room.GetTile(tile.X, tile.Y + 2).Terrain == Room.Tile.TerrainType.Air) ||
                        tile.Terrain == Room.Tile.TerrainType.Slope && room.GetTile(tile.X, tile.Y + 1).Terrain == Room.Tile.TerrainType.Air && room.GetTile(tile.X, tile.Y + 2).Terrain == Room.Tile.TerrainType.Air)
                    {
                        skyreach.Add(room.MiddleOfTile(tile.X, tile.Y - 1));
                        //Add snow decals to surfaces
                        if (settings.currentWeather.weatherIndex == 2 && ForecastMod.decals)
                        {
                            if (UnityEngine.Random.value > 0.8f)
                            {
                                room.AddObject(new SnowPile(room.MiddleOfTile(tile.X, tile.Y - 1), UnityEngine.Random.Range(60f, 80f)));
                            }
                            room.AddObject(new SnowPile(room.MiddleOfTile(tile.X, tile.Y), UnityEngine.Random.Range(20f, 45f)));
                        }
                    }
                }
            }
            else
            {
                interior = true;
            }
        }
        else
        {
            interior = true;
        }
        if (settings.currentWeather != null)
        {
            if (settings.currentWeather.type == WeatherForecast.Weather.WeatherType.Fog) //TODO - Maybe implement fog palette manipulation discovered in PaletteTweaker
            {
                RoomSettings.RoomEffect fog = room.roomSettings.effects.Find(x => x.type == RoomSettings.RoomEffect.Type.Fog);
                if (fog != null && !interior)
                {
                    fog.amount = Mathf.Clamp(1f, 0f, room.roomSettings.RainIntensity);
                }
                else
                {
                    room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Fog, 1f, false));
                }
            }
        }
        //Rain
        if (settings.weatherType == 0 && settings.currentWeather.type != WeatherForecast.Weather.WeatherType.Fog)
        {
            if (settings.rainVolume)
            {
                room.AddObject(new RainSound(room, this));
            }
        }
        //Snow
        if (settings.weatherType == 2)
        {
            room.game.cameras[0].LoadPalette(room.roomSettings.Palette, ref origFadePalA);
            if (room.roomSettings.fadePalette != null && room.roomSettings.fadePalette.palette > -1)
            {
                room.game.cameras[0].LoadPalette(room.roomSettings.fadePalette.palette, ref origFadePalB);
            }

            if (!interior && ForecastConfig.snowSources.Value)
            {
                room.AddObject(new SnowPlacer(this));
            }

            //If MSC is disabled or Blizzard is turned off -- Switch danger type to AerieBlizzard
            if (!ForecastConfig.classicSnow.Value)
            {
                if (ModManager.MSC || ModManager.Watcher)
                {
                    room.roomSettings.DangerType = DLCSharedEnums.RoomRainDangerType.Blizzard;
                }
                else
                {
                    room.roomSettings.DangerType = RoomRain.DangerType.AerieBlizzard;
                }
            }
        }
    }

    public void AddRaindrops(int rainDropsToSpawn)
    {
        if (room != null && skyreach != null && skyreach.Count > 0)
        {
            for (int i = 0; i < rainDropsToSpawn; i++)
            {
                Vector2 rng = skyreach[UnityEngine.Random.Range(0, skyreach.Count)];
                rng += new Vector2(0f, UnityEngine.Random.Range(0f, room.game.cameras[0].pos.y - rng.y));
                RainDrop rainDrop = new RainDrop(rng, Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), settings.currentIntensity, this);
                room.AddObject(rainDrop);
                rainDrops++;
            }
        }
    }

    public void AddHail(int rainDropsToSpawn)
    {
        if (room != null && skyreach != null && skyreach.Count > 0)
        {
            for (int i = 0; i < rainDropsToSpawn; i++)
            {
                Vector2 rng = skyreach[UnityEngine.Random.Range(0, skyreach.Count)];
                rng += new Vector2(0f, UnityEngine.Random.Range(0f, room.game.cameras[0].pos.y - rng.y));
                Hail rainDrop = new Hail(rng, settings.currentIntensity, this);
                room.AddObject(rainDrop);
                rainDrops++;
            }
        }
    }

    public void AddSnowflakes(int snowFlakesToSpawn)
    {
        if (room != null)
        {
            if (camPos != null)
            {
                for (int i = 0; i < snowFlakesToSpawn; i++)
                {
                    try
                    {
                        //Get a random position within range of the RoomCamera
                        Vector2 cam = camPos;
                        IntVector2 randomOffset = IntVector2.FromVector2(new Vector2(cam.x + UnityEngine.Random.Range(-700, 700), cam.y + UnityEngine.Random.Range(-500, 500)));
                        Vector2 offset2 = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
                        //If that random position has line of sight with the sky, spawn a snowflake there
                        Vector2 spawn = randomOffset.ToVector2();
                        Vector2 spawnPos = spawn + offset2;
                        if (RayTraceSky(spawnPos, new Vector2(0f, 1f)))
                        {
                            ForecastLog.LogOnce("Snow3");

                            SnowFlake snowFlake = new SnowFlake(spawnPos, Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), settings.currentIntensity, this);
                            room.AddObject(snowFlake);
                            //snowFlake.reset = true;
                            for (int s = 0; s < 1000; s++)
                            {
                                snowFlake.Update(true);
                            }
                            snowFlakes++;
                        }
                    }
                    catch
                    {
                        ForecastLog.Log("ERROR SPAWNING SNOWFLAKE");
                    }
                }
            }
        }
    }

    public bool RayTraceSky(Vector2 pos, Vector2 testDir)
    {
        Vector2 corner = Custom.RectCollision(pos, pos + testDir * 100000f, room.RoomRect).GetCorner(FloatRect.CornerLabel.D);
        if (SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, pos, corner) != null)
        {
            return false;
        }
        if (corner.y >= room.PixelHeight - 5f)
        {
            return true;
        }
        return false;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (disabled || WeatherForecast.weatherlessRegions.Contains(settings.regionName))
        {
            if (!disabled)
            {
                settings.Update(); //One-time update to display settings
                disabled = true;
            }
            return;
        }
        if (settings.weatherType == 2)
        {
            if ((ModManager.MSC || ModManager.Watcher) && room.fullyLoaded && room.roomSettings.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard)
            {
                if (room.roomRain != null)
                {
                    room.roomRain.slatedForDeletetion = true;
                    room.RemoveObject(room.roomRain);
                    room.roomRain.Destroy();
                    room.roomRain = null;
                    ForecastLog.Log("Removed RoomRain");
                }
            }
        }


        settings.Update();

        if (room.BeingViewed)
        {
            camPos = room.game.cameras[0].pos + new Vector2(room.game.rainWorld.screenSize.x / 2, room.game.rainWorld.screenSize.y / 2);
        }
        //Particle limit
        if (settings.weatherType == 0 || (settings.weatherType == 2 && ForecastConfig.classicSnow.Value))
        {
            rainAmount = Mathf.Lerp(0, settings.particleLimit, settings.currentIntensity);
            if (settings.weatherType == 0)
            {
                rainLimit = (int)Mathf.Lerp(0, Mathf.Lerp(0f, (rainAmount * 9), room.roomSettings.RainIntensity), settings.currentIntensity);
            }
            if (settings.weatherType == 2 && ForecastConfig.classicSnow.Value)
            {
                rainLimit = (int)Mathf.Lerp(0, Mathf.Lerp(0f, (rainAmount * 9), room.roomSettings.RainIntensity), settings.currentIntensity) * 5;
            }
        }
        //Cloud cover
        if (settings.dynamicClouds)
        {
            room.roomSettings.Clouds = Mathf.Lerp(settings.startingIntensity, 1f, room.world.rainCycle.CycleProgression); //Cloud cover should apply everywhere
        }
        else
        {
            room.roomSettings.Clouds = settings.cloudCover;
        }
        //Snowy weather - making it look the same between AerieBlizzard and normal Blizzard
        if (settings.currentWeather.weatherIndex == 2)
        {
            if (room.game.cameras[0].blizzardGraphics != null && !ForecastConfig.classicSnow.Value)
            {
                room.game.cameras[0].blizzardGraphics.oldSnowFallIntensity = settings.currentIntensity;
                room.game.cameras[0].blizzardGraphics.snowfallIntensity = settings.currentIntensity;
                room.game.cameras[0].blizzardGraphics.oldBlizzardIntensity = settings.currentIntensity; //Water glitches
                room.game.cameras[0].blizzardGraphics.blizzardIntensity = settings.currentIntensity; //Watcher glitche
                room.game.cameras[0].blizzardGraphics.oldWindStrength = settings.currentIntensity;
                room.game.cameras[0].blizzardGraphics.windStrength = settings.currentIntensity;
                room.game.cameras[0].blizzardGraphics.oldWindAngle = room.game.cameras[0].blizzardGraphics.windAngle;
                room.game.cameras[0].blizzardGraphics.windAngle = Mathf.Lerp(Mathf.Lerp(room.game.cameras[0].blizzardGraphics.WindAngle, Mathf.Sin((float)room.world.rainCycle.TimeUntilRain / 900f) * 0.64f, 0.1f) * settings.currentIntensity, Mathf.Sign(Mathf.Sin((float)room.world.rainCycle.TimeUntilRain / 900f) * 0.64f * 1.2f), 0.2f * (0f - Mathf.Abs(Mathf.Lerp(room.game.cameras[0].blizzardGraphics.WindAngle, Mathf.Sin((float)room.world.rainCycle.TimeUntilRain / 900f) * 0.64f, 0.1f) * settings.currentIntensity))) * Mathf.Lerp(0f, 0.75f, room.world.rainCycle.CycleProgression * 3f);
                room.game.cameras[0].blizzardGraphics.oldWhiteOut = Mathf.Pow(settings.currentIntensity, 1.3f) * settings.currentIntensity;
                room.game.cameras[0].blizzardGraphics.whiteOut = Mathf.Pow(settings.currentIntensity, 1.3f) * settings.currentIntensity;
            }
            else
            {
                if (!interior && (room.world.rainCycle.timer - room.world.rainCycle.cycleLength) / 2400f > -0.5f && blizzard == null)
                {
                    blizzard = new Blizzard(this);
                    room.AddObject(blizzard);
                }
            }
            //Apply snowy palette
            if (room.BeingViewed)
            {
                ApplyPalette();
            }
        }

        if (!interior && room.game != null && room != null && !room.abstractRoom.gate && room.ReadyForPlayer)
        {
            //Add background lightning flashes
            if ((ForecastConfig.strikeWeathers.Value == 0 && settings.currentWeather.type == WeatherForecast.Weather.WeatherType.Thunderstorm) || (ForecastConfig.strikeWeathers.Value == 1 && (settings.currentWeather.type == WeatherForecast.Weather.WeatherType.Thunderstorm || settings.currentWeather.type == WeatherForecast.Weather.WeatherType.Blizzard)) || ForecastConfig.strikeWeathers.Value == 2)
            {
                if (settings.currentIntensity > 0.7f || ForecastConfig.strikeWeathers.Value == 2)
                {
                    if (room.game != null && !room.abstractRoom.shelter && settings.backgroundLightning)
                    {
                        if (room.lightning == null)
                        {
                            ForecastLog.Log($"Adding lightning to room {room.abstractRoom.name}");
                            room.lightning = new Lightning(room, 1f, false);
                            room.lightning.bkgOnly = true;
                            room.lightning.bkgGradient[0] = room.game.cameras[0].currentPalette.skyColor;
                            room.lightning.bkgGradient[1] = Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), settings.currentIntensity);
                            room.AddObject(room.lightning);
                        }
                    }
                    //Generate lightning strikes
                    if (settings.lightningStrikes && room.BeingViewed)
                    {
                        lightningCounter += 0.025f;
                        if (lightningCounter >= settings.lightningInterval)
                        {
                            lightningCounter = 0;
                            if (UnityEngine.Random.Range(0, 100) <= settings.lightningChance)
                            {
                                if (settings.greenLightning && room.game.IsStorySession)
                                {
                                    List<string> greenRegions = new List<string>()
                                {
                                    "UW","SH","RM","LM"
                                };
                                    if (greenRegions.Contains(room.world.name))
                                    {
                                        room.AddObject(new LightningStrike(this, new Color(0f, 1f, 0f)));
                                    }
                                    else
                                    {
                                        room.AddObject(new LightningStrike(this, settings.strikeColor));
                                    }
                                }
                                else
                                {
                                    room.AddObject(new LightningStrike(this, settings.strikeColor));
                                }
                            }
                        }
                    }
                }
            }
            //Add rain particles
            if (settings.weatherType == 0)
            {
                snowFlakes = 0;
                if (rainDrops < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddRaindrops(rainLimit - rainDrops);
                }
            }
            if (settings.weatherType == 2 && ForecastConfig.classicSnow.Value)
            {
                rainDrops = 0;
                if (snowFlakes < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddSnowflakes(rainLimit - snowFlakes);
                }
            }
        }

        //Quick reload palette
        if (ForecastConfig.debugMode.Value && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha5))
        {
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
            exportTexture = true;
        }
    }


    public void ApplyPalette()
    {
        if (origFadePalA == null || ForecastMod.snowExt == null || ForecastMod.snowInt == null)
        {
            return;
        }

        float darkness = room.game.cameras[0].PaletteDarkness();
        Color[] snowPixels = darkness > 0.6f ? ForecastMod.snowInt.GetPixels() : ForecastMod.snowExt.GetPixels();

        float fadePercent = Mathf.Lerp(settings.currentIntensity, 0f, Mathf.InverseLerp(0f, 0.9f, darkness));
        //ForecastLog.Log($"{room.abstractRoom.name} Darkness: {darkness} : FadePercent: {fadePercent}");

        //Fade Tex A
        Texture2D newFadeA = new Texture2D(origFadePalA.width, origFadePalA.height, TextureFormat.ARGB32, false);
        Color[] newAPixels = origFadePalA.GetPixels();
        for (int i = 0; i < newAPixels.Length; i++)
        {
            if (interior)
            {
                //Desaturate
                newAPixels[i] = Custom.Desaturate(newAPixels[i], settings.currentIntensity);
            }
            else
            {
                //Screen Blending
                Color invertA = new Color(1f - newAPixels[i].r, 1f - newAPixels[i].g, 1f - newAPixels[i].b, newAPixels[i].a);
                Color invertB = new Color(1f - snowPixels[i].r, 1f - snowPixels[i].g, 1f - snowPixels[i].b, snowPixels[i].a);

                Color blend = invertA * invertB;
                Color result = new Color(1f - blend.r, 1f - blend.g, 1f - blend.b, blend.a);

                newAPixels[i] = Color.Lerp(newAPixels[i], result, fadePercent);
            }
        }
        newFadeA.SetPixels(newAPixels);
        newFadeA.Apply(false);
        room.game.cameras[0].fadeTexA = newFadeA;
        room.game.cameras[0].ApplyEffectColorsToPaletteTexture(ref room.game.cameras[0].fadeTexA, room.roomSettings.EffectColorA, room.roomSettings.EffectColorB);

        //Fade Tex B
        if (room.game.cameras[0].paletteB > -1 && origFadePalB != null)
        {
            Texture2D newFadeB = new Texture2D(origFadePalB.width, origFadePalB.height, TextureFormat.ARGB32, false);
            Color[] newBPixels = origFadePalB.GetPixels();
            for (int i = 0; i < newBPixels.Length; i++)
            {
                if (interior)
                {
                    //Desaturate
                    newBPixels[i] = Custom.Desaturate(newBPixels[i], settings.currentIntensity);
                }
                else
                {
                    //Screen Blending
                    Color invertA = new Color(1f - newBPixels[i].r, 1f - newBPixels[i].g, 1f - newBPixels[i].b, newBPixels[i].a);
                    Color invertB = new Color(1f - snowPixels[i].r, 1f - snowPixels[i].g, 1f - snowPixels[i].b, snowPixels[i].a);

                    Color blend = invertA * invertB;
                    Color result = new Color(1f - blend.r, 1f - blend.g, 1f - blend.b, blend.a);

                    newBPixels[i] = Color.Lerp(newBPixels[i], result, fadePercent);
                }
            }
            newFadeB.SetPixels(newBPixels);
            newFadeB.Apply(false);
            room.game.cameras[0].fadeTexB = newFadeB;
            room.game.cameras[0].ApplyEffectColorsToPaletteTexture(ref room.game.cameras[0].fadeTexB, room.roomSettings.EffectColorA, room.roomSettings.EffectColorB);
        }

        room.game.cameras[0].ApplyFade();

        //Effect Colors - It looks kinda bad but it works so its probably fine :)
        Color[] palCols = room.game.cameras[0].paletteTexture.GetPixels();

        //Effect Color 1
        palCols[190] = Custom.Desaturate(palCols[190], fadePercent);
        palCols[191] = Custom.Desaturate(palCols[191], fadePercent);
        palCols[158] = Custom.Desaturate(palCols[158], fadePercent);
        palCols[159] = Custom.Desaturate(palCols[159], fadePercent);

        //palCols[190] = Color.Lerp(palCols[190], Color.white, fadePercent);
        //palCols[191] = Color.Lerp(palCols[191], Color.white, fadePercent);
        //palCols[158] = Color.Lerp(palCols[158], Color.white, fadePercent);
        //palCols[159] = Color.Lerp(palCols[159], Color.white, fadePercent);

        //Effect Color 2
        palCols[126] = Custom.Desaturate(palCols[126], fadePercent);
        palCols[127] = Custom.Desaturate(palCols[127], fadePercent);
        palCols[94] = Custom.Desaturate(palCols[94], fadePercent);
        palCols[95] = Custom.Desaturate(palCols[95], fadePercent);

        //palCols[126] = Color.Lerp(palCols[126], Color.white, fadePercent);
        //palCols[127] = Color.Lerp(palCols[127], Color.white, fadePercent);
        //palCols[94] = Color.Lerp(palCols[94], Color.white, fadePercent);
        //palCols[95] = Color.Lerp(palCols[95], Color.white, fadePercent);

        room.game.cameras[0].paletteTexture.SetPixels(palCols);
        room.game.cameras[0].paletteTexture.Apply(false);

        if (exportTexture)
        {
            exportTexture = false;
            File.WriteAllBytes($"{Custom.rootFolderDirectory}\\FullPalette.png", room.game.cameras[0].paletteTexture.EncodeToPNG());
        }
    }


    public class WeatherSettings
    {
        public int reloadDelay;
        public string regionName;
        public string roomName;

        public WeatherController owner;
        public WeatherForecast.Weather currentWeather;
        public int weatherType;
        public int weatherIntensity;
        public float startingIntensity;
        public float currentIntensity;
        public float lastIntensity;
        public float fixedIntensity;
        public bool dynamic;
        public int weatherChance;
        public int windDirection;
        public int particleLimit;
        public bool backgroundCollision;
        public bool waterCollision;
        public bool dynamicClouds;
        public float cloudCover;
        public bool backgroundLightning;
        public bool rainVolume;
        public bool lightningStrikes;
        public bool greenLightning;
        public int lightningInterval;
        public int lightningChance;
        public int strikeDamageType;
        public Color strikeColor;
        public List<string> globalTags;
        public List<string> roomTags;
        //Debug HUD
        public FLabel debug;
        public bool show;

        public WeatherSettings(string region, string room, WeatherController owner)
        {
            this.owner = owner;
            regionName = region;
            roomName = room;
            Setup();
        }

        public void Setup()
        {
            //Apply generic settings
            weatherType = ForecastConfig.weatherType.Value;
            //Weather override
            weatherType = 2;

            //Probably only need this if forecasts are off
            weatherIntensity = ForecastConfig.weatherIntensity.Value;
            weatherChance = ForecastConfig.weatherChance.Value;
            windDirection = ForecastConfig.windDirection.Value;
            particleLimit = ForecastConfig.particleLimit.Value;
            backgroundCollision = ForecastConfig.backgroundCollision.Value;
            waterCollision = ForecastConfig.waterCollision.Value;
            backgroundLightning = ForecastConfig.backgroundLightning.Value;
            dynamicClouds = ForecastConfig.dynamicClouds.Value;
            cloudCover = ForecastConfig.cloudCover.Value;
            rainVolume = ForecastConfig.rainVolume.Value;
            lightningInterval = ForecastConfig.lightningInterval.Value;
            lightningChance = ForecastConfig.lightningChance.Value;
            lightningStrikes = ForecastConfig.lightningStrikes.Value;
            strikeDamageType = ForecastConfig.strikeDamageType.Value;
            strikeColor = ForecastConfig.strikeColor.Value;
            greenLightning = ForecastConfig.greenLightning.Value;

            if (windDirection == 0)
            {
                windDirection = UnityEngine.Random.Range(1, 3);
            }

            //Overwrite with global settings from region if they exist and that region is set to custom
            if (ForecastConfig.customRegionSettings.ContainsKey(regionName) && ForecastConfig.regionSettings.ContainsKey(regionName) && ForecastConfig.regionSettings[regionName] == 3)
            {
                foreach (string key in ForecastConfig.customRegionSettings[regionName].Keys)
                {
                    if (key == "GLOBAL" || key == roomName)
                    {
                        if (globalTags == null)
                        {
                            globalTags = new List<string>();
                        }
                        if (roomTags == null)
                        {
                            roomTags = new List<string>();
                        }
                        List<string> tags = ForecastConfig.customRegionSettings[regionName][key];
                        foreach (string tag in tags)
                        {
                            if (key == "GLOBAL")
                            {
                                globalTags.Add(tag);
                            }
                            if (key == roomName)
                            {
                                roomTags.Add(tag);
                            }
                            string type = tag.Split('_')[0];
                            string data = tag.Split(new[] { '_' }, 2)[1];
                            switch (type)
                            {
                                case "WI": //Weather Intensity
                                    if (data == "DYNAMIC")
                                    {
                                        weatherIntensity = 0;
                                        dynamic = true;
                                    }
                                    else
                                    {
                                        int.TryParse(data, out int num);
                                        weatherIntensity = -1;
                                        fixedIntensity = num / 100f;
                                        dynamic = false;
                                    }
                                    break;
                                case "WD": //Wind Direction
                                    int dir = 0;
                                    if (data == "RANDOM") { dir = UnityEngine.Random.Range(1, 3); }
                                    if (data == "LEFT") { dir = 1; }
                                    if (data == "MID") { dir = 2; }
                                    if (data == "RIGHT") { dir = 3; }
                                    windDirection = dir;
                                    break;
                                case "WC": //Weather Chance
                                    int.TryParse(data, out int num3);
                                    weatherChance = num3;
                                    break;
                                case "PL": //Particle Limit
                                    int.TryParse(data, out int num4);
                                    particleLimit = num4;
                                    break;
                                case "BG": //Background Collision
                                    backgroundCollision = data == "ON";
                                    break;
                                case "WA": //Water Collision
                                    waterCollision = data == "ON";
                                    break;
                                case "RV": //Water Collision
                                    rainVolume = data == "ON";
                                    break;
                                case "BL": //Background Lightning
                                    backgroundLightning = data == "ON";
                                    break;
                                case "CC": //Cloud Cover
                                    dynamicClouds = data == "ON";
                                    if (data != "ON")
                                    {
                                        dynamicClouds = false;
                                        int.TryParse(data, out int num5);
                                        cloudCover = num5;
                                    }
                                    break;
                                case "LS": //Lightning Strikes
                                    lightningStrikes = data == "ON";
                                    break;
                                case "LC": //Lightning interval and chance
                                    string[] split = data.Split('_');
                                    int.TryParse(split[0], out int interval);
                                    int.TryParse(split[1], out int chance);
                                    lightningInterval = interval;
                                    lightningChance = chance;
                                    break;
                                case "ST": //Strike Damage Type
                                    int dam = 0;
                                    if (data == "NONE") { dam = 0; }
                                    if (data == "STUN") { dam = 1; }
                                    if (data == "LETHAL") { dam = 2; }
                                    strikeDamageType = dam;
                                    break;
                                case "SC": //Strike color
                                    Color col = new Color();
                                    string[] RGB = data.Split('_');
                                    int.TryParse(RGB[0], out int R);
                                    int.TryParse(RGB[1], out int G);
                                    int.TryParse(RGB[2], out int B);
                                    col.r = R / 255f;
                                    col.g = G / 255f;
                                    col.b = B / 255f;
                                    col.a = 1f;
                                    strikeColor = col;
                                    break;
                            }
                        }
                    }
                }
            }

            //Wind Direction
            if (!WeatherForecast.regionWindDirection.ContainsKey(regionName))
            {
                WeatherForecast.regionWindDirection.Add(regionName, windDirection);
            }
            else
            {
                windDirection = WeatherForecast.regionWindDirection[regionName];
            }
            if (!dynamic)
            {
                switch (weatherIntensity)
                {
                    case 0:
                        dynamic = true;
                        fixedIntensity = 0f;
                        break;
                    case 1:
                        dynamic = false;
                        fixedIntensity = 0.25f;
                        break;
                    case 2:
                        dynamic = false;
                        fixedIntensity = 0.6f;
                        break;
                    case 3:
                        dynamic = false;
                        fixedIntensity = 1f;
                        break;
                }

                currentIntensity = fixedIntensity;
                startingIntensity = currentIntensity;
            }
            else
            {
                //Determine initial starting intensity
                startingIntensity = UnityEngine.Random.Range(-0.5f, 0.8f);

                //If this is the first time its been determined, add it to the dictionary for reference later
                if (!WeatherForecast.dynamicRegionStartingIntensity.ContainsKey(regionName))
                {
                    WeatherForecast.dynamicRegionStartingIntensity.Add(regionName, startingIntensity);
                }
                //If there's already an entry for this region, overwrite the startingIntensity with what's stored
                else
                {
                    startingIntensity = WeatherForecast.dynamicRegionStartingIntensity[regionName];
                }
            }

            //Load Weather
            currentWeather = new WeatherForecast.Weather(WeatherForecast.regionWeatherForecasts[regionName][0]);

            ForecastLog.Log($"Current weather for {regionName} is {currentWeather.type}");
            weatherType = currentWeather.weatherIndex;
            startingIntensity = currentWeather.minIntensity;

            //Determine whether region weather is disabled
            if (!WeatherForecast.weatherlessRegions.Contains(regionName))
            {
                //Weather disabled because it failed the weather chance check
                if (weatherChance < UnityEngine.Random.Range(0, 100))
                {
                    WeatherForecast.weatherlessRegions.Add(regionName);
                    ForecastLog.Log($"FORECAST: Region: {regionName} failed weatherChance - DISABLED this cycle");
                }
                //Weather disabled via Remix menu
                if (regionName != "GLOBAL" && ForecastConfig.regionSettings[regionName] == 0)
                {
                    WeatherForecast.weatherlessRegions.Add(regionName);
                    ForecastLog.Log($"FORECAST: Region: {regionName} weather disabled via Remix");
                }
                //Weather disabled because support mode is active and this region isn't using custom settings
                else if (regionName != "GLOBAL" && ForecastConfig.regionSettings[regionName] != 3 && ForecastConfig.supportMode.Value)
                {
                    WeatherForecast.weatherlessRegions.Add(regionName);
                    ForecastLog.Log($"FORECAST: Region: {regionName} weather disabled due to Support Mode");
                }
            }
            ForecastLog.Log($"FORECAST: Generated settings for {regionName}");
        }

        public void Update()
        {
            if (ForecastConfig.debugMode.Value)
            {
                if (owner.room.BeingViewed)
                {
                    if (reloadDelay <= 0)
                    {
                        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Q))
                        {
                            reloadDelay = 100;
                            ForecastConfig.customRegionSettings = new Dictionary<string, Dictionary<string, List<string>>>();
                            ForecastConfig.LoadCustomRegionSettings();
                        }
                    }
                    else
                    {
                        reloadDelay--;
                    }
                    if (WeatherHooks.debugUI != null)
                    {
                        if (WeatherHooks.debugUI.settings != this)
                        {
                            WeatherHooks.debugUI.UpdateLabels(this);
                        }
                        else if (!WeatherHooks.debugUI.toggle)
                        {
                            WeatherHooks.debugUI.activeCounter = 10;
                            WeatherHooks.debugUI.refreshCounter++;
                            if (WeatherHooks.debugUI.refreshCounter >= 40)
                            {
                                WeatherHooks.debugUI.UpdateLabels(this);
                                WeatherHooks.debugUI.refreshCounter = 0;
                            }
                        }
                    }
                }
            }
            //Rain intensity increases with cycle duration if in dynamic mode
            if (dynamic)
            {
                if (currentWeather != null)
                {
                    lastIntensity = currentIntensity;
                    currentIntensity = Mathf.Lerp(currentWeather.minIntensity, currentWeather.maxIntensity, owner.room.world.rainCycle.CycleProgression);
                    if (weatherType == 2)
                    {
                        Shader.SetGlobalFloat("_snowStrength", currentIntensity);
                    }
                }
                else
                {
                    //Rain
                    if (weatherType == 0)
                    {
                        if (owner.room.world.rainCycle.RainDarkPalette <= 0)
                        {
                            currentIntensity = Mathf.Lerp(startingIntensity, 1f, owner.room.world.rainCycle.CycleProgression);
                        }
                        else
                        {
                            currentIntensity = Mathf.Lerp(0.95f, 0f, owner.room.world.rainCycle.RainDarkPalette);
                        }
                    }
                    //Snow
                    if (weatherType == 2)
                    {
                        currentIntensity = Mathf.Lerp(startingIntensity, 1f, owner.room.world.rainCycle.CycleProgression);
                        Shader.SetGlobalFloat("_snowStrength", currentIntensity);
                    }
                }
                //Cap intensity at roomSettings intensity
                if(currentIntensity > owner.room.roomSettings.RainIntensity)
                {
                    currentIntensity = owner.room.roomSettings.RainIntensity;
                }
            }
        }
    }
}


