using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

public class SnowFlake : CosmeticSprite
{
    public Vector2 lastLastLastPos;
    public Vector2 lastLastPos;
    public Color color;
    public float gravity;
    public bool foreground;
    public float screenXPos;
    public float splashCounter;
    public bool timeToDie;
    public Vector2 dir;
    public bool invert;
    public float dirCounter;
    public bool backgroundDrop;
    public float depth;
    public bool collision;
    public bool reset;
    public Player player;
    public Vector2 resetPos;
    public Preciptator spawner;
    public Vector2 offset;


    public SnowFlake(Vector2 pos, Color color, float rainIntensity, Preciptator spawner)
    {
        this.timeToDie = false;
        this.foreground = false;
        this.splashCounter = 0;
        this.collision = false;
        this.spawner = spawner;
        this.offset = new Vector2(UnityEngine.Random.Range(-70f, 70f), UnityEngine.Random.Range(-60f, 60f));
        if (Downpour.rainbow)
        {
            this.color = Custom.HSL2RGB(UnityEngine.Random.Range(0f, 1f), 0.5f, 0.5f);
        }
        else
        {
            this.color = Color.Lerp(color, new Color(1f, 1f, 1f), 0.7f);
        }
        this.resetPos = pos;
        this.pos = new Vector2(pos.x, pos.y - UnityEngine.Random.Range(100f, 600f));
        this.lastPos = this.pos;
        this.lastLastPos = this.pos;
        this.lastLastLastPos = this.pos;
        this.dir = (new Vector2(UnityEngine.Random.Range(-6f, 0.1f), -5f) * rainIntensity);
        this.vel = this.dir;
        this.pos += vel * (2f * rainIntensity);
        this.gravity = Mathf.Lerp(0.4f, 0.9f, UnityEngine.Random.value);
    }
    public override void Update(bool eu)
    {
        if (!Downpour.snow)
        {
            this.Destroy();
        }
        this.player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);
        if (this.reset)
        {
            float rng = UnityEngine.Random.value;
            if (rng < 0.05f && this.room.world.rainCycle.RainDarkPalette > 0)
            {
                this.Destroy();
            }
            if (player.mainBodyChunk != null && !player.inShortcut)
            {
                this.resetPos = new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1300f, player.mainBodyChunk.pos.x + 1300f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            else
            {
                this.resetPos = new Vector2(UnityEngine.Random.Range(this.room.RoomRect.left - 100f, this.room.RoomRect.right + 100f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            this.dir = (new Vector2(UnityEngine.Random.Range(-4f, 4f), -5f) * RainFall.rainIntensity);
            this.vel = this.dir;
            this.pos = this.resetPos;
            this.lastPos = this.resetPos;
            this.lastLastPos = this.resetPos;
            this.lastLastLastPos = this.resetPos;
            this.collision = false;
            this.timeToDie = false;
            this.foreground = false;
            this.reset = false;
        }
        this.lastLastLastPos = this.lastLastPos;
        this.lastLastPos = this.lastPos;
        this.vel.x = this.vel.x * (1f + (RainFall.rainIntensity * 0.005f));
        this.vel.y = this.vel.y - (this.gravity * 0.5f) * (RainFall.rainIntensity * 2f);
        if (this.vel.y < (-10f * RainFall.rainIntensity))
        {
            this.vel.y = (-10f * RainFall.rainIntensity);
        }
        if ((vel.x > 5f * (RainFall.rainIntensity * 1.1f) || vel.x < -5f * (RainFall.rainIntensity * 1.1f)) && dirCounter <= 0f)
        {
            this.dir = new Vector2(-this.dir.x, this.dir.y);
            dirCounter = UnityEngine.Random.Range(1f, 5f);
        }
        this.vel += this.dir * 0.03f;
        this.dirCounter = this.dirCounter - (0.1f + (RainFall.rainIntensity * 0.3f));
        if (dirCounter < 0f)
        {
            dirCounter = 0;
        }
        if ((this.room.GetTile(this.pos).Terrain == Room.Tile.TerrainType.Solid || this.room.GetTile(this.pos).Solid || this.room.GetTile(this.pos).AnyWater) && !foreground)
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.1f)
            {
                foreground = true;
            }
            else
            {
                this.reset = true;
            }
        }
        if (this.pos.y < -100f)
        {
            this.reset = true;
        }
        base.Update(eu);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
        {
            sLeaser.sprites[0] = new FSprite("SkyDandelion", true);
            sLeaser.sprites[0].scale = 0.15f + (RainFall.rainIntensity * 0.2f);
        }
        else
        {
            sLeaser.sprites[0] = new FSprite("deerEyeB", true);
            sLeaser.sprites[0].scale = 0.33f + (RainFall.rainIntensity * 0.2f);
        }
        sLeaser.sprites[0].color = color;
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (reset)
        {
            sLeaser.sprites[0].alpha = UnityEngine.Random.Range(0.85f, 1f);
        }
        sLeaser.sprites[0].x = Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.lastLastPos, this.lastPos, timeStacker), Vector2.Lerp(this.lastPos, this.pos, timeStacker));
        sLeaser.sprites[0].scaleY = Mathf.Max(0.45f, 0.45f + 0.1f * Vector2.Distance(Vector2.Lerp(this.lastLastPos, this.lastPos, timeStacker), Vector2.Lerp(this.lastPos, this.pos, timeStacker)));
        if (this.foreground)
        {
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
            sLeaser.sprites[0].alpha = sLeaser.sprites[0].alpha - 0.02f;
        }
        if (rCam.room.BeingViewed && this.pos.y < (rCam.pos.y - 100f))
        {
            this.reset = true;
        }
        if (sLeaser.sprites[0].alpha < 0f)
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

