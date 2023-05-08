using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.IO;
using System.Reflection;
using Menu.Remix;
using Menu.Remix.MixedUI;

public class ForecastConfig : OptionInterface
{
    public ForecastConfig(Forecast mod) 
    { 
        
    }

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        Tabs = new[]
        {
            opTab
        };

        OpImage banner = new OpImage(new Vector2(0f, 420f), "logo");
        OpLabel version = new OpLabel(300f, 425f, $"Version: {Forecast.version}     -     By LeeMoriya", false);
        version.label.alignment = FLabelAlignment.Center;
        opTab.AddItems(version, banner);
    }
}

//public class ForecastConfig : OptionInterface
//{
//    public static Vector2 topAnchor = new Vector2(30f, 480f);
//    public static Vector2 checkAnchor = new Vector2(topAnchor.x + 260f, topAnchor.y - 20f);
//    public OpRect topRect;
//    public OpRadioButtonGroup weatherType;
//    public OpRadioButton rainWeather;
//    public OpRadioButton snowWeather;
//    public OpLabel weatherTypeLabel;
//    public OpLabel rainIntensity;
//    public OpLabel rainSettings;
//    public OpSlider weatherIntensity;
//    public OpLabel weatherIntensityDescription;
//    public OpLabel rainLabel;
//    public OpLabel snowLabel;
//    public OpLabel dyn;
//    public OpLabel low;
//    public OpLabel med;
//    public OpLabel hig;
//    public OpLabel rainSettingsDescription;
//    public OpCheckBox lightningCheck;
//    public OpLabel intensitySliderLabel;
//    public OpLabel bgLabel;
//    public OpLabel directionSliderLabel;
//    public OpSlider weatherDirection;
//    public OpLabel rainChanceLabel;
//    public OpSlider rainChanceSlider;
//    public OpLabel lightningLabel;
//    public OpCheckBox paletteCheck;
//    public OpLabel paletteLabel;
//    public OpCheckBox muteCheck;
//    public OpLabel muteLabel;
//    public OpCheckBox effectCheck;
//    public OpLabel effectLabel;
//    public OpCheckBox waterCheck;
//    public OpLabel waterLabel;
//    public OpCheckBox bgOn;
//    public OpCheckBox dustCheck;
//    public OpLabel dustLabel;
//    public OpCheckBox blizzardCheck;
//    public OpLabel blizzardLabel;
//    public OpCheckBox decalCheck;
//    public OpLabel decalLabel;
//    public OpCheckBox strikeCheck;
//    public OpLabel strikeLabel;
//    public OpImage logo;
//    public OpImage logo2;
//    public OpLabel rainOption;
//    public OpSlider rainSlider;
//    public OpLabel[] regionLabelList;
//    public OpCheckBox[] regionChecks;
//    public OpLabel regionLabel;
//    public OpLabel regionDescription;
//    public OpLabel versionNumber;
//    public OpSimpleButton rainButton;
//    public OpSimpleButton snowButton;
//    public bool customRegionsEnabled;
//    public string[] regionList;
//    public OpSliderSubtle strikeDamage;
//    public OpLabel damageLabel;
//    public OpLabel snowWarning;

//    public ForecastConfig() : base(mod: Forecast.mod)
//    {
//    }

//    public override bool Configuable()
//    {
//        return true;
//    }

//    public override void Initialize()
//    {
//        regionList = RegionFinder.Generate().ToArray();

//        //Tabs
//        this.Tabs = new OpTab[1];
//        this.Tabs[0] = new OpTab("Weather");

