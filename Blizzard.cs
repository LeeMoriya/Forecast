using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using Music;
using HUD;


public class Vignette : ISingleCameraDrawable
{
    public ExposureController controller;
    public FSprite vignette;
    public RoomCamera camera;
    public Vignette(ExposureController controller)
    {
        this.controller = controller;
        this.camera = this.controller.cam;
        this.vignette = new FSprite("Futile_White", true);
        this.vignette.alpha = 0f;
        this.vignette.color = Color.white;
        this.vignette.SetAnchor(0.5f, 0.5f);
        this.vignette.x = this.camera.game.rainWorld.screenSize.x / 2f;
        this.vignette.y = this.camera.game.rainWorld.screenSize.y / 2f;
        this.vignette.scaleX = this.camera.game.rainWorld.screenSize.x;
        this.vignette.scaleY = this.camera.game.rainWorld.screenSize.y;
        this.vignette.shader = this.camera.game.rainWorld.Shaders["EdgeFade"];
        this.camera.AddSingleCameraDrawable(this);
        this.camera.ReturnFContainer("HUD").AddChild(this.vignette);
        Debug.Log("VIGNETTE CREATED");
    }

    public void Draw(RoomCamera camera, float timeStacker, Vector2 camPos)
    {
        if (!this.camera.ReturnFContainer("HUD")._childNodes.Contains(this.vignette))
        {
            this.camera.ReturnFContainer("HUD").AddChild(this.vignette);
        }
        if (camera.currentPalette.darkness > 0.5f)
        {
            this.vignette.color = Color.Lerp(camera.currentPalette.skyColor, new Color(0.2f, 0.2f, 0.2f), 0.4f);
        }
        else
        {
            this.vignette.color = Color.Lerp(camera.currentPalette.texture.GetPixel(9, 5), Color.white, 0.4f);
        }
        this.vignette.x = this.camera.game.rainWorld.screenSize.x / 2f;
        this.vignette.y = this.camera.game.rainWorld.screenSize.y / 2f;
        this.vignette.scaleX = (this.camera.game.rainWorld.screenSize.x * Mathf.Lerp(1.5f, 1f, this.controller.exposure) + 2f) / 16f;
        this.vignette.scaleY = (this.camera.game.rainWorld.screenSize.y * Mathf.Lerp(2.5f, 1.5f, this.controller.exposure) + 2f) / 16f;
        this.vignette.alpha = Mathf.Lerp(0f, 0.5f, this.controller.exposure);
    }
}

public class ExposureController
{
    public Player player;
    public SlugcatStats stats;
    public RoomCamera cam;
    public Vignette vignette;
    public Blizzard blizzard;
    public float exposure = 0;
    public float ambient = 0f;
    public int cooldown;
    public int bellCooldown;
    public int bellRing;
    public bool dead;
    public FLabel labelPlayer;
    public FLabel labelExposure;
    public FLabel labelAmbient;
    public FLabel labelBlizzard;
    public FLabel labelCooldown;
    public ExposureController(Player player)
    {
        this.player = player;
        this.stats = this.player.slugcatStats;
        this.cam = this.player.room.game.cameras[0];
        if (this.player.playerState.playerNumber == 0)
        {
            this.vignette = new Vignette(this);
        }
        this.dead = false;

        Debug.Log("EXPOSURE CONTROLLER - PLAYER " + this.player.playerState.playerNumber);

        if (Downpour.debug)
        {
            Vector2 sSize = this.player.abstractCreature.world.game.cameras[0].sSize;
            float offset = 80f * this.player.playerState.playerNumber;
            labelPlayer = new FLabel("font", "Player " + this.player.playerState.playerNumber);
            labelPlayer.SetPosition(30.01f, sSize.y - (25f + offset));
            labelPlayer.color = new Color(0.4f, 0.3f, 0.8f);
            labelPlayer.alignment = FLabelAlignment.Left;
            labelPlayer.alpha = 0f;
            Futile.stage.AddChild(labelPlayer);
            labelExposure = new FLabel("font", "");
            labelExposure.SetPosition(30.01f, sSize.y - (40f + offset));
            labelExposure.color = new Color(0.3f, 1f, 1f);
            labelExposure.alignment = FLabelAlignment.Left;
            Futile.stage.AddChild(labelExposure);
            labelAmbient = new FLabel("font", "");
            labelAmbient.SetPosition(30.01f, sSize.y - (55f + offset));
            labelAmbient.color = new Color(0.3f, 1f, 1f);
            labelAmbient.alignment = FLabelAlignment.Left;
            Futile.stage.AddChild(labelAmbient);
            labelBlizzard = new FLabel("font", "");
            labelBlizzard.SetPosition(30.01f, sSize.y - (70f + offset));
            labelBlizzard.color = new Color(0.3f, 1f, 1f);
            labelBlizzard.alignment = FLabelAlignment.Left;
            Futile.stage.AddChild(labelBlizzard);
            labelCooldown = new FLabel("font", "");
            labelCooldown.SetPosition(30.01f, sSize.y - (85f + offset));
            labelCooldown.color = new Color(0.3f, 1f, 1f);
            labelCooldown.alignment = FLabelAlignment.Left;
            Futile.stage.AddChild(labelCooldown);
        }
    }

