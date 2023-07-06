using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

public class Preciptator : UpdatableAndDeletable
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
    public float direction;
    public bool spawnDecals;
    public List<Vector2> skyreach;
    public int[] rainReach;
    public Vector2 camPos;
    public List<Vector2> camSkyreach;
    public List<IntVector2> ceilingTiles;
    public List<IntVector2> groundTiles;
    public Blizzard blizzard;
    public Color strikeColor = new Color(0f, 1f, 0f);

    public Preciptator(Room room, bool isSnow)
    {
        skyreach = new List<Vector2>();
        camSkyreach = new List<Vector2>();
        ceilingTiles = new List<IntVector2>();
        groundTiles = new List<IntVector2>();
        spawnDecals = false;
        roomBounds = room.RoomRect;
        rainDrops = 0;
        snowFlakes = 0;
        this.room = room;
        this.isSnow = isSnow;
        ceilingCount = 0;
        direction = RainFall.direction;
        for (int r = 0; r < room.TileWidth; r++)
        {
            if (room.Tiles[r, room.TileHeight - 1].Solid)
            {
                ceilingCount++;
            }
        }

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
        //Debug.Log(ceilingTiles.Count.ToString() + " || " + groundTiles.Count.ToString());
        //if (ceilingTiles.Count != groundTiles.Count)
        //{
        //    Debug.Log("Tile list mismatch!");
        //}
    }

    public void AddRaindrops(int rainDropsToSpawn)
    {
        if (room != null && skyreach != null && skyreach.Count > 0)
        {
            for (int i = 0; i < rainDropsToSpawn; i++)
            {
                Vector2 rng = skyreach[UnityEngine.Random.Range(0, skyreach.Count)];
                RainDrop rainDrop = new RainDrop(rng, Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), RainFall.rainIntensity, this);
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
                            SnowFlake snowFlake = new SnowFlake(spawnPos, Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), RainFall.rainIntensity, this);
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
                        //Debug.Log("ERROR SPAWNING SNOWFLAKE");
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
        if (room.BeingViewed)
        {
            camPos = room.game.cameras[0].pos + new Vector2(room.game.rainWorld.screenSize.x / 2, room.game.rainWorld.screenSize.y / 2);
        }
        isSnow = Forecast.snow;
        if (isSnow)
        {
            if (Forecast.blizzard && (room.world.rainCycle.timer - room.world.rainCycle.cycleLength) / 2400f > -0.5f && blizzard == null)
            {
                blizzard = new Blizzard(this);
                room.AddObject(blizzard);
            }
            rainAmount = Mathf.Lerp(Forecast.rainAmount * 0.5f, Forecast.rainAmount, RainFall.rainIntensity);
            rainLimit = (int)Mathf.Lerp(Mathf.Lerp(0f, rainAmount * 50,room.roomSettings.RainIntensity), Mathf.Lerp(rainAmount * 50,(rainAmount * 80), room.roomSettings.RainIntensity), RainFall.rainIntensity);
        }
        else
        {
            rainAmount = Mathf.Lerp(0, Forecast.rainAmount, RainFall.rainIntensity);
            rainLimit = (int)Mathf.Lerp(0, Mathf.Lerp(0f,(rainAmount * 9), room.roomSettings.RainIntensity), RainFall.rainIntensity);
        }
        player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);

        if (room.game != null && room != null && !room.abstractRoom.gate && room.ReadyForPlayer)
        {
            if (room.game != null && !room.abstractRoom.shelter && Forecast.lightning && room.roomRain != null)
            {
                if ((room.roomRain.dangerType == RoomRain.DangerType.Rain || room.roomRain.dangerType == RoomRain.DangerType.FloodAndRain) && RainFall.rainIntensity > 0.7f && room.lightning == null)
                {
                    room.lightning = new Lightning(room, 1f, false);
                    room.lightning.bkgOnly = true;
                    room.lightning.bkgGradient[0] = room.game.cameras[0].currentPalette.skyColor;
                    room.lightning.bkgGradient[1] = Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), RainFall.rainIntensity);
                    strikeColor = new Color(1f, 1f, 0.95f);
                    room.AddObject(room.lightning);
                }
                if (Forecast.strike && room.lightning != null && room.BeingViewed && room.roomRain != null && room.roomRain.dangerType == RoomRain.DangerType.Rain && Forecast.strike && UnityEngine.Random.value < RainFall.rainIntensity * 0.0010f)
                {
                    room.AddObject(new LightningStrike(this, strikeColor));
                }
            }
            if (!isSnow)
            {
                snowFlakes = 0;
                if (!RainFall.noRainThisCycle && rainDrops < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddRaindrops(rainLimit - rainDrops);
                }
            }
            else
            {
                rainDrops = 0;
                if (!RainFall.noRainThisCycle && snowFlakes < ((room.Width - ceilingCount) * rainLimit) / room.Width)
                {
                    AddSnowflakes(rainLimit - snowFlakes);
                }
            }
        }
        if (Forecast.snow && Forecast.dust && RainFall.rainList.Contains(room.abstractRoom.name))
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
}

