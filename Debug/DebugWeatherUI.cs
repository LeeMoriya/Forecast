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
        if (settings == null || settings?.owner?.room?.game?.cameras[0]?.room?.abstractRoom?.name != settings?.roomName)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                labels[i].alpha -= 0.025f;
            }
            return;
        }
        if (toggle)
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
            if (toggle)
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

        FLabel toggleLabel = new FLabel(RWCustom.Custom.GetFont(), ForecastMod.Translate("TOGGLE UI  -  F9\n"));
        labels.Add(toggleLabel);
        FLabel roomHeading = new FLabel(RWCustom.Custom.GetFont(), string.Format(ForecastMod.Translate("{0} - Weather Settings"), settings.roomName));
        labels.Add(roomHeading);

        if (ForecastConfig.customRegionSettings.ContainsKey(settings.regionName))
        {
            foreach (KeyValuePair<string, List<string>> pair in ForecastConfig.customRegionSettings[settings.regionName])
            {
                //GLOBAL TAGS
                if (pair.Key == "GLOBAL")
                {
                    FLabel globalTags = new FLabel(RWCustom.Custom.GetFont(), ForecastMod.Translate("Global Tags: "));
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        globalTags.text += pair.Value[i] + ", ";
                    }
                    labels.Add(globalTags);
                }
                //ROOM TAGS
                else if (pair.Key == settings.roomName)
                {
                    FLabel roomLabel = new FLabel(RWCustom.Custom.GetFont(), ForecastMod.Translate("Room Tags: "));
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
        FLabel settingsLabel = new FLabel(RWCustom.Custom.GetFont(), ForecastMod.Translate("\n\n\nGENERAL:\n\n"));

        if (settings.currentWeather != null)
        {
            settingsLabel.text += string.Format(ForecastMod.Translate("Forecast:\nNow: {0}\nNext: {1}\nLater: {2}\n\n"),
                WeatherName(settings.currentWeather.type),
                WeatherName(WeatherForecast.regionWeatherForecasts[settings.regionName][1]),
                WeatherName(WeatherForecast.regionWeatherForecasts[settings.regionName][2]));
        }

        settingsLabel.text += string.Format(ForecastMod.Translate("Interior: {0}\n"), ForecastMod.Translate(settings.owner.interior ? "YES" : "NO"));
        settingsLabel.text += string.Format(ForecastMod.Translate("DangerType: {0}\n"), settings.owner.room.roomSettings.DangerType.value);
        settingsLabel.text += string.Format(ForecastMod.Translate("Intensity: {0}% - {1}\n"), Mathf.RoundToInt(settings.currentIntensity * 100f), ForecastMod.Translate(settings.weatherIntensity == 0 ? "DYNAMIC" : "FIXED"));
        settingsLabel.text += string.Format(ForecastMod.Translate("Particle Limit: {0}\n"), settings.particleLimit);
        settingsLabel.text += string.Format(ForecastMod.Translate("Wind Direction: {0}\n"), WindDir(settings.windDirection));
        settingsLabel.text += string.Format(ForecastMod.Translate("Rain Volume: {0}\n\n"), ForecastMod.Translate(settings.rainVolume ? "ON" : "OFF"));

        settingsLabel.text += ForecastMod.Translate("VISUALS:\n");
        settingsLabel.text += string.Format(ForecastMod.Translate("Background Collision: {0}\n"), ForecastMod.Translate(settings.backgroundCollision ? "ON" : "OFF"));
        settingsLabel.text += string.Format(ForecastMod.Translate("Water Collision: {0}\n"), ForecastMod.Translate(settings.waterCollision ? "ON" : "OFF"));
        settingsLabel.text += string.Format(ForecastMod.Translate("Dynamic Clouds: {0}\n"), ForecastMod.Translate(settings.dynamicClouds ? "ON" : "OFF"));
        settingsLabel.text += string.Format(ForecastMod.Translate("Background Lightning: {0}\n\n"), ForecastMod.Translate(settings.backgroundLightning ? "ON" : "OFF"));

        settingsLabel.text += ForecastMod.Translate("LIGHTNING:\n");
        settingsLabel.text += string.Format(ForecastMod.Translate("Lightning Strikes: {0}\n"), ForecastMod.Translate(settings.lightningStrikes ? "ON" : "OFF"));
        settingsLabel.text += string.Format(ForecastMod.Translate("Lightning Interval: {0} seconds\n"), Mathf.RoundToInt(settings.lightningInterval));
        settingsLabel.text += string.Format(ForecastMod.Translate("Lightning Chance: {0}%\n"), Mathf.RoundToInt(settings.lightningChance));
        settingsLabel.text += string.Format(ForecastMod.Translate("Lightning Color: R:{0}, G:{1}, B:{2}\n"), Mathf.RoundToInt(settings.strikeColor.r * 255), Mathf.RoundToInt(settings.strikeColor.g * 255), Mathf.RoundToInt(settings.strikeColor.b * 255));

        labels.Add(settingsLabel);

        for (int i = 0; i < labels.Count; i++)
        {
            labels[i].SetAnchor(0f, 1f);
            labels[i].SetPosition(new Vector2(20.01f, 768.01f - 45f - (20f * i)));
            container.AddChild(labels[i]);
            if (toggle)
            {
                labels[i].alpha = 0f;
            }
        }
        container.MoveToFront();
    }

    private static string WeatherName(WeatherForecast.Weather.WeatherType weather)
    {
        return ForecastMod.Translate(System.Text.RegularExpressions.Regex.Replace(weather.ToString(), "(?<!^)([A-Z])", " $1"));
    }

    public string WindDir(int i)
    {
        switch (i)
        {
            case 1:
                return ForecastMod.Translate("LEFT");
            case 2:
                return ForecastMod.Translate("MID");
            case 3:
                return ForecastMod.Translate("RIGHT");
        }
        return ForecastMod.Translate("RANDOM");
    }
}