    public void RemoveDebugLabels()
    {
        labelPlayer.RemoveFromContainer();
        labelExposure.RemoveFromContainer();
        labelAmbient.RemoveFromContainer();
        labelBlizzard.RemoveFromContainer();
        labelCooldown.RemoveFromContainer();
    }

    public void UpdateDebugLabels()
    {
        labelPlayer.alpha = 1f;
        labelExposure.text = "Exposure: " + this.exposure;
        labelAmbient.text = "Ambient: " + this.ambient;
        if (blizzard == null)
        {
            labelBlizzard.text = "Blizzard: NO";
            labelBlizzard.color = new Color(1f, 0f, 0f);
        }
        else
        {
            labelBlizzard.text = "Blizzard: YES";
            labelBlizzard.color = new Color(0f, 1f, 0f);
        }
        labelCooldown.text = "Cycle End: " + this.TimePastCycleEnd;
    }

    public bool IsCold()
    {
        //Not a shelter, has RoomRain and is enabled in config
        if (!this.player.room.abstractRoom.shelter && this.player.room.roomRain != null && this.player.room.roomSettings.RainIntensity > 0f && (this.player.room.world.region != null && Downpour.rainRegions.Contains(this.player.room.world.region.name)))
        {
            return true;
        }
        return false;
    }

