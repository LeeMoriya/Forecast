﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;

public class Blizzard : UpdatableAndDeletable
{
    public Preciptator preciptator;
    public int particleCount;
    public int particleLimit;
    public int cooldown;

    public Blizzard(Preciptator preciptator)
    {
        Debug.Log("DOWNPOUR: Blizzard Created");
        this.preciptator = preciptator;
        this.room = this.preciptator.room;
        this.particleLimit = 70;
        this.room.AddObject(new Blizzard.ScrollingTexture("overlay1", 1.5f, 0.3f));
        this.room.AddObject(new Blizzard.ScrollingTexture("overlay2", 2f, 1f));
        this.room.AddObject(new Blizzard.ScrollingTexture("overlay2", 2.3f, 1f));
    }
    public override void Update(bool eu)
    {
        cooldown++;
        if (cooldown >= Mathf.Lerp(50, 10, Mathf.Lerp(0f, 1f, this.room.world.rainCycle.RainDarkPalette)))
        {
            cooldown = 0;
            if (this.particleCount < this.particleLimit)
            {
                this.particleCount++;
                this.room.AddObject(new Blizzard.Particle(this));
            }
        }
        this.room.game.cameras[0].microShake = Mathf.Lerp(0f, 0.01f, Mathf.Lerp(0f, this.particleLimit, this.particleCount));
        base.Update(eu);
    }

    public class Particle : CosmeticSprite
    {
        public Blizzard owner;
        public Vector2 lastLastPos;
        public bool reset;
        public float alpha = 0f;
        public float xSway;
        public float ySway;

        public Particle(Blizzard owner)
        {
            this.owner = owner;
            this.xSway = UnityEngine.Random.Range(15f, 25f) * UnityEngine.Random.Range(1f, 1.5f);
            this.ySway = UnityEngine.Random.Range(7f, 12f) * UnityEngine.Random.Range(1f, 1.5f);
            this.pos = new Vector2(UnityEngine.Random.Range(0f, 1400f), UnityEngine.Random.Range(0f, 900f));
        }

        public override void Update(bool eu)
        {
            if (this.reset)
            {
                this.reset = false;
                this.alpha = 0f;
                this.pos = new Vector2(UnityEngine.Random.Range(-50f, 1500f), UnityEngine.Random.Range(-50f, 1000f));
            }
            this.lastLastPos = this.lastPos;
            this.pos.x -= this.xSway * 1.4f;
            this.pos.y -= this.ySway * 1.4f;
            if(this.pos.x < -100f || this.pos.y < -100f)
            {
                this.reset = true;
            }
            base.Update(eu);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("blizzard");
            sLeaser.sprites[0].alpha = 0f;
            sLeaser.sprites[0].scaleY = UnityEngine.Random.Range(1.6f, 3f);
            sLeaser.sprites[0].scaleX = UnityEngine.Random.Range(1f, 1.6f);
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if(sLeaser.sprites[0].alpha < Mathf.Lerp(0f,0.5f, Mathf.Lerp(0f, 1f, this.room.world.rainCycle.RainDarkPalette)))
            {
                this.alpha += 0.015f * timeStacker;
            }
            sLeaser.sprites[0].alpha = this.alpha;
            sLeaser.sprites[0].x = Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker);
            sLeaser.sprites[0].y = Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker);
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.lastLastPos, this.lastPos, timeStacker), Vector2.Lerp(this.lastPos, this.pos, timeStacker));
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = palette.fogColor;
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }

    public class ScrollingTexture : CosmeticSprite
    {
        public string spriteName;
        public float scrollSpeed;
        public float alpha;
        public ScrollingTexture(string sprite, float scrollSpeed, float alpha)
        {
            this.spriteName = sprite;
            this.scrollSpeed = scrollSpeed;
            this.alpha = alpha;
            Debug.Log("DOWNPOUR: ScrollingTexture Added");
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            var tris = new TriangleMesh.Triangle[]
            {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(2, 1, 3)
            };
            var mesh = new TriangleMesh(spriteName, tris, false);
            mesh.MoveVertice(0, new Vector2(0f, 0f));
            mesh.MoveVertice(1, new Vector2(0f, rCam.sSize.y));
            mesh.MoveVertice(2, new Vector2(rCam.sSize.x, 0f));
            mesh.MoveVertice(3, new Vector2(rCam.sSize.x, rCam.sSize.y));

            mesh.UVvertices[0] = new Vector2(0f, 0f);
            mesh.UVvertices[1] = new Vector2(0f, 2f);
            mesh.UVvertices[2] = new Vector2(2f, 0f);
            mesh.UVvertices[3] = new Vector2(2f, 2f);
            sLeaser.sprites[0] = mesh;
            sLeaser.sprites[0].alpha = 0f;
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].alpha = Mathf.Lerp(0f, this.alpha, rCam.room.world.rainCycle.RainDarkPalette);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, new Vector2(0f, 0f));
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, new Vector2(0f, rCam.sSize.y));
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, new Vector2(rCam.sSize.x, 0f));
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, new Vector2(rCam.sSize.x, rCam.sSize.y));
            for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).UVvertices.Length; i++)
            {
                (sLeaser.sprites[0] as TriangleMesh).UVvertices[i] += new Vector2(0.007f, 0.006f) * scrollSpeed;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = palette.fogColor;
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }
}
