using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        if (RainFall.rainIntensity > 0.3f || room.game.IsArenaSession)
        {
            if (ceiling < (owner.Width * 0.65) && owner.regionGate == null)
            {
                normalRainSound = new DisembodiedDynamicSoundLoop(this);
                normalRainSound.sound = SoundID.Normal_Rain_LOOP;
                normalRainSound.Volume = 1f;
                heavyRainSound = new DisembodiedDynamicSoundLoop(this);
                heavyRainSound.sound = SoundID.Heavy_Rain_LOOP;
                heavyRainSound.Volume = 0.3f;
            }
            else
            {
                interiorRainSound = new DisembodiedDynamicSoundLoop(this);
                interiorRainSound.sound = SoundID.Flash_Flood_LOOP;
                interiorRainSound.Volume = 0.7f;
                rumbleSound = new DisembodiedDynamicSoundLoop(this);
                rumbleSound.sound = SoundID.Death_Rain_Heard_From_Underground_LOOP;
                rumbleSound.Volume = 0.2f;
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
                interiorRainSound.Update();
            }
            if (normalRainSound != null)
            {
                normalRainSound.Update();
            }
            if (heavyRainSound != null)
            {
                heavyRainSound.Update();
            }
            if (rumbleSound != null)
            {
                rumbleSound.Update();
            }
        }
    }
}