    public void Update()
    {
        if (this.player.room != null)
        {
            //Outdoors
            if (this.blizzard != null && IsCold())
            {
                //Switch Blizzard
                if (this.blizzard.room != this.player.room)
                {
                    SwitchBlizzard();
                    return;
                }
                //Exposed
                if (this.exposure < 1f)
                {
                    if (this.blizzard.TimePastCycleEnd > 0f)
                    {
                        float scale = Mathf.Clamp(this.blizzard.TimePastCycleEnd, 0.00143f, 0.45f);
                        this.exposure += 0.0007f * scale;
                    }
                    else
                    {
                        this.exposure += 0.0001f;
                    }
                    dead = false;
                }
                cooldown++;
            }
            //Indoors
            else
            {
                SwitchBlizzard();
                if (this.TimePastCycleEnd != -1f)
                {
                    //Exposure matches ambient temp
                    if (IsCold())
                    {
                        this.cam.microShake = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.005f, this.player.room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.4f, 1f, TimePastCycleEnd));
                        this.ambient = Mathf.Lerp(0f, Mathf.Lerp(0f, 1f, this.player.room.roomSettings.RainIntensity), Mathf.InverseLerp(0f, 3f, this.TimePastCycleEnd));
                        if (this.exposure > this.ambient)
                        {
                            this.exposure -= 0.0005f;
                        }
                        else
                        {
                            this.exposure += 0.0005f;
                        }
                    }
                    //Safe in a shelter, exposure decreases
                    else if(this.player.room.abstractRoom.shelter && this.player.room.shelterDoor != null && this.player.room.shelterDoor.IsClosing)
                    {
                        this.exposure -= 0.005f;
                    }
                }
            }
            if (this.exposure > 0f && !dead)
            {
                //Stats
                this.player.slugcatStats.runspeedFac = Mathf.Lerp(this.stats.runspeedFac, 0.75f, Mathf.Lerp(0.2f, 0.7f, this.exposure));
                this.player.slugcatStats.poleClimbSpeedFac = Mathf.Lerp(this.stats.poleClimbSpeedFac, 0.85f, Mathf.Lerp(0.2f, 0.7f, this.exposure));
                //Stun
                if (this.exposure < 0.4f)
                {
                    if (cooldown >= UnityEngine.Random.Range(700, 1000))
                    {
                        cooldown = 0;
                        this.player.Stun((int)(20 * this.exposure));
                    }
                }
                //Exhaustion
                else
                {
                    if (cooldown >= UnityEngine.Random.Range(600, 1000))
                    {
                        cooldown = 0;
                        this.player.lungsExhausted = true;
                        this.player.slowMovementStun = (int)(UnityEngine.Random.Range(0.5f, 2f) * this.exposure);
                        if (UnityEngine.Random.value < 0.3f * this.exposure)
                        {
                            this.player.Stun(UnityEngine.Random.Range(50, 120));
                        }
                    }
                }
                this.player.bodyChunks[0].vel += Custom.RNV() * (0.4f * this.exposure);
                if (UnityEngine.Random.value < 0.0015)
                {
                    this.player.Blink(30);
                }

                //Death Bells
                if (this.exposure >= 1f && !this.dead && this.player.playerState.playerNumber == 0)
                {
                    bellCooldown++;
                    if (bellCooldown > (int)Mathf.Lerp(200f, 50f, Mathf.InverseLerp(0f, 22f, bellRing)))
                    {
                        bellCooldown = 0;
                        bellRing++;
                        if (Downpour.debug && bellRing > 0)
                        {
                            Debug.Log(bellRing);
                        }
                        this.player.room.PlaySound(SoundID.MENU_Start_New_Game, this.player.mainBodyChunk, false, Mathf.Lerp(0.7f, 1.8f, Mathf.InverseLerp(0f, 25f, bellRing)), 1.3f);
                        if (bellRing == 25)
                        {
                            if (!this.dead)
                            {
                                this.player.Die();
                                this.dead = true;
                            }
                        }
                        else
                        {
                            this.dead = false;
                        }
                    }
                }
                else
                {
                    bellCooldown++;
                    if (bellCooldown > 150)
                    {
                        bellCooldown = 0;
                        bellRing--;
                        if (Downpour.debug && bellRing > 0)
                        {
                            Debug.Log(bellRing);
                        }
                    }
                }
                bellRing = Mathf.Clamp(bellRing, 0, 25);
            }
            this.exposure = Mathf.Clamp(this.exposure, 0f, 1f);
            if (Downpour.debug)
            {
                UpdateDebugLabels();
            }
        }
    }



    public void SwitchBlizzard()
    {
        if (this.player.room != null)
        {
            for (int i = 0; i < this.player.room.updateList.Count; i++)
            {
                if (this.player.room.updateList[i] is Blizzard)
                {
                    this.blizzard = this.player.room.updateList[i] as Blizzard;
                    if (Downpour.debug)
                    {
                        Debug.Log("UPDATED BLIZZARD TO " + this.player.room.abstractRoom.name);
                    }
                    return;
                }
            }
            this.blizzard = null;
        }
    }

    public float TimePastCycleEnd
    {
        get
        {
            if (this.player.room != null)
            {
                return (this.player.room.world.rainCycle.timer - this.player.room.world.rainCycle.cycleLength) / 2400f;
            }
            return -1f;
        }
    }

    public float rainDeath
    {
        get
        {
            if (this.player != null)
            {
                return this.player.rainDeath;
            }
            return 0f;
        }
    }
}

