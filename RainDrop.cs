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
    public List<RainDrop> rainDrops = new List<RainDrop>();
    public float rainAmount;

    public Preciptator(Room room, bool isSnow)
    {
        this.currentRoom = room;
        this.isSnow = isSnow;
        if (Downpour.rainAmount == 0)
        {
            this.rainAmount = Mathf.Lerp(0, 60, RainFall.rainIntensity);
        }
        else
        {
            this.rainAmount = Mathf.Lerp(0, Downpour.rainAmount, RainFall.rainIntensity);
        }
    }

    public void AddRaindrop()
    {
        Vector2 spawn = new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1400f, player.mainBodyChunk.pos.x + 1400f), room.RoomRect.top - 5f);
        IntVector2 tilePos = room.GetTilePosition(spawn);
        if (room.RayTraceTilesForTerrain(tilePos.x, tilePos.y, tilePos.x, tilePos.y - 5))
        {
            RainDrop rainDrop =  new RainDrop(new Vector2(spawn.x, spawn.y + 200f), new Vector2(UnityEngine.Random.Range(-1f, 1f), -20f), Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.12f), 10, 10, this.room, false, RainFall.rainIntensity);
            this.room.AddObject(rainDrop);
            this.rainDrops.Add(rainDrop);
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        this.player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);
        if (this.player != null && room != null)
        {
            for (int i = this.rainDrops.Count - 1; i >= 0; i--)
            {
                if (this.rainDrops[i].slatedForDeletetion)
                {
                    this.rainDrops.RemoveAt(i);
                }
            }
            if (this.rainDrops.Count < 700)
            {
                for(int i = 0; i < this.rainAmount; i++)
                {
                    this.AddRaindrop();
                }
            }
        }
    }
}
public class RainDrop : CosmeticSprite
{
    public float life;
    public float lastLife;
    public int lifeTime;
    public Vector2 lastLastLastPos;
    public Vector2 lastLastPos;
    public Color color;
    public float gravity;
    public bool foreground;
    public float screenXPos;
    public bool isSnow;
    public float splashCounter;
    public bool timeToDie;
    public Vector2 dir;
    public bool invert;
    public float dirCounter;
    public bool backgroundDrop;

