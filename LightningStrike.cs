﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;

public class LightningStrike : UpdatableAndDeletable
{
    public Vector2 origin;
    public float delay;
    public bool contact;
    public int splits;
    public LightningStrike(Preciptator pre)
    {
        //Assign the starting position of the lightning strike
        if (pre.room != null && pre.ceilingTiles != null)
        {
            this.room = pre.room;
            try
            {
                this.origin = this.room.MiddleOfTile(pre.ceilingTiles[UnityEngine.Random.Range(0, pre.ceilingTiles.Count - 1)]) + new Vector2(0f, 100f);
            }
            catch
            {
                Debug.Log("ERROR ASSIGNING ORIGIN");
            }
            this.room.AddObject(new LightningPath(this.origin, this, new Color(1f,1f,1f)));
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (contact)
        {
            //this.Destroy();
        }
    }

    public class LightningPath : CosmeticSprite
    {
        public LightningStrike lightningStrike;
        public Vector2 endPos;
        public List<Vector2> pathPositions;
        public Color color;
        public bool search = false;
        public bool spawn = false;
        public bool fade = false;
        public float xVar = 25f;
        public float yVar = 20f;
        public float fadeRate = 0.7f;
        public LightningPath(Vector2 startPos, LightningStrike strike, Color col)
        {
            this.lightningStrike = strike;
            this.pos = startPos;
            this.color = col;
            pathPositions = new List<Vector2>()
            {
                this.pos
            };
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (lightningStrike.contact)
            {
                return;
            }
            if (!search)
            {
                Vector2 vec = pathPositions.Last();
                vec.x += UnityEngine.Random.Range(xVar, -xVar);
                vec.y -= UnityEngine.Random.Range(yVar, yVar * 2);
                if (this.room.GetTile(vec).Terrain == Room.Tile.TerrainType.Solid)
                {
                    this.pathPositions.Add(vec);
                    search = true;
                }
                else
                {
                    this.pathPositions.Add(vec);
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            this.color = new Color(1f, 1f, 0.82f);
            sLeaser.sprites = new FSprite[pathPositions.Count];
            for (int i = 0; i < pathPositions.Count; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", false);
                sLeaser.sprites[i].color = this.color;
                sLeaser.sprites[i].scaleX = 5f;
            }
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (search)
            {
                if (!spawn)
                {
                    this.InitiateSprites(sLeaser, rCam);
                    this.room.PlaySound(SoundID.Bomb_Explode, pathPositions.Last(), 2f, 0.8f);
                    this.room.PlaySound(SoundID.Thunder, pathPositions.Last(), 1.2f, 1f);
                    this.room.AddObject(new Smoke.BombSmoke(this.room, pathPositions.Last() + new Vector2(0f, 15f), null, new Color(0.01f, 0.01f, 0.01f)));
                    this.room.AddObject(new SootMark(this.room, pathPositions.Last() + new Vector2(0f, 15f), 50f, false));
                    this.room.AddObject(new LightningFlash(pathPositions.Last() + new Vector2(0f, 15f), this.color, 90f, 1f));
                    this.room.AddObject(new LightningImpact(pathPositions.Last() + new Vector2(0f, 10f), 35f, this.color));
                    switch (Downpour.strikeDamage)
                    {
                        case 0:
                            this.room.AddObject(new Explosion(this.room, null, pathPositions.Last() + new Vector2(0f, 15f), 7, 10f, 0f, 0f, 0f, 0.02f, null, 0.7f, 160f, 1f));
                            break;
                        case 1:
                            this.room.AddObject(new Explosion(this.room, null, pathPositions.Last() + new Vector2(0f, 15f), 7, 90f, 2f, 0f, 80f, 0.02f, null, 0.7f, 160f, 1f));
                            break;
                        case 2:
                            this.room.AddObject(new Explosion(this.room, null, pathPositions.Last() + new Vector2(0f, 15f), 7, 100f, 3f, 2f, 280f, 0.02f, null, 0.7f, 160f, 1f));
                            break;
                    }
                    for (int i = 0; i < pathPositions.Count; i++)
                    {
                        this.room.AddObject(new LightningImpact(pathPositions[i], 20f, this.color));
                    }
                    spawn = true;
                }
                for (int i = 0; i < pathPositions.Count; i++)
                {
                    pathPositions[i] += new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                }
                if (sLeaser.sprites != null)
                {
                    for (int i = 0; i < sLeaser.sprites.Length; i++)
                    {
                        if (i + 1 > pathPositions.Count - 1)
                        {
                            break;
                        }
                        sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(pathPositions[i], pathPositions[i + 1]);
                        sLeaser.sprites[i].x = Mathf.Lerp(pathPositions[i].x, pathPositions[i + 1].x, 0.5f) - camPos.x;
                        sLeaser.sprites[i].y = Mathf.Lerp(pathPositions[i].y, pathPositions[i + 1].y, 0.5f) - camPos.y;
                        sLeaser.sprites[i].scaleY = Vector2.Distance(pathPositions[i], pathPositions[i + 1]);
                        sLeaser.sprites[i].scaleX -= 0.12f;
                        sLeaser.sprites[i].alpha -= 0.03f;
                    }
                }
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }

    public class LightningFlash : CosmeticSprite
    {
        public Color color;
        public float rad;
        public float fade;
        public LightningFlash(Vector2 pos, Color col, float rad, float fade)
        {
            this.pos = pos;
            this.color = col;
            this.rad = rad;
            this.fade = fade;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            rCam.microShake = 0.9f;
            sLeaser.sprites = new FSprite[2];
            //Flash Sprite
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlareBomb"];
            sLeaser.sprites[0].color = this.color;
            sLeaser.sprites[0].scale = this.rad;
            sLeaser.sprites[0].alpha = 0.5f;
            //Darkening Sprite
            sLeaser.sprites[1] = new FSprite("Futile_White", true);
            sLeaser.sprites[1].color = new Color(0.01f, 0.01f, 0.01f);
            sLeaser.sprites[1].alpha = 0.6f;
            sLeaser.sprites[1].scale = 1000f;
            //Positions
            sLeaser.sprites[0].x = this.pos.x - rCam.pos.x;
            sLeaser.sprites[0].y = this.pos.y - rCam.pos.y;
            sLeaser.sprites[1].x = this.pos.x - rCam.pos.x;
            sLeaser.sprites[1].y = this.pos.y - rCam.pos.y;
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].x = this.pos.x - camPos.x;
            sLeaser.sprites[0].y = this.pos.y - camPos.y;
            sLeaser.sprites[1].x = this.pos.x - camPos.x;
            sLeaser.sprites[1].y = this.pos.y - camPos.y;
            sLeaser.sprites[0].alpha -= 0.025f;
            sLeaser.sprites[1].alpha -= 0.01f;
            if (sLeaser.sprites[0].alpha <= -1f)
            {
                this.slatedForDeletetion = true;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }

    public class LightningImpact : CosmeticSprite
    {
        public float size;
        public float life;
        public float lastLife;
        public float lifeTime;
        public Color color;
        public LightningImpact(Vector2 pos, float size, Color col)
        {
            this.pos = pos;
            this.lastPos = pos;
            this.size = size;
            this.color = col;
            this.life = 1f;
            this.lastLife = 1f;
            this.lifeTime = Mathf.Lerp(2f, 16f, size * UnityEngine.Random.value);
        }

        public override void Update(bool eu)
        {
            if (this.size > 25f)
            {
                this.room.AddObject(new Spark(this.pos, Custom.RNV() * 60f * UnityEngine.Random.value, this.color, null, 4, 50));
            }
            if (this.life <= 0f && this.lastLife <= 0f)
            {
                this.Destroy();
            }
            else
            {
                this.lastLife = this.life;
                this.life = Mathf.Max(0f, this.life - 1f / this.lifeTime);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
            sLeaser.sprites[0].color = this.color;
            sLeaser.sprites[1] = new FSprite("Futile_White", true);
            sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
            sLeaser.sprites[1].color = this.color;
            sLeaser.sprites[2] = new FSprite("Futile_White", true);
            sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
            sLeaser.sprites[2].color = this.color;
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            float num = Mathf.Lerp(this.lastLife, this.life, timeStacker);
            for (int i = 0; i < 3; i++)
            {
                sLeaser.sprites[i].x = this.pos.x - camPos.x;
                sLeaser.sprites[i].y = this.pos.y - camPos.y;
            }
            float num2 = 1f;
            if(this.size > 25f)
            {
                num2 = Mathf.Lerp(20f, 120f, Mathf.Pow(this.size, 1.5f));
                sLeaser.sprites[0].scale = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 * 4f / 8f;
                sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value);
                sLeaser.sprites[1].scale = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 * 4f / 8f;
                sLeaser.sprites[1].alpha = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value) * 0.2f;
                sLeaser.sprites[2].scale = Mathf.Lerp(0.5f, 1f, Mathf.Sin(num * 3.14159274f)) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 / 8f;
                sLeaser.sprites[2].alpha = Mathf.Sin(num * 3.14159274f) * UnityEngine.Random.value;

            }
            else
            {
                num2 = Mathf.Lerp(5f, 20f, Mathf.Pow(this.size, 1.5f));
                sLeaser.sprites[0].scale = 25f;
                sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.2f, 0.8f, UnityEngine.Random.value);
                sLeaser.sprites[1].scale = 0f;
                sLeaser.sprites[1].alpha = 0f;
                sLeaser.sprites[2].scale = 0f;
                sLeaser.sprites[2].alpha = 0f;
            }
        }
    }
}
