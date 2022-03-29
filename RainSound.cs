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
        if (!Forecast.snow)
        {
            if (ceiling < (owner.Width * 0.95) && owner.regionGate == null)
            {
                normalRainSound = new DisembodiedDynamicSoundLoop(this);
                normalRainSound.sound = SoundID.Normal_Rain_LOOP;
                heavyRainSound = new DisembodiedDynamicSoundLoop(this);
                heavyRainSound.sound = SoundID.Heavy_Rain_LOOP;
            }
            else if (!Forecast.interiorRain)
            {
                interiorRainSound = new DisembodiedDynamicSoundLoop(this);
                interiorRainSound.sound = SoundID.Flash_Flood_LOOP;
                rumbleSound = new DisembodiedDynamicSoundLoop(this);
                rumbleSound.sound = SoundID.Death_Rain_Heard_From_Underground_LOOP;
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
        else if(RainFall.rainIntensity > 0f)
        {
            if (this.room.roomRain != null && this.room.roomSettings.RainIntensity > 0f)
            {
                if (interiorRainSound != null)
                {
                    interiorRainSound.Volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.3f, this.room.roomSettings.RainIntensity), RainFall.rainIntensity);
                    interiorRainSound.Update();
                }
                if (normalRainSound != null)
                {
                    normalRainSound.Volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.85f, this.room.roomSettings.RainIntensity), RainFall.rainIntensity * 1.7f);
                    normalRainSound.Update();
                }
                if (heavyRainSound != null)
                {
                    heavyRainSound.Volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.45f, this.room.roomSettings.RainIntensity), RainFall.rainIntensity * 1.2f);
                    heavyRainSound.Update();
                }
                if (rumbleSound != null)
                {
                    rumbleSound.Volume = Mathf.Lerp(0f, Mathf.Lerp(0f, 0.4f, this.room.roomSettings.RainIntensity), RainFall.rainIntensity);
                    rumbleSound.Update();
                }
            }
        }
    }
}

