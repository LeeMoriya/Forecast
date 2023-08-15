using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using RWCustom;

public class SnowPlacer : CosmeticSprite
{
    public WeatherController weatherController;
    public List<SnowSource> sources;
    public Vector2[] points;
    public float[] radii;

    public SnowPlacer(WeatherController weatherController)
    {
        ForecastLog.Log("Placing SnowSources");
        this.weatherController = weatherController;
        points = new Vector2[20];
        radii = new float[20];

        //Assign points to random positions
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Custom.RandomPointInRect(weatherController.room.RoomRect);
        }
        //Spread the points out so they are a certain distance away from each other
        points = SpreadPoints(points, weatherController.room.RoomRect, 300f, 20);

        //Set the radii for each point to the shortest distance between them
        for (int i = 0; i < points.Length; i++)
        {
            float closestPoint = 100000f;
            for (int s = 0; s < points.Length; s++)
            {
                float distance = Custom.Dist(points[i], points[s]);
                if (distance > 0f && distance < closestPoint)
                {
                    closestPoint = distance;
                }
            }
            if (closestPoint == 100000f)
            {
                closestPoint = 100f;
            }
            radii[i] = closestPoint + 100f;
        }

        //Create the snow sources
        sources = new List<SnowSource>();
        for (int i = 0; i < points.Length; i++)
        {
            SnowSource so = new SnowSource(points[i]);
            so.rad = radii[i];
            sources.Add(so);
            weatherController.room.AddObject(so);
        }
        //Increase the radii for random points based on how many screens there are
        for (int i = 0; i < weatherController.room.cameraPositions.Length; i++)
        {
            sources[UnityEngine.Random.Range(0, 19)].rad += 300f;
        }

        if (ForecastConfig.debugMode.Value)
        {
            for (int i = 0; i < points.Length; i++)
            {
                ForecastLog.Log($"Point {i} radius: {radii[i]} - x{points[i].x} : y{points[i].y}");
            }
        }
    }

    public Vector2[] SpreadPoints(Vector2[] points, FloatRect bounds, float targetDist, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            for (int tp = 0; tp < points.Length; tp++)
            {
                for (int op = 0; op < points.Length; op++)
                {
                    if (tp == op)
                    {
                        continue;
                    }
                    Vector2 offset = points[tp] - points[op];
                    float magnitude = offset.magnitude;
                    float pushAmount = (targetDist - magnitude) * 0.1f;
                    if (pushAmount > 0f)
                    {
                        points[tp] += (offset / magnitude) * pushAmount;
                    }
                    if (!bounds.Vector2Inside(points[tp]))
                    {
                        Vector2 ang = Custom.DirVec(points[tp], bounds.Center) * 5f;
                        points[tp] += ang;
                    }
                }
            }
        }
        return points;
    }


    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[points.Length];
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i] = new FSprite("Futile_White", true);
            sLeaser.sprites[i].alpha = 0.8f;
            sLeaser.sprites[i].color = new Color(0.3f,1f,1f);
        }
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].x = points[i].x - camPos.x;
            sLeaser.sprites[i].y = points[i].y - camPos.y;
            sLeaser.sprites[i].alpha = WeatherHooks.debugUI.toggle ? 0f : 0.7f;
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        for (int i = 0; i < sources.Count; i++)
        {
            sources[i].intensity = Mathf.Lerp(0f, (1f * Mathf.InverseLerp(100f, 500f, radii[i])) + 0.1f, weatherController.settings.currentIntensity * 0.75f);
        }
    }
}
