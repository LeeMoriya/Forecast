using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SnowFlake : CosmeticSprite
{
    public Vector2 lastLastLastPos;
    public Vector2 lastLastPos;
    public Color color;
    public float gravity;
    public bool foreground;
    public float screenXPos;
    public float splashCounter;
    public Vector2 dir;
    public bool invert;
    public float dirCounter;
    public bool backgroundDrop;
    public float depth;
    public bool collision;
    public bool reset;
    public Player player;
    public Vector2 resetPos;
    public WeatherController spawner;
    public Vector2 offset;
    public bool randomOffset;
    public bool screenReset;
    public Vector2 shortcutPos;
    public Vector2 currentCamPos;
    public Vector2 defaultPos;
    public float directionAdjust;
    public bool simulate = false;
    public bool drawn = false;
    public SnowFlake(Vector2 pos, Color color, float rainIntensity, WeatherController spawner)
    {
        screenReset = false;
        foreground = false;
        splashCounter = 0;
        collision = false;
        this.spawner = spawner;
        if (ForecastMod.rainbow)
        {
            color = Custom.HSL2RGB(UnityEngine.Random.Range(0f, 1f), 0.5f, 0.5f);
        }
        else
        {
            color = Color.Lerp(color, new Color(1f, 1f, 1f), 0.7f);
        }
        defaultPos = pos;
        resetPos = new Vector2(pos.x, spawner.room.RoomRect.top + UnityEngine.Random.Range(130f, 1700f));
        this.pos = pos;
        lastPos = pos;
        lastLastPos = pos;
        lastLastLastPos = pos;
        dir = (new Vector2(UnityEngine.Random.Range(-2f, 0.1f), -5f) * rainIntensity);
        gravity = Mathf.Lerp(0.4f, 0.9f, UnityEngine.Random.value);
        this.pos += vel * (40f * rainIntensity);
        dir = new Vector2(-dir.x + (directionAdjust * spawner.settings.currentIntensity), dir.y);
        vel = dir;
        this.pos += vel * (4f * rainIntensity);
        dirCounter = UnityEngine.Random.Range(2f, 10f);
        switch (spawner.settings.windDirection)
        {
            case 1:
                directionAdjust = -1.2f;
                break;
            case 2:
                directionAdjust = 0f;
                break;
            case 3:
                directionAdjust = 1.2f;
                break;
        }
        //Depth
        depth = UnityEngine.Random.Range(0f, 0.9f);
    }

    public override void Update(bool eu)
    {
        if (room.world.rainCycle.RainDarkPalette > 0f)
        {
            if (UnityEngine.Random.value > 0.98f)
            {
                Destroy();
            }
        }

        //Upon Reset
        player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);

        if (reset)
        {
            if (player != null && player.mainBodyChunk != null && !player.inShortcut && room.BeingViewed)
            {
                depth = UnityEngine.Random.Range(0f, 0.9f);
                Vector2 cam = spawner.camPos;
                IntVector2 randomOffset = IntVector2.FromVector2(new Vector2(cam.x + UnityEngine.Random.Range(-700, 700), cam.y + UnityEngine.Random.Range(-500, 500)));
                Vector2 offset2 = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
                //If that random position has line of sight with the sky, spawn a snowflake there
                Vector2 spawn = randomOffset.ToVector2();
                Vector2 spawnPos = spawn + offset2;
                if ((spawner.RayTraceSky(spawnPos, new Vector2(0f, 1f)))&& spawnPos.y > room.floatWaterLevel)
                {
                    resetPos = spawnPos;
                }
                else if(depth > 0.7f)
                {
                    return;
                }
            }
            dir = (new Vector2(UnityEngine.Random.Range(-4f, 4f) + (directionAdjust * spawner.settings.currentIntensity), -5f) * spawner.settings.currentIntensity);
            vel = dir;
            pos = resetPos;
            lastPos = pos;
            lastLastPos = pos;
            lastLastLastPos = pos;
            reset = false;
        }
        lastLastLastPos = lastLastPos;
        lastLastPos = lastPos;

        //Depth test
        //depth = Mathf.Lerp(0f, 0.9f, Mathf.PingPong(Time.time * 0.5f, 1));

        //Debug Depth controls
        if (ForecastConfig.debugMode.Value)
        {
            if (Input.GetKey(KeyCode.KeypadPlus))
            {
                depth += 0.01f;
            }
            if (Input.GetKey(KeyCode.KeypadMinus))
            {
                depth -= 0.01f;
            }
            if (Input.GetKey(KeyCode.KeypadMultiply))
            {
                depth = UnityEngine.Random.Range(0f, 0.9f);
            }
        }

        //Vertical Velocity
        vel.y = Mathf.Lerp((-2.5f * spawner.settings.currentIntensity), (-4f * spawner.settings.currentIntensity), depth);

        //Horizontal Velocity
        if (spawner.settings.windDirection == 1)
        {
            if ((vel.x > 1f * spawner.settings.currentIntensity || vel.x < -4f * spawner.settings.currentIntensity) && dirCounter <= 0f)
            {
                if (vel.x > 1f)
                {
                    vel.x = vel.x * 0.03f;
                }
                dir = new Vector2(-dir.x + (directionAdjust * spawner.settings.currentIntensity), dir.y);
                dirCounter = UnityEngine.Random.Range(2f, 10f);
            }
        }
        else if (spawner.settings.windDirection == 2)
        {
            if ((vel.x > 4f * spawner.settings.currentIntensity || vel.x < -4f * spawner.settings.currentIntensity) && dirCounter <= 0f)
            {
                dir = new Vector2(-dir.x, dir.y);
                dirCounter = UnityEngine.Random.Range(2f, 10f);
            }
        }
        else
        {
            if ((vel.x > 4f * spawner.settings.currentIntensity || vel.x < -1f * spawner.settings.currentIntensity) && dirCounter <= 0f)
            {
                if (vel.x < -1f)
                {
                    vel.x = vel.x * 0.03f;
                }
                dir = new Vector2(-dir.x + (directionAdjust * spawner.settings.currentIntensity), dir.y);
                dirCounter = UnityEngine.Random.Range(2f, 10f);
            }
        }
        vel += dir * 0.02f;
        dirCounter = dirCounter - (0.1f + (spawner.settings.currentIntensity * UnityEngine.Random.value));
        if (dirCounter < 0f)
        {
            dirCounter = 0;
        }

        //Reset
        if (room.waterObject != null && room.GetTile(pos).WaterSurface || pos.y < room.floatWaterLevel)
        {
            reset = true;
        }
        if (depth > 0.8f)
        {
            if ((room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(pos).Solid || room.GetTile(pos).AnyWater))
            {
                reset = true;
            }

        }
        if (room.terrain != null && room.terrain.ObstructsTile(room.GetTilePosition(pos)))
        {
            reset = true;
        }
        if (pos.y < -100f)
        {
            reset = true;
        }
        base.Update(eu);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        if (UnityEngine.Random.value > 0.4f)
        {
            sLeaser.sprites[0] = new FSprite("SkyDandelion", true);
            sLeaser.sprites[0].scale = 0.25f;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
        }
        else
        {
            sLeaser.sprites[0] = new FSprite("deerEyeB", true);
            sLeaser.sprites[0].scale = 0.45f;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
        }
        sLeaser.sprites[0].alpha = depth;
        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastLastPos, lastPos, timeStacker), Vector2.Lerp(lastPos, pos, timeStacker));
        sLeaser.sprites[0].scaleY = Mathf.Max(0.45f, 0.45f + 0.1f * Vector2.Distance(Vector2.Lerp(lastLastPos, lastPos, timeStacker), Vector2.Lerp(lastPos, pos, timeStacker)));
        sLeaser.sprites[0].scaleX = Custom.LerpMap(depth, 0f, 1f, 0.2f, 0.55f);
        sLeaser.sprites[0].alpha = depth;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (palette.darkness > 0.5f)
        {
            sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, new Color(0.15f, 0.15f, 0.15f), Mathf.Lerp(0.8f, 0.5f, depth));
        }
        else
        {
            sLeaser.sprites[0].color = Color.Lerp(palette.texture.GetPixel(9, 5), Color.white, Mathf.Lerp(0.8f, 0.5f, depth));
        }
        base.ApplyPalette(sLeaser, rCam, palette);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner = rCam.ReturnFContainer("GrabShaders");
        sLeaser.sprites[0].RemoveFromContainer();
        newContatiner.AddChild(sLeaser.sprites[0]);
    }
}

