using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

public class WeatherController : UpdatableAndDeletable
{
    public Player player;
    public bool isSnow;
    public Room currentRoom;
    public int rainDrops;
    public int snowFlakes;
    public float rainAmount;
    public Vector2 spawn;
    public FloatRect roomBounds;
    public int rainLimit;
    public float ratio;
    public int ceilingCount;
    public int direction;
    public bool spawnDecals;

    public List<Vector2> skyreach;
    public int[] rainReach;
    public Vector2 camPos;
    public List<Vector2> camSkyreach;
    public List<IntVector2> ceilingTiles;
    public List<IntVector2> groundTiles;

    public Blizzard blizzard;
    public WeatherSettings settings;

    public Color strikeColor = new Color(0f, 1f, 0f);

    public WeatherController(Room room, int weatherType)
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
        spawnDecals = false;
        roomBounds = room.RoomRect;
        rainDrops = 0;
        snowFlakes = 0;
        this.room = room;
        isSnow = weatherType == 1;
        ceilingCount = 0;
        direction = settings.windDirection;
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
                    if (isSnow && Forecast.decals)
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
        //ForecastLog.Log(ceilingTiles.Count.ToString() + " || " + groundTiles.Count.ToString());
        //if (ceilingTiles.Count != groundTiles.Count)
        //{
        //    ForecastLog.Log("Tile list mismatch!");
        //}
        room.AddObject(new RainSound(room, this));
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
        settings.Update();
        if (room.BeingViewed)
        {
            camPos = room.game.cameras[0].pos + new Vector2(room.game.rainWorld.screenSize.x / 2, room.game.rainWorld.screenSize.y / 2);
        }
        isSnow = settings.weatherType == 1;
        if (isSnow)
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
        player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);

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
                    strikeColor = new Color(1f, 1f, 0.95f);
                    room.AddObject(room.lightning);
                }
                if (settings.lightningStrikes && room.lightning != null && room.BeingViewed && room.roomRain != null && room.roomRain.dangerType == RoomRain.DangerType.Rain && UnityEngine.Random.value < settings.currentIntensity * 0.0010f)
                {
                    room.AddObject(new LightningStrike(this, strikeColor)); //TODO - Use counter for spawning these and tie to spawn chance tag
                }
            }
            if (!isSnow)
            {
                snowFlakes = 0;
                if (!WeatherHooks.noRainThisCycle && rainDrops < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddRaindrops(rainLimit - rainDrops);
                }
            }
            else
            {
                rainDrops = 0;
                if (!WeatherHooks.noRainThisCycle && snowFlakes < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddSnowflakes(rainLimit - snowFlakes);
                }
            }
        }
        //Puffs of snow when slugcat walks
        if (settings.weatherType == 1 && ForecastConfig.snowPuffs.Value && WeatherHooks.rainList.Contains(room.abstractRoom.name))
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
        public WeatherController owner;
        public int weatherType;
        public int weatherIntensity;
        public float currentIntensity;
        public float fixedIntensity;
        public bool dynamic;
        public float weatherChance;
        public int windDirection;
        public int particleLimit;
        public bool backgroundCollision;
        public bool waterCollision;
        public bool backgroundLightning;
        public bool lightningStrikes;
        public int strikeDamageType;
        public List<string> weatherTags;
        //Debug HUD
        public FLabel debug;
        public bool show;

        public WeatherSettings(string region, string room, WeatherController owner)
        {
            Debug.Log("New Settings");
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
            lightningStrikes = ForecastConfig.lightningStrikes.Value;
            strikeDamageType = ForecastConfig.strikeDamageType.Value;

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

            //Overwrite with global settings from region if they exist
            if (ForecastConfig.customRegionSettings.ContainsKey(region))
            {
                foreach (string key in ForecastConfig.customRegionSettings[region].Keys)
                {
                    if (key == "GLOBAL" || key == room)
                    {
                        if(weatherTags == null)
                        {
                            weatherTags = new List<string>();
                        }
                        List<string> tags = ForecastConfig.customRegionSettings[region][key];
                        foreach (string tag in tags)
                        {
                            if (!weatherTags.Contains(tag))
                            {
                                weatherTags.Add(tag);
                            }
                            string type = tag.Split('_')[0];
                            string data = tag.Split('_')[1];
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
                                        fixedIntensity = num;
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
                                    float.TryParse(data, out float num3);
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
                                case "BL": //Background Lightning
                                    backgroundLightning = data == "ON";
                                    break;
                                case "LS": //Lightning Strikes
                                    lightningStrikes = data == "ON";
                                    break;
                                case "ST": //Strike Damage Type
                                    int dam = 0;
                                    if (data == "NONE") { dam = 0; }
                                    if (data == "STUN") { dam = 1; }
                                    if (data == "LETHAL") { dam = 2; }
                                    strikeDamageType = dam;
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
            }
            ForecastLog.Log($"FORECAST: Generated settings for {room}");
        }

        public void Update()
        {
            if (owner.room.BeingViewed)
            {
                if(debug == null)
                {
                    debug = new FLabel("font", "");
                    debug.alignment = FLabelAlignment.Left;
                    debug.SetPosition(50f, 700f);
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
                    info += $"{owner.room.abstractRoom.name} - WEATHER SETTINGS \n";
                    info += "Intensity: " + (dynamic ? "Dynamic" : "Fixed") + $" - {currentIntensity}\n";
                    info += "Tags: ";
                    if(weatherTags == null)
                    {
                        info += "NONE\n";
                    }
                    else if(weatherTags.Count > 0)
                    {
                        for (int i = 0; i < weatherTags.Count; i++)
                        {
                            info += weatherTags[i] + ", ";
                        }
                        info += "\n";
                    }
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
                    currentIntensity = Mathf.Lerp(fixedIntensity, 1f, owner.room.world.rainCycle.CycleProgression);
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


