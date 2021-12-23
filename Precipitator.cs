﻿using System;
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

    public Preciptator(Room room, bool isSnow)
    {
        this.skyreach = new List<Vector2>();
        this.camSkyreach = new List<Vector2>();
        this.ceilingTiles = new List<IntVector2>();
        this.groundTiles = new List<IntVector2>();
        this.spawnDecals = false;
        this.roomBounds = room.RoomRect;
        this.rainDrops = 0;
        this.snowFlakes = 0;
        this.room = room;
        this.isSnow = isSnow;
        this.ceilingCount = 0;
        this.direction = RainFall.direction;
        if (Downpour.snow)
        {
            this.room.AddObject(new SnowDecal(this.room));
        }
        for (int r = 0; r < this.room.TileWidth; r++)
        {
            if (this.room.Tiles[r, this.room.TileHeight - 1].Solid)
            {
                ceilingCount++;
            }
        }
        //Debug.Log("Ceiling Count: " + ceilingCount);
        //Debug.Log(this.room.Width);
        //Debug.Log(this.room.Width * 0.95f);

        if (ceilingCount < (this.room.Width * 0.95f))
        {
            for (int i = 0; i < this.room.TileWidth; i++)
            {
                //Add every open air tile at the top of the room to a list
                int j = this.room.TileHeight - 1;
                if (this.room.GetTile(i, j).Terrain != Room.Tile.TerrainType.Solid && this.room.GetTile(i, j - 1).Terrain != Room.Tile.TerrainType.Solid && j > this.room.defaultWaterLevel)
                {
                    this.ceilingTiles.Add(new IntVector2(i, j));
                    //Check each tile below this one until it hits something solid
                    for (int t = j - 1; t > 0; t--)
                    {
                        //If this tile is solid or is below the water level, add it to the list
                        if (this.room.GetTile(i, t).Terrain == Room.Tile.TerrainType.Solid || t < this.room.defaultWaterLevel)
                        {
                            this.groundTiles.Add(new IntVector2(i, t));
                            break;
                        }
                        //If there are no solid tiles below this one, add a position for the bottom of the room
                        if (t == 0)
                        {
                            this.groundTiles.Add(new IntVector2(i, 0));
                            break;
                        }
                    }
                }
            }
            foreach (Room.Tile tile in this.room.Tiles)
            {
                if ((tile.Solid && this.room.GetTile(tile.X, tile.Y + 1).Terrain == Room.Tile.TerrainType.Air && this.room.GetTile(tile.X, tile.Y + 2).Terrain == Room.Tile.TerrainType.Air) ||
                    tile.Terrain == Room.Tile.TerrainType.Slope && this.room.GetTile(tile.X, tile.Y + 1).Terrain == Room.Tile.TerrainType.Air && this.room.GetTile(tile.X, tile.Y + 2).Terrain == Room.Tile.TerrainType.Air)
                {
                    this.skyreach.Add(this.room.MiddleOfTile(tile.X, tile.Y - 1));
                    //Add snow decals to surfaces
                    if (this.isSnow && Downpour.decals)
                    {
                        if (UnityEngine.Random.value > 0.8f)
                        {
                            this.room.AddObject(new SnowPile(this.room.MiddleOfTile(tile.X, tile.Y - 1), UnityEngine.Random.Range(60f, 80f)));
                        }
                        this.room.AddObject(new SnowPile(this.room.MiddleOfTile(tile.X, tile.Y), UnityEngine.Random.Range(20f, 45f)));
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
        if (room != null && this.skyreach != null && this.skyreach.Count > 0)
        {
            for (int i = 0; i < rainDropsToSpawn; i++)
            {
                Vector2 rng = this.skyreach[UnityEngine.Random.Range(0, this.skyreach.Count)];
                RainDrop rainDrop = new RainDrop(rng, Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), RainFall.rainIntensity, this);
                this.room.AddObject(rainDrop);
                this.rainDrops++;
            }
        }
    }

    public void AddSnowflakes(int snowFlakesToSpawn)
    {
        if (room != null && room.BeingViewed)
        {
            if (this.camPos != null)
            {
                for (int i = 0; i < snowFlakesToSpawn; i++)
                {
                    try
                    {
                        //Get a random position within range of the RoomCamera
                        Vector2 cam = this.camPos;
                        IntVector2 randomOffset = IntVector2.FromVector2(new Vector2(cam.x + UnityEngine.Random.Range(-700, 700), cam.y + UnityEngine.Random.Range(-500, 500)));
                        Vector2 offset2 = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
                        //If that random position has line of sight with the sky, spawn a snowflake there
                        Vector2 spawn = randomOffset.ToVector2();
                        Vector2 spawnPos = spawn + offset2;
                        if (RayTraceSky(spawnPos, new Vector2(0f, 1f)))
                        {
                            SnowFlake snowFlake = new SnowFlake(spawnPos, Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), RainFall.rainIntensity, this);
                            this.room.AddObject(snowFlake);
                            //snowFlake.reset = true;
                            for (int s = 0; s < 1000; s++)
                            {
                                snowFlake.Update(true);
                            }
                            this.snowFlakes++;
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
        Vector2 corner = Custom.RectCollision(pos, pos + testDir * 100000f, this.room.RoomRect).GetCorner(FloatRect.CornerLabel.D);
        if (SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, pos, corner) != null)
        {
            return false;
        }
        if (corner.y >= this.room.PixelHeight - 5f)
        {
            return true;
        }
        return false;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (Downpour.debug && this.room.BeingViewed && Input.GetKeyDown(KeyCode.F5))
        {
            this.room.AddObject(new LightningStrike(this));
        }
        if (this.room.BeingViewed)
        {
            this.camPos = this.room.game.cameras[0].pos + new Vector2(this.room.game.rainWorld.screenSize.x / 2, this.room.game.rainWorld.screenSize.y / 2);
        }
        this.isSnow = Downpour.snow;
        if (isSnow)
        {
            this.rainAmount = Mathf.Lerp(Downpour.rainAmount * 0.5f, Downpour.rainAmount, RainFall.rainIntensity);
            this.rainLimit = (int)Mathf.Lerp(this.rainAmount * 50, (this.rainAmount * 60), RainFall.rainIntensity);
        }
        else
        {
            this.rainAmount = Mathf.Lerp(0, Downpour.rainAmount, RainFall.rainIntensity);
            this.rainLimit = (int)Mathf.Lerp(0, (this.rainAmount * 9), RainFall.rainIntensity);
        }
        this.player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);

        if (this.room.game != null && this.room != null && !room.abstractRoom.gate && this.room.ReadyForPlayer)
        {
            if (this.room.game != null && !this.room.abstractRoom.shelter && Downpour.lightning && this.room.roomRain != null)
            {
                if (this.room.roomRain.dangerType == RoomRain.DangerType.Rain && RainFall.rainIntensity > 0.7f && this.room.lightning == null)
                {
                    this.room.lightning = new Lightning(this.room, 1f, false);
                    this.room.lightning.bkgOnly = true;
                    this.room.AddObject(this.room.lightning);
                }
                if (this.room.roomRain.dangerType == RoomRain.DangerType.FloodAndRain && RainFall.rainIntensity > 0.7f && this.room.lightning == null)
                {
                    this.room.lightning = new Lightning(this.room, 1f, false);
                    this.room.lightning.bkgOnly = true;
                    this.room.AddObject(this.room.lightning);
                }
            }
            if (!isSnow)
            {
                this.snowFlakes = 0;
                if (!RainFall.noRainThisCycle && this.rainDrops < ((this.room.Width - this.ceilingCount) * this.rainLimit) / this.room.Width)
                {
                    this.AddRaindrops(rainLimit - this.rainDrops);
                }
                if (this.room.lightning != null && this.room.BeingViewed && this.room.roomRain != null && this.room.roomRain.dangerType == RoomRain.DangerType.Rain && Downpour.strike && UnityEngine.Random.value < RainFall.rainIntensity * 0.0010f)
                {
                    this.room.AddObject(new LightningStrike(this));
                }
            }
            else
            {
                this.rainDrops = 0;
                if (!RainFall.noRainThisCycle && this.snowFlakes < ((this.room.Width - this.ceilingCount) * this.rainLimit) / this.room.Width)
                {
                    this.AddSnowflakes(rainLimit - this.snowFlakes);
                }
                if (this.room.lightning != null && this.room.BeingViewed && this.room.roomRain != null && this.room.roomRain.dangerType == RoomRain.DangerType.Rain && UnityEngine.Random.value < 0.0004f)
                {
                    this.room.AddObject(new LightningStrike(this));
                }
            }
        }
        if (Downpour.snow && Downpour.dust && RainFall.rainList.Contains(this.room.abstractRoom.name))
        {
            for (int i = 0; i < this.room.game.Players.Count; i++)
            {
                if (this.room.game.Players[i].realizedCreature != null && this.room.game.Players[i].realizedCreature.room == this.room)
                {
                    for (int j = 0; j < this.room.game.Players[i].realizedCreature.bodyChunks.Length; j++)
                    {
                        if (this.room.game.Players[i].realizedCreature.bodyChunks[j].ContactPoint.y < 0)
                        {
                            if (this.room.game.Players[i].realizedCreature.bodyChunks[j].lastContactPoint.y >= 0 && this.room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.y - this.room.game.Players[i].realizedCreature.bodyChunks[j].pos.y > 5f)
                            {
                                this.room.AddObject(new SnowDust(this.room.game.Players[i].realizedCreature.bodyChunks[j].pos + new Vector2(0f, -this.room.game.Players[i].realizedCreature.bodyChunks[j].rad), Custom.LerpMap(this.room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.y - this.room.game.Players[i].realizedCreature.bodyChunks[j].pos.y, 5f, 10f, 0.5f, 1f)));
                            }
                            else if (UnityEngine.Random.value < 0.1f && Mathf.Abs(this.room.game.Players[i].realizedCreature.bodyChunks[j].lastPos.x - this.room.game.Players[i].realizedCreature.bodyChunks[j].pos.x) > 3f)
                            {
                                this.room.AddObject(new SnowDust(this.room.game.Players[i].realizedCreature.bodyChunks[j].pos + new Vector2(0f, -this.room.game.Players[i].realizedCreature.bodyChunks[j].rad), 0.25f * UnityEngine.Random.value));
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
        this.alpha = UnityEngine.Random.Range(0.9f, 1f);
        this.spawner = spawner;
        this.timeToDie = false;
        this.splashCounter = 0;
        this.collision = false;
        //Small chance for any raindrop to be a background drop, assign it a random depth value
        if (Downpour.bg && UnityEngine.Random.value > 0.85f)
        {
            backgroundDrop = true;
            this.depth = UnityEngine.Random.value;
        }
        else
        {
            backgroundDrop = false;
            this.depth = UnityEngine.Random.Range(0.7f, 1f);
        }
        if (Downpour.rainbow)
        {
            this.color = Custom.HSL2RGB(UnityEngine.Random.value, 0.5f, 0.5f);
        }
        else
        {
            this.color = color;
        }
        this.resetPos = new Vector2(pos.x, spawner.room.RoomRect.top + 150f);
        this.pos = new Vector2(pos.x, UnityEngine.Random.Range(pos.y, spawner.room.RoomRect.top + 150f));
        this.lastPos = this.pos;
        this.lastLastPos = this.pos;
        this.lastLastLastPos = this.pos;
        //Vary starting velocity
        this.vel.y = UnityEngine.Random.Range(-10f * rainIntensity, -20f * rainIntensity);
        //Increase spread of raindrops as rain intensity increases
        this.vel.x = Mathf.Lerp(UnityEngine.Random.Range(-4, 3f), UnityEngine.Random.Range(-12f, 3f), RainFall.rainIntensity);
        this.gravity = Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value);
    }
    public override void Update(bool eu)
    {
        this.player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);
        if (this.reset)
        {
            float rng = UnityEngine.Random.value;
            if (rng < 0.05f && this.room.world.rainCycle.RainDarkPalette > 0)
            {
                this.Destroy();
            }
            if (Downpour.snow)
            {
                this.Destroy();
            }
            if (player != null && player.mainBodyChunk != null && !player.inShortcut)
            {
                this.resetPos = new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1300f, player.mainBodyChunk.pos.x + 1300f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            else
            {
                this.resetPos = new Vector2(UnityEngine.Random.Range(this.room.RoomRect.left - 100f, this.room.RoomRect.right + 100f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            this.pos = this.resetPos;
            this.lastPos = this.resetPos;
            this.lastLastPos = this.resetPos;
            this.lastLastLastPos = this.resetPos;
            this.collision = false;
            this.timeToDie = false;
            switch (spawner.direction)
            {
                case 1:
                    this.vel.x = Mathf.Lerp(UnityEngine.Random.Range(8f, -4f), UnityEngine.Random.Range(-13f, -4f), Mathf.Lerp(0f, 1f, RainFall.rainIntensity));
                    break;
                case 2:
                    this.vel.x = Mathf.Lerp(UnityEngine.Random.Range(5f, -5f), UnityEngine.Random.Range(-2f, 2f), Mathf.Lerp(0f, 1f, RainFall.rainIntensity));
                    break;
                case 3:
                    this.vel.x = Mathf.Lerp(UnityEngine.Random.Range(-8f, 4f), UnityEngine.Random.Range(4f, 13f), Mathf.Lerp(0f, 1f, RainFall.rainIntensity));
                    break;
            }
            this.vel.y = UnityEngine.Random.Range(-10f * RainFall.rainIntensity, -18f * RainFall.rainIntensity);
            this.splashCounter = 0f;
            this.reset = false;
        }
        this.lastLastLastPos = this.lastLastPos;
        this.lastLastPos = this.lastPos;
        //Control fall speed of normal and background drops, tied to rain intensity.
        if (backgroundDrop)
        {
            if (collision)
            {
                this.vel.y = 0f;
            }
            else
            {
                this.vel.y = this.vel.y - (this.gravity * 3.9f);
                if (this.vel.y < Mathf.Lerp(-20f, -35f, RainFall.rainIntensity))
                {
                    this.vel.y = Mathf.Lerp(-20f, -35f, RainFall.rainIntensity);
                }
            }
        }
        else
        {
            if (collision)
            {
                this.vel.y = this.vel.y - (this.gravity * 2.5f);
                if (this.vel.y < Mathf.Lerp(-1f, -2f, RainFall.rainIntensity))
                {
                    this.vel.y = Mathf.Lerp(-1f, -2f, RainFall.rainIntensity);
                }
                if (this.vel.x < Mathf.Lerp(-1f, -4f, RainFall.rainIntensity))
                {
                    this.vel.x = Mathf.Lerp(-1f, -4f, RainFall.rainIntensity);
                }
            }
            else
            {
                this.vel.y = this.vel.y - (this.gravity * 9.5f);
                if (this.vel.y < Mathf.Lerp(-32f, -40f, RainFall.rainIntensity))
                {
                    this.vel.y = Mathf.Lerp(-32f, -40f, RainFall.rainIntensity);
                }
            }
        }
        //Decrease splash scale on hit
        if (collision)
        {
            this.splashCounter = this.splashCounter - 0.1f;
            if (splashCounter < 0f)
            {
                splashCounter = 0f;
            }
        }
        //Raindrop hits floor or water
        bool hitWater = this.room.GetTile(this.pos).WaterSurface;
        if (hitWater && Downpour.water)
        {
            if (room.water && UnityEngine.Random.value > 0.98)
            {
                room.waterObject?.Explosion(this.pos, 0.45f, 0.89f);
            }
            this.pos.y = this.room.MiddleOfTile(this.pos).y + UnityEngine.Random.Range(2f, 9f);
            this.vel.y = this.vel.y * -0.01f;
            this.vel.x = this.vel.x * 0.2f;
            timeToDie = true;
            splashCounter = UnityEngine.Random.Range(0.9f, 1.1f);
            collision = true;
        }
        //Raindrop hits floor or water
        if (this.room.GetTile(this.pos).Solid || hitWater)
        {
            //Decrease velocity if raindrop hits a solid surface or water and increase splash counter
            if (UnityEngine.Random.value > 0.01f)
            {
                if (this.vel.y < 0f && !timeToDie)
                {
                    this.pos.y = this.room.MiddleOfTile(this.pos).y + 11f;
                    this.vel.y = this.vel.y * -0.01f;
                    this.vel.x = this.vel.x * 0.2f;
                    timeToDie = true;
                    splashCounter = UnityEngine.Random.Range(0.9f, 1.1f);
                    collision = true;
                }
                else
                {
                    this.reset = true;
                }
            }
        }
        //If raindrop falls below room bottom, or if rain intensity is 0, remove it.
        if (this.pos.y < -100f || RainFall.rainIntensity == 0f)
        {
            this.reset = true;
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
            sLeaser.sprites[1].alpha = Mathf.Lerp(0.6f, 1f, this.depth);
        }
        else
        {
            sLeaser.sprites[1].alpha = UnityEngine.Random.Range(0.82f, 1f);
        }
        sLeaser.sprites[0].alpha = this.alpha;
        sLeaser.sprites[0].color = Color.Lerp(rCam.currentPalette.fogColor, color, this.depth * 0.9f);
        sLeaser.sprites[1].color = Color.Lerp(rCam.currentPalette.fogColor, color, this.depth * 0.9f);
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {

        Vector2 vector = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
        Vector2 vector2 = Vector2.Lerp(this.lastLastLastPos, this.lastLastPos, timeStacker);
        if (Custom.DistLess(vector, vector2, 9f))
        {
            vector2 = vector + Custom.DirVec(vector, vector2) * 12f;
        }
        vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, 1f));
        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + a * (this.depth + (this.depth * 0.1f)) - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - a * (this.depth + (this.depth * 0.1f)) - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
        sLeaser.sprites[1].x = vector.x - camPos.x;
        sLeaser.sprites[1].y = vector.y - camPos.y;
        sLeaser.sprites[1].rotation = UnityEngine.Random.value * 360f;
        if (Downpour.rainbow)
        {
            sLeaser.sprites[0].color = Color.Lerp(color, Color.Lerp(rCam.PixelColorAtCoordinate(this.pos), rCam.PixelColorAtCoordinate(this.lastLastLastPos), 0.5f), 0.36f);
        }
        else
        {
            sLeaser.sprites[0].color = Color.Lerp(new Color(1f, 1f, 1f), Color.Lerp(rCam.currentPalette.fogColor, Color.Lerp(rCam.PixelColorAtCoordinate(this.pos), rCam.PixelColorAtCoordinate(this.lastLastLastPos), 0.5f), 0.83f), 0.95f);
        }
        //If background drops encounter a depth in the room texture lower than their own depth value, treat it as a collision.
        if (backgroundDrop && !reset && !this.collision && rCam.IsViewedByCameraPosition(rCam.cameraNumber, this.pos) && rCam.DepthAtCoordinate(this.pos) < this.depth)
        {
            splashCounter = 1f;
            timeToDie = true;
            sLeaser.sprites[1].color = Color.Lerp(color, rCam.PixelColorAtCoordinate(this.pos), 0.7f);
            this.collision = true;
        }
        if (splashCounter > 0f && !backgroundDrop)
        {
            sLeaser.sprites[1].color = Color.Lerp(color, rCam.PixelColorAtCoordinate(this.pos), 0.2f);
        }
        if (splashCounter > 0f)
        {
            //If splash counter is greater than 0 adjust the scale of the splash sprite based on whether its a background drop or not.
            if (backgroundDrop)
            {
                sLeaser.sprites[1].scale = Mathf.Lerp(0f, this.depth * 0.4f, splashCounter);
                this.vel.y = 0f;
                this.vel.x = 0f;
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
            sLeaser.sprites[0].alpha = this.alpha;
        }
        //Delete raindrop if it falls a certain distance below the currently viewed room camera
        if (this.pos.y < (rCam.pos.y - 100f) || (splashCounter <= 0f && timeToDie))
        {
            this.reset = true;
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