    public RainDrop(Vector2 pos, Vector2 vel, Color color, int standardLifeTime, int exceptionalLifeTime, Room room, bool isSnow, float rainIntensity)
    {
        this.timeToDie = false;

        invert = false;
        this.isSnow = isSnow;
        this.life = 1f;
        this.foreground = false;
        this.lastLife = 1f;
        this.splashCounter = 0;
        if (UnityEngine.Random.Range(0f, 1f) > 0.98f && !isSnow)
        {
            backgroundDrop = true;
        }
        else
        {
            backgroundDrop = false;
        }
        if (Downpour.rainbow)
        {
            this.color = Custom.HSL2RGB(UnityEngine.Random.Range(0f, 1f), 0.5f, 0.5f);
        }
        else if (isSnow)
        {
            this.color = Color.Lerp(color, new Color(1f, 1f, 1f), 0.7f);
        }
        else
        {
            this.color = color;
        }
        this.pos = pos;
        this.lastPos = this.pos;
        this.lastLastPos = this.pos;
        this.lastLastLastPos = this.pos;
        this.dir = (new Vector2(UnityEngine.Random.Range(-6f, 0.1f), -5f) * rainIntensity);
        if (isSnow)
        {
            this.vel = this.dir;
            this.pos += vel * (2f * rainIntensity);
        }
        else
        {
            this.vel = vel;
            this.vel.x = this.vel.x + (UnityEngine.Random.Range(rainIntensity * -12, 1f));
            this.pos += vel * 3f;
        }
        this.gravity = Mathf.Lerp(0.4f, 0.9f, UnityEngine.Random.value);
        this.lifeTime = UnityEngine.Random.Range(0, standardLifeTime);
    }
    public override void Update(bool eu)
    {
        this.lastLastLastPos = this.lastLastPos;
        this.lastLastPos = this.lastPos;
        if (isSnow)
        {
            //Slowly increase horizontal speed.
            this.vel.x = this.vel.x * (1f + (RainFall.rainIntensity * 0.005f));
            this.vel.y = this.vel.y - (this.gravity * 0.5f) * (RainFall.rainIntensity * 2f);
            if (backgroundDrop)
            {
                if (this.vel.y < (-5f * RainFall.rainIntensity))
                {
                    this.vel.y = (-5f * RainFall.rainIntensity);
                }
            }
            else
            {
                if (this.vel.y < (-10f * RainFall.rainIntensity))
                {
                    this.vel.y = (-10f * RainFall.rainIntensity);
                }
            }
            //If horizontal speed exceeds 5f, invert the direction it was heading and increase dirCounter
            if ((vel.x > 5f * (RainFall.rainIntensity * 1.4f) || vel.x < -5f * (RainFall.rainIntensity * 1.4f)) && dirCounter <= 0f)
            {
                this.dir = new Vector2(-this.dir.x, this.dir.y);
                dirCounter = UnityEngine.Random.Range(1f, 5f);
            }
            this.vel += this.dir * 0.03f;
        }
        else
        {
            if (backgroundDrop)
            {
                this.vel.y = this.vel.y - (this.gravity * 1.9f);
                if (this.vel.y < -35f)
                {
                    this.vel.y = -35f;
                }
            }
            else
            {
                this.vel.y = this.vel.y - (this.gravity * 2.5f);
                if (this.vel.y < -50f)
                {
                    this.vel.y = -50f;
                }
            }
        }
        this.splashCounter = this.splashCounter - 0.1f;
        if (splashCounter < 0f)
        {
            splashCounter = 0f;
        }
        this.dirCounter = this.dirCounter - (0.1f + (RainFall.rainIntensity * 0.3f));
        if (dirCounter < 0f)
        {
            dirCounter = 0;
        }
        this.lastLife = this.life;
        //Background drop hits midground
        if (backgroundDrop && this.room.aimap != null && this.room.aimap.getAItile(this.room.GetTilePosition(this.pos)).walkable)
        {
            if (isSnow)
            {
                this.slatedForDeletetion = true;
            }
            else if (!timeToDie)
            {
                this.pos.y = this.room.MiddleOfTile(this.pos).y + UnityEngine.Random.Range(4f, 14f);
                this.vel.y = 0f;
                this.vel.x = 0f;
                timeToDie = true;
                splashCounter = 0.4f;
            }
        }
        //Raindrop hits floor
        if ((this.room.GetTile(this.pos).Terrain == Room.Tile.TerrainType.Solid || this.room.GetTile(this.pos).Solid || this.room.GetTile(this.pos).AnyWater) && !foreground)
        {
            if (isSnow && !backgroundDrop)
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f)
                {
                    foreground = true;
                }
                else
                {
                    this.slatedForDeletetion = true;
                }
            }
            //If a raindrop hits a solid surface it's velocity is forced upwards so it appears to 'bounce'.
            else if (UnityEngine.Random.Range(0f, 1f) > 0.01f)
            {
                if (this.vel.y < 0f && this.room.GetTile(this.pos + new Vector2(0f, 20f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    if (this.room.GetTile(this.pos).AnyWater)
                    {
                        this.pos.y = this.room.MiddleOfTile(this.pos).y + UnityEngine.Random.Range(-2f, 6f);
                    }
                    else
                    {
                        this.pos.y = this.room.MiddleOfTile(this.pos).y + 9f;
                    }
                    this.vel.y = this.vel.y * -0.1f;
                    this.vel.x = this.vel.x * -0.5f;
                    this.life -= 0.083333343f;
                    if (Mathf.Abs(this.vel.y) < 2f)
                    {
                        this.life -= 0.083333343f;
                    }
                    timeToDie = true;
                    splashCounter = 0.4f;
                }
                else
                {
                    this.slatedForDeletetion = true;
                }
            }
            //There is a small chance when the raindrop hits a surface that it will become a part of the foreground layer, passing in front of tiles.
            else
            {
                foreground = true;
            }
        }
        if (splashCounter <= 0f && timeToDie)
        {
            this.slatedForDeletetion = true;
        }
        if (this.life <= 0f || (this.vel.y == 0f && !backgroundDrop) || this.pos.y < -100f || RainFall.rainIntensity == 0f)
        {
            this.slatedForDeletetion = true;
        }
        if (slatedForDeletetion)
        {
            this.Destroy();
        }
        base.Update(eu);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (isSnow)
        {
            sLeaser.sprites = new FSprite[1];
            if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
            {
                sLeaser.sprites[0] = new FSprite("SkyDandelion", true);
                sLeaser.sprites[0].scale = 0.20f + (RainFall.rainIntensity * 0.2f);
            }
            else
            {
                sLeaser.sprites[0] = new FSprite("deerEyeB", true);
                sLeaser.sprites[0].scale = 0.40f + (RainFall.rainIntensity * 0.2f);
            }
            sLeaser.sprites[0].color = color;
        }
        else
        {
            if (backgroundDrop)
            {
                sLeaser.sprites = new FSprite[2];
                TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
                {
            new TriangleMesh.Triangle(0, 1, 2)
                };
                TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
                sLeaser.sprites[0] = triangleMesh;
                sLeaser.sprites[0].scale = 0.7f;
                sLeaser.sprites[1] = new FSprite("RainSplash", true);
                sLeaser.sprites[0].color = Color.Lerp(color, new Color(0f, 0f, 0f), 0.33f);
                sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
                sLeaser.sprites[1].alpha = 1f;
                sLeaser.sprites[1].color = Color.Lerp(color, new Color(0f, 0f, 0f), 0.2f);
                sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
            }
            else
            {
                sLeaser.sprites = new FSprite[2];
                TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
                {
            new TriangleMesh.Triangle(0, 1, 2)
                };
                TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
                sLeaser.sprites[0] = triangleMesh;
                sLeaser.sprites[1] = new FSprite("RainSplash", true);
                sLeaser.sprites[0].color = color;
                sLeaser.sprites[1].alpha = 1f;
                sLeaser.sprites[1].color = color;
            }
        }
        sLeaser.sprites[0].alpha = UnityEngine.Random.Range(0.7f, 1f);
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (rCam.DistanceFromViewedScreen(this.pos) < 100f)
        {
            if (isSnow)
            {
                sLeaser.sprites[0].x = Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker) - camPos.x;
                sLeaser.sprites[0].y = Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker) - camPos.y;
                sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.lastLastPos, this.lastPos, timeStacker), Vector2.Lerp(this.lastPos, this.pos, timeStacker));
                sLeaser.sprites[0].scaleY = Mathf.Max(0.45f, 0.45f + 0.1f * Vector2.Distance(Vector2.Lerp(this.lastLastPos, this.lastPos, timeStacker), Vector2.Lerp(this.lastPos, this.pos, timeStacker)));
            }
            else
            {
                Vector2 vector = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
                Vector2 vector2 = Vector2.Lerp(this.lastLastLastPos, this.lastLastPos, timeStacker);
                if (Custom.DistLess(vector, vector2, 9f))
                {
                    vector2 = vector + Custom.DirVec(vector, vector2) * 9f;
                }
                vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, this.life));
                Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + a * 1f - camPos);
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - a * 1f - camPos);
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
                sLeaser.sprites[1].x = vector.x - camPos.x;
                sLeaser.sprites[1].y = vector.y - camPos.y;
                if (backgroundDrop)
                {
                    sLeaser.sprites[1].scale = splashCounter * 0.8f;
                }
                else
                {
                    sLeaser.sprites[1].scale = splashCounter * 1.2f;
                }
                sLeaser.sprites[1].rotation = UnityEngine.Random.value * 360f;
            }
            if (this.foreground)
            {
                this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
                if (!isSnow)
                {
                    sLeaser.sprites[0].alpha = sLeaser.sprites[0].alpha - 0.07f;
                }
                else
                {
                    sLeaser.sprites[0].alpha = sLeaser.sprites[0].alpha - 0.02f;
                }
            }
        }
        //Delete raindrop if it falls a certain distance below the current camera
        if (rCam.room.BeingViewed && this.pos.y < (rCam.pos.y - 200f))
        {
            this.slatedForDeletetion = true;
        }
        if (sLeaser.sprites[0].alpha < 0f)
        {
            this.slatedForDeletetion = true;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {

    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}

