using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class DebugWeatherUI
{
    public FContainer container;
    public List<FLabel> labels;
    public WeatherController.WeatherSettings settings;
    public int activeCounter;
    public int refreshCounter;
    public bool toggle;

    public DebugWeatherUI()
    {
        container = new FContainer();
        labels = new List<FLabel>();
        Futile.stage.AddChild(container);
    }

    public void RemoveSprites()
    {
        container.RemoveAllChildren();
        container.RemoveFromContainer();
    }

    public void Update()
    {
        activeCounter--;
        if (activeCounter <= 0)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                labels[i].alpha -= 0.025f;
            }
        }
        else
        {
            for (int i = 0; i < labels.Count; i++)
            {
                labels[i].alpha += 0.025f;
                labels[i].alpha = Mathf.Clamp(labels[i].alpha, 0f, 1f);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if(toggle)
            {
                toggle = false;
            }
            else
            {
                toggle = true;
            }
        }
    }

    public void UpdateLabels(WeatherController.WeatherSettings settings)
    {
        this.settings = settings;
        container.RemoveAllChildren();
        labels = new List<FLabel>();

        FLabel toggleLabel = new FLabel("font", "TOGGLE UI  -  F9\n");
        labels.Add(toggleLabel);
        FLabel roomHeading = new FLabel("font", $"{settings.roomName} - Weather Settings");
        labels.Add(roomHeading);

        if (ForecastConfig.customRegionSettings.ContainsKey(settings.regionName))
        {
            foreach (KeyValuePair<string, List<string>> pair in ForecastConfig.customRegionSettings[settings.regionName])
            {
                //GLOBAL TAGS
                if (pair.Key == "GLOBAL")
                {
                    FLabel globalTags = new FLabel("font", "Global Tags: ");
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        globalTags.text += pair.Value[i] + ", ";
                    }
                    labels.Add(globalTags);
                }
                //ROOM TAGS
                else if (pair.Key == settings.roomName)
                {
                    FLabel roomLabel = new FLabel("font", "Room Tags: ");
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        roomLabel.text += pair.Value[i] + ", ";
                    }
                    roomLabel.text += "\n";
                    labels.Add(roomLabel);
                }
            }
        }

        //SETTINGS
        FLabel settingsLabel = new FLabel("font", "\n\n\nGENERAL:\n");
        settingsLabel.text += $"Intensity: {Mathf.RoundToInt(settings.currentIntensity * 100f)}% - {(settings.weatherIntensity == 0 ? "DYNAMIC" : "FIXED")}\n";
        settingsLabel.text += $"Particle Limit: {settings.particleLimit}\n";
        settingsLabel.text += $"Wind Direction: {WindDir(settings.windDirection)}\n";
        settingsLabel.text += $"Rain Volume: {(settings.rainVolume ? "ON" : "OFF")}\n\n";


        settingsLabel.text += "VISUALS:\n";
        settingsLabel.text += $"Background Collision: {(settings.backgroundCollision ? "ON" : "OFF")}\n";
        settingsLabel.text += $"Water Collision: {(settings.waterCollision ? "ON" : "OFF")}\n";
        settingsLabel.text += $"Dynamic Clouds: {(settings.dynamicClouds ? "ON" : "OFF")}\n";
        settingsLabel.text += $"Background Lightning: {(settings.backgroundLightning ? "ON" : "OFF")}\n\n";


        settingsLabel.text += $"LIGHTNING:\n";
        settingsLabel.text += $"Lightning Strikes: {(settings.lightningStrikes ? "ON" : "OFF")}\n";
        settingsLabel.text += $"Lightning Interval: {Mathf.RoundToInt(settings.lightningInterval)} seconds\n";
        settingsLabel.text += $"Lightning Chance: {Mathf.RoundToInt(settings.lightningChance)}%\n";
        settingsLabel.text += $"Lightning Color: R:{Mathf.RoundToInt(settings.strikeColor.r * 255)}, G:{Mathf.RoundToInt(settings.strikeColor.g * 255)}, B:{Mathf.RoundToInt(settings.strikeColor.b * 255)}\n";

        labels.Add(settingsLabel);

        for (int i = 0; i < labels.Count; i++)
        {
            labels[i].SetAnchor(0f, 1f);
            labels[i].SetPosition(new Vector2(20.01f, 768.01f - 45f - (20f * i)));
            container.AddChild(labels[i]);
        }
    }

    public string WindDir(int i)
    {
        switch (i)
        {
            case 1:
                return "LEFT";
            case 2:
                return "MID";
            case 3:
                return "RIGHT";
        }
        return "RANDOM";
    }
}

