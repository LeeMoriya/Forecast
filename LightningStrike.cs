using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.Runtime.Remoting.Messaging;

public class LightningStrike : UpdatableAndDeletable
{
    public Vector2 origin = Vector2.zero;
    public Color col;
    public float delay;
    public bool contact;
    public int splits;
    public bool once = false;
    public WeatherController weatherController;
    public int attemptCounter = 0;


    public LightningStrike(WeatherController pre, Color col)
    {
        weatherController = pre;
        room = weatherController.room;
        this.col = col;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!room.BeingViewed || attemptCounter > 4)
        {
            ForecastLog.Log("Lightning failed to spawn near player 4 times - deleted");
            slatedForDeletetion = true;
        }
        if (origin == Vector2.zero)
        {
            float minDist = 10000f;
            //Search for strike position
            if (weatherController.room != null && weatherController.ceilingTiles != null)
            {
                int tile = UnityEngine.Random.Range(0, weatherController.ceilingTiles.Count - 1);
                Vector2 testPos = room.MiddleOfTile(weatherController.ceilingTiles[tile]) + new Vector2(0f, 100f);
                ForecastLog.Log($"{room.abstractRoom.name}: Ceiling - {tile} / {weatherController.ceilingTiles.Count}");
                for (int i = 0; i < room.game.Players.Count; i++)
                {
                    if (room.game.Players[i] != null && room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
                    {
                        float dist = Vector2.Distance(new Vector2(origin.x, room.game.Players[i].realizedCreature.mainBodyChunk.pos.y), room.game.Players[i].realizedCreature.mainBodyChunk.pos);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            ForecastLog.Log($"Lightning MinDist = {minDist}");
                        }
                    }
                }
                if (minDist < 2000f || attemptCounter >= 3)
                {
                    origin = testPos;
                    ForecastLog.Log("Lightning found origin near player");
                }
                else
                {
                    ForecastLog.Log("Lightning origin too far");
                    attemptCounter++;
                }
            }
        }
        else if (origin != Vector2.zero && !once)
        {
            ForecastLog.Log("Lightning spawned");
            room.AddObject(new LightningPath(origin, this, col));
            once = true;
        }
    }


    public class LightningPath : UpdatableAndDeletable
    {
        public float warning = 0f;
        public LightningStrike lightningStrike;
        public Vector2 endPos;
        public List<Vector2> pathPositions;
        public Color color;
        public bool searchFinished = false;
        public bool spawn = false;
        public bool fade = false;
        public float xVar = 25f;
        public float yVar = 20f;
        public float fadeRate = 0.7f;
        public bool tooFar = false;
        public float lastAlpha, alpha;
        public Vector2 pos;
        public LightningPath(Vector2 startPos, LightningStrike strike, Color col)
        {
            lightningStrike = strike;
            pos = startPos;
            color = col;
            pathPositions = new List<Vector2>()
            {
                pos
            };
            alpha = 1f;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (!room.BeingViewed)
            {
                slatedForDeletetion = true;
            }
            if (!searchFinished)
            {
                Vector2 vec = pathPositions.Last();
                vec.x += UnityEngine.Random.Range(xVar, -xVar);
                vec.y -= UnityEngine.Random.Range(yVar, yVar * 2);

                //Lightning path hit a surface
                if (room.GetTile(vec).Terrain == Room.Tile.TerrainType.Solid || vec.y < room.floatWaterLevel)
                {
                    pathPositions.Add(vec);
                    searchFinished = true;

                    tooFar = false;
                    if (room != null && room.BeingViewed)
                    {
                        for (int i = 0; i < room.game.Players.Count; i++)
                        {
                            if (room.game.Players[i] != null && room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
                            {
                                float dist = Vector2.Distance(pos, room.game.Players[i].realizedCreature.mainBodyChunk.pos);
                                if (dist > 2000f)
                                {
                                    ForecastLog.Log($"Flash dist: {dist}");
                                    tooFar = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    pathPositions.Add(vec);
                }
            }
            else
            {
                if (warning == 0f && !spawn)
                {
                    room.AddObject(new LightningFlash(pathPositions.Last() + new Vector2(0f, 15f), color, 15f, 30f, true));
                    room.PlaySound(SoundID.Thunder_Close, pathPositions.Last(), 0.7f, 1f);
                }
                if (!spawn && warning >= 1.5f)
                {
                    room.AddObject(new BoltGraphics(pathPositions, color));
                    room.PlaySound(SoundID.Bomb_Explode, pathPositions.Last(), 1.2f, 0.8f);
                    room.PlaySound(SoundID.Thunder, pathPositions.Last(), 1f, 1f);
                    room.AddObject(new Smoke.BombSmoke(room, pathPositions.Last() + new Vector2(0f, 15f), null, new Color(0.01f, 0.01f, 0.01f)));
                    room.AddObject(new SootMark(room, pathPositions.Last() + new Vector2(0f, 15f), 50f, false));
                    if (!tooFar)
                    {
                        room.AddObject(new LightningFlash(pathPositions.Last() + new Vector2(0f, 15f), color, 90f, 1f, false));
                    }
                    room.AddObject(new LightningImpact(pathPositions.Last() + new Vector2(0f, 10f), 35f, color));
                    if (room.waterObject != null)
                    {
                        room.waterObject.Explosion(pathPositions.Last() + new Vector2(0f, 20f), 150f, 20f);
                    }
                    switch (lightningStrike.weatherController.settings.strikeDamageType)
                    {
                        case 0:
                            room.AddObject(new Explosion(room, null, pathPositions.Last() + new Vector2(0f, 15f), 7, 10f, 0f, 0f, 0f, 0.02f, null, 0.7f, 160f, 1f));
                            break;
                        case 1:
                            room.AddObject(new Explosion(room, null, pathPositions.Last() + new Vector2(0f, 15f), 7, 120f, 2f, 0f, 80f, 0.02f, null, 0.7f, 160f, 1f));
                            break;
                        case 2:
                            room.AddObject(new Explosion(room, null, pathPositions.Last() + new Vector2(0f, 15f), 7, 140f, 3f, 2f, 280f, 0.02f, null, 0.7f, 160f, 1f));
                            break;
                    }
                    for (int i = 0; i < pathPositions.Count; i++)
                    {
                        room.AddObject(new LightningImpact(pathPositions[i], 20f, color));
                        for (int s = 0; s < 20; s++)
                        {
                            room.AddObject(new MouseSpark(pathPositions[i], Custom.RNV() * UnityEngine.Random.Range(1f,3f), UnityEngine.Random.Range(10f,50f), color));
                        }
                    }
                    spawn = true;
                }
                warning += 0.025f;
            }
            if (spawn)
            {
                alpha -= 0.05f;
            }
            lastAlpha = alpha;
        }
    }

    public class BoltGraphics : CosmeticSprite
    {
        public List<Vector2> pathPositions = new List<Vector2>();
        public float alpha, lastAlpha;
        public Color color;
        public FSprite[] sprites;

        public BoltGraphics(List<Vector2> pathPositions, Color color)
        {
            this.pathPositions = pathPositions;
            this.color = color;
            alpha = 1f;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            lastAlpha = alpha;
            alpha -= 0.035f;
            if (alpha < 0f)
            {
                Destroy();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[pathPositions.Count];
            for (int i = 0; i < pathPositions.Count; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", false);
                sLeaser.sprites[i].alpha = alpha;
                sLeaser.sprites[i].color = color;
                sLeaser.sprites[i].scaleX = 5f;
            }
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < pathPositions.Count; i++)
            {
                pathPositions[i] += new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
            }
            if (sLeaser.sprites != null)
            {
                if (sLeaser.sprites[0].alpha > 0f)
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
                        //sLeaser.sprites[i].scaleX -= 0.12f;
                        sLeaser.sprites[i].alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
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
        public bool warn;
        public float alpha = 0.5f;
        public float lastAlpha;
        public bool done = false;
        public float darkAlpha = 0.6f;
        public LightningFlash(Vector2 pos, Color col, float rad, float fade, bool warn)
        {
            this.pos = pos;
            color = col;
            this.rad = rad;
            this.fade = fade;
            this.warn = warn;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            lastAlpha = alpha;
            if (!room.BeingViewed)
            {
                slatedForDeletetion = true;
            }
            if (!done)
            {
                room.game.cameras[0].microShake = 0.34f;
                if (!warn)
                {
                    room.game.cameras[0].microShake = 0.9f;
                }
            }
            alpha -= 0.02f;
            if (alpha <= 0f || !room.BeingViewed)
            {
                done = true;
            }
            
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            
            sLeaser.sprites = new FSprite[2];
            //Flash Sprite
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlareBomb"];
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[0].scale = rad;
            sLeaser.sprites[0].alpha = 0.5f;
            //Darkening Sprite
            sLeaser.sprites[1] = new FSprite("Futile_White", true);
            sLeaser.sprites[1].color = new Color(0.01f, 0.01f, 0.01f);
            sLeaser.sprites[1].alpha = darkAlpha;
            if (warn)
            {
                sLeaser.sprites[1].alpha = 0f;
            }
            sLeaser.sprites[1].scale = 1000f;
            //Positions
            sLeaser.sprites[0].x = pos.x - rCam.pos.x;
            sLeaser.sprites[0].y = pos.y - rCam.pos.y;
            sLeaser.sprites[1].x = pos.x - rCam.pos.x;
            sLeaser.sprites[1].y = pos.y - rCam.pos.y;
            if (done)
            {
                sLeaser.sprites[0].alpha = 0f;
                sLeaser.sprites[1].alpha = 0f;
            }
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
            rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[1]);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;
            sLeaser.sprites[1].x = pos.x - camPos.x;
            sLeaser.sprites[1].y = pos.y - camPos.y;
            sLeaser.sprites[0].alpha = Mathf.Lerp(lastAlpha,alpha, timeStacker);
            if (warn)
            {
                sLeaser.sprites[0].alpha = Mathf.Lerp(alpha, 0f, UnityEngine.Random.value); 
            }
            sLeaser.sprites[1].alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);

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
            lastPos = this.pos;
            this.size = size;
            color = col;
            life = 1f;
            lastLife = 1f;
            lifeTime = Mathf.Lerp(2f, 16f, size * UnityEngine.Random.value);
        }

        public override void Update(bool eu)
        {
            if (size > 25f)
            {
                for (int i = 0; i < 50; i++)
                {
                    room.AddObject(new Spark(pos, Custom.RNV() * 30f * UnityEngine.Random.value, color, null, 4, 50));
                }
            }
            if (life <= 0f && lastLife <= 0f)
            {
                Destroy();
            }
            else
            {
                lastLife = life;
                life = Mathf.Max(0f, life - 1f / lifeTime);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[1] = new FSprite("Futile_White", true);
            sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
            sLeaser.sprites[1].color = color;
            sLeaser.sprites[2] = new FSprite("Futile_White", true);
            sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
            sLeaser.sprites[2].color = color;
            for (int i = 0; i < 3; i++)
            {
                sLeaser.sprites[i].x = pos.x - rCam.pos.x;
                sLeaser.sprites[i].y = pos.y - rCam.pos.y;
            }
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            float num = Mathf.Lerp(lastLife, life, timeStacker);
            for (int i = 0; i < 3; i++)
            {
                sLeaser.sprites[i].x = pos.x - camPos.x;
                sLeaser.sprites[i].y = pos.y - camPos.y;
            }
            float num2 = 1f;
            if (size > 25f)
            {
                num2 = Mathf.Lerp(20f, 120f, Mathf.Pow(size, 1.5f));
                sLeaser.sprites[0].scale = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 * 4f / 8f;
                sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value);
                sLeaser.sprites[1].scale = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 * 4f / 8f;
                sLeaser.sprites[1].alpha = Mathf.Pow(Mathf.Sin(num * 3.14159274f), 0.5f) * Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value) * 0.2f;
                sLeaser.sprites[2].scale = Mathf.Lerp(0.5f, 1f, Mathf.Sin(num * 3.14159274f)) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 / 8f;
                sLeaser.sprites[2].alpha = Mathf.Sin(num * 3.14159274f) * UnityEngine.Random.value;

            }
            else
            {
                num2 = Mathf.Lerp(5f, 20f, Mathf.Pow(size, 1.5f));
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