//        //Weather Type
//        this.logo = new OpImage(new Vector2(270f, 540f), "logo");
//        this.logo2 = new OpImage(new Vector2(270f, 540f), "logo2");
//        this.weatherType = new OpRadioButtonGroup("Type", 0);
//        this.weatherTypeLabel = new OpLabel(new Vector2(30f, 570f), new Vector2(400f, 40f), "Weather Type", FLabelAlignment.Left, true);
//        this.rainWeather = new OpRadioButton(new Vector2(0f, 800f));
//        this.rainButton = new OpSimpleButton(new Vector2(30f, 540f), new Vector2(70f, 25f), "rainButton", "Rain");
//        this.snowWeather = new OpRadioButton(new Vector2(0f, 800f));
//        this.snowButton = new OpSimpleButton(new Vector2(110f, 540f), new Vector2(70f, 25f), "snowButton", "Snow");
//        this.versionNumber = new OpLabel(new Vector2(10f, -5f), new Vector2(0f, 0f), "Version: " + Forecast.mod.Version, FLabelAlignment.Left, false);
//        this.weatherType.SetButtons(new OpRadioButton[] { rainWeather, snowWeather });
//        this.Tabs[0].AddItems(rainButton, rainWeather, snowWeather, snowButton, weatherType, weatherTypeLabel, versionNumber);

//        //Weather Sliders
//        this.rainIntensity = new OpLabel(new Vector2(topAnchor.x, topAnchor.y), new Vector2(400f, 40f), "Rain Settings", FLabelAlignment.Left, true);
//        this.intensitySliderLabel = new OpLabel(new Vector2(topAnchor.x + 40f, topAnchor.y - 83f), new Vector2(400f, 40f), "Weather Progression", FLabelAlignment.Left, false);
//        this.weatherIntensity = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 60f), "weatherIntensity", new IntVector2(0, 3), 50f, false, 0);
//        this.rainSettingsDescription = new OpLabel(new Vector2(topAnchor.x, topAnchor.y - 20f), new Vector2(400f, 40f), "Enable or disable rain specific settings:", FLabelAlignment.Left, false);
//        this.directionSliderLabel = new OpLabel(new Vector2(topAnchor.x + 47f, topAnchor.y - 143f), new Vector2(400f, 40f), "Weather Direction", FLabelAlignment.Left, false);
//        this.weatherDirection = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 120f), "weatherDirection", new IntVector2(0, 3), 50f, false, 0);
//        this.rainChanceLabel = new OpLabel(new Vector2(topAnchor.x + 47f, topAnchor.y - 203f), new Vector2(400f, 40f), "Weather Chance", FLabelAlignment.Left, false);
//        this.rainChanceSlider = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 180f), "weatherChance", new IntVector2(0, 100), 1.5f, false, 100);
//        this.weatherIntensity.description = "Configure whether the intensity of the chosen weather increases as the cycle progresses or fix it to a certain intensity.";
//        this.weatherDirection.description = "Configure whether rain should fall towards a random or chosen direction.";
//        this.rainChanceSlider.description = "Configure whether the chosen weather will occur during a cycle.";
//        this.topRect = new OpRect(new Vector2(15f, 250f), new Vector2(570f, 270f), 0.1f);
//        this.Tabs[0].AddItems(rainIntensity, weatherIntensity, intensitySliderLabel, directionSliderLabel, weatherDirection, topRect, logo, logo2, rainChanceSlider, rainChanceLabel);

