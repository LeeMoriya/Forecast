using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

public class Preciptator : UpdatableAndDeletable
{
    public Player player;
    public bool isSnow;
    public Room currentRoom;
    public List<RainDrop> rainDrops = new List<RainDrop>();
    public float rainAmount;
    public Vector2 spawn;
    public int rainLimit;

    public Preciptator(Room room, bool isSnow)
    {
        this.currentRoom = room;
        this.isSnow = isSnow;
        //If ConfigMachine isn't present, use default rain amount values
        if (Downpour.rainAmount == 0)
        {
            this.rainAmount = Mathf.Lerp(0, 30, RainFall.rainIntensity);
        }
        //rain amount values based on config menu slider
        else
        {
            this.rainAmount = Mathf.Lerp(0, Downpour.rainAmount, RainFall.rainIntensity);
        }
        //Maximum amount of raindrops that can be present in a room, changes based on rain amount
        this.rainLimit = (int)Mathf.Lerp(0, this.rainAmount * 10, RainFall.rainIntensity);
    }

    public void AddRaindrop()
    {
        //If player is present in the room, adjust spawn position to be a range around them
        if (player.mainBodyChunk.pos != null && room.BeingViewed)
        {
            spawn = new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1300f, player.mainBodyChunk.pos.x + 1300f), room.RoomRect.top - 5f);
        }
        //Otherwise spawn rain across the room's width
        else
        {
            spawn = new Vector2(UnityEngine.Random.Range(room.RoomRect.left - 100f, room.RoomRect.right + 100f), room.RoomRect.top - 5f);
        }
        //Raindrop is added to a dictionary when spawned
        RainDrop rainDrop = new RainDrop(new Vector2(spawn.x, spawn.y + UnityEngine.Random.Range(70f, 250f)), Color.Lerp(room.game.cameras[0].currentPalette.skyColor, new Color(1f, 1f, 1f), 0.1f), RainFall.rainIntensity);
        this.room.AddObject(rainDrop);
        this.rainDrops.Add(rainDrop);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        //Get player in room
        this.player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);
        //If room is realised and isn't a gate room
        if (room != null && !room.abstractRoom.gate)
        {
            //When a raindrop is deleted from the room, remove it from the dictionary
            for (int i = this.rainDrops.Count - 1; i >= 0; i--)
            {
                if (this.rainDrops[i].slatedForDeletetion)
                {
                    this.rainDrops.RemoveAt(i);
                }
            }
            //If the raindrop dictionary is less than 90% full, spawn rain normally
            if (this.rainDrops.Count < this.rainLimit * 0.9f)
            {
                for (int i = 0; i < this.rainAmount * RainFall.rainIntensity; i++)
                {
                    this.AddRaindrop();
                }
            }
            //If the raindrop dictionary is reaching its limit, spawn rain at half the rain
            else
            {
                for (int i = 0; i < this.rainAmount * (RainFall.rainIntensity * 0.5f); i++)
                {
                    this.AddRaindrop();
                }
            }
        }
    }
}
public class RainDrop : CosmeticSprite
{
    public Vector2 lastLastLastPos;
    public Vector2 lastLastPos;
    public Color color;
    public float gravity;
    public float splashCounter;
    public bool timeToDie;
    public Vector2 dir;
    public float dirCounter;
    public bool backgroundDrop;
    public float depth;
    public bool collision;

