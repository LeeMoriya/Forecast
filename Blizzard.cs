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
        camera = controller.cam;
        vignette = new FSprite("Futile_White", true);
        vignette.alpha = 0f;
        vignette.color = Color.white;
        vignette.SetAnchor(0.5f, 0.5f);
        vignette.x = camera.game.rainWorld.screenSize.x / 2f;
        vignette.y = camera.game.rainWorld.screenSize.y / 2f;
        vignette.scaleX = camera.game.rainWorld.screenSize.x;
        vignette.scaleY = camera.game.rainWorld.screenSize.y;
        vignette.shader = camera.game.rainWorld.Shaders["EdgeFade"];
        camera.AddSingleCameraDrawable(this);
        camera.ReturnFContainer("HUD").AddChild(vignette);
        Debug.Log("VIGNETTE CREATED");
    }

    public void Draw(RoomCamera camera, float timeStacker, Vector2 camPos)
    {
        if (!camera.ReturnFContainer("HUD")._childNodes.Contains(vignette))
        {
            camera.ReturnFContainer("HUD").AddChild(vignette);
        }
        if (camera.currentPalette.darkness > 0.5f)
        {
            vignette.color = Color.Lerp(camera.currentPalette.skyColor, new Color(0.2f, 0.2f, 0.2f), 0.4f);
        }
        else
        {
            vignette.color = Color.Lerp(camera.currentPalette.texture.GetPixel(9, 5), Color.white, 0.4f);
        }
        vignette.x = camera.game.rainWorld.screenSize.x / 2f;
        vignette.y = camera.game.rainWorld.screenSize.y / 2f;
        vignette.scaleX = (camera.game.rainWorld.screenSize.x * Mathf.Lerp(1.5f, 1f, controller.exposure) + 2f) / 16f;
        vignette.scaleY = (camera.game.rainWorld.screenSize.y * Mathf.Lerp(2.5f, 1.5f, controller.exposure) + 2f) / 16f;
        vignette.alpha = Mathf.Lerp(0f, 0.5f, controller.exposure);
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
    public float cooldown;
    public float bellCooldown;
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
        stats = player.slugcatStats;
        cam = player.room.game.cameras[0];
        if (player.playerState.playerNumber == 0)
        {
            vignette = new Vignette(this);
        }
        dead = false;

        Debug.Log("EXPOSURE CONTROLLER - PLAYER " + player.playerState.playerNumber);

        if (Forecast.debug)
        {
            Vector2 sSize = player.abstractCreature.world.game.cameras[0].sSize;
            float offset = 80f * player.playerState.playerNumber;
            labelPlayer = new FLabel("font", "Player " + player.playerState.playerNumber);
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
        labelExposure.text = "Exposure: " + exposure;
        labelAmbient.text = "Ambient: " + ambient;
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
        labelCooldown.text = "Cycle End: " + TimePastCycleEnd;
    }

    public bool IsCold()
    {
        //Not a shelter, has RoomRain and is enabled in config
        if (!player.room.abstractRoom.shelter && player.room.roomRain != null && player.room.roomSettings.RainIntensity > 0f && (player.room.world.region != null && Forecast.rainRegions.Contains(player.room.world.region.name)))
        {
            return true;
        }
        return false;
    }

    public void Update()
    {
        if (player.room != null)
        {
            //Outdoors
            if (blizzard != null && IsCold())
            {
                //Switch Blizzard
                if (blizzard.room != player.room)
                {
                    SwitchBlizzard();
                    return;
                }
                //Exposed
                if (exposure < 1f)
                {
                    if (blizzard.TimePastCycleEnd > 0f)
                    {
                        float scale = Mathf.Clamp(blizzard.TimePastCycleEnd, 0.00143f, 0.45f);
                        exposure += 0.075f * scale * Time.deltaTime;
                    }
                    else
                    {
                        exposure += 0.025f * Time.deltaTime;
                    }
                    dead = false;
                }
                cooldown += 1f * Time.deltaTime;
            }
            //Indoors
            else
            {
                SwitchBlizzard();
                if (TimePastCycleEnd != -1f)
                {
                    //Exposure matches ambient temp
                    if (IsCold())
                    {
                        cam.microShake = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.005f, player.room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.4f, 1f, TimePastCycleEnd));
                        ambient = Mathf.Lerp(0f, Mathf.Lerp(0f, 1f, player.room.roomSettings.RainIntensity), Mathf.InverseLerp(0f, 3f, TimePastCycleEnd));
                        if (exposure > ambient)
                        {
                            exposure -= 0.065f * Time.deltaTime;
                        }
                        else
                        {
                            exposure += 0.065f * Time.deltaTime;
                        }
                    }
                    //Safe in a shelter, exposure decreases
                    else if (player.room.abstractRoom.shelter && player.room.shelterDoor != null && player.room.shelterDoor.IsClosing)
                    {
                        exposure -= 0.65f * Time.deltaTime;
                    }
                }
            }
            if (exposure > 0f && !dead)
            {
                //Stats
                player.slugcatStats.runspeedFac = Mathf.Lerp(stats.runspeedFac, 0.75f, Mathf.Lerp(0.2f, 0.7f, exposure));
                player.slugcatStats.poleClimbSpeedFac = Mathf.Lerp(stats.poleClimbSpeedFac, 0.85f, Mathf.Lerp(0.2f, 0.7f, exposure));
                //Stun
                if (exposure < 0.4f)
                {
                    if (cooldown >= UnityEngine.Random.Range(5, 15))
                    {
                        cooldown = 0f;
                        player.Stun((int)(20 * exposure));
                    }
                }
                //Exhaustion
                else
                {
                    if (cooldown >= UnityEngine.Random.Range(10, 25))
                    {
                        cooldown = 0f;
                        player.lungsExhausted = true;
                        player.slowMovementStun = (int)(UnityEngine.Random.Range(0.5f, 2f) * exposure);
                        if (UnityEngine.Random.value < 0.3f * exposure)
                        {
                            player.Stun(UnityEngine.Random.Range(50, 120));
                        }
                    }
                }
                player.bodyChunks[0].vel += Custom.RNV() * (0.4f * exposure);
                if (UnityEngine.Random.value < 0.0015)
                {
                    player.Blink(30);
                }

                //Death Bells
                if (exposure >= 1f && !dead && player.playerState.playerNumber == 0)
                {
                    bellCooldown += 1f * Time.deltaTime;
                    if (bellCooldown > Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp(0f, 22, bellRing)))
                    {
                        bellCooldown = 0f;
                        bellRing++;
                        if (Forecast.debug && bellRing > 0)
                        {
                            Debug.Log(bellRing);
                        }
                        player.room.PlaySound(SoundID.MENU_Start_New_Game, player.mainBodyChunk, false, Mathf.Lerp(0.7f, 1.8f, Mathf.InverseLerp(0f, 25f, bellRing)), 1.3f);
                        if (bellRing == 25)
                        {
                            if (!dead)
                            {
                                player.Die();
                                dead = true;
                            }
                        }
                        else
                        {
                            dead = false;
                        }
                    }
                }
                else
                {
                    bellCooldown += 1f * Time.deltaTime;
                    if (bellCooldown > Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp(0f, 22, bellRing)))
                    {
                        bellCooldown = 0;
                        bellRing--;
                        if (Forecast.debug && bellRing > 0)
                        {
                            Debug.Log(bellRing);
                        }
                    }
                }
                bellRing = Mathf.Clamp(bellRing, 0, 25);
            }
            exposure = Mathf.Clamp(exposure, 0f, 1f);
            if (Forecast.debug)
            {
                UpdateDebugLabels();
            }
        }
    }



    public void SwitchBlizzard()
    {
        if (player.room != null)
        {
            for (int i = 0; i < player.room.updateList.Count; i++)
            {
                if (player.room.updateList[i] is Blizzard)
                {
                    blizzard = player.room.updateList[i] as Blizzard;
                    if (Forecast.debug)
                    {
                        Debug.Log("UPDATED BLIZZARD TO " + player.room.abstractRoom.name);
                    }
                    return;
                }
            }
            blizzard = null;
        }
    }

    public float TimePastCycleEnd
    {
        get
        {
            if (player.room != null)
            {
                return (player.room.world.rainCycle.timer - player.room.world.rainCycle.cycleLength) / 2400f;
            }
            return -1f;
        }
    }

    public float rainDeath
    {
        get
        {
            if (player != null)
            {
                return player.rainDeath;
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
            return (room.world.rainCycle.timer - room.world.rainCycle.cycleLength) / 2400f;
        }
    }
    public WeatherSounds(Room room)
    {
        this.room = room;
        sfx = new OmniDirectionalSound[3];
        for (int i = 0; i < sfx.Length; i++)
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
            sfx[i] = new OmniDirectionalSound(sample, false)
            {
                volume = 0f,
                pitch = 1f,
                type = AmbientSound.Type.Omnidirectional
            };
            room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(room.game.cameras[0].virtualMicrophone, sfx[i]));
        }
        CheckBlizzard();
    }
    public void CheckBlizzard()
    {
        if (room != null)
        {
            if (room.roomRain == null || (room.world.region != null && !Forecast.rainRegions.Contains(room.world.region.name)))
            {
                Destroy();
                return;
            }
            for (int i = 0; i < room.updateList.Count; i++)
            {
                if (room.updateList[i] is Blizzard)
                {
                    blizzard = true;
                    return;
                }
            }
            blizzard = false;
        }
    }

    public override void Update(bool eu)
    {
        bool sfx1 = false;
        bool sfx2 = false;
        bool sfx3 = false;
        for (int i = 0; i < room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Count; i++)
        {
            if (room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == sfx[0])
            {
                sfx1 = true;
            }
            if (room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == sfx[1])
            {
                sfx2 = true;
            }
            if (room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == sfx[2])
            {
                sfx3 = true;
            }
            if (sfx.Contains(room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound))
            {
                if (room.BeingViewed)
                {
                    //All three sounds play
                    if (blizzard)
                    {
                        room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 1.2f, room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0f, TimePastCycleEnd));
                    }
                    //Indoors, so only sound two plays
                    else
                    {
                        if (room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound == sfx[2])
                        {
                            room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.65f, room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.3f, TimePastCycleEnd));
                        }
                        else
                        {
                            room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = 0f;
                        }
                    }
                }
                else
                {
                    room.game.cameras[0].virtualMicrophone.ambientSoundPlayers[i].aSound.volume = 0f;
                }
            }
        }
        if (!sfx1 || !sfx2 || !sfx3)
        {
            CheckBlizzard();
        }
        if (!sfx1)
        {
            room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(room.game.cameras[0].virtualMicrophone, sfx[0]));
        }
        if (!sfx2)
        {
            room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(room.game.cameras[0].virtualMicrophone, sfx[1]));
        }
        if (!sfx3)
        {
            room.game.cameras[0].virtualMicrophone.ambientSoundPlayers.Add(new AmbientSoundPlayer(room.game.cameras[0].virtualMicrophone, sfx[2]));
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
        if (Forecast.debug)
        {
            Debug.Log("DOWNPOUR: Blizzard Created");
        }
        this.preciptator = preciptator;
        room = preciptator.room;
        particleLimit = 70;
        if (room.roomSettings.RainIntensity > 0f)
        {
            room.AddObject(new Blizzard.ScrollingTexture(room, this, "overlay1", 4.5f, 0.3f));
            room.AddObject(new Blizzard.ScrollingTexture(room, this, "overlay1", 8.5f, 0.31f));
            room.AddObject(new Blizzard.ScrollingTexture(room, this, "overlay2", 5f, 1f));
            room.AddObject(new Blizzard.ScrollingTexture(room, this, "overlay2", 6.3f, 1f));
        }
    }

    public override void Update(bool eu)
    {
        if (room.roomSettings.RainIntensity == 0f)
        {
            return;
        }
        //Particles
        cooldown++;
        if (cooldown >= Mathf.Lerp(50, 10, Mathf.Lerp(0f, 1f, Mathf.InverseLerp(-0.5f, 0.5f, TimePastCycleEnd))))
        {
            cooldown = 0;
            if (particleCount < Mathf.Lerp(0f, Mathf.Lerp(0f, particleLimit, room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.5f, TimePastCycleEnd)))
            {
                particleCount++;
                room.AddObject(new Blizzard.Particle(this));
            }
        }
        //Wind
        intensity = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.081f, room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.5f, TimePastCycleEnd));
        ThrowAroundObjects();
        //Camera Shake
        if (room.BeingViewed)
        {
            room.game.cameras[0].screenShake = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.3f, room.roomSettings.RainIntensity), Mathf.InverseLerp(-0.5f, 0.5f, TimePastCycleEnd));
        }
        base.Update(eu);
    }

    public float TimePastCycleEnd
    {
        get
        {
            return (room.world.rainCycle.timer - room.world.rainCycle.cycleLength) / 2400f;
        }
    }

    public void ThrowAroundObjects()
    {
        if (room.BeingViewed && room.roomRain != null && room.roomRain.rainReach != null)
        {
            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
                    {
                        BodyChunk bodyChunk = room.physicalObjects[i][j].bodyChunks[k];
                        IntVector2 tilePosition = room.GetTilePosition(bodyChunk.pos + new Vector2(Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value)));
                        float num = intensity;
                        bool flag = false;
                        if (room.roomRain.rainReach[Custom.IntClamp(tilePosition.x, 0, room.TileWidth - 1)] < tilePosition.y)
                        {
                            flag = true;
                            num = intensity;
                        }
                        if (room.water)
                        {
                            num *= Mathf.InverseLerp(room.FloatWaterLevel(bodyChunk.pos.x) - 100f, room.FloatWaterLevel(bodyChunk.pos.x), bodyChunk.pos.y);
                        }
                        if (num > 0f)
                        {
                            //Wind
                            if (bodyChunk.contactPoint.y < 0)
                            {
                                //On Ground
                                if (Forecast.windDirection == 1)
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
                                if (Forecast.windDirection == 1)
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
            xSway = UnityEngine.Random.Range(15f, 25f) * UnityEngine.Random.Range(1f, 1.5f);
            ySway = UnityEngine.Random.Range(7f, 12f) * UnityEngine.Random.Range(1f, 1.5f);
            pos = new Vector2(UnityEngine.Random.Range(0f, 1400f), UnityEngine.Random.Range(0f, 900f));
        }

        public override void Update(bool eu)
        {
            if (reset)
            {
                reset = false;
                alpha = 0f;
                pos = new Vector2(UnityEngine.Random.Range(-50f, 1600f), UnityEngine.Random.Range(-50f, 1100f));
            }
            lastLastPos = lastPos;

            if (Forecast.windDirection == 1)
            {
                pos.x -= xSway * 2f;
                pos.y -= ySway * 2f;
                if (pos.x < -100f || pos.y < -100f)
                {
                    reset = true;
                }
            }
            else
            {
                pos.x += xSway * 2f;
                pos.y -= ySway * 2f;
                if (pos.x > 1400f || pos.y < -100f)
                {
                    reset = true;
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
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (sLeaser.sprites[0].alpha < Mathf.Lerp(0f, Mathf.Lerp(0f, 0.55f, room.roomSettings.RainIntensity), Mathf.Lerp(0f, 1f, Mathf.InverseLerp(-0.5f, 0.5f, owner.TimePastCycleEnd))))
            {
                alpha += 1.2f * Time.deltaTime;
            }
            sLeaser.sprites[0].alpha = alpha;
            sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, Time.deltaTime);
            sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, Time.deltaTime);
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastLastPos, lastPos, Time.deltaTime), Vector2.Lerp(lastPos, pos, Time.deltaTime));
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
            spriteName = sprite;
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
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].alpha = Mathf.Lerp(0f, alpha, Mathf.InverseLerp(-0.5f, 0.5f, owner.TimePastCycleEnd));

            ////Left
            if (Forecast.windDirection == 1)
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
                (sLeaser.sprites[0] as TriangleMesh).UVvertices[i] += new Vector2(0.35f, 0.25f) * scrollSpeed * Time.deltaTime;
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

