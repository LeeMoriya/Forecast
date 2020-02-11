using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

public class SnowDrop : CosmeticSprite
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
    public float depth;
    public bool collision;

    public SnowDrop(Vector2 pos, Vector2 vel, Color color, int standardLifeTime, bool isSnow, float rainIntensity)
    {
        this.timeToDie = false;
        this.isSnow = isSnow;
        this.life = 1f;
        this.foreground = false;
        this.lastLife = 1f;
        this.splashCounter = 0;
        this.collision = false;
        if (UnityEngine.Random.Range(0f, 1f) > 0.8f && !isSnow)
        {
            backgroundDrop = true;
            this.depth = UnityEngine.Random.Range(0.2f, 1f);
        }
        else
        {
            backgroundDrop = false;
            this.depth = 1f;
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
            this.vel.x = this.vel.x + (UnityEngine.Random.Range(Mathf.Lerp(-3f, -15f, rainIntensity), Mathf.Lerp(-4, 2f, rainIntensity)));
            this.pos += vel * 3f;
        }
        this.gravity = Mathf.Lerp(0.4f, 0.9f, UnityEngine.Random.value);
        this.lifeTime = UnityEngine.Random.Range(0, standardLifeTime);
    }
    public override void Update(bool eu)
    {
        if (slatedForDeletetion)
        {
            this.Destroy();
        }
        this.lastLastLastPos = this.lastLastPos;
        this.lastLastPos = this.lastPos;
        //SNOW - Velocity
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
        //RAIN - Y Velocity
        {
            if (backgroundDrop)
            {
                if (!collision)
                {
                    this.vel.y = this.vel.y - (this.gravity * 3.9f);
                    if (this.vel.y < Mathf.Lerp(-20f, -35f, RainFall.rainIntensity))
                    {
                        this.vel.y = Mathf.Lerp(-20f, -35f, RainFall.rainIntensity);
                    }
                }
                else
                {
                    this.vel.y = 0f;
                }
            }
            else
            {
                this.vel.y = this.vel.y - (this.gravity * 9.5f);
                if (this.vel.y < Mathf.Lerp(-30f, -40f, RainFall.rainIntensity))
                {
                    this.vel.y = Mathf.Lerp(-30f, -40f, RainFall.rainIntensity);
                }
            }
        }
        this.splashCounter = this.splashCounter - 0.25f;
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
        //Raindrop hits floor
        if ((this.room.GetTile(this.pos).Terrain == Room.Tile.TerrainType.Solid || this.room.GetTile(this.pos).Solid || this.room.GetTile(this.pos).AnyWater) && !foreground)
        {
            if (isSnow)
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
                        this.pos.y = this.room.MiddleOfTile(this.pos).y + UnityEngine.Random.Range(-4f, 3f);
                    }
                    else
                    {
                        this.pos.y = this.room.MiddleOfTile(this.pos).y + 9f;
                    }
                    this.vel.y = this.vel.y * -0.1f;
                    this.vel.x = this.vel.x * -0.9f;
                    this.life -= 0.083333343f;
                    if (Mathf.Abs(this.vel.y) < 2f)
                    {
                        this.life -= 0.083333343f;
                    }
                    timeToDie = true;
                    splashCounter = 0.7f;
                }
                else
                {
                    this.slatedForDeletetion = true;
                }
            }
            //There is a small chance when the raindrop hits a surface that it will become a part of the foreground layer, passing in front of tiles.
            else if (!backgroundDrop)
            {
                foreground = true;
            }
        }
        if (this.life <= 0f || this.pos.y < -100f || RainFall.rainIntensity == 0f)
        {
            this.slatedForDeletetion = true;
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
            sLeaser.sprites = new FSprite[2];
            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
{
            new TriangleMesh.Triangle(0, 1, 2)
};
            TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
            sLeaser.sprites[0] = triangleMesh;
            sLeaser.sprites[1] = new FSprite("RainSplash", true);
            sLeaser.sprites[1].alpha = Mathf.Lerp(0.6f, 1f, this.depth);
            sLeaser.sprites[0].color = Color.Lerp(new Color(color.r - 0.4f, color.g - 0.4f, color.b - 0.4f), color, this.depth);
            sLeaser.sprites[1].color = Color.Lerp(new Color(color.r - 0.4f, color.g - 0.4f, color.b - 0.4f), color, this.depth);
            if (backgroundDrop)
            {
                sLeaser.sprites[1].height = sLeaser.sprites[1].height * 0.5f;
            }
        }
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
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
            if (backgroundDrop)
            {
                Vector2 vector3 = rCam.ApplyDepth(this.pos, this.depth);
                sLeaser.sprites[1].x = vector3.x - camPos.x;
                sLeaser.sprites[1].y = vector3.y - camPos.y;
                sLeaser.sprites[1].sortZ = this.depth;
            }
            else
            {
                sLeaser.sprites[1].x = vector.x - camPos.x;
                sLeaser.sprites[1].y = vector.y - camPos.y;
            }
            sLeaser.sprites[1].rotation = UnityEngine.Random.value * 360f;
            if (backgroundDrop && !slatedForDeletetion && !this.collision && rCam.IsViewedByCameraPosition(rCam.cameraNumber, this.pos) && rCam.DepthAtCoordinate(this.pos) < this.depth)
            {
                this.vel.y = 0f;
                this.vel.x = 0f;
                timeToDie = true;
                sLeaser.sprites[0].alpha = 0f;
                splashCounter = 1f;
                this.collision = true;
            }
            if (backgroundDrop)
            {
                sLeaser.sprites[1].scale = Mathf.Lerp(0f, this.depth * 0.4f, splashCounter);
            }
            else
            {
                sLeaser.sprites[1].scale = Mathf.Lerp(0f, this.depth * 0.32f, splashCounter);
            }
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
        //Delete raindrop if it falls a certain distance below the current camera
        if (rCam.room.BeingViewed && this.pos.y < (rCam.pos.y - 100f))
        {
            this.slatedForDeletetion = true;
        }
        if (sLeaser.sprites[0].alpha < 0f)
        {
            this.slatedForDeletetion = true;
        }
        if (splashCounter <= 0f && timeToDie)
        {
            this.slatedForDeletetion = true;
        }
        if (base.slatedForDeletetion || this.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
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

