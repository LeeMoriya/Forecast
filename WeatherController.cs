using System;
using System.Collections.Generic;
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

    public Blizzard blizzard;
    public WeatherSettings settings;

    public Color strikeColor = new Color(0f, 1f, 0f);
    public float lightningCounter;

    public WeatherController(Room room)
    {
        if (!WeatherHooks.roomSettings.TryGetValue(room, out settings))
        {
            settings = new WeatherSettings(room.world.region.name, room.abstractRoom.name, this);
            WeatherHooks.roomSettings.Add(room, settings);
        }

        skyreach = new List<Vector2>();
        camSkyreach = new List<Vector2>();
        ceilingTiles = new List<IntVector2>();
        groundTiles = new List<IntVector2>();
        rainDrops = 0;
        snowFlakes = 0;
        this.room = room;
        ceilingCount = 0;

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
                        if (room.GetTile(i, t).Terrain == Room.Tile.TerrainType.Solid || t < room.defaultWaterLevel)
                        {
                            groundTiles.Add(new IntVector2(i, t));
                            break;
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
                    if (settings.weatherType == 1 && Forecast.decals)
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
        if (settings.rainVolume)
        {
            room.AddObject(new RainSound(room, this));
        }
    }

    public void AddRaindrops(int rainDropsToSpawn)
    {
        if (room != null && skyreach != null && skyreach.Count > 0)
        {
            for (int i = 0; i < rainDropsToSpawn; i++)
            {
                Vector2 rng = skyreach[UnityEngine.Random.Range(0, skyreach.Count)];
                RainDrop rainDrop = new RainDrop(rng, Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), settings.currentIntensity, this);
                room.AddObject(rainDrop);
                rainDrops++;
            }
        }
    }

    public void AddSnowflakes(int snowFlakesToSpawn)
    {
        if (room != null && room.BeingViewed)
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
                        //ForecastLog.Log("ERROR SPAWNING SNOWFLAKE");
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
        if (disabled || WeatherHooks.weatherForecast.weatherlessRegions.Contains(room.world.region.name))
        {
            if (!disabled)
            {
                settings.Update(); //One-time update to display settings
                disabled = true;
            }
            return;
        }
        settings.Update();
        if (room.BeingViewed)
        {
            camPos = room.game.cameras[0].pos + new Vector2(room.game.rainWorld.screenSize.x / 2, room.game.rainWorld.screenSize.y / 2);
        }
        if (settings.weatherType == 1)
        {
            if (ForecastConfig.endBlizzard.Value && (room.world.rainCycle.timer - room.world.rainCycle.cycleLength) / 2400f > -0.5f && blizzard == null)
            {
                blizzard = new Blizzard(this);
                room.AddObject(blizzard);
            }
            rainAmount = Mathf.Lerp(settings.particleLimit * 0.5f, settings.particleLimit, settings.currentIntensity);
            rainLimit = (int)Mathf.Lerp(Mathf.Lerp(0f, rainAmount * 50, room.roomSettings.RainIntensity), Mathf.Lerp(rainAmount * 50, (rainAmount * 80), room.roomSettings.RainIntensity), settings.currentIntensity);
        }
        else
        {
            rainAmount = Mathf.Lerp(0, settings.particleLimit, settings.currentIntensity);
            rainLimit = (int)Mathf.Lerp(0, Mathf.Lerp(0f, (rainAmount * 9), room.roomSettings.RainIntensity), settings.currentIntensity);
        }

        if (settings.dynamicClouds)
        {
            room.roomSettings.Clouds = Mathf.Lerp(settings.startingIntensity, 1f, room.world.rainCycle.CycleProgression); //Cloud cover should apply everywhere
        }
        else
        {
            room.roomSettings.Clouds = settings.cloudCover;
        }

        if (room.game != null && room != null && !room.abstractRoom.gate && room.ReadyForPlayer)
        {
            if (room.game != null && !room.abstractRoom.shelter && settings.backgroundLightning && room.roomRain != null)
            {
                if ((room.roomRain.dangerType == RoomRain.DangerType.Rain || room.roomRain.dangerType == RoomRain.DangerType.FloodAndRain) && settings.currentIntensity > 0.7f && room.lightning == null)
                {
                    room.lightning = new Lightning(room, 1f, false);
                    room.lightning.bkgOnly = true;
                    room.lightning.bkgGradient[0] = room.game.cameras[0].currentPalette.skyColor;
                    room.lightning.bkgGradient[1] = Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), settings.currentIntensity);
                    room.AddObject(room.lightning);
                }
                if (settings.lightningStrikes && room.BeingViewed && room.roomRain != null)
                {
                    lightningCounter += 0.025f;
                    if(lightningCounter >= settings.lightningInterval)
                    {
                        lightningCounter = 0;
                        if(UnityEngine.Random.Range(0,100) <= settings.lightningChance)
                        {
                            room.AddObject(new LightningStrike(this, settings.strikeColor));
                        }
                    }
                }
            }
            if (settings.weatherType == 0)
            {
                snowFlakes = 0;
                if (rainDrops < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddRaindrops(rainLimit - rainDrops);
                }
            }
            else
            {
                rainDrops = 0;
                if (snowFlakes < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddSnowflakes(rainLimit - snowFlakes);
                }
            }
        }
        //Puffs of snow when slugcat walks
        if (settings.weatherType == 1 && ForecastConfig.snowPuffs.Value && WeatherHooks.roomSettings.TryGetValue(room, out WeatherController.WeatherSettings s))
        {
            for (int i = 0; i < room.game.Players.Count; i++)
            {
                if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
                {
                    for (int j = 0; j < room.game.Players[i].realizedCreature.bodyChunks.Length; j++)
                    {
                        if (room.game.Players[i].realizedCreature.bodyChunks[j].ContactPoint.y < 0)
                        {
                            if (room.game.Players[i].realizedCreature.bodyChunks[j].lastContactPoint.y >= 0 && room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.y - room.game.Players[i].realizedCreature.bodyChunks[j].pos.y > 5f)
                            {
                                room.AddObject(new SnowDust(room.game.Players[i].realizedCreature.bodyChunks[j].pos + new Vector2(0f, -room.game.Players[i].realizedCreature.bodyChunks[j].rad), Custom.LerpMap(room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.y - room.game.Players[i].realizedCreature.bodyChunks[j].pos.y, 5f, 10f, 0.5f, 1f)));
                            }
                            else if (UnityEngine.Random.value < 0.1f && Mathf.Abs(room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.x - room.game.Players[i].realizedCreature.bodyChunks[j].pos.x) > 3f)
                            {
                                room.AddObject(new SnowDust(room.game.Players[i].realizedCreature.bodyChunks[j].pos + new Vector2(0f, -room.game.Players[i].realizedCreature.bodyChunks[j].rad), 0.25f * UnityEngine.Random.value));
                            }
                        }
                    }
                }
            }
        }
    }

    public class WeatherSettings
    {
        public int reloadDelay;

        public WeatherController owner;
        public int weatherType;
        public int weatherIntensity;
        public float startingIntensity;
        public float currentIntensity;
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
            //Apply generic settings
            weatherType = 0;
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

            switch (weatherIntensity)
            {
                case 0:
                    dynamic = true;
                    fixedIntensity = 0f;
                    break;
                case 1:
                    fixedIntensity = 0.25f;
                    break;
                case 2:
                    fixedIntensity = 0.6f;
                    break;
                case 3:
                    fixedIntensity = 1f;
                    break;
            }
            if(windDirection == 0)
            {
                windDirection = UnityEngine.Random.Range(1, 3);
            }

            //Overwrite with global settings from region if they exist and that region is set to custom
            if (ForecastConfig.customRegionSettings.ContainsKey(region) && ForecastConfig.regionSettings.ContainsKey(region) && ForecastConfig.regionSettings[region] == 2)
            {
                foreach (string key in ForecastConfig.customRegionSettings[region].Keys)
                {
                    if (key == "GLOBAL" || key == room)
                    {
                        if(globalTags == null)
                        {
                            globalTags = new List<string>();
                        }
                        if (roomTags == null)
                        {
                            roomTags = new List<string>();
                        }
                        List<string> tags = ForecastConfig.customRegionSettings[region][key];
                        foreach (string tag in tags)
                        {
                            if(key == "GLOBAL")
                            {
                                globalTags.Add(tag);
                            }
                            if(key == room)
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
                                        fixedIntensity = num / 100f;
                                        dynamic = false;
                                    }
                                    break;
                                case "WD": //Wind Direction
                                    int dir = 0;
                                    if (data == "RANDOM") { dir = UnityEngine.Random.Range(1,3); }
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
                                    if(data != "ON")
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
                        if(key == "GLOBAL")
                        {
                            switch (weatherIntensity)
                            {
                                case 0:
                                    dynamic = true;
                                    fixedIntensity = 0f;
                                    break;
                                case 1:
                                    fixedIntensity = 0.25f;
                                    break;
                                case 2:
                                    fixedIntensity = 0.6f;
                                    break;
                                case 3:
                                    fixedIntensity = 1f;
                                    break;
                            }
                        }
                    }
                }
            }
            if(!dynamic)
            {
                currentIntensity = fixedIntensity;
                startingIntensity = currentIntensity;
            }
            else
            {
                //Determine initial starting intensity
                startingIntensity = UnityEngine.Random.Range(-0.5f, 0.8f);
                //If this is the first time its been determined, add it to the dictionary for reference later
                if (!WeatherHooks.weatherForecast.dynamicRegionStartingIntensity.ContainsKey(region))
                {
                    WeatherHooks.weatherForecast.dynamicRegionStartingIntensity.Add(region, startingIntensity);
                }
                //If there's already an entry for this region, overwrite the startingIntensity with what's stored
                else
                {
                    startingIntensity = WeatherHooks.weatherForecast.dynamicRegionStartingIntensity[region];
                }
            }
            //Determine whether region weather is disabled
            if (!WeatherHooks.weatherForecast.weatherlessRegions.Contains(region))
            {
                //Weather disabled because it failed the weather chance check
                if (weatherChance < UnityEngine.Random.Range(0, 100))
                {
                    WeatherHooks.weatherForecast.weatherlessRegions.Add(region);
                    ForecastLog.Log($"FORECAST: Region: {region} failed weatherChance - DISABLED this cycle");
                }
                //Weather disabled via Remix menu
                if (ForecastConfig.regionSettings[region] == 0)
                {
                    WeatherHooks.weatherForecast.weatherlessRegions.Add(region);
                    ForecastLog.Log($"FORECAST: Region: {region} weather disabled via Remix");
                }
                //Weather disabled because support mode is active and this region isn't using custom settings
                else if (ForecastConfig.regionSettings[region] == 1 && ForecastConfig.supportMode.Value)
                {
                    WeatherHooks.weatherForecast.weatherlessRegions.Add(region);
                    ForecastLog.Log($"FORECAST: Region: {region} weather disabled due to Support Mode");
                }
            }
            ForecastLog.Log($"FORECAST: Generated settings for {room}");
        }

        public void Update()
        {
            if (owner.room.BeingViewed)
            {
                if(reloadDelay <= 0)
                {
                    if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Q))
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
                if(debug == null)
                {
                    debug = new FLabel("font", "");
                    debug.alignment = FLabelAlignment.Left;
                    debug.SetPosition(50.01f, 700.01f);
                    debug.SetAnchor(new Vector2(0f, 1f));
                    Futile.stage.AddChild(debug);
                    show = true;
                }
                else
                {
                    if (!show)
                    {
                        Futile.stage.AddChild(debug);
                    }
                    string info = "";
                    info += $"{owner.room.abstractRoom.name} - WEATHER SETTINGS \n\n";
                    info += $"Weather Chance: {weatherChance}% - {(owner.disabled ? "DISABLED" : "ENABLED")}\n";
                    info += "Intensity: " + (dynamic ? "Dynamic" : "Fixed") + $" - {Mathf.RoundToInt(currentIntensity * 100f)}%\n";
                    info += $"Particle Limit: {particleLimit}\n\n";
                    info += "Global Tags: ";
                    if(globalTags == null)
                    {
                        info += "NONE\n";
                    }
                    else if(globalTags.Count > 0)
                    {
                        for (int i = 0; i < globalTags.Count; i++)
                        {
                            info += globalTags[i] + ", ";
                        }
                        info += "\n";
                    }
                    info += "Room Tags: ";
                    if (roomTags == null)
                    {
                        info += "NONE\n";
                    }
                    else if (roomTags.Count > 0)
                    {
                        for (int i = 0; i < roomTags.Count; i++)
                        {
                            info += roomTags[i] + ", ";
                        }
                        info += "\n";
                    }
                    info += "\n";
                    info += "Lightning Strikes: " + (lightningStrikes ? "ON" : "OFF") + "\n";
                    info += "Lightning Interval: " + (lightningStrikes ? ($"{Mathf.RoundToInt(owner.lightningCounter)} / {lightningInterval}") : "N/A") + "\n";
                    info += "Lightning Chance: " + (lightningStrikes ? (lightningChance + "%") : "N/A") + "\n";
                    debug.text = info;
                }
            }
            else if(debug != null)
            {
                Futile.stage.RemoveChild(debug);
                show = false;
            }
            //Rain intensity increases with cycle duration if in dynamic mode
            if (dynamic && owner.room.BeingViewed)
            {
                if (owner.room.world.rainCycle.RainDarkPalette <= 0)
                {
                    currentIntensity = Mathf.Lerp(startingIntensity, 1f, owner.room.world.rainCycle.CycleProgression);
                    if(Input.GetKey(KeyCode.Semicolon))
                    {
                        ForecastLog.Log($"FORECAST: {owner.room.abstractRoom.name} - Current intensity: {currentIntensity}");
                    }
                }
                else
                {
                    currentIntensity = Mathf.Lerp(0.95f, 0f, owner.room.world.rainCycle.RainDarkPalette);
                }
            }
        }
    }
}