//Raindrops
public class RainDrop : CosmeticSprite
{
    public Vector2 lastLastLastPos;
    public Vector2 lastLastPos;
    public Color color;
    public float gravity;
    public float splashCounter;
    public bool timeToDie;
    public Vector2 dir;
    public float dirCounter;
    public bool backgroundDrop;
    public float depth;
    public bool collision;
    public Preciptator spawner;
    public bool reset;
    public Vector2 resetPos;
    public float alpha;
    public Player player;

    public RainDrop(Vector2 pos, Color color, float rainIntensity, Preciptator spawner)
    {
        alpha = UnityEngine.Random.Range(0.9f, 1f);
        this.spawner = spawner;
        timeToDie = false;
        splashCounter = 0;
        collision = false;
        //Small chance for any raindrop to be a background drop, assign it a random depth value
        if (Forecast.bg && UnityEngine.Random.value > 0.85f)
        {
            backgroundDrop = true;
            depth = UnityEngine.Random.value;
        }
        else
        {
            backgroundDrop = false;
            depth = UnityEngine.Random.Range(0.7f, 1f);
        }
        if (Forecast.rainbow)
        {
            this.color = Custom.HSL2RGB(UnityEngine.Random.value, 0.5f, 0.5f);
        }
        else
        {
            this.color = color;
        }
        resetPos = new Vector2(pos.x, spawner.room.RoomRect.top + 150f);
        pos = new Vector2(pos.x, UnityEngine.Random.Range(pos.y, spawner.room.RoomRect.top + 150f));
        lastPos = pos;
        lastLastPos = pos;
        lastLastLastPos = pos;
        //Vary starting velocity
        vel.y = UnityEngine.Random.Range(-10f * rainIntensity, -20f * rainIntensity);
        //Increase spread of raindrops as rain intensity increases
        vel.x = Mathf.Lerp(UnityEngine.Random.Range(-4, 3f), UnityEngine.Random.Range(-12f, 3f), RainFall.rainIntensity);
        gravity = Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value);
    }
    public override void Update(bool eu)
    {
        player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);
        if (reset)
        {
            float rng = UnityEngine.Random.value;
            if (rng < 0.05f && room.world.rainCycle.RainDarkPalette > 0)
            {
                Destroy();
            }
            if (Forecast.snow)
            {
                Destroy();
            }
            if (player != null && player.mainBodyChunk != null && !player.inShortcut)
            {
                resetPos = new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1300f, player.mainBodyChunk.pos.x + 1300f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            else
            {
                resetPos = new Vector2(UnityEngine.Random.Range(room.RoomRect.left - 100f, room.RoomRect.right + 100f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            pos = resetPos;
            lastPos = resetPos;
            lastLastPos = resetPos;
            lastLastLastPos = resetPos;
            collision = false;
            timeToDie = false;
            switch (spawner.direction)
            {
                case 1:
                    vel.x = Mathf.Lerp(UnityEngine.Random.Range(8f, -4f), UnityEngine.Random.Range(-13f, -4f), Mathf.Lerp(0f, 1f, RainFall.rainIntensity));
                    break;
                case 2:
                    vel.x = Mathf.Lerp(UnityEngine.Random.Range(5f, -5f), UnityEngine.Random.Range(-2f, 2f), Mathf.Lerp(0f, 1f, RainFall.rainIntensity));
                    break;
                case 3:
                    vel.x = Mathf.Lerp(UnityEngine.Random.Range(-8f, 4f), UnityEngine.Random.Range(4f, 13f), Mathf.Lerp(0f, 1f, RainFall.rainIntensity));
                    break;
            }
            vel.y = UnityEngine.Random.Range(-10f * RainFall.rainIntensity, -18f * RainFall.rainIntensity);
            splashCounter = 0f;
            reset = false;
        }
        lastLastLastPos = lastLastPos;
        lastLastPos = lastPos;
        //Control fall speed of normal and background drops, tied to rain intensity.
        if (backgroundDrop)
        {
            if (collision)
            {
                vel.y = 0f;
            }
            else
            {
                vel.y -= (gravity * 3.9f);
                if (vel.y < Mathf.Lerp(-20f, -35f, RainFall.rainIntensity))
                {
                    vel.y = Mathf.Lerp(-20f, -35f, RainFall.rainIntensity);
                }
            }
        }
        else
        {
            if (collision)
            {
                vel.y -= (gravity * 2.5f);
                if (vel.y < Mathf.Lerp(-1f, -2f, RainFall.rainIntensity))
                {
                    vel.y = Mathf.Lerp(-1f, -2f, RainFall.rainIntensity);
                }
                if (vel.x < Mathf.Lerp(-1f, -4f, RainFall.rainIntensity))
                {
                    vel.x = Mathf.Lerp(-1f, -4f, RainFall.rainIntensity);
                }
            }
            else
            {
                vel.y -= (gravity * 9.5f);
                if (vel.y < Mathf.Lerp(-32f, -40f, RainFall.rainIntensity))
                {
                    vel.y = Mathf.Lerp(-32f, -40f, RainFall.rainIntensity);
                }
            }
        }
        //Decrease splash scale on hit
        if (collision)
        {
            splashCounter = splashCounter - 0.1f;
            if (splashCounter < 0f)
            {
                splashCounter = 0f;
            }
        }
        //Raindrop hits floor or water
        bool hitWater = room.GetTile(pos).WaterSurface;
        if (hitWater && Forecast.water)
        {
            if (room.water && UnityEngine.Random.value > 0.98)
            {
                room.waterObject?.Explosion(pos, 0.45f, 0.89f);
            }
            pos.y = room.MiddleOfTile(pos).y + UnityEngine.Random.Range(2f, 9f);
            vel.y *= -0.01f;
            vel.x *= 0.2f;
            timeToDie = true;
            splashCounter = UnityEngine.Random.Range(0.9f, 1.1f);
            collision = true;
        }
        //Raindrop hits floor or water
        if (room.GetTile(pos).Solid || hitWater)
        {
            //Decrease velocity if raindrop hits a solid surface or water and increase splash counter
            if (UnityEngine.Random.value > 0.01f)
            {
                if (vel.y < 0f && !timeToDie)
                {
                    pos.y = room.MiddleOfTile(pos).y + 11f;
                    vel.y = vel.y * -0.01f;
                    vel.x = vel.x * 0.2f;
                    timeToDie = true;
                    splashCounter = UnityEngine.Random.Range(0.9f, 1.1f);
                    collision = true;
                }
                else
                {
                    reset = true;
                }
            }
        }
        //If raindrop falls below room bottom, or if rain intensity is 0, remove it.
        if (pos.y < -100f || RainFall.rainIntensity == 0f)
        {
            reset = true;
        }
        base.Update(eu);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[] { new TriangleMesh.Triangle(0, 1, 2) };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[1] = new FSprite("RainSplash", true);
        if (backgroundDrop)
        {
            sLeaser.sprites[1].alpha = Mathf.Lerp(0.6f, 1f, depth);
        }
        else
        {
            sLeaser.sprites[1].alpha = UnityEngine.Random.Range(0.82f, 1f);
        }
        sLeaser.sprites[0].alpha = alpha;
        sLeaser.sprites[0].color = Color.Lerp(rCam.currentPalette.fogColor, color, depth * 0.9f);
        sLeaser.sprites[1].color = Color.Lerp(rCam.currentPalette.fogColor, color, depth * 0.9f);
        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {

        Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
        Vector2 vector2 = Vector2.Lerp(lastLastLastPos, lastLastPos, timeStacker);
        if (Custom.DistLess(vector, vector2, 9f))
        {
            vector2 = vector + Custom.DirVec(vector, vector2) * 12f;
        }
        vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, 1f));
        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + a * (depth + (depth * 0.1f)) - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - a * (depth + (depth * 0.1f)) - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
        sLeaser.sprites[1].x = vector.x - camPos.x;
        sLeaser.sprites[1].y = vector.y - camPos.y;
        sLeaser.sprites[1].rotation = UnityEngine.Random.value * 360f;
        if (Forecast.rainbow)
        {
            sLeaser.sprites[0].color = Color.Lerp(color, Color.Lerp(rCam.PixelColorAtCoordinate(pos), rCam.PixelColorAtCoordinate(lastLastLastPos), 0.5f), 0.36f);
        }
        else
        {
            sLeaser.sprites[0].color = Color.Lerp(new Color(1f, 1f, 1f), Color.Lerp(rCam.currentPalette.fogColor, Color.Lerp(rCam.PixelColorAtCoordinate(pos), rCam.PixelColorAtCoordinate(lastLastLastPos), 0.5f), 0.83f), 0.95f);
        }
        //If background drops encounter a depth in the room texture lower than their own depth value, treat it as a collision.
        if (backgroundDrop && !reset && !collision && rCam.IsViewedByCameraPosition(rCam.cameraNumber, pos) && rCam.DepthAtCoordinate(pos) < depth)
        {
            splashCounter = 1f;
            timeToDie = true;
            sLeaser.sprites[1].color = Color.Lerp(color, rCam.PixelColorAtCoordinate(pos), 0.7f);
            collision = true;
        }
        if (splashCounter > 0f && !backgroundDrop)
        {
            sLeaser.sprites[1].color = Color.Lerp(color, rCam.PixelColorAtCoordinate(pos), 0.2f);
        }
        if (splashCounter > 0f)
        {
            //If splash counter is greater than 0 adjust the scale of the splash sprite based on whether its a background drop or not.
            if (backgroundDrop)
            {
                sLeaser.sprites[1].scale = Mathf.Lerp(0f, depth * 0.4f, splashCounter);
                vel.y = 0f;
                vel.x = 0f;
            }
            else
            {
                sLeaser.sprites[1].scale = Mathf.Lerp(0f, UnityEngine.Random.Range(0.25f, 0.42f), splashCounter);
            }
            sLeaser.sprites[0].alpha = 0f;
        }
        else
        {
            sLeaser.sprites[1].scale = 0f;
            sLeaser.sprites[0].alpha = alpha;
        }
        //Delete raindrop if it falls a certain distance below the currently viewed room camera
        if (pos.y < (rCam.pos.y - 100f) || (splashCounter <= 0f && timeToDie))
        {
            reset = true;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}
