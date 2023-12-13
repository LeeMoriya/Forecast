using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

public class Hail : CosmeticSprite
{
    public Vector2 lastLastLastPos;
    public Vector2 lastLastPos;
    public Color color;
    public float gravity;
    public float splashCounter;
    public bool timeToDie;
    public Vector2 dir;
    public float dirCounter;
    public WeatherController spawner;
    public bool reset;
    public Vector2 resetPos;
    public float alpha;
    public Player player;
    public bool forceReset;

    public Hail(Vector2 pos, float rainIntensity, WeatherController spawner)
    {
        alpha = UnityEngine.Random.Range(0.9f, 1f);
        this.spawner = spawner;
        timeToDie = false;
        splashCounter = 0;

        resetPos = new Vector2(pos.x, spawner.room.RoomRect.top + 150f);
        this.pos = new Vector2(pos.x, UnityEngine.Random.Range(pos.y, spawner.room.RoomRect.top + 150f));
        lastPos = pos;
        lastLastPos = pos;
        lastLastLastPos = pos;
        //Vary starting velocity
        vel.y = UnityEngine.Random.Range(-10f * rainIntensity, -20f * rainIntensity);
        vel.x = Mathf.Lerp(UnityEngine.Random.Range(-4, 3f), UnityEngine.Random.Range(-12f, 3f), spawner.settings.currentIntensity);
        gravity = Mathf.Lerp(0.8f, 1f, UnityEngine.Random.value);
    }
    public override void Update(bool eu)
    {
        player = (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);
        if (reset)
        {
            float rng = UnityEngine.Random.value;
            if (rng < 0.05f && room.world.rainCycle.RainDarkPalette > 0)
            {
                Destroy();
            }
            if (spawner.settings.weatherType == 1)
            {
                Destroy();
            }
            if (player != null && player.mainBodyChunk != null && !player.inShortcut)
            {
                resetPos = new Vector2(UnityEngine.Random.Range(player.mainBodyChunk.pos.x - 1300f, player.mainBodyChunk.pos.x + 1300f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            else
            {
                resetPos = new Vector2(UnityEngine.Random.Range(room.RoomRect.left - 100f, room.RoomRect.right + 100f), room.RoomRect.top + UnityEngine.Random.Range(100, 200f));
            }
            pos = resetPos;
            lastPos = resetPos;
            lastLastPos = resetPos;
            lastLastLastPos = resetPos;
            timeToDie = false;
            switch (spawner.settings.windDirection)
            {
                case 1:
                    vel.x = Mathf.Lerp(UnityEngine.Random.Range(8f, -4f), UnityEngine.Random.Range(-13f, -4f), Mathf.Lerp(0f, 1f, spawner.settings.currentIntensity));
                    break;
                case 2:
                    vel.x = Mathf.Lerp(UnityEngine.Random.Range(5f, -5f), UnityEngine.Random.Range(-2f, 2f), Mathf.Lerp(0f, 1f, spawner.settings.currentIntensity));
                    break;
                case 3:
                    vel.x = Mathf.Lerp(UnityEngine.Random.Range(-8f, 4f), UnityEngine.Random.Range(4f, 13f), Mathf.Lerp(0f, 1f, spawner.settings.currentIntensity));
                    break;
            }
            vel.y = UnityEngine.Random.Range(-10f * spawner.settings.currentIntensity, -18f * spawner.settings.currentIntensity);
            splashCounter = 0f;
            reset = false;
        }
        lastLastLastPos = lastLastPos;
        lastLastPos = lastPos;
        //Control fall speed of normal and background drops, tied to rain intensity.
        vel.y -= (gravity * 9.5f);
        if (vel.y < Mathf.Lerp(-32f, -40f, spawner.settings.currentIntensity))
        {
            vel.y = Mathf.Lerp(-32f, -40f, spawner.settings.currentIntensity);
        }
        //Raindrop hits floor or water
        bool hitWater = room.GetTile(pos).WaterSurface;
        if (hitWater && spawner.settings.waterCollision)
        {
            if (room.water && UnityEngine.Random.value > 0.98)
            {
                room.waterObject?.Explosion(pos, 0.45f, 0.89f);
            }
            pos.y = room.MiddleOfTile(pos).y + UnityEngine.Random.Range(2f, 9f);
            vel.y = -vel.y * 0.5f;
            vel.x *= 0.2f;
            //timeToDie = true;
            splashCounter = UnityEngine.Random.Range(0.9f, 1.1f);
        }
        //Raindrop hits floor or water
        if (room.GetTile(pos).Solid || hitWater)
        {
            //Decrease velocity if raindrop hits a solid surface or water and increase splash counter
            if (UnityEngine.Random.value > 0.01f)
            {
                if (vel.y < 0f && !timeToDie)
                {
                    pos.y = room.MiddleOfTile(pos).y + 11f;
                    vel.y = -vel.y * 0.5f;
                    vel.x = vel.x * 0.2f;
                    //timeToDie = true;
                    splashCounter = UnityEngine.Random.Range(0.9f, 1.1f);
                }
                else
                {
                    reset = true;
                }
            }
        }
        //If raindrop falls below room bottom, or if rain intensity is 0, remove it.
        if (pos.y < -100f || spawner.settings.currentIntensity == 0f)
        {
            reset = true;
        }
        base.Update(eu);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[] { new TriangleMesh.Triangle(0, 1, 2) };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[0].alpha = alpha;
        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {

        Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
        Vector2 vector2 = Vector2.Lerp(lastLastLastPos, lastLastPos, timeStacker);
        if (Custom.DistLess(vector, vector2, 9f))
        {
            vector2 = vector + Custom.DirVec(vector, vector2) * 12f;
        }
        vector2 = Vector2.Lerp(vector, vector2, Mathf.InverseLerp(0f, 0.1f, 1f));
        Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector + a - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector - a - camPos);
        (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector2 - camPos);
        //Delete raindrop if it falls a certain distance below the currently viewed room camera
        if (pos.y < (rCam.pos.y - 100f) || (splashCounter <= 0f && timeToDie))
        {
            reset = true;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        sLeaser.sprites[0].color = Color.Lerp(rCam.currentPalette.fogColor, color, 0.9f);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}