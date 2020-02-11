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
        this.Tabs = new OpTab[2];
        this.Tabs[0] = new OpTab("Options");
        this.Tabs[1] = new OpTab("Regions");
        //Rain
        OptionalUI.OpLabel rainIntensity = new OpLabel(new Vector2(30f, 560f), new Vector2(400f, 40f), "Rain Intensity", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainIntensityDescription = new OpLabel(new Vector2(30f, 537f), new Vector2(400f, 40f), "Change the intensity of the rainfall to be dynamic, or a fixed value.", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButtonGroup intensityGroup = new OpRadioButtonGroup("Setting", 0);
        OptionalUI.OpRadioButton intensityDynamic = new OpRadioButton(new Vector2(30f, 510f));
        intensityDynamic.description = "Rain intensity can vary at the start of a cycle and may start mid-way through it, it will then increase as the cycle progresses.";
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
        OptionalUI.OpCheckBox rainbowOn = new OpCheckBox(new Vector2(200f, 415f), "Rainbow", false);
        rainbowOn.description = "Raindrop colors will be randomized.";
        OptionalUI.OpLabel onrainbowLabel = new OpLabel(new Vector2(230f, 408f), new Vector2(400f, 40f), "Taste the rainbow", FLabelAlignment.Left, false);
        this.Tabs[0].AddItems(onrainbowLabel, rainbowOn);
        //Snow
        OptionalUI.OpCheckBox snowOn = new OpCheckBox(new Vector2(200f, 385f), "Snow", false);
        //snowOn.description = "Replace rain with snow.";
        OptionalUI.OpLabel snowLabel = new OpLabel(new Vector2(230f, 378f), new Vector2(400f, 40f), "Coming Soon", FLabelAlignment.Left, false);
        snowOn.greyedOut = true;
        this.Tabs[0].AddItems(snowOn, snowLabel);
        //Coming soon
        OptionalUI.OpCheckBox placeholder = new OpCheckBox(new Vector2(200f, 355f), "placeholder", false);
        //snowOn.description = "Replace rain with snow.";
        OptionalUI.OpLabel placeholderLabel = new OpLabel(new Vector2(230f, 348f), new Vector2(400f, 40f), "Coming Soon", FLabelAlignment.Left, false);
        placeholder.greyedOut = true;
        this.Tabs[0].AddItems(placeholder, placeholderLabel);
        //Background Rain
        OptionalUI.OpCheckBox bgOn = new OpCheckBox(new Vector2(30f, 355f), "Background", true);
        bgOn.description = "Enable or disable collision with background elements.";
        OptionalUI.OpLabel bgLabel = new OpLabel(new Vector2(60f, 348f), new Vector2(400f, 40f), "Background Collision", FLabelAlignment.Left, false);
        this.Tabs[0].AddItems(bgOn, bgLabel);

        //Direction
        OptionalUI.OpLabel rainDirection = new OpLabel(new Vector2(30f, 310f), new Vector2(400f, 40f), "Rain Direction", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainDirectionDescription = new OpLabel(new Vector2(30f, 287f), new Vector2(400f, 40f), "Change the direction rain falls as intensity increases.", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButtonGroup rainDirectionGroup = new OpRadioButtonGroup("Direction", 0);
        OptionalUI.OpRadioButton rainDirectionRandom = new OpRadioButton(new Vector2(30f, 260f));
        OptionalUI.OpRadioButton rainDirectionLeft = new OpRadioButton(new Vector2(130f, 260f));
        OptionalUI.OpRadioButton rainDirectionCenter = new OpRadioButton(new Vector2(210, 260f));
        OptionalUI.OpRadioButton rainDirectionRight = new OpRadioButton(new Vector2(290f, 260f));
        OptionalUI.OpLabel randomLabel = new OpLabel(new Vector2(60f, 253f), new Vector2(400f, 40f), "Random", FLabelAlignment.Left, false);
        OptionalUI.OpLabel leftLabel = new OpLabel(new Vector2(160f, 253f), new Vector2(400f, 40f), "Left", FLabelAlignment.Left, false);
        OptionalUI.OpLabel centerLabel = new OpLabel(new Vector2(240f, 253f), new Vector2(400f, 40f), "Center", FLabelAlignment.Left, false);
        OptionalUI.OpLabel rightLabel = new OpLabel(new Vector2(320, 253f), new Vector2(400f, 40f), "Right", FLabelAlignment.Left, false);
        rainDirectionGroup.SetButtons(new OpRadioButton[] { rainDirectionRandom, rainDirectionLeft, rainDirectionCenter, rainDirectionRight });
        this.Tabs[0].AddItems(rainDirection, rainDirectionDescription, rainDirectionGroup, rainDirectionLeft, leftLabel, rainDirectionRandom, randomLabel, rainDirectionRight, rightLabel, rainDirectionCenter, centerLabel);

        //Raindrops
        OptionalUI.OpLabel rainOption = new OpLabel(new Vector2(30f, 208f), new Vector2(400f, 40f), "Raindrops", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainOptionDescription = new OpLabel(new Vector2(30f, 188f), new Vector2(400f, 40f), "Configure the maximum amount of raindrops that can be spawned a single room (x10).", FLabelAlignment.Left, false);
        OptionalUI.OpLabel rainOptionWarning = new OpLabel(new Vector2(30f, 173f), new Vector2(400f, 40f), "Warning: You may experience significant framedrops if this slider is set too high.", FLabelAlignment.Left, false);
        OptionalUI.OpSlider rainSlider = new OpSlider(new Vector2(30f, 140f), "rainAmount", new IntVector2(10, 80), 4.235f, false, 50);
        this.Tabs[0].AddItems(rainSlider, rainOption, rainOptionDescription, rainOptionWarning);
        //Rain Chance
        OptionalUI.OpLabel rainChance = new OpLabel(new Vector2(30f, 108f), new Vector2(400f, 40f), "Rain Chance", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainChanceDescription = new OpLabel(new Vector2(30f, 88f), new Vector2(400f, 40f), "Configure the chance rain will occur.", FLabelAlignment.Left, false);
        OptionalUI.OpSlider rainChanceSlider = new OpSlider(new Vector2(30f, 55f), "rainChance", new IntVector2(0, 100), 3f, false, 75);
        this.Tabs[0].AddItems(rainChance, rainChanceDescription, rainChanceSlider);
        //Regions 
        if (regionList != null)
        {
            OptionalUI.OpLabel[] regionLabelList = new OpLabel[regionList.Length];
            OptionalUI.OpCheckBox[] regionChecks = new OpCheckBox[regionList.Length];
            OptionalUI.OpLabel regionLabel = new OpLabel(new Vector2(30f, 200f), new Vector2(400f, 40f), "Region Settings", FLabelAlignment.Left, true);
            OptionalUI.OpLabel regionDescription = new OpLabel(new Vector2(30f, 175f), new Vector2(400f, 40f), "Enable and Disable rainfall on a per-region basis.", FLabelAlignment.Left, false);
            this.Tabs[1].AddItems(regionLabel, regionDescription);
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
                this.Tabs[1].AddItems(regionLabelList[i], regionChecks[i]);
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
        Downpour.direction = int.Parse(OptionalUI.OptionInterface.config["Direction"]);
        Downpour.rainChance = int.Parse(OptionalUI.OptionInterface.config["rainChance"]);
    }
}