//        //Checkboxes
//        this.lightningCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y - 30f), "Lightning", false);
//        this.lightningLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y - 36f), new Vector2(400f, 40f), "Lightning", FLabelAlignment.Left, false);
//        this.paletteCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y + 10), "Palette", true);
//        this.paletteLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y +4f), new Vector2(400f, 40f), "Palette changes", FLabelAlignment.Left, false);
//        this.muteCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y + 10), "Mute", false);
//        this.muteLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y +4f), new Vector2(400f, 40f), "Mute interiors", FLabelAlignment.Left, false);
//        this.waterCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y - 30f), "Water", false);
//        this.waterLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 36f), new Vector2(400f, 40f), "Water ripples", FLabelAlignment.Left, false);
//        this.strikeCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y - 70f), "Strike", true);
//        this.strikeLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y - 76f), new Vector2(400f, 40f), "Lightning Strikes", FLabelAlignment.Left, false);
//        this.strikeDamage = new OpSliderSubtle(new Vector2(checkAnchor.x + 10f, checkAnchor.y - 105f), "Damage", new IntVector2(0, 2), 110, false, 1);
//        this.damageLabel = new OpLabel(new Vector2(checkAnchor.x + 10f, checkAnchor.y - 123f), new Vector2(), "Damage Type: ", FLabelAlignment.Left, false);
//        this.bgOn = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y - 70f), "Background", true);
//        this.bgLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 76f), new Vector2(400f, 40f), "Background", FLabelAlignment.Left, false);
//        this.decalCheck = new OpCheckBox(new Vector2(checkAnchor.x+150f, checkAnchor.y - 30f), "Decals", true);
//        this.decalLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 36f), new Vector2(400f, 40f), "Surface decals", FLabelAlignment.Left, false);
//        this.dustCheck = new OpCheckBox(new Vector2(checkAnchor.x+150f, checkAnchor.y - 70f), "Dust", true);
//        this.dustLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 76f), new Vector2(400f, 40f), "Snow dust", FLabelAlignment.Left, false);
//        this.blizzardCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y - 110f), "Blizzard", true);
//        this.blizzardLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 116f), new Vector2(400f, 40f), "Blizzard", FLabelAlignment.Left, false);
//        this.effectCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y + 10), "Effect", true);
//        this.effectLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y + 4f), new Vector2(400f, 40f), "Effect Colors", FLabelAlignment.Left, false);
//        this.dustCheck.description = "Puffs of snow appear when landing on the ground.";
//        this.decalCheck.description = "Adds snowy decals to surfaces.";
//        this.lightningCheck.description = "Lightning will appear at higher weather intensities.";
//        this.paletteCheck.description = "The region will become darker with higher rain intensity.";
//        this.muteCheck.description = "Mute the sound effect added to interiors when its raining outside.";
//        this.waterCheck.description = "Rain drops can interact with water surfaces and cause ripples, may impact performance.";
//        this.bgOn.description = "Enable or disable collision with background elements, may impact performance.";
//        this.strikeDamage.description = "Adjust the damage type of Lightning Strikes";
//        this.strikeCheck.description = "When weather intensity is high enough, lightning strikes can occur.";
//        this.effectCheck.description = "Whitens things like plants and signs so they better match the snowy palette, can ruin some custom props";
//        this.blizzardCheck.description = "Replaces the end-of-cycle rain with a roaring blizzard and affects normal gameplay";
//        this.Tabs[0].AddItems(lightningLabel, lightningCheck, strikeCheck, strikeLabel, strikeDamage, damageLabel, rainSettingsDescription, paletteCheck, muteCheck, waterCheck, bgOn, paletteLabel, muteLabel, waterLabel, bgLabel, dustCheck,dustLabel,decalCheck,decalLabel, effectCheck, effectLabel, blizzardCheck,blizzardLabel);

//        //Particle Limit
//        this.rainOption = new OpLabel(new Vector2(topAnchor.x + 366f, topAnchor.y - 223f), new Vector2(400f, 40f), "Particle Limit", FLabelAlignment.Left, false);
//        this.rainSlider = new OpSlider(new Vector2(topAnchor.x + 275f, topAnchor.y - 200f), "rainAmount", new IntVector2(10, 80), 3.3f, false, 50);
//        this.Tabs[0].AddItems(rainSlider, rainOption);

