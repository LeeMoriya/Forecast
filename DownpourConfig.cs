﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using UnityEngine;
using RWCustom;
using System.IO;

public class DownpourConfig : OptionInterface
{
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
        //List<string> regionList = Menu.FastTravelScreen.GetRegionOrder();
        this.Tabs = new OpTab[1];
        this.Tabs[0] = new OpTab("Options");
        //Rain
        OptionalUI.OpLabel rainIntensity = new OpLabel(new Vector2(30f, 560f), new Vector2(400f, 40f), "Rain Intensity", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainIntensityDescription = new OpLabel(new Vector2(30f, 537f), new Vector2(400f, 40f), "Change the intensity of the rainfall to be dynamic, or a fixed value.", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButtonGroup intensityGroup = new OpRadioButtonGroup("Setting", 0);
        OptionalUI.OpRadioButton intensityDynamic = new OpRadioButton(new Vector2(30f, 510f));
        intensityDynamic.description = "Intensity of the rain is randomly determined and affected by karma level, there can also be no rain at all.";
        OptionalUI.OpLabel dynamicLabel = new OpLabel(new Vector2(60f, 503f), new Vector2(400f, 40f), "Dynamic", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButton intensityLow = new OpRadioButton(new Vector2(130f, 510f));
        intensityLow.description = "Intensity of the rain will be fixed to Low intensity.";
        OptionalUI.OpLabel lowLabel = new OpLabel(new Vector2(160f, 503f), new Vector2(400f, 40f), "Low", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButton intensityMed = new OpRadioButton(new Vector2(210f, 510f));
        intensityMed.description = "Intensity of the rain will be fixed to Medium intensity.";
        OptionalUI.OpLabel medLabel = new OpLabel(new Vector2(240f, 503f), new Vector2(400f, 40f), "Med", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButton intensityHigh = new OpRadioButton(new Vector2(290f, 510f));
        intensityHigh.description = "Intensity of the rain will be fixed to High intensity.";
        OptionalUI.OpLabel highLabel = new OpLabel(new Vector2(320f, 503f), new Vector2(400f, 40f), "High", FLabelAlignment.Left, false);
        intensityGroup.SetButtons(new OpRadioButton[] { intensityDynamic, intensityLow, intensityMed, intensityHigh });
        this.Tabs[0].AddItems(intensityGroup, highLabel, intensityDynamic, intensityHigh, intensityMed, intensityLow, rainIntensity, rainIntensityDescription, medLabel, lowLabel, dynamicLabel);
        //Lightning
        OptionalUI.OpLabel environmentOption = new OpLabel(new Vector2(30f, 460f), new Vector2(400f, 40f), "Environment", FLabelAlignment.Left, true);
        OptionalUI.OpLabel lightningOptionDescription = new OpLabel(new Vector2(30f, 437f), new Vector2(400f, 40f), "Configure which effects are added to the environment during heavy rain.", FLabelAlignment.Left, false);
        OptionalUI.OpCheckBox lightningCheck = new OpCheckBox(new Vector2(30f, 415f), "Lightning", true);
        lightningCheck.description = "Lightning will appear in regions when rain intensity is high enough.";
        OptionalUI.OpLabel lightningLabel = new OpLabel(new Vector2(60f, 408f), new Vector2(400f, 40f), "Lightning storms", FLabelAlignment.Left, false);
        this.Tabs[0].AddItems(lightningLabel, lightningCheck, lightningOptionDescription, environmentOption);
        //Palette
        OptionalUI.OpCheckBox paletteCheck = new OpCheckBox(new Vector2(30f, 385f), "Palette", true);
        paletteCheck.description = "The region will become darker with higher rain intensity.";
        OptionalUI.OpLabel paletteLabel = new OpLabel(new Vector2(60f, 378f), new Vector2(400f, 40f), "Regions become darker", FLabelAlignment.Left, false);
        this.Tabs[0].AddItems(paletteLabel, paletteCheck);
        //Rainbow
        OptionalUI.OpCheckBox rainbowOn = new OpCheckBox(new Vector2(30f, 355f), "Rainbow", false);
        rainbowOn.description = "Raindrop colors will be randomized.";
        OptionalUI.OpLabel onrainbowLabel = new OpLabel(new Vector2(60f, 348f), new Vector2(400f, 40f), "Taste the rainbow", FLabelAlignment.Left, false);
        this.Tabs[0].AddItems(onrainbowLabel, rainbowOn);
        //Snow
        OptionalUI.OpCheckBox snowOn = new OpCheckBox(new Vector2(200f, 415f), "Snow", false);
        snowOn.description = "Replace rain with snow.";
        OptionalUI.OpLabel snowLabel = new OpLabel(new Vector2(230f, 408f), new Vector2(400f, 40f), "Snow", FLabelAlignment.Left, false);
        this.Tabs[0].AddItems(snowOn, snowLabel);
        //Background Rain
        OptionalUI.OpCheckBox bgOn = new OpCheckBox(new Vector2(200f, 385f), "Background", true);
        bgOn.description = "Enable or disable collision with background elements, can improve performance if disabled.";
        OptionalUI.OpLabel bgLabel = new OpLabel(new Vector2(230f, 378f), new Vector2(400f, 40f), "Background Collision", FLabelAlignment.Left, false);
        this.Tabs[0].AddItems(bgOn, bgLabel);
        //Raindrops
        OptionalUI.OpLabel rainOption = new OpLabel(new Vector2(30f, 308f), new Vector2(400f, 40f), "Raindrops", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainOptionDescription = new OpLabel(new Vector2(30f, 288f), new Vector2(400f, 40f), "Configure the maximum amount of raindrops that can be spawned each frame.", FLabelAlignment.Left, false);
        OptionalUI.OpLabel rainOptionWarning = new OpLabel(new Vector2(30f, 273f), new Vector2(400f, 40f), "Warning: You may experience significant framedrops if this slider is set too high.", FLabelAlignment.Left, false);
        OptionalUI.OpSlider rainSlider = new OpSlider(new Vector2(30f, 240f), "rainAmount", new IntVector2(10, 80), 4f, false, 50);
        this.Tabs[0].AddItems(rainSlider, rainOption, rainOptionDescription, rainOptionWarning);
        //Regions 
        if (regionList != null)
        {
            OptionalUI.OpLabel[] regionLabelList = new OpLabel[regionList.Length];
            OptionalUI.OpCheckBox[] regionChecks = new OpCheckBox[regionList.Length];
            OptionalUI.OpLabel regionLabel = new OpLabel(new Vector2(30f, 200f), new Vector2(400f, 40f), "Region Settings", FLabelAlignment.Left, true);
            OptionalUI.OpLabel regionDescription = new OpLabel(new Vector2(30f, 175f), new Vector2(400f, 40f), "Enable and Disable rainfall on a per-region basis.", FLabelAlignment.Left, false);
            this.Tabs[0].AddItems(regionLabel, regionDescription);
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
    // Apply changes to the mod
    public override void ConfigOnChange()
    {
        string[] regionList = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");
        //List<string> regionList = Menu.FastTravelScreen.GetRegionOrder();
        Downpour.rainRegions = new List<string>();
        for (int i = 0; i < regionList.Length; i++)
        {
            if (OptionalUI.OptionInterface.config[regionList[i]] == "true")
            {
                Downpour.rainRegions.Add(regionList[i]);
            }
        }
        Downpour.rainAmount = int.Parse(OptionalUI.OptionInterface.config["rainAmount"]);
        if (OptionalUI.OptionInterface.config["Palette"] == "false")
        {
            Downpour.paletteChange = false;
        }
        else
        {
            Downpour.paletteChange = true;
        }
        if (OptionalUI.OptionInterface.config["Snow"] == "false")
        {
            Downpour.snow = false;
        }
        else
        {
            Downpour.snow = true;
        }
        if (OptionalUI.OptionInterface.config["Background"] == "false")
        {
            Downpour.bg = false;
        }
        else
        {
            Downpour.bg = true;
        }
        if (OptionalUI.OptionInterface.config["Lightning"] == "false")
        {
            Downpour.lightning = false;
        }
        else
        {
            Downpour.lightning = true;
        }
        if (OptionalUI.OptionInterface.config["Rainbow"] == "false")
        {
            Downpour.rainbow = false;
        }
        else
        {
            Downpour.rainbow = true;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "0")
        {
            Downpour.intensity = 0;
            Downpour.dynamic = true;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "1")
        {
            Downpour.intensity = 1;
            Downpour.dynamic = false;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "2")
        {
            Downpour.intensity = 2;
            Downpour.dynamic = false;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "3")
        {
            Downpour.intensity = 3;
            Downpour.dynamic = false;
        }
    }
}