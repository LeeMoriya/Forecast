using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;


public class BackgroundRain : UpdatableAndDeletable
{
    public WeatherController weatherController;
    public List<RainCluster> clusters;
    public ScrollingTexture lightRain;
    public ScrollingTexture heavyRain;
    public int cameraPos;
    public BackgroundRain(WeatherController owner)
    {
        room = owner.room;
        weatherController = owner;
        clusters= new List<RainCluster>();
        //for (int i = 0; i < 50; i++)
        //{
        //    RainCluster cluster = new RainCluster(this);
        //    room.AddObject(cluster);
        //    clusters.Add(cluster);
        //}
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if(weatherController.settings.currentIntensity > 0.3f && lightRain == null)
        {
            lightRain = new ScrollingTexture(room, this, false, 7f);
            room.AddObject(lightRain);
        }
        if (weatherController.settings.currentIntensity > 0.7f && heavyRain == null)
        {
            heavyRain = new ScrollingTexture(room, this, true, 7f);
            room.AddObject(heavyRain);
        }
    }

    public class RainCluster : CosmeticSprite
    {
        public BackgroundRain owner;
        public int windDir;
        public float speed;

        public RainCluster(BackgroundRain owner)
        {
            room = owner.room;
            this.owner = owner;
            speed = UnityEngine.Random.Range(0f, 5f);
            windDir = owner.weatherController.settings.windDirection;
            pos.y = (owner.room.cameraPositions[0].x + 360f) + UnityEngine.Random.Range(-500f, 700f);
            pos.x = (owner.room.cameraPositions[0].x + 640f) + UnityEngine.Random.Range(-640f, 640f);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("bg_rain", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            sLeaser.sprites[0].alpha = 0f;
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
            sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.x;
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(lastPos, pos);

            if (pos.y < camPos.y - 500f)
            {
                pos.y = camPos.y + UnityEngine.Random.Range(1500f, 1700f);
                pos.x = (camPos.x + 640f) + UnityEngine.Random.Range(-640f, 640f);
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            pos.y -= Mathf.Lerp(20f, 30f, owner.weatherController.settings.currentIntensity) + speed;
            switch(windDir)
            {
                case 1:
                    pos.x -= Mathf.Lerp(1f, 5f, owner.weatherController.settings.currentIntensity);
                    break;
                case 3:
                    pos.x += Mathf.Lerp(1f, 5f, owner.weatherController.settings.currentIntensity);
                    break;
            }
        }

        public void ResetPositions(Vector2 camPos)
        {
            pos.x = (camPos.x + 640f) + UnityEngine.Random.Range(-640f, 640f);
            pos.y = (camPos.y + 360f) + UnityEngine.Random.Range(-640f, 640f);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            sLeaser.sprites[0].color = Color.Lerp(palette.skyColor, new Color(1f, 1f, 1f), UnityEngine.Random.Range(0.1f, 0.3f));
        }
    }

    public class ScrollingTexture : CosmeticSprite
    {
        public BackgroundRain owner;
        public string spriteName;
        public float scrollSpeed;
        public float alpha;
        public ScrollingTexture(Room room, BackgroundRain owner, bool heavy, float scrollSpeed)
        {
            this.room = room;
            this.owner = owner;
            spriteName = $"rain{(owner.weatherController.settings.windDirection == 2 ? "Mid" : "Dir")}{(heavy ? "Heavy" : "Light")}";
            this.scrollSpeed = scrollSpeed;
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
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            //sLeaser.sprites[0].alpha = Mathf.Lerp(0f, alpha, Mathf.InverseLerp(0.3f, 1f, owner.weatherController.settings.currentIntensity));

            ////Left or Right
            if (owner.weatherController.settings.windDirection != 2)
            {
                if (owner.weatherController.settings.windDirection == 1)
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
                    (sLeaser.sprites[0] as TriangleMesh).UVvertices[i] += new Vector2(0.35f, 0.25f) * scrollSpeed * 0.0256f * timeStacker;
                }
            }
            //Mid
            else
            {
                //Bottom Left
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, new Vector2(0f, 0f));
                //Top Left
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, new Vector2(0f, rCam.sSize.y));
                //Bottom Right
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, new Vector2(rCam.sSize.x, 0f));
                //Top Right
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, new Vector2(rCam.sSize.x, rCam.sSize.y));

                for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).UVvertices.Length; i++)
                {
                    (sLeaser.sprites[0] as TriangleMesh).UVvertices[i] += new Vector2(0f, 0.25f) * scrollSpeed * 0.025f * timeStacker;
                }
            }
            sLeaser.sprites[0].color = Color.Lerp(rCam.currentPalette.fogColor, Color.black, Mathf.Lerp(0f, 0.3f, Mathf.InverseLerp(0.3f, 1f, owner.weatherController.settings.currentIntensity)));
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = Color.Lerp(palette.fogColor, Color.black, 0.1f);
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }
}