    public RainDrop(Vector2 pos, Color color, float rainIntensity)
    {
        this.timeToDie = false;
        this.splashCounter = 0;
        this.collision = false;
        //Small chance for any raindrop to be a background drop, asign it a random depth value
        if (UnityEngine.Random.Range(0f, 1f) > 0.9f)
        {
            backgroundDrop = true;
            this.depth = UnityEngine.Random.Range(0f, 1f);
        }
        else
        {
            backgroundDrop = false;
            this.depth = 1f;
        }
        if (Downpour.rainbow)
        {
            this.color = Custom.HSL2RGB(UnityEngine.Random.Range(0f, 1f), 0.5f, 0.5f);
        }
        else
        {
            this.color = color;
        }
        this.pos = pos;
        this.lastPos = this.pos;
        this.lastLastPos = this.pos;
        this.lastLastLastPos = this.pos;
        //Vary starting velocity
        this.vel.y = UnityEngine.Random.Range(-10f * rainIntensity, -20f * rainIntensity);
        //Increase spread of raindrops as rain intensity increases
        this.vel.x = this.vel.x + (UnityEngine.Random.Range(Mathf.Lerp(2f, -2f, rainIntensity), Mathf.Lerp(2f, -10f, rainIntensity)));
        this.gravity = Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value);
    }
    public override void Update(bool eu)
    {
        if (slatedForDeletetion)
        {
            this.Destroy();
        }
        this.lastLastLastPos = this.lastLastPos;
        this.lastLastPos = this.lastPos;
        //Control fall speed of normal and background drops, tied to rain intensity.
        if (backgroundDrop)
        {
            if (collision)
            {
                this.vel.y = 0f;
            }
            else
            {
                this.vel.y = this.vel.y - (this.gravity * 3.9f);
                if (this.vel.y < Mathf.Lerp(-20f, -35f, RainFall.rainIntensity))
                {
                    this.vel.y = Mathf.Lerp(-20f, -35f, RainFall.rainIntensity);
                }
            }
        }
        else
        {
            this.vel.y = this.vel.y - (this.gravity * 9.5f);
            if (this.vel.y < Mathf.Lerp(-37f, -45f, RainFall.rainIntensity))
            {
                this.vel.y = Mathf.Lerp(-37f, -45f, RainFall.rainIntensity);
            }
        }
        //Decrease splash scale on hit
        if (collision)
        {
            this.splashCounter = this.splashCounter - 0.1f;
            if (splashCounter < 0f)
            {
                splashCounter = 0f;
            }
        }
        //Raindrop hits floor or water
        if ((this.room.GetTile(this.pos).Solid || this.room.GetTile(this.pos).AnyWater))
        {
            //Decrease velocity if raindrop hits a solid surface or water and increase splash counter
            if (UnityEngine.Random.Range(0f, 1f) > 0.01f)
            {
                if (this.vel.y < 0f && !timeToDie)
                {
                    if (this.room.GetTile(this.pos).AnyWater)
                    {
                        this.pos.y = this.room.MiddleOfTile(this.pos).y + UnityEngine.Random.Range(-4f, 3f);
                    }
                    else
                    {
                        this.pos.y = this.room.MiddleOfTile(this.pos).y + 8.3f;
                    }
                    this.vel.y = this.vel.y * -0.2f;
                    this.vel.x = this.vel.x * -0.9f;
                    timeToDie = true;
                    splashCounter = UnityEngine.Random.Range(0.9f, 1.1f);
                    collision = true;
                }
                else
                {
                    this.slatedForDeletetion = true;
                }
            }
        }
        //If raindrop falls below room bottom, or if rain intensity is 0, remove it.
        if (this.pos.y < -100f || RainFall.rainIntensity == 0f)
        {
            this.slatedForDeletetion = true;
        }
        base.Update(eu);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[] { new TriangleMesh.Triangle(0, 1, 2) };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[1] = new FSprite("RainSplash", true);
        if (backgroundDrop)
        {
            sLeaser.sprites[1].alpha = Mathf.Lerp(0.6f, 1f, this.depth);
        }
        else
        {
            sLeaser.sprites[1].alpha = UnityEngine.Random.Range(0.82f, 1f);
        }
        sLeaser.sprites[0].alpha = UnityEngine.Random.Range(0.77f, 0.94f);
        sLeaser.sprites[0].color = Color.Lerp(rCam.currentPalette.fogColor, color, this.depth * 0.9f);
        sLeaser.sprites[1].color = Color.Lerp(rCam.currentPalette.fogColor, color, this.depth * 0.9f);
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
        vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, this.depth));
        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + a * 1f - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - a * 1f - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
        sLeaser.sprites[1].x = vector.x - camPos.x;
        sLeaser.sprites[1].y = vector.y - camPos.y;
        sLeaser.sprites[1].rotation = UnityEngine.Random.value * 360f;
        //If background drops encounter a depth in the room texture lower than their own depth value, treat it as a collision.
        if (backgroundDrop && !slatedForDeletetion && !this.collision && rCam.IsViewedByCameraPosition(rCam.cameraNumber, this.pos) && rCam.DepthAtCoordinate(this.pos) < this.depth)
        {
            splashCounter = 1f;
            timeToDie = true;
            this.collision = true;
        }
        if (splashCounter > 0f)
        {
            //If splash counter is greater than 0 adjust the scale of the splash sprite based on whether its a background drop or not.
            if (backgroundDrop)
            {
                sLeaser.sprites[1].scale = Mathf.Lerp(0f, this.depth * 0.4f, splashCounter);
                this.vel.y = 0f;
                this.vel.x = 0f;
            }
            else
            {
                sLeaser.sprites[1].scale = Mathf.Lerp(0f, UnityEngine.Random.Range(0.3f, 0.55f), splashCounter);
            }
            sLeaser.sprites[0].alpha = 0f;
        }
        else
        {
            sLeaser.sprites[1].scale = 0f;
        }
        //Delete raindrop if it falls a certain distance below the currently viewed room camera
        if (rCam.room.BeingViewed && this.pos.y < (rCam.pos.y - 100f) || sLeaser.sprites[0].alpha < 0f || (splashCounter <= 0f && timeToDie) || base.slatedForDeletetion || this.room != rCam.room)
        {
            this.slatedForDeletetion = true;
            sLeaser.CleanSpritesAndRemove();
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}

