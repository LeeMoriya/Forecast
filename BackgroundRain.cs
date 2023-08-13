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
    public int cameraPos;
    public BackgroundRain(WeatherController owner)
    {
        room = owner.room;
        weatherController = owner;
        clusters= new List<RainCluster>();
        for (int i = 0; i < 50; i++)
        {
            RainCluster cluster = new RainCluster(this);
            room.AddObject(cluster);
            clusters.Add(cluster);
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if(cameraPos != room.game.cameras[0].currentCameraPosition)
        {
            cameraPos = room.game.cameras[0].currentCameraPosition;
            for (int i = 0; i < clusters.Count; i++)
            {
                clusters[i].ResetPositions(clusters[i].room.cameraPositions[cameraPos]);
            }
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
}