//        //Regions
//        if (regionList != null)
//        {
//            this.regionLabelList = new OpLabel[regionList.Length];
//            this.regionChecks = new OpCheckBox[regionList.Length];
//            this.regionLabel = new OpLabel(new Vector2(30f, 200f), new Vector2(400f, 40f), "Region Settings", FLabelAlignment.Left, true);
//            this.regionDescription = new OpLabel(new Vector2(30f, 175f), new Vector2(400f, 40f), "Enable and Disable weather on a per-region basis.", FLabelAlignment.Left, false);
//            this.Tabs[0].AddItems(regionLabel, regionDescription);
//            for (int i = 0; i < regionList.Length; i++)
//            {
//                if (i < 10)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(30f + (55f * i), 150f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(60f + (55f * i), 142f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                else if (i >= 10 && i < 20)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(-520f + (55f * i), 105f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(-490f + (55f * i), 97f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                else if (i >= 20 && i < 30)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(-1070f + (55f * i), 60f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(-1040f + (55f * i), 52f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                else if (i >= 30)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(-1650f + (55f * i), 15f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(-1620f + (55f * i), 7f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                this.Tabs[0].AddItems(regionLabelList[i], regionChecks[i]);
//                if (regionList[i] == "SS")
//                {
//                    regionChecks[i].valueBool = false;
//                }
//            }
//        }
//        Forecast.configLoaded = true;
//    }
//    public override void Signal(UItrigger trigger, string signal)
//    {
//        base.Signal(trigger, signal);
//        if (signal == "rainButton")
//        {
//            this.rainWeather.valueBool = true;
//        }
//        if (signal == "snowButton")
//        {
//            this.snowWeather.valueBool = true;
//        }
//    }
//    public override void Update(float dt)
//    {
//        base.Update(dt);
//        //Toggle between rain and snow mode
//        if (this.rainWeather.valueBool)
//        {
//            rainIntensity.text = "Rain Settings";
//            rainSettingsDescription.text = "Enable or disable rain specific settings:";
//            paletteCheck.description = "The region will become darker with higher rain intensity.";
//            logo.Show();
//            logo2.Hide();
//            //Hide rain checks
//            muteCheck.Show();
//            waterCheck.Show();
//            bgOn.Show();
//            //Show rain check labels
//            bgLabel.Show();
//            muteLabel.Show();
//            waterLabel.Show();
//            //Hide snow checks
//            decalCheck.Hide();
//            dustCheck.Hide();
//            effectCheck.Hide();
//            blizzardCheck.Hide();
//            //Hide snow labels
//            decalLabel.Hide();
//            dustLabel.Hide();
//            effectLabel.Hide();
//            blizzardLabel.Hide();
//        }
//        else
//        {
//            rainIntensity.text = "Snow Settings";
//            rainSettingsDescription.text = "Enable or disable snow specific settings:";
//            paletteCheck.description = "A snowy palette will be overlayed onto the current palette.";
//            logo.Hide();
//            logo2.Show();
//            //Disable rain checks
//            muteCheck.valueBool = false;
//            waterCheck.valueBool = false;
//            bgOn.valueBool = false;
//            //Hide rain checks
//            muteCheck.Hide();
//            waterCheck.Hide();
//            bgOn.Hide();
//            //Hide rain check labels
//            bgLabel.Hide();
//            muteLabel.Hide();
//            waterLabel.Hide();
//            //Hide snow checks
//            decalCheck.Show();
//            dustCheck.Show();
//            effectCheck.Show();
//            blizzardCheck.Show();
//            //Hide snow labels
//            decalLabel.Show();
//            dustLabel.Show();
//            effectLabel.Show();
//            blizzardLabel.Show();
//        }
//        if(lightningCheck.valueBool == false)
//        {
//            strikeCheck.valueBool = false;
//            strikeCheck.greyedOut = true;
//            strikeCheck.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//            strikeLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//        }
//        else
//        {
//            strikeCheck.greyedOut = false;
//            strikeCheck.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//            strikeLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//        }
//        if (lightningCheck.valueBool == false || strikeCheck.valueBool == false)
//        {
//            strikeDamage.greyedOut = true;
//            strikeDamage.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//            strikeDamage.colorLine = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//            damageLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//        }
//        else
//        {
//            strikeDamage.greyedOut = false;
//            strikeDamage.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//            strikeDamage.colorLine = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//            damageLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//        }
//        if (this.strikeDamage != null)
//        {
//            switch (this.strikeDamage.valueInt)
//            {
//                case 0:
//                    this.damageLabel.text = "Damage Type: None";
//                    break;
//                case 1:
//                    this.damageLabel.text = "Damage Type: Stun";
//                    break;
//                case 2:
//                    this.damageLabel.text = "Damage Type: Lethal";
//                    break;
//            }
//        }
//        //Intensity Slider
//        switch (weatherIntensity.value)
//        {
//            case "0":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Dynamic";
//                break;
//            case "1":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Low";
//                break;
//            case "2":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Medium";
//                break;
//            case "3":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "High";
//                break;
//        }
//        //Direction Slider
//        switch (weatherDirection.value)
//        {
//            case "0":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Random";
//                break;
//            case "1":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Left";
//                break;
//            case "2":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Center";
//                break;
//            case "3":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Right";
//                break;
//        }
//    }
//    public override void ConfigOnChange()
//    {
//        Forecast.rainRegions = new List<string>();
//        for (int i = 0; i < regionList.Length; i++)
//        {
//            if (config[regionList[i]] == "true")
//            {
//                Forecast.rainRegions.Add(regionList[i]);
//            }
//        }
//        if (config["Type"] == "0")
//        {
//            Forecast.snow = false;
//        }
//        else
//        {
//            Forecast.snow = true;
//        }
//        if (config["Palette"] == "false")
//        {
//            Forecast.paletteChange = false;
//        }
//        else
//        {
//            Forecast.paletteChange = true;
//        }
//        if(config["Effect"] == "false")
//        {
//            Forecast.effectColors = false;
//        }
//        else
//        {
//            Forecast.effectColors = true;
//        }
//        if(config["Blizzard"] == "false")
//        {
//            Forecast.blizzard = false;
//        }
//        else
//        {
//            Forecast.blizzard = true;
//        }
//        if (config["Mute"] == "false")
//        {
//            Forecast.interiorRain = false;
//        }
//        else
//        {
//            Forecast.interiorRain = true;
//        }
//        if (config["Water"] == "false")
//        {
//            Forecast.water = false;
//        }
//        else
//        {
//            Forecast.water = true;
//        }
//        if (config["Background"] == "false")
//        {
//            Forecast.bg = false;
//        }
//        else
//        {
//            Forecast.bg = true;
//        }
//        if (config["Lightning"] == "false")
//        {
//            Forecast.lightning = false;
//        }
//        else
//        {
//            Forecast.lightning = true;
//        }
//        if (config["Strike"] == "false")
//        {
//            Forecast.strike = false;
//        }
//        else
//        {
//            Forecast.strike = true;
//        }
//        if (config["Decals"] == "false")
//        {
//            Forecast.decals = false;
//        }
//        else
//        {
//            Forecast.decals = true;
//        }
//        if (config["Dust"] == "false")
//        {
//            Forecast.dust = false;
//        }
//        else
//        {
//            Forecast.dust = true;
//        }
//        if (config["weatherIntensity"] == "0")
//        {
//            Forecast.intensity = 0;
//            Forecast.dynamic = true;
//        }
//        if (config["weatherIntensity"] == "1")
//        {
//            Forecast.intensity = 1;
//            Forecast.dynamic = false;
//        }
//        if (config["weatherIntensity"] == "2")
//        {
//            Forecast.intensity = 2;
//            Forecast.dynamic = false;
//        }
//        if (config["weatherIntensity"] == "3")
//        {
//            Forecast.intensity = 3;
//            Forecast.dynamic = false;
//        }
//        Forecast.rainAmount = int.Parse(config["rainAmount"]);
//        Forecast.direction = int.Parse(config["weatherDirection"]);
//        Forecast.rainChance = int.Parse(config["weatherChance"]);
//        Forecast.strikeDamage = int.Parse(config["Damage"]);
//    }
//}
