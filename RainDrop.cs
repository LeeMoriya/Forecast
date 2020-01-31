﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;


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
    public bool transitionRain;

    public RainDrop(Vector2 pos, Vector2 vel, Color color, int standardLifeTime, int exceptionalLifeTime, Room room, bool transitionRain)
    {
        Player player = room.game.Players[0].realizedCreature as Player;
        this.transitionRain = transitionRain;
        this.life = 1f;
        this.foreground = false;
        this.lastLife = 1f;
        this.color = color;
        this.pos = pos;
        this.lastPos = this.pos;
        this.lastLastPos = this.pos;
        this.lastLastLastPos = this.pos;
        this.vel = vel;
        this.pos += vel * 3f;
        this.gravity = Mathf.Lerp(0.4f, 0.9f, UnityEngine.Random.value);
        this.lifeTime = UnityEngine.Random.Range(0, standardLifeTime);
        if (UnityEngine.Random.value < 0.1f)
        {
            this.lifeTime = UnityEngine.Random.Range(standardLifeTime, exceptionalLifeTime);
        }
            }
    public override void Update(bool eu)
    {
        this.lastLastLastPos = this.lastLastPos;
        this.lastLastPos = this.lastPos;
        this.vel.y = this.vel.y - (this.gravity * 2);
        this.lastLife = this.life;
        if (this.room.GetTile(this.pos).Terrain == Room.Tile.TerrainType.Solid && !foreground)
        {
            //transitionRain (determined when called) is rain that is spawned mid-fall when entering a room.
            //When transition rain collides with a tile it's removed
            if (transitionRain)
            {
                this.Destroy();
            }
            //If a raindrop hits a solid surface it's velocity is forced upwards so it appears to 'bounce'.
            if (UnityEngine.Random.Range(0f, 1f) > 0.05f)
            {
                if (this.vel.y < 0f && this.room.GetTile(this.pos + new Vector2(0f, 20f)).Terrain == Room.Tile.TerrainType.Air)
                {
                    this.pos.y = this.room.MiddleOfTile(this.pos).y + 10f;
                    this.vel.y = this.vel.y * -0.1f;
                    this.life -= 0.083333343f;
                    if (Mathf.Abs(this.vel.y) < 2f)
                    {
                        this.life -= 0.083333343f;
                    }
                }
                else
                {
                    this.Destroy();
                }
            }
            //There is a small chance when the raindrop hits a surface that it will become a part of the foreground layer, passing in front of tiles.
            else
            {
                foreground = true;
            }
        }
        if (this.vel.y < 0.2f)
        {
            this.life -= 0.00433343f;
        }
        this.life -= 0.000383343f;
        if (this.vel.y == 0f)
        {
            this.life = 0.01f;
        }
        if (this.life <= 0f)
        {
            this.Destroy();
        }
        //If a raindrop falls below the room's bottom its deleted.
        if (this.pos.y < -100f)
        {
            this.Destroy();
        }
        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
        {
            new TriangleMesh.Triangle(0, 1, 2)
        };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[0].alpha = UnityEngine.Random.Range(0.7f, 1f);
        sLeaser.sprites[0].color = color;
        this.AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
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
        sLeaser.sprites[0].alpha = sLeaser.sprites[0].alpha - 0.0003f;
        if (this.foreground)
        {
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
            sLeaser.sprites[0].alpha = sLeaser.sprites[0].alpha - 0.07f;
        }
        //Delete raindrop if it falls a certain distance below the current camera
        if (this.pos.y < (rCam.pos.y - 300f))
        {
            this.Destroy();
        }
        if(sLeaser.sprites[0].alpha < 0f)
        {
            this.Destroy();
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

