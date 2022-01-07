using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;

public class Blizzard : UpdatableAndDeletable
{
    public Preciptator preciptator;
    public OmniDirectionalSound[] sfx;
    public int particleCount;
    public int particleLimit;
    public int cooldown;
    public float intensity = 0f;
    public float extremelyTemporaryPlayerExposureVariable = 0f;

    public Blizzard(Preciptator preciptator)
    {
        Debug.Log("DOWNPOUR: Blizzard Created");
        this.preciptator = preciptator;
        this.room = this.preciptator.room;
        this.particleLimit = 70;
        this.room.AddObject(new Blizzard.ScrollingTexture("overlay1", 4.5f, 0.3f));
        this.room.AddObject(new Blizzard.ScrollingTexture("overlay1", 8.5f, 0.31f));
        this.room.AddObject(new Blizzard.ScrollingTexture("overlay2", 5f, 1f));
        this.room.AddObject(new Blizzard.ScrollingTexture("overlay2", 6.3f, 1f));
        this.sfx = new OmniDirectionalSound[3];
        for (int i = 0; i < this.sfx.Length; i++)
        {
            string sample = "";
            switch (i)
            {
                case 0:
                    sample = "AM_WIN-HowlingWnd.ogg";
                    break;
                case 1:
                    sample = "AM_WIN-NatWind.ogg";
                    break;
                case 2:
                    sample = "AM_IND-Midsex02.ogg";
                    break;
            }
            this.sfx[i] = new OmniDirectionalSound(sample, false)
            {
                volume = 0f,
                pitch = 1f,
                type = AmbientSound.Type.Omnidirectional
            };
            this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(this.room.game.cameras[0].virtualMicrophone, this.sfx[i]));
        }
    }
    public override void Update(bool eu)
    {
        //Particles
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
        //Wind
        if (this.intensity <= 0.08f)
        {
            this.intensity = Mathf.Lerp(0f, 0.081f, this.room.world.rainCycle.RainDarkPalette);
        }
        this.ThrowAroundObjects();
        //Sound
        bool sfx1 = false;
        bool sfx2 = false;
        bool sfx3 = false;
        for (int i = 0; i < this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Count; i++)
        {
            if (this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == this.sfx[0])
            {
                sfx1 = true;
            }
            if (this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == this.sfx[1])
            {
                sfx2 = true;
            }
            if (this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == this.sfx[2])
            {
                sfx3 = true;
            }
            if (this.sfx.Contains(this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound))
            {
                if (this.room.BeingViewed)
                {
                    if (this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == this.sfx[2])
                    {
                        this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(-1f, 0f, TimePastCycleEnd));
                    }
                    else
                    {
                        this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = Mathf.Lerp(0f, 1f, this.room.world.rainCycle.RainDarkPalette);
                    }
                }
                else
                {
                    this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = 0f;
                }
            }
        }
        if (!sfx1)
        {
            this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(this.room.game.cameras[0].virtualMicrophone, this.sfx[0]));
        }
        if (!sfx2)
        {
            this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(this.room.game.cameras[0].virtualMicrophone, this.sfx[1]));
        }
        if (!sfx3)
        {
            this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(this.room.game.cameras[0].virtualMicrophone, this.sfx[2]));
        }
        //Camera Shake
        if (this.room.BeingViewed)
        {
            this.room.game.cameras[0].screenShake = Mathf.Lerp(0f, 0.3f, Mathf.InverseLerp(-1f, 0f, TimePastCycleEnd));
        }
        base.Update(eu);
    }

    public float TimePastCycleEnd
    {
        get
        {
            return (this.room.world.rainCycle.timer - this.room.world.rainCycle.cycleLength) / 2400f;
        }
    }

    public void ThrowAroundObjects()
    {
        if (this.room.BeingViewed && this.room.roomRain != null && this.room.roomRain.rainReach != null)
        {
            for (int i = 0; i < this.room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < this.room.physicalObjects[i].Count; j++)
                {
                    for (int k = 0; k < this.room.physicalObjects[i][j].bodyChunks.Length; k++)
                    {
                        BodyChunk bodyChunk = this.room.physicalObjects[i][j].bodyChunks[k];
                        IntVector2 tilePosition = this.room.GetTilePosition(bodyChunk.pos + new Vector2(Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value)));
                        float num = this.intensity;
                        bool flag = false;
                        if (this.room.roomRain.rainReach[Custom.IntClamp(tilePosition.x, 0, this.room.TileWidth - 1)] < tilePosition.y)
                        {
                            flag = true;
                            num = this.intensity;
                        }
                        if (this.room.water)
                        {
                            num *= Mathf.InverseLerp(this.room.FloatWaterLevel(bodyChunk.pos.x) - 100f, this.room.FloatWaterLevel(bodyChunk.pos.x), bodyChunk.pos.y);
                        }
                        if (num > 0f)
                        {
                            //Wind
                            bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(245f, 270f, UnityEngine.Random.value)) * UnityEngine.Random.value * ((!flag) ? 1.2f : 1.8f) * num / bodyChunk.mass;

                            //Player
                            if (bodyChunk.owner is Player)
                            {
                                //Apply rainDeath
                                if (bodyChunk == (bodyChunk.owner as Creature).mainBodyChunk)
                                {
                                    (bodyChunk.owner as Creature).rainDeath += num;
                                    Debug.Log("RAIN DEATH: " + (bodyChunk.owner as Player).rainDeath);

                                    if (num > 0.03f)
                                    {
                                        (bodyChunk).vel += Custom.RNV() * extremelyTemporaryPlayerExposureVariable;

                                        if (extremelyTemporaryPlayerExposureVariable > 1f)
                                        {
                                            (bodyChunk.owner as Player).Die();
                                        }
                                        if ((bodyChunk.owner as Player).stun > 0)
                                        {
                                            extremelyTemporaryPlayerExposureVariable += 0.01f;
                                            Debug.Log("EXPOSURE: " + extremelyTemporaryPlayerExposureVariable);
                                        }
                                        //Exhaustion
                                        if (UnityEngine.Random.value < extremelyTemporaryPlayerExposureVariable + 0.01f * 0.025f)
                                        {
                                            if ((bodyChunk.owner as Player).rainDeath > 1f)
                                            {
                                                (bodyChunk.owner as Player).exhausted = true;
                                                (bodyChunk.owner as Player).lungsExhausted = true;
                                                (bodyChunk.owner as Player).aerobicLevel = 1f;
                                                (bodyChunk.owner as Player).slowMovementStun = 2;
                                                (bodyChunk.owner as Player).Stun(7);
                                                (bodyChunk.owner as Player).rainDeath -= 1f;
                                            }
                                            else
                                            {
                                                (bodyChunk.owner as Player).Blink((int)UnityEngine.Random.Range(1f, 15f));
                                            }
                                        }
                                    }
                                }
                            }
                            //Creatures
                            else if(bodyChunk.owner is Creature)
                            {
                                //Apply rainDeath
                                if (bodyChunk == (bodyChunk.owner as Creature).mainBodyChunk)
                                {
                                    (bodyChunk.owner as Creature).rainDeath += num;
                                }
                                //Random Stun
                                if (Mathf.Pow(UnityEngine.Random.value, 1.2f) * 2f * (float)bodyChunk.owner.bodyChunks.Length < num)
                                {
                                    (bodyChunk.owner as Creature).Stun(UnityEngine.Random.Range(1, 1 + (int)(9f * num)));
                                }
                                //Kill - TODO
                                if (num > 0.05f && (bodyChunk.owner as Creature).rainDeath > 1f)
                                {
                                    if (UnityEngine.Random.value < 0.025f)
                                    {
                                        (bodyChunk.owner as Creature).Die();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
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
                this.pos = new Vector2(UnityEngine.Random.Range(-50f, 1600f), UnityEngine.Random.Range(-50f, 1100f));
            }
            this.lastLastPos = this.lastPos;
            this.pos.x -= this.xSway * 2f;
            this.pos.y -= this.ySway * 2f;
            if (this.pos.x < -100f || this.pos.y < -100f)
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
            if (sLeaser.sprites[0].alpha < Mathf.Lerp(0f, 0.55f, Mathf.Lerp(0f, 1f, this.room.world.rainCycle.RainDarkPalette)))
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
                (sLeaser.sprites[0] as TriangleMesh).UVvertices[i] += new Vector2(0.007f, 0.006f) * scrollSpeed * timeStacker;
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