//Snowdust
public class SnowDust : CosmeticSprite
{
    public SnowDust(Vector2 pos, float size)
    {
        this.pos = pos;
        lastPos = pos;
        this.size = size;
        lastLife = 1f;
        life = 1f;
        lifeTime = Mathf.Lerp(40f, 120f, UnityEngine.Random.value) * Mathf.Lerp(0.5f, 1.5f, size);
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        pos.y = pos.y + 0.5f;
        pos.x = pos.x + 0.25f;
        lastLife = life;
        life -= 1f / lifeTime;
        if (lastLife < 0f)
        {
            Destroy();
        }
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White", true);
        sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Spores"];
        sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
        base.InitiateSprites(sLeaser, rCam);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[0].scale = 10f * Mathf.Pow(1f - Mathf.Lerp(lastLife, life, timeStacker), 0.35f) * Mathf.Lerp(0.5f, 1.5f, size);
        sLeaser.sprites[0].alpha = Mathf.Lerp(lastLife, life, timeStacker);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (palette.darkness > 0.5f)
        {
            sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, new Color(0.2f, 0.2f, 0.2f), 0.4f);
        }
        else
        {
            sLeaser.sprites[0].color = Color.Lerp(palette.texture.GetPixel(9, 5), Color.white, 0.12f);
        }
        base.ApplyPalette(sLeaser, rCam, palette);
    }
    public float life;
    public float lastLife;
    public float lifeTime;
    public float size;
}