public class WeatherSounds : UpdatableAndDeletable
{
    public OmniDirectionalSound[] sfx;
    public bool blizzard;
    public float TimePastCycleEnd
    {
        get
        {
            return (this.room.world.rainCycle.timer - this.room.world.rainCycle.cycleLength) / 2400f;
        }
    }
    public WeatherSounds(Room room)
    {
        this.room = room;
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
        CheckBlizzard();
    }
    public void CheckBlizzard()
    {
        if (this.room != null)
        {
            if(this.room.roomRain == null || (this.room.world.region != null && !Downpour.rainRegions.Contains(this.room.world.region.name)))
            {
                this.Destroy();
                return;
            }
            for (int i = 0; i < this.room.updateList.Count; i++)
            {
                if (this.room.updateList[i] is Blizzard)
                {
                    this.blizzard = true;
                    return;
                }
            }
            this.blizzard = false;
        }
    }

    public override void Update(bool eu)
    {
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
                    //All three sounds play
                    if (this.blizzard)
                    {
                        this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 1.2f, this.room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0f, TimePastCycleEnd));
                    }
                    //Indoors, so only sound two plays
                    else
                    {
                        if (this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == this.sfx[2])
                        {
                            this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.65f, this.room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.3f, TimePastCycleEnd));
                        }
                        else
                        {
                            this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = 0f;
                        }
                    }
                }
                else
                {
                    this.room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = 0f;
                }
            }
        }
        if(!sfx1 || !sfx2 || !sfx3)
        {
            CheckBlizzard();
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
        base.Update(eu);
    }
}

public class Blizzard : UpdatableAndDeletable
{
    public Preciptator preciptator;
    public int particleCount;
    public int particleLimit;
    public int cooldown;
    public float intensity = 0f;

    public Blizzard(Preciptator preciptator)
    {
        if (Downpour.debug)
        {
            Debug.Log("DOWNPOUR: Blizzard Created");
        }
        this.preciptator = preciptator;
        this.room = this.preciptator.room;
        this.particleLimit = 70;
        if (this.room.roomSettings.RainIntensity > 0f)
        {
            this.room.AddObject(new Blizzard.ScrollingTexture(this.room, this, "overlay1", 4.5f, 0.3f));
            this.room.AddObject(new Blizzard.ScrollingTexture(this.room, this, "overlay1", 8.5f, 0.31f));
            this.room.AddObject(new Blizzard.ScrollingTexture(this.room, this, "overlay2", 5f, 1f));
            this.room.AddObject(new Blizzard.ScrollingTexture(this.room, this, "overlay2", 6.3f, 1f));
        }
    }

