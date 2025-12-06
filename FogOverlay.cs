using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FogOverlay : CosmeticSprite
{
    public string spriteName;
    public float scrollSpeed;
    public float alpha;
    public TriangleMesh mesh;
    public Vector2[] UVs, lastUVs;
    public WeatherController owner;
    public Vector2 dir;
    public FogOverlay(Room room, WeatherController owner, Vector2 dir, string sprite, float scrollSpeed, float alpha)
    {
        this.owner = owner;
        spriteName = sprite;
        this.scrollSpeed = scrollSpeed;
        this.alpha = alpha;
        this.dir = dir;
        ForecastLog.Log("FogOverlay Added");
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

        this.mesh = mesh;
        UVs = new Vector2[mesh.UVvertices.Length];
        lastUVs = new Vector2[mesh.UVvertices.Length];
        for (int i = 0; i < mesh.UVvertices.Length; i++)
        {
            UVs[i] = mesh.UVvertices[i];
        }

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].alpha = Mathf.Lerp(0.15f, alpha, Mathf.InverseLerp(owner.settings.currentWeather.minIntensity, owner.settings.currentWeather.maxIntensity, owner.settings.currentIntensity));

        ////Left
        if (ForecastMod.blizzardDirection == 1)
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
        for (int i = 0; i < mesh.UVvertices.Length; i++)
        {
            mesh.UVvertices[i] = Vector2.Lerp(lastUVs[i], UVs[i], timeStacker);
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (mesh != null)
        {
            for (int i = 0; i < UVs.Length; i++)
            {
                lastUVs[i] = UVs[i];
                UVs[i] += dir * scrollSpeed * 0.025f;
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = palette.fogColor;
        base.ApplyPalette(sLeaser, rCam, palette);
    }
}
