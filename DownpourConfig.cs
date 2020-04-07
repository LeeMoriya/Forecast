using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using UnityEngine;
using RWCustom;
using System.IO;

public class DownpourConfig : OptionInterface
{
    public static Vector2 topAnchor = new Vector2(30f, 480f);
    public static Vector2 checkAnchor = new Vector2(topAnchor.x + 260f, topAnchor.y - 50f);
    public OpRect topRect;
    public OpRadioButtonGroup weatherType;
    public OpRadioButton rainWeather;
    public OpRadioButton snowWeather;
    public OpLabel weatherTypeLabel;
    public OpLabel rainIntensity;
    public OpLabel rainSettings;
    public OpSlider weatherIntensity;
    public OpLabel weatherIntensityDescription;
    public OpLabel rainLabel;
    public OpLabel snowLabel;
    public OpLabel dyn;
    public OpLabel low;
    public OpLabel med;
    public OpLabel hig;
    public OpLabel rainSettingsDescription;
    public OpCheckBox lightningCheck;
    public OpLabel intensitySliderLabel;
    public OpLabel bgLabel;
    public OpLabel directionSliderLabel;
    public OpSlider weatherDirection;
    public OpLabel rainChanceLabel;
    public OpSlider rainChanceSlider;
    public OpLabel lightningLabel;
    public OpCheckBox paletteCheck;
    public OpLabel paletteLabel;
    public OpCheckBox muteCheck;
    public OpLabel muteLabel;
    public OpCheckBox waterCheck;
    public OpLabel waterLabel;
    public OpCheckBox bgOn;
    public OpImage logo;
    public OpImage logo2;
    public OpLabel rainOption;
    public OpSlider rainSlider;
    public OpLabel[] regionLabelList;
    public OpCheckBox[] regionChecks;
    public OpLabel regionLabel;
    public OpLabel regionDescription;
    public OpLabel customRegionSupport;

    public DownpourConfig() : base(RainScript.mod)
    {
    }

    public override bool Configuable()
    {
        return true;
    }