    public override void Update(bool eu)
    {
        if (this.room.roomSettings.RainIntensity == 0f)
        {
            return;
        }
        //Particles
        cooldown++;
        if (cooldown >= Mathf.Lerp(50, 10, Mathf.Lerp(0f, 1f, Mathf.InverseLerp(-0.5f, 0.5f, TimePastCycleEnd))))
        {
            cooldown = 0;
            if (this.particleCount < Mathf.Lerp(0f, Mathf.Lerp(0f, this.particleLimit, this.room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.5f, TimePastCycleEnd)))
            {
                this.particleCount++;
                this.room.AddObject(new Blizzard.Particle(this));
            }
        }
        //Wind
        this.intensity = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.081f,this.room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.5f, this.TimePastCycleEnd));
        this.ThrowAroundObjects();
        //Camera Shake
        if (this.room.BeingViewed)
        {
            this.room.game.cameras[0].screenShake = Mathf.Lerp(0f, Mathf.Lerp(0f,0.3f, this.room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.5f, TimePastCycleEnd));
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
                            if (bodyChunk.contactPoint.y < 0)
                            {
                                //On Ground
                                if (Downpour.windDirection == 1)
                                {
                                    bodyChunk.vel += Custom.DegToVec(270f) * UnityEngine.Random.value * ((!flag) ? 1.2f : 1.8f) * num / bodyChunk.mass;
                                }
                                else
                                {
                                    bodyChunk.vel += Custom.DegToVec(90f) * UnityEngine.Random.value * ((!flag) ? 1.2f : 1.8f) * num / bodyChunk.mass;
                                }
                            }
                            else
                            {
                                //Off Ground
                                if (Downpour.windDirection == 1)
                                {
                                    bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(245f, 270f, UnityEngine.Random.value)) * UnityEngine.Random.value * ((!flag) ? 1.2f : 1.8f) * num / bodyChunk.mass;
                                }
                                else
                                {
                                    bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(90f, 115f, UnityEngine.Random.value)) * UnityEngine.Random.value * ((!flag) ? 1.2f : 1.8f) * num / bodyChunk.mass;
                                }
                            }
                            //Player
                            if (bodyChunk.owner is Player)
                            {
                                //Apply rainDeath
                                if (bodyChunk == (bodyChunk.owner as Creature).mainBodyChunk)
                                {
                                    (bodyChunk.owner as Creature).rainDeath += num * 0.1f;
                                }
                            }
                            //Creatures
                            else if (bodyChunk.owner is Creature)
                            {
                                //Apply rainDeath
                                if (bodyChunk == (bodyChunk.owner as Creature).mainBodyChunk)
                                {
                                    (bodyChunk.owner as Creature).rainDeath += num * 0.35f;
                                }
                                //Random Stun
                                if (Mathf.Pow(UnityEngine.Random.value, 1.2f) * 2f * (float)bodyChunk.owner.bodyChunks.Length < num)
                                {
                                    (bodyChunk.owner as Creature).Stun(UnityEngine.Random.Range(1, 1 + (int)(9f * num)));
                                }
                                //Kill - TODO
                                if (num > 0.05f && (bodyChunk.owner as Creature).rainDeath > 1f)
                                {
                                    if (UnityEngine.Random.value < 0.0025f)
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

            if (Downpour.windDirection == 1)
            {
                this.pos.x -= this.xSway * 2f;
                this.pos.y -= this.ySway * 2f;
                if (this.pos.x < -100f || this.pos.y < -100f)
                {
                    this.reset = true;
                }
            }
            else
            {
                this.pos.x += this.xSway * 2f;
                this.pos.y -= this.ySway * 2f;
                if (this.pos.x > 1400f || this.pos.y < -100f)
                {
                    this.reset = true;
                }
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
            if (sLeaser.sprites[0].alpha < Mathf.Lerp(0f, Mathf.Lerp(0f, 0.55f, this.room.roomSettings.RainIntensity), Mathf.Lerp(0f, 1f, Mathf.InverseLerp(-0.5f, 0.5f, this.owner.TimePastCycleEnd))))
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
        public Blizzard owner;
        public ScrollingTexture(Room room, Blizzard owner, string sprite, float scrollSpeed, float alpha)
        {
            this.owner = owner;
            this.spriteName = sprite;
            this.scrollSpeed = scrollSpeed;
            this.alpha = Mathf.Lerp(0f, alpha, room.roomSettings.RainIntensity);
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
            sLeaser.sprites[0].alpha = Mathf.Lerp(0f, this.alpha, Mathf.InverseLerp(-0.5f, 0.5f, this.owner.TimePastCycleEnd));

            ////Left
            if (Downpour.windDirection == 1)
            {
                //Bottom Left
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, new Vector2(0f, 0f));
                //Top Left
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, new Vector2(0f, rCam.sSize.y));
                //Bottom Right
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, new Vector2(rCam.sSize.x, 0f));
                //Top Right
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, new Vector2(rCam.sSize.x, rCam.sSize.y));
            }
            else
            {
                //Right
                //Bottom Left
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, new Vector2(0f, 0f));
                //Top Left
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, new Vector2(0f, rCam.sSize.y));
                //Bottom Right
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, new Vector2(rCam.sSize.x, 0f));
                //Top Right
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, new Vector2(rCam.sSize.x, rCam.sSize.y));

            }
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

