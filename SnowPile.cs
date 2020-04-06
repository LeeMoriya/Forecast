using System;
using System.IO;
using RWCustom;
using UnityEngine;

public class SnowPile : UpdatableAndDeletable, IDrawable
{
    public SnowPile(Vector2 pos, float scale)
    {
        this.near = 0f;
        this.far = 0.44f;
        this.alphaFade = 1f;
        this.pos = pos;
        this.scale = scale;
        this.gridDiv = 1;
        this.quad = new Vector2[4];
        this.quad[0] = this.pos + new Vector2(-this.scale, this.scale);
        this.quad[1] = this.pos + new Vector2(this.scale, this.scale);
        this.quad[3] = this.pos + new Vector2(-this.scale, -this.scale);
        this.quad[2] = this.pos + new Vector2(this.scale, -this.scale);
        this.vertices = new float[4, 2];
        for (int i = 0; i < this.vertices.GetLength(0); i++)
        {
            this.vertices[i, 0] = 0.5f;
        }
        this.meshDirty = true;
    }

    public void UpdateMesh()
    {
        this.meshDirty = true;
    }

    public override void Update(bool eu)
    {
        if (!Downpour.snow)
        {
            base.slatedForDeletetion = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Insert))
        {
            this.alphaFade = this.alphaFade + 0.01f;
            this.meshDirty = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Delete))
        {
            this.alphaFade = this.alphaFade - 0.01f;
            this.meshDirty = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Home))
        {
            this.near = this.near + 0.01f;
            this.meshDirty = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.End))
        {
            this.near = this.near - 0.01f;
            this.meshDirty = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.PageUp))
        {
            this.far = this.far + 0.01f;
            this.meshDirty = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.PageDown))
        {
            this.far = this.far - 0.01f;
            this.meshDirty = true;
        }
        base.Update(eu);
    }


    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh("snowpile", this.gridDiv);
        sLeaser.sprites[0] = triangleMesh;
        sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Decal"];
        //sLeaser.sprites[0].rotation = UnityEngine.Random.value * 360f;
        this.verts = new Vector2[(sLeaser.sprites[0] as TriangleMesh).vertices.Length];
        this.AddToContainer(sLeaser, rCam, null);
        this.meshDirty = true;
    }

    public void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        alphaFade = 1 - rCam.currentPalette.darkness;
        sLeaser.sprites[0].RemoveFromContainer();
        this.InitiateSprites(sLeaser, rCam);
        float[,] vertices = this.vertices;
        for (int i = 0; i <= this.gridDiv; i++)
        {
            for (int j = 0; j <= this.gridDiv; j++)
            {
                Vector2 a = Vector2.Lerp(this.quad[0], this.quad[1], (float)j / (float)this.gridDiv);
                Vector2 b = Vector2.Lerp(this.quad[1], this.quad[2], (float)i / (float)this.gridDiv);
                Vector2 b2 = Vector2.Lerp(this.quad[3], this.quad[2], (float)j / (float)this.gridDiv);
                Vector2 a2 = Vector2.Lerp(this.quad[0], this.quad[3], (float)i / (float)this.gridDiv);
                float num = Mathf.Lerp(Mathf.Lerp(vertices[3, 1], vertices[2, 1], (float)i / (float)this.gridDiv), Mathf.Lerp(vertices[0, 1], vertices[1, 1], (float)i / (float)this.gridDiv), (float)j / (float)this.gridDiv);
                float num2 = Mathf.Lerp(Mathf.Lerp(vertices[3, 0], vertices[2, 0], (float)i / (float)this.gridDiv), Mathf.Lerp(vertices[0, 0], vertices[1, 0], (float)i / (float)this.gridDiv), (float)j / (float)this.gridDiv);
                num = Mathf.Pow(num, 1f + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value) * 0.1f);
                num2 = Mathf.Pow(num2, 1f + Mathf.Lerp(-0.5f, 0.5f, UnityEngine.Random.value) * 0.1f);
                num = Mathf.Lerp(num, UnityEngine.Random.value, 0.1f * Mathf.Pow(1f - 2f * Mathf.Abs(num - 0.2f), 2.5f));
                num2 = Mathf.Lerp(num2, UnityEngine.Random.value, 0.1f * Mathf.Pow(1f - 2f * Mathf.Abs(num - 0.2f), 2.5f));
                this.verts[j * (this.gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
                (sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (this.gridDiv + 1) + i] = new Color(near, far, num, alphaFade);
            }
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (this.meshDirty)
        {
            this.UpdateVerts(sLeaser, rCam);
            this.meshDirty = false;
        }
        if (this.elementDirty)
        {
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("snowpile");
            this.elementDirty = false;
        }
        for (int i = 0; i < this.verts.Length; i++)
        {
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, this.verts[i] - camPos);
        }
        if (base.slatedForDeletetion || this.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {

    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.sprites[0].RemoveFromContainer();
        rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
    }

    public Vector2 pos;

    public Vector2[] quad;

    public Vector2[] verts;

    public string color;

    public float scale;

    public float[,] vertices;

    public bool meshDirty;

    public bool elementDirty;

    public int gridDiv;

    public float alphaFade;

    public float near;

    public float far;
}