    public override void Initialize()
    {
        string[] regionList = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");

        //Tabs
        this.Tabs = new OpTab[1];
        this.Tabs[0] = new OpTab("Weather");
        
        //Weather Type
        this.logo = new OpImage(new Vector2(270f, 540f), "logo");
        this.logo2 = new OpImage(new Vector2(270f, 540f), "logo2");
        this.weatherType = new OpRadioButtonGroup("Type", 0);
        this.weatherTypeLabel = new OpLabel(new Vector2(30f, 570f), new Vector2(400f, 40f), "Weather Type", FLabelAlignment.Left, true);
        this.rainWeather = new OpRadioButton(new Vector2(30f, 540f));
        this.snowWeather = new OpRadioButton(new Vector2(95f, 540f));
        this.rainLabel = new OpLabel(new Vector2(60f, 533f), new Vector2(400f, 40f), "Rain", FLabelAlignment.Left, false);
        this.snowLabel = new OpLabel(new Vector2(125f, 533f), new Vector2(400f, 40f), "Snow", FLabelAlignment.Left, false);
        this.weatherType.SetButtons(new OpRadioButton[] { rainWeather, snowWeather });
        this.Tabs[0].AddItems(rainWeather, snowWeather, rainLabel, snowLabel, weatherType, weatherTypeLabel);
        
        //Weather Sliders
        this.rainIntensity = new OpLabel(new Vector2(topAnchor.x, topAnchor.y), new Vector2(400f, 40f), "Rain Settings", FLabelAlignment.Left, true);
        this.intensitySliderLabel = new OpLabel(new Vector2(topAnchor.x + 40f, topAnchor.y - 83f), new Vector2(400f, 40f), "Weather Progression", FLabelAlignment.Left, false);
        this.weatherIntensity = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 60f), "weatherIntensity", new IntVector2(0, 3), 50f, false, 0);
        this.rainSettingsDescription = new OpLabel(new Vector2(topAnchor.x, topAnchor.y - 20f), new Vector2(400f, 40f), "Enable or disable rain specific settings:", FLabelAlignment.Left, false);
        this.directionSliderLabel = new OpLabel(new Vector2(topAnchor.x + 47f, topAnchor.y - 143f), new Vector2(400f, 40f), "Weather Direction", FLabelAlignment.Left, false);
        this.weatherDirection = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 120f), "weatherDirection", new IntVector2(0, 3), 50f, false, 0);
        this.rainChanceLabel = new OpLabel(new Vector2(topAnchor.x + 47f, topAnchor.y - 203f), new Vector2(400f, 40f), "Weather Chance", FLabelAlignment.Left, false);
        this.rainChanceSlider = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 180f), "weatherChance", new IntVector2(0, 100), 1.5f, false, 100);
        this.weatherIntensity.description = "Configure whether the intensity of the chosen weather increases as the cycle progresses or fix it to a certain intensity.";
        this.weatherDirection.description = "Configure whether rain should fall towards a random or chosen direction.";
        this.rainChanceSlider.description = "Configure whether the chosen weather will occur during a cycle.";
        this.topRect = new OpRect(new Vector2(15f, 250f), new Vector2(590f, 270f), 0.1f);
        this.Tabs[0].AddItems(rainIntensity, weatherIntensity, intensitySliderLabel, directionSliderLabel, weatherDirection, topRect, logo, logo2, rainChanceSlider, rainChanceLabel);
        
        //Checkboxes
        this.lightningCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y), "Lightning", true);
        this.lightningLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y - 6f), new Vector2(400f, 40f), "Lightning", FLabelAlignment.Left, false);
        this.paletteCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y - 40f), "Palette", true);
        this.paletteLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y - 46f), new Vector2(400f, 40f), "Palette changes", FLabelAlignment.Left, false);
        this.muteCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y), "Mute", false);
        this.muteLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 6f), new Vector2(400f, 40f), "Mute interiors", FLabelAlignment.Left, false);
        this.waterCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y - 40f), "Water", false);
        this.waterLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 46f), new Vector2(400f, 40f), "Water ripples", FLabelAlignment.Left, false);
        this.bgOn = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y - 80f), "Background", true);
        this.bgLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y - 86f), new Vector2(400f, 40f), "Background", FLabelAlignment.Left, false);
        this.lightningCheck.description = "Lightning will have a chance of appearing if the cycle starts with high weather intensity.";
        this.paletteCheck.description = "The region will become darker with higher rain intensity.";
        this.muteCheck.description = "Mute the sound effect added to interiors when its raining outside.";
        this.waterCheck.description = "Rain drops can interact with water surfaces and cause ripples, may impact performance.";
        this.bgOn.description = "Enable or disable collision with background elements.";
        this.Tabs[0].AddItems(lightningLabel, lightningCheck, rainSettingsDescription, paletteCheck, muteCheck, waterCheck, bgOn, paletteLabel, muteLabel, waterLabel, bgLabel);
        
        //Particle Limit
        this.rainOption = new OpLabel(new Vector2(topAnchor.x + 366f, topAnchor.y - 203f), new Vector2(400f, 40f), "Particle Limit", FLabelAlignment.Left, false);
        this.rainSlider = new OpSlider(new Vector2(topAnchor.x + 275f, topAnchor.y - 180f), "rainAmount", new IntVector2(10, 80), 3.3f, false, 50);
        this.Tabs[0].AddItems(rainSlider, rainOption);

        //Regions 
        this.customRegionSupport = new OpLabel(new Vector2(425f, -5f), new Vector2(400f, 0f), "CustomRegions Support: Disabled", FLabelAlignment.Left, false);
        if (regionList != null)
        {
            this.regionLabelList = new OpLabel[regionList.Length];
            this.regionChecks = new OpCheckBox[regionList.Length];
            this.regionLabel = new OpLabel(new Vector2(30f, 200f), new Vector2(400f, 40f), "Region Settings", FLabelAlignment.Left, true);
            this.regionDescription = new OpLabel(new Vector2(30f, 175f), new Vector2(400f, 40f), "Enable and Disable weather on a per-region basis.", FLabelAlignment.Left, false);
            this.Tabs[0].AddItems(regionLabel, regionDescription, customRegionSupport);
            for (int i = 0; i < regionList.Length; i++)
            {
                if (i < 6)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(30f + (95f * i), 150f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(60f + (95f * i), 145f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                else if (i >= 6 && i < 12)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(-540f + (95f * i), 105f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(-510f + (95f * i), 100f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                else if (i >= 12 && i < 18)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(-1110f + (95f * i), 60f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(-1080f + (95f * i), 55f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                else if (i >= 18)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(-1680f + (95f * i), 15f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(-1650f + (95f * i), 10f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                this.Tabs[0].AddItems(regionLabelList[i], regionChecks[i]);
                if (regionList[i] == "UW" || regionList[i] == "SB" || regionList[i] == "SS")
                {
                    regionChecks[i].valueBool = false;
                }
            }
        }
        Downpour.configLoaded = true;
    }
    public override void Update(float dt)
    {
        base.Update(dt);
        //Toggle between rain and snow mode
        if (rainWeather.valueBool)
        {
            rainIntensity.text = "Rain Settings";
            rainSettingsDescription.text = "Enable or disable rain specific settings:";
            bgOn.greyedOut = false;
            muteCheck.greyedOut = false;
            waterCheck.greyedOut = false;
            logo.Show();
            logo2.Hide();
        }
        else
        {
            rainIntensity.text = "Snow Settings";
            rainSettingsDescription.text = "Enable or disable snow specific settings:";
            bgOn.greyedOut = true;
            muteCheck.greyedOut = true;
            waterCheck.greyedOut = true;
            logo.Hide();
            logo2.Show();
        }
        //Intensity Slider
        switch (weatherIntensity.value)
        {
            case "0":
                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Dynamic";
                break;
            case "1":
                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Low";
                break;
            case "2":
                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Medium";
                break;
            case "3":
                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "High";
                break;
        }
        //Direction Slider
        switch (weatherDirection.value)
        {
            case "0":
                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Random";
                break;
            case "1":
                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Left";
                break;
            case "2":
                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Center";
                break;
            case "3":
                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Right";
                break;
        }
        this.customRegionSupport.label.label.color = new Color(0.37f, 0.1f, 0.1f);
    }
    public override void ConfigOnChange()
    {
        string[] regionList = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");
        //List<string> regionList = Menu.FastTravelScreen.GetRegionOrder();
        Downpour.rainRegions = new List<string>();
        for (int i = 0; i < regionList.Length; i++)
        {
            if (config[regionList[i]] == "true")
            {
                Downpour.rainRegions.Add(regionList[i]);
            }
        }
        if (config["Type"] == "0")
        {
            Downpour.snow = false;
        }
        else
        {
            Downpour.snow = true;
        }
        if (config["Palette"] == "false")
        {
            Downpour.paletteChange = false;
        }
        else
        {
            Downpour.paletteChange = true;
        }
        if (config["Mute"] == "false")
        {
            Downpour.interiorRain = false;
        }
        else
        {
            Downpour.interiorRain = true;
        }
        if (config["Water"] == "false")
        {
            Downpour.water = false;
        }
        else
        {
            Downpour.water = true;
        }
        if (config["Background"] == "false")
        {
            Downpour.bg = false;
        }
        else
        {
            Downpour.bg = true;
        }
        if (config["Lightning"] == "false")
        {
            Downpour.lightning = false;
        }
        else
        {
            Downpour.lightning = true;
        }
        if (config["weatherIntensity"] == "0")
        {
            Downpour.intensity = 0;
            Downpour.dynamic = true;
        }
        if (config["weatherIntensity"] == "1")
        {
            Downpour.intensity = 1;
            Downpour.dynamic = false;
        }
        if (config["weatherIntensity"] == "2")
        {
            Downpour.intensity = 2;
            Downpour.dynamic = false;
        }
        if (config["weatherIntensity"] == "3")
        {
            Downpour.intensity = 3;
            Downpour.dynamic = false;
        }
        Downpour.rainAmount = int.Parse(config["rainAmount"]);
        Downpour.direction = int.Parse(config["weatherDirection"]);
        Downpour.rainChance = int.Parse(config["weatherChance"]);
    }
}
