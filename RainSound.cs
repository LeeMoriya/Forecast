using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class RainSound : UpdatableAndDeletable
{
    public DisembodiedDynamicSoundLoop normalRainSound;
    public DisembodiedDynamicSoundLoop heavyRainSound;
    public DisembodiedDynamicSoundLoop interiorRainSound;
    public DisembodiedDynamicSoundLoop rumbleSound;
    public Room owner;
    public int ceiling = 0;

    public RainSound(Room room)
    {
        owner = room;
        for (int r = 0; r < owner.TileWidth; r++)
        {
            if (owner.Tiles[r, owner.TileHeight - 1].Solid)
            {
                ceiling++;
            }
        }
        if (!Downpour.snow)
        {
            if (ceiling < (owner.Width * 0.95) && owner.regionGate == null)
            {
                normalRainSound = new DisembodiedDynamicSoundLoop(this);
                normalRainSound.sound = SoundID.Normal_Rain_LOOP;
                normalRainSound.Volume = Mathf.Lerp(0f, 1.2f, RainFall.rainIntensity);
                heavyRainSound = new DisembodiedDynamicSoundLoop(this);
                heavyRainSound.sound = SoundID.Heavy_Rain_LOOP;
                heavyRainSound.Volume = Mathf.Lerp(0f, 0.6f, RainFall.rainIntensity);
            }
            else if (!Downpour.interiorRain)
            {
                interiorRainSound = new DisembodiedDynamicSoundLoop(this);
                interiorRainSound.sound = SoundID.Flash_Flood_LOOP;
                interiorRainSound.Volume = Mathf.Lerp(0f, 0.35f, RainFall.rainIntensity);
                rumbleSound = new DisembodiedDynamicSoundLoop(this);
                rumbleSound.sound = SoundID.Death_Rain_Heard_From_Underground_LOOP;
                rumbleSound.Volume = Mathf.Lerp(0f, 0.18f, RainFall.rainIntensity);
            }
        }
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (owner.abstractRoom.shelter)
        {
            this.Destroy();
        }
        else
        {
            if (interiorRainSound != null)
            {
                interiorRainSound.Volume = Mathf.Lerp(0f, 0.3f, RainFall.rainIntensity);
                interiorRainSound.Update();
            }
            if (normalRainSound != null)
            {
                normalRainSound.Volume = Mathf.Lerp(0f, 1f, RainFall.rainIntensity);
                normalRainSound.Update();
            }
            if (heavyRainSound != null)
            {
                heavyRainSound.Volume = Mathf.Lerp(0f, 0.4f, RainFall.rainIntensity);
                heavyRainSound.Update();
            }
            if (rumbleSound != null)
            {
                rumbleSound.Volume = Mathf.Lerp(0f, 0.4f, RainFall.rainIntensity);
                rumbleSound.Update();
            }
        }
    }
}

